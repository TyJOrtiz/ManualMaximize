using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ManualMaximize
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static ViewModel AppViewModel { get; private set; }
        public static IntPtr MainHandle { get; set; } = new IntPtr(-1);

        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        public static event EventHandler AppServiceDisconnected;
        public static event EventHandler<AppServiceTriggerDetails> AppServiceConnected;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            AppViewModel = new ViewModel();
            this.Suspending += OnSuspending;
        }
        [ComImport, System.Runtime.InteropServices.Guid("45D64A29-A63E-4CB6-B498-5781D298CB4F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ICoreWindowInterop
        {
            IntPtr WindowHandle { get; }
            bool MessageHandled { set; }
        }
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            if (args.TaskInstance.TriggerDetails.GetType() == typeof(AppServiceTriggerDetails))
            {
                var details = args.TaskInstance.TriggerDetails as AppServiceTriggerDetails;
                // only accept connections from callers in the same package
                if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
                {
                    // connection established from the fulltrust process
                    AppServiceDeferral = args.TaskInstance.GetDeferral();
                    args.TaskInstance.Canceled += OnTaskCanceled;

                    Connection = details.AppServiceConnection;
                    AppServiceConnected?.Invoke(this, args.TaskInstance.TriggerDetails as AppServiceTriggerDetails);
                }
            }
        }

        /// <summary>
        /// Task canceled here means the app service client is gone
        /// </summary>
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            AppServiceDeferral?.Complete();
            AppServiceDeferral = null;
            Connection = null;
            AppServiceDisconnected?.Invoke(this, null);
        }

        //private BackgroundTaskDeferral _appServiceDeferral;
        //public static AppServiceConnection _appServiceConnection;
        //public static Dictionary<UIContext, AppWindow> AppWindows = new Dictionary<UIContext, AppWindow>();
        //internal static bool FirstLaunch = true;

        //protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        //{
        //    base.OnBackgroundActivated(args);

        //    if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
        //    {
        //        // only accept connections from callers in the same package
        //        if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
        //        {
        //            // connection established from the fulltrust process
        //            AppServiceDeferral = args.TaskInstance.GetDeferral();
        //            args.TaskInstance.Canceled += OnTaskCanceled;

        //            Connection = details.AppServiceConnection;
        //            AppServiceConnected?.Invoke(this, args.TaskInstance.TriggerDetails as AppServiceTriggerDetails);
        //        }
        //    }
        //    //base.OnBackgroundActivated(args);
        //    //IBackgroundTaskInstance taskInstance = args.TaskInstance;
        //    //taskInstance.Canceled += OnTaskCanceled;

        //    //AppServiceTriggerDetails appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
        //    //_appServiceDeferral = taskInstance.GetDeferral();
        //    //_appServiceConnection = appService.AppServiceConnection;
        //    //_appServiceConnection.RequestReceived += ServiceConnection_RequestReceived;
        //    //_appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
        //}
        //private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        //{
        //    if (this._appServiceDeferral != null)
        //    {
        //        // Complete the service deferral.
        //        this._appServiceDeferral.Complete();
        //    }
        //}
        //private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        //{
        //    _appServiceDeferral.Complete();
        //}
        public static event EventHandler<AppServiceRequestReceivedEventArgs> AppServiceRequestReceived;
        //private void ServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        //{
        //    AppServiceDeferral messageDeferral = args.GetDeferral();
        //    AppServiceRequestReceived?.Invoke(sender, args);
        //    messageDeferral.Complete();
        //}

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
                dynamic corewin = Window.Current.CoreWindow;
                var interop = (ICoreWindowInterop)corewin;
                var handle = interop.WindowHandle;
                MainHandle = handle;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
