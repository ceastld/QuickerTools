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
using QuickerTools.Utilities.Ext;
using QuickerTools.Utilities;
namespace QuickerTools.Editor
{
    /// <summary>
    /// ExpressionListControl.xaml 的交互逻辑
    /// </summary>
    public partial class ExpressionListControl : UserControl
    {
        private ListCollectionView _collectionView;
        public static int BindCount;
        public ExpressionListControl()
        {
            InitializeComponent();
            CreateBinding();
        }
        private void CreateBinding()
        {
            this._collectionView = new ListCollectionView(AppState.CodeItems);
            this._collectionView.Filter = new Predicate<object>(this.Filter);
            this._collectionView.CustomSort = new SortByTime();
            this.LvCodeItems.ItemsSource = this._collectionView;
            ++ExpressionListControl.BindCount;
        }
        
        public void RefreshList()
        {
            if (this.LvCodeItems.ItemsSource == null)
                this.CreateBinding();
            if (this._collectionView == null)
                return;
            if (AppState.CodeItems == null)
                return;
            try
            {
                this._collectionView.Refresh();
            }
            catch { }
        }

        private void BtnClearFilter_OnClick(object sender, RoutedEventArgs e) => TextFilter.Text = "";

        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.RefreshList();
            if (LvCodeItems.SelectedItem == null && LvCodeItems.Items.Count > 0)
                LvCodeItems.SelectedIndex = 0;
        }

        private bool Filter(object obj)
        {
            if(string.IsNullOrEmpty(TextFilter.Text))
                return true;
            string text = TextFilter.Text;
            if (!(obj is CodeItem codeItem))
                return false;
            return text.ContainedInAny(codeItem.Title, codeItem.Code)
                || PinyinHelper.IsPinyinMatch(PinyinHelper.GetPinYinMatchString(codeItem.Title), text, false, false);

        }

        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = new CodeItem();
            AppState.ExpressionData.AddNewItem(item);
            TextFilter.Text = "";
            LvCodeItems.SelectedIndex = _collectionView.IndexOf(item);
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e) => DeleteSelectedItem();
        public void DeleteSelectedItem()
        {
            if (LvCodeItems.SelectedItem == null) return;
            AppState.ExpressionData.Remove((LvCodeItems.SelectedItem as CodeItem));
        }
        private void LvCodeItems_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                var result = MessageBox.Show("确认要删除这条表达式?", "", MessageBoxButton.OKCancel);
                if(result == MessageBoxResult.OK)
                {
                    DeleteSelectedItem();return;
                }
            }
        }

        private void TrashBtn_Click(object sender, RoutedEventArgs e) { }

        private void TextFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Down)
            {
                e.Handled = true;
                if (LvCodeItems.Items == null) return;
                LvCodeItems.SelectedIndex += 1;
            }
            if(e.Key == Key.Up)
            {
                e.Handled = true;
                if (LvCodeItems.Items == null) return;
                int index = LvCodeItems.SelectedIndex;
                if (index > 0)
                {
                    LvCodeItems.SelectedIndex -= 1;
                }
                
            }
        }

        private void Menu_SoryByTitle_Click(object sender, RoutedEventArgs e) => this._collectionView.CustomSort = new SortByTitle();

        private void Menu_SoryByTime_Click(object sender, RoutedEventArgs e) => this._collectionView.CustomSort = new SortByTime();
    }
}
