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
using System.Windows.Threading;
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

            serviceConnection.ServiceClosed += Connection_ServiceClosed;
            serviceConnection.RequestReceived += ServiceConnection_RequestReceived;
            var status = await this.serviceConnection.OpenAsync();
            Debug.WriteLine(Windows.ApplicationModel.Package.Current.Id.FamilyName);
            if (status != AppServiceConnectionStatus.Success)
            {
                return;
            }
            //ValueSet valueSet = new ValueSet();
            //valueSet.Add("request", "ok");

            //AppServiceResponse response = await serviceConnection.SendMessageAsync(valueSet);
        }
        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            // connection to the UWP lost, so we shut down the desktop process
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Application.Current.Shutdown();
            }));
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
        private const int HTMAXBUTTON = 9;
        private IntPtr HwndSourceHook(IntPtr hwnd)
        {
            return new IntPtr(HTMAXBUTTON);
        }
        public const int WM_NCHITTEST = 0x84; 
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private const uint SC_MOVE = 0xF010;
        private const uint WM_SYSCOMMAND = 0x0112;
        [DllImport("user32.dll")]
        static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y,
           int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        uint TPM_LEFTALIGN = 0x0000;
        uint TPM_RETURNCMD = 0x0100;
        const UInt32 MF_ENABLED = 0x00000000;
        const UInt32 MF_GRAYED = 0x00000001;
        internal const UInt32 SC_MAXIMIZE = 0xF030;
        internal const UInt32 SC_RESTORE = 0xF120;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem,
       uint uEnable);

        [DllImport("user32.dll")]
        static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags,
          int x, int y, IntPtr hwnd, IntPtr lptpm);
        private async void ServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var def = args.GetDeferral();
            //Debug.WriteLine("request is " + args.Request.Message["request"]);
            //Debug.WriteLine("hi");
            string windowState = "";
            if ((string)args.Request.Message["request"] == "toggleState")
            {
                IntPtr activeWindowHandle = GetForegroundWindow();
                //Debug.WriteLine(activeWindowHandle);
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
                    //Console.WriteLine("The active window has been minimized.");
                    windowState = "showsnap";
                }
                else
                {
                    Console.WriteLine("No active window found.");
                }
            }
            else if ((string)args.Request.Message["request"] == "showSnaps")
            {
                IntPtr activeWindowHandle = GetForegroundWindow();

                if (activeWindowHandle != IntPtr.Zero)
                {
                    RECT pos;
                    GetWindowRect(activeWindowHandle, out pos);
                    IntPtr hMenu = GetSystemMenu(activeWindowHandle, false);

                    int cmd = TrackPopupMenuEx(hMenu, 0x100 | 0x002, pos.left, pos.top, activeWindowHandle, IntPtr.Zero);
                    if (cmd > 0) SendMessage(activeWindowHandle, 0x112, (IntPtr)cmd, IntPtr.Zero);
                    // Minimize the active window
                    //RECT pos;
                    //GetWindowRect(activeWindowHandle, out pos);
                    //IntPtr hMenu = GetSystemMenu(activeWindowHandle, false);
                    //bool cmd = TrackPopupMenu(hMenu, 0x100, pos.left, pos.top, 0, activeWindowHandle, IntPtr.Zero);
                    ////if (cmd)
                    ////    SendMessage(activeWindowHandle, WM_SYSCOMMAND, (IntPtr)cmd, IntPtr.Zero);
                    //Console.WriteLine("The active window has been minimized.");
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

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    }
}
