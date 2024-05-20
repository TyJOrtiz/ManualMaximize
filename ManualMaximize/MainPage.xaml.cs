using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ManualMaximize
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AppServiceConnection inventoryService;
        private AppServiceConnection serviceConnection = null;

        public AppWindow AppWindow { get; private set; }

        /// <summary>
        /// CoreWindowInterop (thanks to AhmedWalid @AhmedWalid605 on Twitter)
        /// </summary>
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        [System.Runtime.InteropServices.Guid("0764019b-52c1-41f9-b6f2-9cc205973692")]
        interface IInternalCoreWindowPhone
        {
            object NavigationClient { [return: MarshalAs(UnmanagedType.IUnknown)] get; [param: MarshalAs(UnmanagedType.IUnknown)] set; }
        }
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [System.Runtime.InteropServices.Guid("a257681d-5cdd-401c-89f0-cba89ca8a39e")]
        interface IApplicationWindowTitleBarNavigationClient
        {
            AppWindowTitleBarVisibility TitleBarPreferredVisibilityMode { get; set; }
        }
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.TerminateAppOnFinalViewClose = true;

            this.SizeChanged += MainPage_SizeChanged;
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += MainPage_VisibleBoundsChanged;
            //return;
            App.AppServiceRequestReceived += ServiceConnection_RequestReceived;
        }

        private void MainPage_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            //Debug.WriteLine(ApplicationView.GetForCurrentView().IsFullScreenMode);
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ClickButton2.Visibility = Visibility.Collapsed;
                FullScreenButton.Visibility = Visibility.Visible;
            }
            else
            {
                ClickButton2.Visibility = Visibility.Visible;
                FullScreenButton.Visibility = Visibility.Collapsed;
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            //Debug.WriteLine(bounds.Top);
            //Debug.WriteLine(bounds.Bottom);
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            //Debug.WriteLine(bounds.Width * scaleFactor);
            //Debug.WriteLine(bounds.Bottom * scaleFactor);
            //Debug.WriteLine(48 * scaleFactor);
            //Debug.WriteLine(DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels);
            var isTouchingTaskbar = Math.Round((DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels - (bounds.Bottom * scaleFactor)) / scaleFactor, 0, MidpointRounding.ToEven) <= 48;
            //Debug.WriteLine($"Taskbar height is {Math.Round((DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels - (bounds.Bottom * scaleFactor)) / scaleFactor, 0, MidpointRounding.ToEven)}");
            if (bounds.Top == 0 && isTouchingTaskbar && bounds.Width * scaleFactor == DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels)
            {

                ClickButton2.Content = "\uE923";
            }
            else
            {

                ClickButton2.Content = "\uE922";
            }
            //Debug.WriteLine(size);
            //Debug.WriteLine(40 * scaleFactor);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LaunchFullTrust();
        }

        private void LaunchCustomTitleBar()
        {

            var x = Window.Current.CoreWindow;
            IInternalCoreWindowPhone internalCoreWindowPhone = (IInternalCoreWindowPhone)(object)x;
            IApplicationWindowTitleBarNavigationClient navigationClient = (IApplicationWindowTitleBarNavigationClient)internalCoreWindowPhone.NavigationClient;
            navigationClient.TitleBarPreferredVisibilityMode = AppWindowTitleBarVisibility.AlwaysHidden;
            TitleBarHost.Visibility = Visibility.Visible;
            Window.Current.SetTitleBar(TitleBar);
            Toggle.Toggled += ToggleSwitch_Toggled;
            CheckWindowSize();
        }

        private void CheckWindowSize()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            //Debug.WriteLine(bounds.Top);
            //Debug.WriteLine(bounds.Bottom);
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            //Debug.WriteLine(bounds.Width * scaleFactor);
            //Debug.WriteLine(bounds.Bottom * scaleFactor);
            //Debug.WriteLine(48 * scaleFactor);
            //Debug.WriteLine(DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels);
            var isTouchingTaskbar = Math.Round((DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels - (bounds.Bottom * scaleFactor)) / scaleFactor, 0, MidpointRounding.ToEven) <= 48;
            //Debug.WriteLine($"Distance from bottom of screen is {Math.Round((DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels - (bounds.Bottom * scaleFactor)) / scaleFactor, 0, MidpointRounding.ToEven)} pixels");
            if (bounds.Top == 0 && isTouchingTaskbar && bounds.Width * scaleFactor == DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels)
            {

                ClickButton2.Content = "\uE923";
            }
            else
            {

                ClickButton2.Content = "\uE922";
            }
        }

        private void ServiceConnection_RequestReceived(object sender, AppServiceRequestReceivedEventArgs e)
        {
            //Debug.WriteLine(e.Request.Message.First());
            App.AppServiceRequestReceived -= ServiceConnection_RequestReceived;
        }

        private async void LaunchFullTrust()
        {
            App.AppServiceConnected += MainPage_AppServiceConnected;
            App.AppServiceDisconnected += MainPage_AppServiceDisconnected;
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
        }
        private void MainPage_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            LaunchCustomTitleBar();
            App.Connection.RequestReceived += AppServiceConnection_RequestReceived;
        }

        private void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //Debug.WriteLine(args.Request.Message.First());
        }

        /// <summary>
        /// When the desktop process is disconnected, reconnect if needed
        /// </summary>
        private void MainPage_AppServiceDisconnected(object sender, EventArgs e)
        {
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    // disable UI to access the connection
            //    btnRegKey.IsEnabled = false;

            //    // ask user if they want to reconnect
            //    Reconnect();
            //});
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            //if (App._appServiceConnection != null)
            //{
                ValueSet valueSet = new ValueSet();
                valueSet.Add("request", "toggleState");

                AppServiceResponse response = await App.Connection.SendMessageAsync(valueSet);
                if (response.Message.Any())
                {
                    //Debug.WriteLine(response.Message.First());
                    switch (response.Message.First().Value.ToString())
                    {
                        case "maximize":

                            ClickButton2.Content = "\uE923";
                            break;
                        case "restore":

                            ClickButton2.Content = "\uE922";
                            break;
                    }
                }
            //}
        }
        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            //if (App._appServiceConnection != null)
            //{
                ValueSet valueSet = new ValueSet();
                valueSet.Add("request", "minimize");

                AppServiceResponse response = await App.Connection.SendMessageAsync(valueSet);
                if (response.Message.Any())
                {
                    //Debug.WriteLine(response.Message.First());
                }
            //}
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            try
            {
                Application.Current.Exit();
            }
            catch
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MySplit.IsPaneOpen = !MySplit.IsPaneOpen;
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
        }


        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                var x = Window.Current.CoreWindow;
                IInternalCoreWindowPhone internalCoreWindowPhone = (IInternalCoreWindowPhone)(object)x;
                IApplicationWindowTitleBarNavigationClient navigationClient = (IApplicationWindowTitleBarNavigationClient)internalCoreWindowPhone.NavigationClient;
                navigationClient.TitleBarPreferredVisibilityMode = AppWindowTitleBarVisibility.AlwaysHidden;
                TitleBarHost.Visibility = Visibility.Visible;
                Window.Current.SetTitleBar(TitleBar);
            }
            else
            {
                var x = Window.Current.CoreWindow;
                IInternalCoreWindowPhone internalCoreWindowPhone = (IInternalCoreWindowPhone)(object)x;
                IApplicationWindowTitleBarNavigationClient navigationClient = (IApplicationWindowTitleBarNavigationClient)internalCoreWindowPhone.NavigationClient;
                navigationClient.TitleBarPreferredVisibilityMode = AppWindowTitleBarVisibility.Default;
                TitleBarHost.Visibility = Visibility.Collapsed;
                Window.Current.SetTitleBar(null);
            }
        }

        private void ClickButton2_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DispatcherTimer dispatcher = new DispatcherTimer();
            dispatcher.Interval = TimeSpan.FromMilliseconds(1500);
            pointerexited = (x, s) =>
            {
                dispatcher.Stop();
                ((Button)sender).PointerExited -= pointerexited;
            };

            ((Button)sender).PointerExited += pointerexited;
            dispatcher.Tick += Dispatcher_Tick;
            dispatcher.Start();
        }

        private async void Dispatcher_Tick(object sender, object e)
        {
            ((DispatcherTimer)sender).Stop();
            ValueSet valueSet = new ValueSet();
            valueSet.Add("request", "showSnaps");
            //((Button)sender).PointerExited -= pointerexited;

            AppServiceResponse response = await App.Connection.SendMessageAsync(valueSet);
            if (response.Message.Any())
            {
                //Debug.WriteLine(response.Message.First());
            }
        }

        public event PointerEventHandler pointerexited;
        private void MainPage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
