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
using System.Windows.Shapes;

namespace QuickerTools.Controls
{
    /// <summary>
    /// VariableWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VariableWindow : Window
    {
        public List<string> varNameList = new List<string>();
        public static List<string> TypeList = new List<string>()
        {
            "string","List<string>"
        };
        public Variable Variable { get; set; }
        public VariableWindow(Variable variable)
        {
            InitializeComponent();
            Variable = variable;
            InitVariable();
        }
        public void InitVariable()
        {
            if (Variable == null)
            {
                Variable = new Variable();
            }
            else
            {
                KeyBox.Text = Variable.Key;
                ValueBox.Text = Variable.DefaultValue;
            }
            Variable.Type = typeof(string);
        }
        private void KeyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KeyBox.Text))
                messageBox.Text = "变量名不能为空";
            else if (!AppState.QuickerConnector.TryExecute($"string {KeyBox.Text};"))
                messageBox.Text = "不是合法变量名";
            else if (varNameList.Contains(KeyBox.Text))
                messageBox.Text = "变量名已存在";
            else
                messageBox.Text = "";

        }

        private void TheVariableWindow_Closed(object sender, EventArgs e)
        {
            Variable.DefaultValue = ValueBox.Text;
            Variable.Key = KeyBox.Text.Trim();

        }
        public (bool isSuccess, string message) IsLegalVarName()
        {
            if (string.IsNullOrWhiteSpace(KeyBox.Text))
                return (false, "变量名不能为空");
            if (!AppState.QuickerConnector.TryExecute($"string {KeyBox.Text}"))
                return (false, "不是合法变量名");
            if (varNameList.Contains(KeyBox.Text))
                return (false, "变量名已经存在");
            return (true, "");
        }
        private void TheVariableWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = IsLegalVarName();
            if (!result.isSuccess)
            {
                MessageBox.Show(result.message, "提示窗口", MessageBoxButton.OK);
                e.Cancel = true;
            }
        }

        private void TheVariableWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
