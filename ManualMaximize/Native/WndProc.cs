using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ManualMaximize.Native
{
    public static class WndProc
    {
        public delegate IntPtr WndProcDelegate(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
        private const int GWLP_WNDPROC = -4;

        private static readonly Lazy<IntPtr> _coreWindowHwnd = new Lazy<IntPtr>(GetCoreWindowHwnd);

        // Make sure to hold a reference to the delegate so it doesn't get garbage
        // collected, or you'll get baffling ExecutionEngineExceptions when
        // Windows tries to call your function pointer which no longer points
        // to anything.
        private static WndProcDelegate _currDelegate = null;

        public static IntPtr SetWndProc(WndProcDelegate newProc)
        {
            _currDelegate = newProc;

            IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newProc);

            if (IntPtr.Size == 8)
            {
                return Interop.SetWindowLongPtr64(_coreWindowHwnd.Value, GWLP_WNDPROC, newWndProcPtr);
            }
            else
            {
                return Interop.SetWindowLong32(_coreWindowHwnd.Value, GWLP_WNDPROC, newWndProcPtr);
            }
        }

        private static IntPtr GetCoreWindowHwnd()
        {
            dynamic coreWindow = Windows.UI.Core.CoreWindow.GetForCurrentThread();
            var interop = (ICoreWindowInterop)coreWindow;
            return interop.WindowHandle;
        }
    }
}
