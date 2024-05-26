using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
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
using Control = System.Windows.Controls.Control;

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
                try
                {
                    Debug.WriteLine(process.FirstOrDefault().MainWindowHandle);
                }
                catch
                {

                }
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
        //[DllImport("user32.dll")]
        //private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private const uint SC_MOVE = 0xF010;
        private const uint WM_SYSCOMMAND = 0x0112;
        [DllImport("user32.dll")]
        static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y,
           int nReserved, IntPtr hWnd, IntPtr prcRect);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //Add the needed const
        const int GWL_EXSTYLE = (-20);
        const UInt32 WS_EX_TOPMOST = 0x0008;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
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
  private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        public static void OpenSystemMenu()
        {
            IntPtr hWnd = GetForegroundWindow();
            SendMessage(hWnd, WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)32);
        }
        //public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_KEYMENU = 0xF100;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint WS_CAPTION = 0x00C0;
        [DllImport("user32.dll")]
        private static extern uint TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);


        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        public static void ShowContextMenu(IntPtr appWindow, IntPtr myWindow, Point point)
        {
            const int MF_BYCOMMAND = 0x00000000;
            const uint TPM_LEFTBUTTON = 0x0000;
            const uint TPM_RETURNCMD = 0x0100;
            const uint SC_MOVE = 0xF010;
            const uint SC_SIZE = 0xF000;
            IntPtr wMenu = GetSystemMenu(appWindow, false);
            WINDOWPLACEMENT placement2 = new WINDOWPLACEMENT();
            placement2.length = Marshal.SizeOf(placement2);

            if (GetWindowPlacement(appWindow, ref placement2))
            {
                //if (maximized)
                //{
                //    EnableMenuItem(wMenu, SC_MAXIMIZE, MF_BYCOMMAND | 0x00000001); // Disable Maximize
                //    EnableMenuItem(wMenu, SC_RESTORE, MF_BYCOMMAND | 0x00000000);  // Enable Restore
                //}
                //else
                //{
                //    EnableMenuItem(wMenu, SC_MAXIMIZE, MF_BYCOMMAND | 0x00000000); // Enable Maximize
                //    EnableMenuItem(wMenu, SC_RESTORE, MF_BYCOMMAND | 0x00000001);  // Disable Restore
                //}
                // Check if the window is maximized
                if (placement2.showCmd == SW_MAXIMIZE)
                {
                    EnableMenuItem(wMenu, SC_MAXIMIZE, MF_BYCOMMAND | 0x00000001);
                    EnableMenuItem(wMenu, SC_SIZE, MF_BYCOMMAND | 0x00000001);
                    EnableMenuItem(wMenu, SC_MOVE, MF_BYCOMMAND | 0x00000001); // Disable Maximize
                    EnableMenuItem(wMenu, SC_RESTORE, MF_BYCOMMAND | 0x00000000);  // Enable Restore
                }
                else
                {
                    EnableMenuItem(wMenu, SC_MAXIMIZE, MF_BYCOMMAND | 0x00000000);
                    EnableMenuItem(wMenu, SC_SIZE, MF_BYCOMMAND | 0x00000000);
                    EnableMenuItem(wMenu, SC_MOVE, MF_BYCOMMAND | 0x00000000); // Enable Maximize
                    EnableMenuItem(wMenu, SC_RESTORE, MF_BYCOMMAND | 0x00000001);

                }
            }
            else
            {
                Console.WriteLine("Failed to get window placement.");
            }
            // Display the menu
            SetForegroundWindow(myWindow);
            uint command = TrackPopupMenuEx(wMenu,
                TPM_LEFTBUTTON | TPM_RETURNCMD | 0x0080, (int)point.X, (int)point.Y, myWindow, IntPtr.Zero);
            if (command == 0)
                return;
            PostMessage(appWindow, WM_SYSCOMMAND, new IntPtr(command), IntPtr.Zero);
        }
        private async void ServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var def = args.GetDeferral();
            //Debug.WriteLine("request is " + args.Request.Message["request"]);
            //Debug.WriteLine("hi");
            string windowState = "";
            var activeWindowHandle = GetForegroundWindow();

            // Get the cursor position
            //GetCursorPos(out POINT cursorPos);

            //// Show the system menu at the cursor position
            //TrackPopupMenuEx(hMenu2, TPM_LEFTALIGN | TPM_RETURNCMD, cursorPos.x, cursorPos.y, activeWindowHandle, IntPtr.Zero);
            //SetWindowLong(activeWindowHandle, GWL_STYLE, style);
            //SetWindowPos(activeWindowHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            //OpenSystemMenu();

            //    new IntPtr((int)args.Request.Message.First().Value);
            //Debug.WriteLine(activeWindowHandle);
            //Debug.WriteLine(GetForegroundWindow());
            if ((string)args.Request.Message["request"] == "toggleState")
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();
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
            else if ((string)args.Request.Message["request"] == "showSystemMenu")
            {
                //var v = GetCursorPos(out POINT cursorPos);
                //this.Dispatcher.Invoke(() =>
                //{
                //    ShowContextMenu(activeWindowHandle, new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle, new Point(cursorPos.x, cursorPos.y));
                //});
                OpenSystemMenu();
                windowState = "shown";
            }
            else if ((string)args.Request.Message["request"] == "getWindowHandle")
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();

                var x = new ValueSet();
                x.Add("response", activeWindowHandle.ToInt32());
                try
                {
                    await args.Request.SendResponseAsync(x);
                }
                catch
                {

                }
                finally
                {
                    def.Complete();
                }
                return;
            }
            else if ((string)args.Request.Message["request"] == "requestPin")
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();


                if (activeWindowHandle != IntPtr.Zero)
                {
                    int dwExStyle = GetWindowLong(activeWindowHandle, GWL_EXSTYLE);

                    string isTopMost = "No";

                    if ((dwExStyle & WS_EX_TOPMOST) != 0)
                    {
                        SetWindowPos(activeWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                        windowState = "unpinned";
                    }
                    // Minimize the active window
                    else
                    {
                        SetWindowPos(activeWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                        windowState = "pinned";
                    }
                    Console.WriteLine("The active window has been minimized.");
                    //windowState = "pinned";
                }
                else
                {
                    Console.WriteLine("No active window found.");
                }
            }
            else if (args.Request.Message["request"] is bool value1)
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();


                if (activeWindowHandle != IntPtr.Zero)
                {
                    const long WS_MINIMIZEBOX = 0x00020000L;
                    const long WS_MAXIMIZEBOX = 0x00010000L;

                    long value = GetWindowLong(activeWindowHandle, GWL_STYLE);
                    if (value1 == true)
                    {
                        SetWindowLong(activeWindowHandle, GWL_STYLE, (int)(value & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX));
                        windowState = "disabled";
                    }
                    else
                    {
                        SetWindowLong(activeWindowHandle, GWL_STYLE, (int)(value & WS_MINIMIZEBOX & WS_MAXIMIZEBOX));
                        windowState = "enabled";
                    }
                }
            }
            else if ((string)args.Request.Message["request"] == "minimize")
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();

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
            //else if ((string)args.Request.Message["request"] == "showSnaps")
            //{
            //    //IntPtr activeWindowHandle = GetForegroundWindow();

            //    if (activeWindowHandle != IntPtr.Zero)
            //    {
            //        RECT pos;
            //        GetWindowRect(activeWindowHandle, out pos);
            //        IntPtr hMenu = GetSystemMenu(activeWindowHandle, false);

            //        int cmd = TrackPopupMenuEx(hMenu, 0x100 | 0x002, pos.left, pos.top, activeWindowHandle, IntPtr.Zero);
            //        if (cmd > 0) SendMessage(activeWindowHandle, 0x112, (IntPtr)cmd, IntPtr.Zero);
            //        // Minimize the active window
            //        //RECT pos;
            //        //GetWindowRect(activeWindowHandle, out pos);
            //        //IntPtr hMenu = GetSystemMenu(activeWindowHandle, false);
            //        //bool cmd = TrackPopupMenu(hMenu, 0x100, pos.left, pos.top, 0, activeWindowHandle, IntPtr.Zero);
            //        ////if (cmd)
            //        ////    SendMessage(activeWindowHandle, WM_SYSCOMMAND, (IntPtr)cmd, IntPtr.Zero);
            //        //Console.WriteLine("The active window has been minimized.");
            //        windowState = "minimize";
            //    }
            //    else
            //    {
            //        Console.WriteLine("No active window found.");
            //    }
            //}
            else if ((string)args.Request.Message["request"] == "checkState")
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();

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
