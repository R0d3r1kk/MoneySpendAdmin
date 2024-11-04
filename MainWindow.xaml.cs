using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using MoneySpendAdmin.DAL;
using Microsoft.UI;
using Windows.Graphics;
using MoneySpendAdmin.Views;
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
        public MainWindow(IDataAccess da)
        {
            this.InitializeComponent();
            this.da = da;

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(1920, 1080));

            //var appWindowPresenter = appWindow.Presenter as OverlappedPresenter;
            //appWindowPresenter.IsResizable = false;


            SystemBackdrop = new DesktopAcrylicBackdrop();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            loader.ShowPaused = true;
            loader.Value = 30;



        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            //await Task.Delay(TimeSpan.FromSeconds(3));

            //MainContentFrame.Navigate(typeof(NavigationPage), new Global
            //{
            //    dataAccess = this.da
            //}, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());

            MainContentFrame.Navigate(typeof(LandingPage), null, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
        }

        public string GetAppTitleFromSystem()
        {
            return Windows.ApplicationModel.Package.Current.DisplayName;
        }


        public void NotifyUser(string strMessage, InfoBarSeverity severity, bool isOpen = true)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (DispatcherQueue.HasThreadAccess)
            {
                UpdateStatus(strMessage, severity, isOpen);
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateStatus(strMessage, severity, isOpen);
                });
            }
        }

        private void UpdateStatus(string strMessage, InfoBarSeverity severity, bool isOpen)
        {
            infoBar.Message = strMessage;
            infoBar.IsOpen = isOpen;
            infoBar.Severity = severity;
        }

        public void Loading(bool status, bool error = false)
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
