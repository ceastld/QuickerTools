using QuickerTools.Utilities.Ext;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickerTools.Editor
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : UserControl
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void SearchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                e.Handled = true;
                if (Check_DotNet.IsChecked == true)
                {
                    Process.Start($"https://docs.microsoft.com/zh-cn/search/?terms={SearchBar.Text.UrlEncode()}&scope=.NET");
                }

                if (Check_Google.IsChecked == true)
                {
                    Process.Start($"https://www.google.com/search?q={SearchBar.Text.UrlEncode()}");
                }
            }
        }
    }
}
