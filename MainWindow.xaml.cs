using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MoneySpendAdmin.Shared;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using System.Collections.Generic;
using MoneySpendAdmin.DAL;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MoneySpendAdmin
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        IDataAccess da;
        PDFModel pdfModel;
        public MainWindow(IDataAccess da)
        {
            this.InitializeComponent();
            this.da = da;
            var appWindowPresenter = this.AppWindow.Presenter as OverlappedPresenter;
            appWindowPresenter.IsResizable = false;

            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
            ContentFrame.Navigate(
                       typeof(Views.HomePage),
                       new Global()
                       {
                       },
                       new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                       );

            SystemBackdrop = new MicaBackdrop()
            { Kind = MicaKind.Base };

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            loader.ShowPaused = true;
            loader.Value = 30;
        }

        public string GetAppTitleFromSystem()
        {
            return Windows.ApplicationModel.Package.Current.DisplayName;
        }

        public string GetTodayDate()
        {
            return DateTime.Now.ToLongDateString().ToUpperInvariant();
        }

        private async Task<StorageFile> OpenFileAsync()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.FileTypeFilter.Add(".pdf");

            // When running on win32, FileOpenPicker needs to know the top-level hwnd via IInitializeWithWindow::Initialize.
            if (Window.Current == null)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

                WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);
            }

            StorageFile file = await filePicker.PickSingleFileAsync();
            return file;
        }

        private async Task<StorageFile> GetFileFromInstalledLocation(string path)
        {
            StorageFile file = null;
            var installedPath = Package.Current.InstalledLocation.Path;

            try
            {
                var installedFolder = await StorageFolder.GetFolderFromPathAsync(installedPath);
                file = await installedFolder.GetFileAsync(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return file;
        }

        private void NavigationViewControl_ItemInvoked(NavigationView sender,
                      NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                ContentFrame.Navigate(typeof(Views.SettingsPage), null, args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
            {
                Type newPage = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                ContentFrame.Navigate(
                       newPage,
                       new Global()
                       {
                       },
                       args.RecommendedNavigationTransitionInfo
                       );
            }
        }

        private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack) ContentFrame.GoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(Views.SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(ContentFrame.SourcePageType.FullName.ToString()));
            }

            NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
        }

        private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            loading(true);
            // Open the picker for the user to pick a file
            try
            {
                var file = await OpenFileAsync();
                if (file == null)
                {
                    loading(false);
                    throw new Exception("Archivo sin seleccionar");
                    return;
                }
                var randomAccessStream = await file.OpenReadAsync();
                Stream stream = randomAccessStream.AsStreamForRead();
                var pages = extractTextFromPDF(stream);
                this.pdfModel = new PDFModel(da, pages);
                await pdfModel.formatPageLines(file.Path, file.Name);
                loader.IsIndeterminate = false;
                loading(false);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                loading(false, true);
            }
        }
        public List<string> extractTextFromPDF(Stream document)
        {
            var pdfReader = new PdfReader(document);
            var textList = new List<string>();
            for (int i = 0; i < pdfReader.NumberOfPages; i++)
            {
                var locationTextExtractionStrategy = new LocationTextExtractionStrategy();

                string textFromPage = PdfTextExtractor.GetTextFromPage(pdfReader, i + 1, locationTextExtractionStrategy);

                textList.Add(Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(textFromPage))));
                //Do Something with the text
            }

            return textList;
        }

        private void loading(bool status, bool error = false)
        {
            loader.IsIndeterminate = status;
            if (loader.IsIndeterminate)
            {
                if (error)
                {
                    loader.ShowPaused = false;
                    loader.ShowError = true;
                    loader.Value = 30;
                }
                else
                {
                    loader.ShowPaused = false;
                    loader.ShowError = false;
                    loader.Value = 0;
                }
            }
            else
            {
                if (error)
                {
                    loader.ShowPaused = false;
                    loader.ShowError = true;
                }
                else
                {
                    loader.ShowPaused = true;
                    loader.ShowError = false;
                }

                loader.Value = 30;
            }
        }
    }
}
