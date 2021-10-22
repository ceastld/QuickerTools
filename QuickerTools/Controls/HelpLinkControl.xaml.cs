using QuickerTools.Domain;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickerTools.Controls
{
    /// <summary>
    /// HelpLinkControl.xaml 的交互逻辑
    /// </summary>
    public partial class HelpLinkControl : UserControl
    {
        public HelpLinkControl()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            //if (TheListBox.SelectedItem == null) return;
            System.Diagnostics.Process.Start((sender as Hyperlink).NavigateUri.ToString());
        }

        private void TheListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => RunSelectedItem();
        private void RunSelectedItem()
        {
            if (TheListBox.SelectedItem == null) return;
            var item = TheListBox.SelectedItem as HelpLinkItem;
            System.Diagnostics.Process.Start(item.HelpLink);
            TheListBox.SelectedItem = null;
        }
    }
}
