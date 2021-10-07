using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows;

namespace Morse
{
    public class UserActivityHook
    {
        #region Windows structure definitions

        [StructLayout(LayoutKind.Sequential)]
        private class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class MouseLLHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        #endregion

        #region Windows function imports

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowsHookEx(
            int idHook,
            HookProc lpfn,
            IntPtr hMod,
            int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(
            int idHook,
            int nCode,
            int wParam,
            IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        #endregion

        #region Windows constants

        private const int WH_MOUSE_LL = 14;
        private const int WH_MOUSE = 7;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        private const int WM_MOUSEWHEEL = 0x020A;

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        #endregion

        public UserActivityHook()
        {
            Start();
        }

        ~UserActivityHook()
        {
            //uninstall hooks and do not throw exceptions
            Stop(false);
        }

        public event MouseEventHandler OnMouseActivity;
        private int hMouseHook = 0;
        private int hKeyboardHook = 0;
        private static HookProc MouseHookProcedure;

        public void Start()
        {
            // install Mouse hook only if it is not installed and must be installed
            if (hMouseHook == 0)
            {
                // Create an instance of HookProc.
                MouseHookProcedure = new HookProc(MouseHookProc);
                //install hook
                hMouseHook = SetWindowsHookEx(
                    WH_MOUSE_LL,
                    MouseHookProcedure,
                    Marshal.GetHINSTANCE(
                        Assembly.GetExecutingAssembly().GetModules()[0]),
                    0);
                //If SetWindowsHookEx fails.
                if (hMouseHook == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup
                    Stop(false);
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        public void Stop()
        {
            this.Stop(false);
        }

        public void Stop(bool ThrowExceptions)
        {
            //if mouse hook set and must be uninstalled
            if (hMouseHook != 0)
            {
                //uninstall hook
                int retMouse = UnhookWindowsHookEx(hMouseHook);
                //reset invalid handle
                hMouseHook = 0;
                //if failed and exception must be thrown
                if (retMouse == 0 && ThrowExceptions)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            // if ok and someone listens to our events
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                //Marshall the data from callback.
                MouseLLHookStruct mouseHookStruct =
                    (MouseLLHookStruct) Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));

                //detect button clicked
                MouseButtons button = MouseButtons.None;
                short mouseDelta = 0;
                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        break;
                    case WM_LBUTTONUP:
                        button = MouseButtons.Left;
                        break;
                    case WM_RBUTTONUP:
                        button = MouseButtons.Right;
                        break;
                    case WM_MOUSEWHEEL:
                        mouseDelta = (short) ((mouseHookStruct.mouseData >> 16) & 0xffff);
                        break;
                }

                //double clicks
                int clickCount = 0;
                if (button != MouseButtons.None)
                    if (wParam == WM_LBUTTONUP || wParam == WM_RBUTTONUP) clickCount = 2;
                    else clickCount = 1;

                //generate event 
                MouseEventArgs e = new MouseEventArgs(
                    button,
                    clickCount,
                    mouseHookStruct.pt.x,
                    mouseHookStruct.pt.y,
                    mouseDelta);
                //raise it
                OnMouseActivity(this, e);
            }

            //call next hook
            return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }

        public static void DoMouseClick(System.Drawing.Point p)
        {
            Cursor.Position = p;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}