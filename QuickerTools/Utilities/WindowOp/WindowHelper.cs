using QuickerTools.Domain;
using QuickerTools.Domain.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace QuickerTools.Utilities.WindowOp
{
    public class WindowHelper
    {
        public static bool ShowWindow(Window window, bool active = false)
        {
            if (window == null) return false;
            else
            {
                try
                {
                    if (!window.IsVisible) window.Show();
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;
                    if (active) window.Activate();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static void ShowWindowAndWaitClose(Window window, bool activate = false)
        {
            var frame = new DispatcherFrame();

            window.Closed += (sender, args) =>
            {
                frame.Continue = false;
            };

            window.Show();
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
            if (activate) window.Activate();
            // This will "block" execution of the current dispatcher frame
            // and run our frame until the dialog is closed.
            Dispatcher.PushFrame(frame);

        }

        public static IntPtr GetHandle(Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }
        public static WType GetWindow<WType>(IntPtr handle) where WType : class
        {
            HwndSource hwndSource = HwndSource.FromHwnd(handle);
            if (hwndSource == null) return null;
            WType winGet = hwndSource.RootVisual as WType;
            return winGet;
        }
        public static WType GetWindowByHandle<WType>(int intHandle) where WType : class => GetWindow<WType>(new IntPtr(intHandle));
        public static Window GetWindow(IntPtr handle) => GetWindow<Window>(handle);
        public static Window GetWindowByHandle(int intHandle) => GetWindow(new IntPtr(intHandle));



        public static void SetWindowSize(Window win, WindowConfig config)
        {
            win.Left = config._left;
            win.Top = config._top;
            win.Width = config._width;
            win.Height = config._height;
        }



    }

}
