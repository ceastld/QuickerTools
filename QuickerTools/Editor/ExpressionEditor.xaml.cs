using Newtonsoft.Json.Linq;
using QuickerTools.Domain;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuickerTools.Utilities.WindowOp;
using System.Windows.Data;
using System.Windows.Controls;

namespace QuickerTools.Editor
{
    /// <summary>
    /// ExpressionEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ExpressionEditor : Window
    {
        public static ExpressionEditor Instance = null;
        private static bool? _autoRunExpression = false;

        public bool _isEdited = false;
        private bool _isTitleChanged = false;
        public CodeItem CurrentItem { get; set; }
        public static void ShowItSelf()
        {
            if (Instance != null)
            {
                if (Instance.WindowState == WindowState.Minimized)
                    Instance.WindowState = WindowState.Normal;
                Instance.Show();
                Instance.Activate();
            }
            else
            {
                var editor = new ExpressionEditor();
                //var handle = WindowHelper.GetHandle(editor);
                //AppState.EditorWindowHandle = handle.ToInt32();
                //MessageBox.Show(handle.ToInt32().ToString());
                //这个会阻塞进程,要放到最后
                WindowHelper.ShowWindowAndWaitClose(editor, true);
            }
        }
        public ExpressionEditor()
        {
            InitializeComponent();
            ExpListBox.LvCodeItems.SelectionChanged += ExpListBox_SelectionChanged;
            ExpListBox.AddItemBtn.Click += (s, e) => TextEditor_Input.FocusTextEditor();
            TextEditor_Input.TextEditor.TextChanged += TextEditer_Input_TextChanged;
            AutoRunCheckBox.IsChecked = _autoRunExpression;
            _SetNewCurrentItem();
            Instance = this;
            AppState.CodeItems.CollectionChanged += (s, e) =>
            {
                AppState.Save();
                AppState.BackUp();
            };
        }
        private void _SetNewCurrentItem()
        {
            if (AppState.ExpressionData.IsEmpty())
            {
                CurrentItem = new CodeItem();
                AppState.ExpressionData.AddNewItem(CurrentItem);
            }
            else
            {
                CurrentItem = AppState.CodeItems[0];
            }
            ExpListBox.LvCodeItems.SelectedIndex = 0;
            UpdateEditor();
        }
        public void UpdateWithNewItem(CodeItem codeItem)
        {
            SaveCurrentItem();
            CurrentItem = codeItem;
            UpdateEditor();
            _isEdited = false;
            _isTitleChanged = false;
        }
        public void UpdateEditor()
        {
            ExpTitleBox.Text = CurrentItem?.Title;
            TextEditor_Input.Text = CurrentItem?.Code;
        }

        public void SaveCurrentItem()
        {
            if (CurrentItem == null) return;
            if (string.IsNullOrEmpty(CurrentItem.Title))
            {
                CurrentItem.Title = GetDefaultTitle();
            }
            if (AppState.ExpressionData.ContainsItem(CurrentItem))
            {
                var item = AppState.ExpressionData.CodeItemDict[CurrentItem.Id];
                if (_isEdited)
                {
                    item.Code = TextEditor_Input.Text.Trim() + "\r\n";
                    item.UpdateEditTime();
                }
            }
            else
            {
                //AppState.ExpressionData.AddNewItem(CurrentItem);
            }
        }
        private void ExpListBox_SelectionChanged(object sender, EventArgs e)
        {
            //if (ExpListBox.LvCodeItems.ContextMenu.IsVisible) return;
            if (ExpListBox.LvCodeItems.SelectedItem == null) return;
            UpdateWithNewItem(ExpListBox.LvCodeItems.SelectedItem as CodeItem);
            //ExpListBox.RefreshList();
        }
        private void TextEditer_Input_TextChanged(object sender, EventArgs e)
        {
            _isEdited = true;
            string inputText = TextEditor_Input.Text;

            //实时更新标题
            //if (string.IsNullOrEmpty(CurrentItem.Title)) UpdateExpressionTitle(inputText);

            if (inputText.EndsWith(",")) return;
            if (AutoRunCheckBox.IsChecked == false) return;
            RunExpression(inputText);
        }
        private void RunExpression(string inputText)
        {
            string resultText = "";

            this.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var varDict = AppState.Variables.ToDictionary(x => x.Key, x => x.DefaultValue as object);
                    var result = AppState.QuickerConnector.ExecuteExpression(inputText, varDict);
                    if (result != null)
                    {
                        try
                        {
                            resultText = JToken.FromObject(result).ToString();
                        }
                        catch { }
                    }
                }
                catch
                {
                    resultText = "表达式有误";
                }
                TextOutput.Text = resultText;
            });

        }

        public string GetDefaultTitle()
        {
            string text = TextEditor_Input.Text;
            if (string.IsNullOrEmpty(text)) return CodeItem.defaultTitle;
            int index = text.IndexOf("\r\n");
            if (index == -1)
                return text;
            else
                return text.Substring(0, index);
        }
        private void StretchBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (TheActionToolBox.Visibility)
            {
                case Visibility.Visible:
                    TheActionToolBox.Visibility = Visibility.Collapsed;
                    break;
                default:
                    TheActionToolBox.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            //CurrentItem.Title = ExpTitleBox.Text;
            //CurrentItem.Code = TextEditer_Input.Text.Trim();

            //AppState.ExpressionData.AddNewItem(CurrentItem);
            TextOutput.Text = JToken.FromObject(AppState.CodeItems.Select(x => x.Title)).ToString()
                + "\r\n\r\n" + JToken.FromObject(AppState.ExpressionData.CodeItemDict.Select(x => x.Value.Title)).ToString()
                + "\r\n\r\n" + JToken.FromObject(AppState.CodeItems).ToString()
                + "\r\n\r\n" + JToken.FromObject(AppState.ExpressionData).ToString();

        }

        private void ExpTitleBox_KeyDown(object sender, KeyEventArgs e)
        {
            _isTitleChanged = true;
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                e.Handled = true; //拦截按键
                TextEditor_Input.FocusTextEditor();
            }
        }

        private void ExpTitleBox_GotFocus(object sender, RoutedEventArgs e) => this.Dispatcher.InvokeAsync(() => { ExpTitleBox.SelectAll(); });

        private void TheWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                ExpTitleBox.Focus(); return;
            }

            if (e.Key == Key.F5)
            {
                RunExpression(TextEditor_Input.Text); return;
            }
        }

        private void TheWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Thread.Sleep(100);
        }

        private void TheWindow_Closed(object sender, EventArgs e)
        {
            SaveCurrentItem();
            AppState.Save();
            Instance = null;
            AppState.EditorWindowHandle = 0;
        }

        private void AutoRunCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (AutoRunCheckBox.IsChecked == true) RunExpression(TextEditor_Input.Text);
            _autoRunExpression = AutoRunCheckBox.IsChecked;
        }

        private void RunBtn_Click(object sender, RoutedEventArgs e) => RunExpression(TextEditor_Input.Text);

        private void ExpListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                e.Handled = true;
                TextEditor_Input.FocusTextEditor();
            }
        }

        private void CopyExpBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("$=" + TextEditor_Input.Text);
        }

        private void TheWindow_SourceInitialized(object sender, EventArgs e)
        {
            AppState.EditorWindowHandle = WindowHelper.GetHandle(this).ToInt32();
        }

        private void ExpTitleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentItem.Title = ExpTitleBox.Text;
            ExpListBox.RefreshList();
            //AppState.ExpressionData.CodeItemDict[CurrentItem.Id].Title = ExpTitleBox.Text;
            //AppState.CodeItems.Changed();
        }
    }
}
