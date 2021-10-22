using QuickerTools.Domain.Windows;
using QuickerTools.Utilities.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace QuickerTools.Note
{
    /// <summary>
    /// TextExtractor.xaml 的交互逻辑
    /// </summary>
    public partial class TextExtractor : Window
    {
        private static TextExtractor extractorInstance;
        private static WindowConfig windowConfig = null;
        public static string LastTimeText = "";
        public static void ShowItSelf()
        {
            if (extractorInstance == null)
            {
                extractorInstance = new TextExtractor();
                extractorInstance.Show();
            }
            else
            {
                try
                {
                    if (extractorInstance.WindowState == WindowState.Minimized)
                    {
                        extractorInstance.WindowState = WindowState.Normal;
                    }
                    extractorInstance.Activate();
                }
                catch { }
            }
        }
        public TextExtractor()
        {
            InitializeComponent();
            //DispatcherTimer clipboardMonitor = new DispatcherTimer();
            //clipboardMonitor.Interval = TimeSpan.FromSeconds(2);
            //clipboardMonitor.Tick += ClipboardChanged;
            //clipboardMonitor.Start();
            TextEditor.Text = LastTimeText;
            if (windowConfig == null)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                try
                {
                    windowConfig.SetToWindow(this);
                }
                catch { }
            }
        }

        private void TheExtractorWindow_SourceInitialized(object sender, EventArgs e)
        {
            new ClipboardManager((Window)this).ClipboardChanged += new EventHandler(this.ClipboardChanged);
            this.Topmost = true;
        }
        private string CurrentText = "";
        private void ClipboardChanged(object sender, EventArgs e)
        {
            if (CheckExtract.IsChecked == false) return;
            if (this.IsActive) return;
            try
            {
                if (!Clipboard.ContainsText())
                    return;
                string text = Clipboard.GetText();
                if (text == CurrentText || text == "") return;
                CurrentText = text;
                if (AutoNewLine.IsChecked == true)
                {
                    text = "\r\n" + text + "\r\n";
                }
                else
                {
                    text = "\r\n" + text;
                }
                TextAreaInsetAtPre(text);
                TextEditor.ScrollToLine(TextEditor.Document.LineCount);
            }
            catch
            {

            }
        }
        private void TextAreaInsetAtPre(string text)
        {
            TextEditor.Document.Insert(TextEditor.CaretOffset, text);
        }
        private void TextAreaInsetAtBehind(string text)
        {
            TextEditor.Document.Insert(TextEditor.CaretOffset, text);
            TextEditor.CaretOffset -= text.Length;
        }

        private void TheExtractorWindow_Closed(object sender, EventArgs e)
        {
            if (LastTimeText != TextEditor.Text)
            {
                LastTimeText = TextEditor.Text;
                if (LastTimeText != "")
                {
                    Clipboard.SetText(LastTimeText);
                }
            }
            extractorInstance = null;
            windowConfig = new WindowConfig(this);
        }

        private void CheckTopmost_Checked(object sender, RoutedEventArgs e) => this.Topmost = true;
        private void CheckTopmost_Unchecked(object sender, RoutedEventArgs e) => this.Topmost = false;
    }
}
