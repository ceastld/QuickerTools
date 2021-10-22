using HandyControl.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickerTools.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace QuickerTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppState.Init();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Instance = this;
            //double x = SystemParameters.WorkArea.Width;//得到屏幕工作区域宽度
            //double y = SystemParameters.WorkArea.Height;//得到屏幕工作区域高度
            //double x1 = SystemParameters.PrimaryScreenWidth;//得到屏幕整体宽度
            //double y1 = SystemParameters.PrimaryScreenHeight;//得到屏幕整体高度
            //this.Width = x1;//设置窗体宽度
            //this.Height = y1;//设置窗体高度
            //Mouse.GetPosition(Mouse.DirectlyOver);

        }
        public static MainWindow Instance = null;
        public static void ShowMessage(string mes)
        {
            if (Instance != null)
                Instance.MessageBox.Text = mes;
        }
        public static void ShowMessage(object mes)
        {
            if (Instance != null)
                Instance.MessageBox.Text = JToken.FromObject(mes).ToString();
        }
        public string test()
        {
            string my = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return System.IO.Path.Combine(my, "Quicker", "QuickerTools_Ceastld");

        }
        private void ExpressionEditorBtn_Click(object sender, RoutedEventArgs e)
        {
            Editor.ExpressionEditor.ShowItSelf();
            //new Editor.ExpressionEditor().Show();
            //var handle = Utilities.WindowOp.WindowHelper.GetHandle(Editor.ExpressionEditor.Instance);
            //ShowMessage(handle);
            //AppState.EditorWindowHandle = handle.ToInt32();
            //this.Close();
        }

        private void TextExtractorBtn_Click(object sender, RoutedEventArgs e)
        {
            new Note.TextExtractor().Show();
        }

        private void TextExtractorBtn2_Click(object sender, RoutedEventArgs e)
        {
            Note.InternalExtract.ShowWindow();
        }
    }
}
