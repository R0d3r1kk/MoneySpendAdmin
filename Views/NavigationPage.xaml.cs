using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using MoneySpendAdmin.Shared;
using System.Text;
using MoneySpendAdmin.DAL;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using MoneySpendAdmin.DAL.Entities;
using MoneySpendAdmin.DAL.Repository;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MoneySpendAdmin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationPage : Page
    {
        private IDataAccess dataAccess;
        private PDFModel pdfModel;

        public NavigationPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var global = e.Parameter as Global;
            if (global != null)
            {
                this.dataAccess = global.dataAccess;
                NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
                ContentFrame.Navigate(
                    typeof(Views.HomePage),
                    new Global()
                    {
                        dataAccess = global.dataAccess,
                    },
                    new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                );
            }
            base.OnNavigatedTo(e);
        }

        public string GetTodayDate()
        {
            return DateTime.Now.ToLongDateString().ToUpperInvariant();
        }

        #region Navigation
        private void NavigationViewControl_ItemInvoked(NavigationView sender,
                      NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                ContentFrame.Navigate(
                    typeof(Views.SettingsPage),
                    new Global()
                    {
                        dataAccess = this.dataAccess,
                    },
                    args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
            {
                Type newPage = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                ContentFrame.Navigate(
                       newPage,
                       new Global()
                       {
                           dataAccess = this.dataAccess,
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

        #endregion

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

        private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            App.MAIN.Loading(true);
            // Open the picker for the user to pick a file
            try
            {
                var file = await OpenFileAsync();
                if (file == null)
                {
                    App.MAIN.Loading(false);
                    throw new Exception("Archivo sin seleccionar");
                }
                var randomAccessStream = await file.OpenReadAsync();
                Stream stream = randomAccessStream.AsStreamForRead();
                var pages = extractTextFromPDF(stream);
                this.pdfModel = new PDFModel(dataAccess, pages);
                await pdfModel.formatPageLines(file.Path, file.Name);
                App.MAIN.Loading(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                App.MAIN.Loading(false, true);
            }
        }

        public List<string> extractTextFromPDF(Stream document)
        {
            App.MAIN.NotifyUser("Extrayendo texto de archivo", InfoBarSeverity.Informational);
            var pdfReader = new PdfReader(document);
            var textList = new List<string>();
            for (int i = 0; i < pdfReader.NumberOfPages; i++)
            {
                var locationTextExtractionStrategy = new LocationTextExtractionStrategy();

                string textFromPage = PdfTextExtractor.GetTextFromPage(pdfReader, i + 1, locationTextExtractionStrategy);

                textList.Add(Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(textFromPage))));
                //Do Something with the text
            }
            App.MAIN.NotifyUser("Texto extraido", InfoBarSeverity.Success);
            return textList;
        }
    }
}
