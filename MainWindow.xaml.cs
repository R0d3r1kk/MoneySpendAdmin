using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using pdftron.PDF;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using static pdftron.PDF.Tools.UtilityWinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MoneySpendAdmin
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        PDFViewCtrl mPdfView;

        public MainWindow()
        {
            this.InitializeComponent();

            var appWindowPresenter = this.AppWindow.Presenter as OverlappedPresenter;
            appWindowPresenter.IsResizable = false;

            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
            ContentFrame.Navigate(
                       typeof(Views.HomePage),
                       null,
                       new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                       );

            SystemBackdrop = new MicaBackdrop()
            { Kind = MicaKind.Base };

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        public string GetAppTitleFromSystem()
        {
            return Windows.ApplicationModel.Package.Current.DisplayName;
        }

        public string GetTodayDate()
        {
            return DateTime.Now.ToLongDateString().ToUpperInvariant();
        }

        private void OpenFileOnViewer(PDFDoc doc)
        {
            if (mPdfView == null)
            {
                mPdfView = new PDFViewCtrl();
                //pdfViewBorder.Child = mPdfView;

                var toolManager = new pdftron.PDF.Tools.ToolManager(mPdfView);
            }
            else
            {
                mPdfView.CloseDoc();
            }

            mPdfView.SetDoc(doc);
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
                IntPtr hwnd = GetActiveWindow();
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
                       null,
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

            // Open the picker for the user to pick a file
            var file = await OpenFileAsync();
            if (file == null) return;

            var doc = PDFDoc.CreateFromStorageFile(file);

            OpenFileOnViewer(doc);
        }


    }
}
