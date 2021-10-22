using ICSharpCode.AvalonEdit;
using Quicker.Utilities;
using Quicker.Utilities.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Z.Expressions;

namespace QuickerTools.Note
{
    public class InternalExtract
    {
        public static void ShowWindow()
        {
            var window = new Quicker.View.TextWindow();
            window.Show();
            new ClipboardManager(window).ClipboardChanged += ClipboardChangedWrapper(window);
            Eval.Execute("");
        }

        public static class WinOp
        {
            [DllImport("User32.dll")]
            public static extern IntPtr GetForegroundWindow();     //获取活动窗口句柄

            public static IntPtr GetHandle(Window window)
            {
                return new WindowInteropHelper(window).Handle;
            }
            public static WType GetWindow<WType>() where WType : class
            {
                IntPtr handle = GetForegroundWindow();
                HwndSource hwndSource = HwndSource.FromHwnd(handle);
                WType winGet = hwndSource.RootVisual as WType;
                return winGet;
            }
            public static Window GetWindowByHandle(int intHandle)
            {
                IntPtr handle = new IntPtr(intHandle);
                HwndSource hwndSource = HwndSource.FromHwnd(handle);
                Window winGet = hwndSource.RootVisual as Window;
                return winGet;
            }
            public static WType GetWindowByHandle<WType>(int intHandle) where WType : class
            {
                IntPtr handle = new IntPtr(intHandle);
                HwndSource hwndSource = HwndSource.FromHwnd(handle);
                WType winGet = hwndSource.RootVisual as WType;
                return winGet;
            }
        }
        static bool topmost = false;
        public static void Exec(Quicker.Public.IStepContext context)
        {
            object win;
            try
            {
                win = WinOp.GetWindow<Quicker.View.TextWindow>();
                if (win == null)
                    throw new Exception("请在Quicker的文本窗口使用此动作");
            }
            catch
            {
                win = new Quicker.View.TextWindow()
                {
                    Width = 500,
                    Height = 400
                };
                var window = win as Quicker.View.TextWindow;
                window.Activated += (s, e) =>
                {
                    window.Topmost = false;
                };
                window.Deactivated += (s, e) =>
                {
                    window.Topmost = true;
                };
                window.Closed += (s, e) =>
                {
                    Clipboard.SetText(window.ResultText);
                    AppHelper.ShowSuccess("文本已写入剪贴板");
                };
                window.Show();


            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = win as Window;
                new ClipboardManager((Window)win).ClipboardChanged += ClipboardChangedWrapper((Window)win);
                //window.Activated += (s, e) =>
                //{
                //    topmost = window.Topmost;
                //    window.Topmost = false;
                //};
                //window.Deactivated += (s, e) =>
                //{
                //    if (topmost == false)
                //    {
                //        window.Topmost = false;
                //    }
                //};
                AppHelper.ShowInformation("添加完成");
            });

        }
        private static EventHandler ClipboardChangedWrapper(Window win)
        {
            return (object sender, EventArgs args) =>
            {
                //AppHelper.ShowInformation("剪贴板改变");
                if (win.IsActive)
                {
                    return;
                }
                try
                {
                    if (!Clipboard.ContainsText())
                    {
                        return;
                    }
                    string text = "";
                    Thread.Sleep(10);
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            text = Clipboard.GetText();
                        }
                        catch
                        {
                            Thread.Sleep(20);
                            if (i > 2)
                            {
                                throw new Exception("获取剪贴板文本失败");
                            }
                        }
                    }
                    //if (text == CurrentText || text == "") return;
                    //CurrentText = text;
                    var textEditor = GetPrivateFieid<TextEditor>("TheText", win);
                    text = "\r\n" + text + "\r\n";
                    textEditor.Document.Insert(textEditor.CaretOffset, text);
                    textEditor.ScrollToLine(textEditor.Document.LineCount);
                }
                catch (Exception ee)
                {
                    throw new Exception(ee.ToString());
                }

            };
        }
        public static object CallNonPublicMethod(object instance, string methodName, object[] param)
        {
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            object result;
            try
            {
                result = method.Invoke(instance, param);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            return result;
        }

        public static T GetPrivateFieid<T>(string name, object instance) where T : class
        {
            if (instance == null)
                return null;
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
                return field.GetValue(instance) as T;
            return null;
        }
    }
}
