using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowHelper
{
    public class WindowInterop
    {

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Constants for ShowWindow function
        public const int SW_MAXIMIZE = 3;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        // Constants for ShowWindow function
        public const int SW_RESTORE = 9;


        public const int SW_MINIMIZE = 6;


        // WINDOWPLACEMENT structure
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        public struct MARGINS
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public const int WM_NCHITTEST = 0x84;
        public const uint WM_SYSCOMMAND = 0x0112;
        public const int GWL_EXSTYLE = (-20);
        public const UInt32 WS_EX_TOPMOST = 0x0008;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        public const UInt32 SC_MAXIMIZE = 0xF030;
        public const UInt32 SC_RESTORE = 0xF120;
        public const int SC_KEYMENU = 0xF100;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint WS_CAPTION = 0x00C0;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //Add the needed const
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        public const int GWL_STYLE = -16;
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out WindowInterop.RECT rect);
        public static void OpenSystemMenu()
        {
            IntPtr hWnd = WindowInterop.GetForegroundWindow();
            SendMessage(hWnd, WindowInterop.WM_SYSCOMMAND, (IntPtr)WindowInterop.SC_KEYMENU, (IntPtr)32);
        }
    }
}
