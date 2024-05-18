using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

namespace WindowHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppServiceConnection serviceConnection;
        public MainWindow()
        {
            InitializeComponent();
            InitializeService();
            WatchProcess();
        }

        private void WatchProcess()
        {
            var process = Process.GetProcesses().Where(x => x.ProcessName == "ManualMaximize");
            if (process != null && process.Any())
            {
                process.FirstOrDefault().EnableRaisingEvents = true;
                process.FirstOrDefault().Exited += Process_Exited;
                //process.SingleOrDefault()?. += Process_Exited;
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private async void InitializeService()
        {
            serviceConnection = new AppServiceConnection();
            serviceConnection.AppServiceName = "com.tortiz.windowhelper";

            // Use Windows.ApplicationModel.Package.Current.Id.FamilyName 
            // within the app service provider to get this value.
            this.serviceConnection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;

            var status = await this.serviceConnection.OpenAsync();
            Debug.WriteLine(Windows.ApplicationModel.Package.Current.Id.FamilyName);
            if (status != AppServiceConnectionStatus.Success)
            {
                return;
            }
            ValueSet valueSet = new ValueSet();
            valueSet.Add("request", "ok");

            AppServiceResponse response = await serviceConnection.SendMessageAsync(valueSet);
            serviceConnection.RequestReceived += ServiceConnection_RequestReceived;
        }
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Constants for ShowWindow function
        private const int SW_MAXIMIZE = 3;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        // Constants for ShowWindow function
        private const int SW_RESTORE = 9;


        private const int SW_MINIMIZE = 6;


        // WINDOWPLACEMENT structure
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private async void ServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var def = args.GetDeferral();
            Debug.WriteLine("request is " + args.Request.Message["request"]);
            Debug.WriteLine("hi");
            string windowState = "";
            if ((string)args.Request.Message["request"] == "toggleState")
            {
                IntPtr activeWindowHandle = GetForegroundWindow();

                if (activeWindowHandle != IntPtr.Zero)
                {
                    // Get the window placement
                    WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                    placement.length = Marshal.SizeOf(placement);

                    if (GetWindowPlacement(activeWindowHandle, ref placement))
                    {
                        // Check if the window is maximized
                        if (placement.showCmd == SW_MAXIMIZE)
                        {
                            // Restore the window if it is maximized
                            ShowWindow(activeWindowHandle, SW_RESTORE);
                            Console.WriteLine("The active window has been restored.");
                            windowState = "restore";
                        }
                        else
                        {
                            // Maximize the window if it is not maximized
                            ShowWindow(activeWindowHandle, SW_MAXIMIZE);
                            Console.WriteLine("The active window has been maximized.");
                            windowState = "maximize";

                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to get window placement.");
                    }
                }
                else
                {
                    Console.WriteLine("No active window found.");
                }
            }
            else if ((string)args.Request.Message["request"] == "minimize")
            {
                IntPtr activeWindowHandle = GetForegroundWindow();

                if (activeWindowHandle != IntPtr.Zero)
                {
                    // Minimize the active window
                    ShowWindow(activeWindowHandle, SW_MINIMIZE);
                    Console.WriteLine("The active window has been minimized.");
                    windowState = "minimize";
                }
                else
                {
                    Console.WriteLine("No active window found.");
                }
            }
            else
            if ((string)args.Request.Message["request"] == "checkState")
            {
                IntPtr activeWindowHandle = GetForegroundWindow();

                if (activeWindowHandle != IntPtr.Zero)
                {
                    // Get the window placement
                    WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                    placement.length = Marshal.SizeOf(placement);

                    if (GetWindowPlacement(activeWindowHandle, ref placement))
                    {
                        // Check if the window is maximized
                        if (placement.showCmd == SW_MAXIMIZE)
                        {
                            // Restore the window if it is maximized
                            //ShowWindow(activeWindowHandle, SW_RESTORE);
                            Console.WriteLine("The active window has been restored.");
                            windowState = "\uE923";
                        }
                        else
                        {
                            // Maximize the window if it is not maximized
                            //ShowWindow(activeWindowHandle, SW_MAXIMIZE);
                            Console.WriteLine("The active window has been maximized.");
                            windowState = "\uE922";

                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to get window placement.");
                    }
                }
                else
                {
                    Console.WriteLine("No active window found.");
                }
            }
            ValueSet valueSet = new ValueSet();
            valueSet.Add("repsonse", windowState);
            try
            {
                await args.Request.SendResponseAsync(valueSet);
            }
            catch
            {

            }
            finally
            {
                def.Complete();
            }

        }
    }
}
