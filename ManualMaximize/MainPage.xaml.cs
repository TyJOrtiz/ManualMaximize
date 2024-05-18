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
            LaunchFullTrust();
            LaunchCustomTitleBar();
            App.AppServiceRequestReceived += ServiceConnection_RequestReceived;
        }

        private void LaunchCustomTitleBar()
        {
            var x = Window.Current.CoreWindow;
            IInternalCoreWindowPhone internalCoreWindowPhone = (IInternalCoreWindowPhone)(object)x;
            IApplicationWindowTitleBarNavigationClient navigationClient = (IApplicationWindowTitleBarNavigationClient)internalCoreWindowPhone.NavigationClient;
            navigationClient.TitleBarPreferredVisibilityMode = AppWindowTitleBarVisibility.AlwaysHidden;
            Window.Current.SetTitleBar(TitleBar);
            ApplicationView.TerminateAppOnFinalViewClose = true;
        }

        private void ServiceConnection_RequestReceived(object sender, AppServiceRequestReceivedEventArgs e)
        {
            Debug.WriteLine(e.Request.Message.First());
            App.AppServiceRequestReceived -= ServiceConnection_RequestReceived;
        }

        private async void LaunchFullTrust()
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            if (App._appServiceConnection != null)
            {
                ValueSet valueSet = new ValueSet();
                valueSet.Add("request", "toggleState");

                AppServiceResponse response = await App._appServiceConnection.SendMessageAsync(valueSet);
                if (response.Message.Any())
                {
                    Debug.WriteLine(response.Message.First());
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
            }
        }
        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (App._appServiceConnection != null)
            {
                ValueSet valueSet = new ValueSet();
                valueSet.Add("request", "minimize");

                AppServiceResponse response = await App._appServiceConnection.SendMessageAsync(valueSet);
                if (response.Message.Any())
                {
                    Debug.WriteLine(response.Message.First());
                }
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await ApplicationView.GetForCurrentView().TryConsolidateAsync();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MySplit.IsPaneOpen = !MySplit.IsPaneOpen;
        }
    }
}
