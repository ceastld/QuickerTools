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

namespace QuickerTools.Editor
{
    /// <summary>
    /// VariableList.xaml 的交互逻辑
    /// </summary>
    public partial class VariableList : UserControl
    {
        public VariableList()
        {
            InitializeComponent();
        }

        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {
            Variable variable = VariavleListBox.SelectedItem as Variable;
            if (variable == null)
            {
                variable = new Variable();
            }
            var win = new Controls.VariableWindow(variable)
            {
                varNameList = AppState.Variables.Select(x => x.Key).ToList()
            };
            win.ShowDialog();
            AppState.Variables.Add(win.Variable);
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (VariavleListBox.SelectedItem != null)
            {
                var varName = (VariavleListBox.SelectedItem as Variable).Key;
                for (int i = 0; i < AppState.Variables.Count; i++)
                {
                    if (varName == AppState.Variables[i].Key)
                    {
                        AppState.Variables.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void VariavleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (VariavleListBox.SelectedItem != null)
            {
                var variable = VariavleListBox.SelectedItem as Variable;

                var _varnamelist = AppState.Variables.Select(x => x.Key).ToList();
                _varnamelist.Remove(variable.Key);
                //MainWindow.ShowMessage(_varnamelist);
                var win = new Controls.VariableWindow(variable)
                {
                    varNameList = _varnamelist
                };
                win.ShowDialog();
            }
        }

        private void VariavleList_LostFocus(object sender, RoutedEventArgs e)
        {
            VariavleListBox.SelectedItem = null;
        }
    }
}
