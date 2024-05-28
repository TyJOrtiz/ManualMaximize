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
        }
        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            // connection to the UWP lost, so we shut down the desktop process
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Application.Current.Shutdown();
            }));
        }

        
        private async void ServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var def = args.GetDeferral();
            string windowState = "";
            var activeWindowHandle = WindowInterop.GetForegroundWindow();
            if ((string)args.Request.Message["request"] == "toggleState")
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();
                //Debug.WriteLine(activeWindowHandle);
                if (activeWindowHandle != IntPtr.Zero)
                {
                    // Get the window placement
                    WindowInterop.WINDOWPLACEMENT placement = new WindowInterop.WINDOWPLACEMENT();
                    placement.length = Marshal.SizeOf(placement);

                    if (WindowInterop.GetWindowPlacement(activeWindowHandle, ref placement))
                    {
                        // Check if the window is maximized
                        if (placement.showCmd == WindowInterop.SW_MAXIMIZE)
                        {
                            // Restore the window if it is maximized
                            WindowInterop.ShowWindow(activeWindowHandle, WindowInterop.SW_RESTORE);
                            Console.WriteLine("The active window has been restored.");
                            windowState = "restore";
                        }
                        else
                        {
                            // Maximize the window if it is not maximized
                            WindowInterop.ShowWindow(activeWindowHandle, WindowInterop.SW_MAXIMIZE);
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
                WindowInterop.OpenSystemMenu();
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
                    int dwExStyle = WindowInterop.GetWindowLong(activeWindowHandle, WindowInterop.GWL_EXSTYLE);

                    //string isTopMost = "No";

                    if ((dwExStyle & WindowInterop.WS_EX_TOPMOST) != 0)
                    {
                        WindowInterop.SetWindowPos(activeWindowHandle, WindowInterop.HWND_NOTOPMOST, 0, 0, 0, 0, WindowInterop.TOPMOST_FLAGS);
                        windowState = "unpinned";
                    }
                    // Minimize the active window
                    else
                    {
                        WindowInterop.SetWindowPos(activeWindowHandle, WindowInterop.HWND_TOPMOST, 0, 0, 0, 0, WindowInterop.TOPMOST_FLAGS);
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

                    long value = WindowInterop.GetWindowLong(activeWindowHandle, WindowInterop.GWL_STYLE);
                    if (value1 == true)
                    {
                        WindowInterop.SetWindowLong(activeWindowHandle, WindowInterop.GWL_STYLE, (int)(value & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX));
                        windowState = "disabled";
                    }
                    else
                    {
                        WindowInterop.SetWindowLong(activeWindowHandle, WindowInterop.GWL_STYLE, (int)(value & WS_MINIMIZEBOX & WS_MAXIMIZEBOX));
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
                    WindowInterop.ShowWindow(activeWindowHandle, WindowInterop.SW_MINIMIZE);
                    //Console.WriteLine("The active window has been minimized.");
                    windowState = "showsnap";
                }
                else
                {
                    Console.WriteLine("No active window found.");
                }
            }
            else if ((string)args.Request.Message["request"] == "checkState")
            {
                //IntPtr activeWindowHandle = GetForegroundWindow();

                if (activeWindowHandle != IntPtr.Zero)
                {
                    // Get the window placement
                    WindowInterop.WINDOWPLACEMENT placement = new WindowInterop.WINDOWPLACEMENT();
                    placement.length = Marshal.SizeOf(placement);

                    if (WindowInterop.GetWindowPlacement(activeWindowHandle, ref placement))
                    {
                        // Check if the window is maximized
                        if (placement.showCmd == WindowInterop.SW_MAXIMIZE)
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
