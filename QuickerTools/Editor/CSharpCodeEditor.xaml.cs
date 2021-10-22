using HandyControl.Controls;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using QuickerTools.Utilities;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace QuickerTools.Editor
{
    /// <summary>
    /// CSharpCodeEditor.xaml 的交互逻辑
    /// </summary>
    public partial class CSharpCodeEditor : UserControl
    {
        public CSharpCodeEditor()
        {
            InitializeComponent();
            InitInputTextEditer();
        }
        public void FocusTextEditor() => TextEditor.Focus();
        private void InitInputTextEditer()
        {
            TextEditor.TextArea.TextEntered += TextAreaOnTextEntered;
            TextEditor.TextArea.TextEntering += TextArea_TextEntering;
            //TextEditor.TextArea.SelectionChanged += (s, e) =>
            //{
            //    TextEditor.TextArea.TextView.Redraw();
            //};
            foldingManager = FoldingManager.Install(TextEditor.TextArea);
            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += (s, e) => { UpdateFoldings(); };
            foldingUpdateTimer.Start();
            InitializeCommands();
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            string text = e.Text;
            if (!char.IsLetterOrDigit(text[0]))
            {
                if(_completionWindow == null)
                {
                    return;
                }
                try
                {
                    _completionWindow.Close();
                }
                catch { }
            }
            if (char.IsLetter(text[0]))
            {
                ShowCompletionWindow();
                return;
            }
        }

        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs e)
        {
            string text = e.Text;
            if(text == ".")
            {

                ShowCompletionWindow();
                return;
            }
            if (text == "(")
            {
                TextAreaInsertAtBehind(")");return;
                
            }
            if(text == "{")
            {
                TextAreaInsertAtBehind("}");return;
            }
            if(text == "[")
            {
                TextAreaInsertAtBehind("]");return;
            }

            if (text == ")" || text == "}" || text == "]")
            {
                if (GetBehindText() == e.Text)
                {
                    RemoveBehindText();
                }
                return;
            }
            if(!char.IsLetterOrDigit(text[0]))
            {
                if (_completionWindow != null)
                    _completionWindow.Close();
                
            }
        }
        private void ShowCompletionWindow()
        {
            if (_completionWindow != null && _completionWindow.IsVisible) return;
            _completionWindow = new CompletionWindow(TextEditor.TextArea);
            CompletionDataBase.SetCompletions(_completionWindow.CompletionList.CompletionData, TextEditor);

            //var completions = _completionWindow.CompletionList.CompletionData;
            _completionWindow.Show();
            
            _completionWindow.Closed += (o, args) => _completionWindow = null;

        }
        private void TextAreaInsertAtBehind(string text)
        {
            TextEditor.Document.Insert(TextEditor.CaretOffset, text);
            TextEditor.CaretOffset -= text.Length;
        }
        private void ExchangeWithFrontLine(int lineOffset = -1)
        {
            //int lineCount = TextEditor.Document.LineCount;
            int offset = TextEditor.CaretOffset;
            DocumentLine lineNow = TextEditor.Document.GetLineByOffset(offset);
            int lineNumber2 = lineNow.LineNumber;
            if (lineNumber2 <= 1) return;
            int endOffset = lineNow.EndOffset;
            var line1 = TextEditor.Document.GetLineByNumber(lineNumber2 - 1);
            int startOffset = line1.Offset;
            int line1_length = line1.Length;
            var segment = new SelectionSegment(startOffset, endOffset);
            string theText = TextEditor.Document.GetText(segment);
            string[] list = theText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string temp = list[0];
            list[0] = list[1];
            list[1] = temp;
            string theReplaceText = string.Join("\r\n", list);
            TextEditor.Document.Replace(startOffset, endOffset-startOffset, theReplaceText);
            TextEditor.CaretOffset = offset - line1_length - 2;
        }
        private void ExchangeWithNextLine()
        {
            int offset = TextEditor.CaretOffset;
            DocumentLine lineNow = TextEditor.Document.GetLineByOffset(offset);
            int lineNumber1 = lineNow.LineNumber;
            if (lineNumber1 >= TextEditor.LineCount) return;
            int startOffset = lineNow.Offset;
            var line2 = TextEditor.Document.GetLineByNumber(lineNumber1 + 1);
            int endOffset = line2.EndOffset;
            int line2_length = line2.Length;

            var segment = new SelectionSegment(startOffset, endOffset);
            string theText = TextEditor.Document.GetText(segment);
            string[] list = theText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string temp = list[0];
            list[0] = list[1];
            list[1] = temp;
            string theReplaceText = string.Join("\r\n", list);

            TextEditor.Document.Replace(startOffset, endOffset - startOffset, theReplaceText);
            TextEditor.CaretOffset = offset + line2_length + 2;

        }

        private string GetFrontWord()
        {
            DocumentLine docLine = TextEditor.Document.GetLineByOffset(TextEditor.CaretOffset);
            string text = TextEditor.Document.GetText(docLine);
            string word = Regex.Match(text, @"\w+", RegexOptions.IgnoreCase).Value;
            MainWindow.ShowMessage(word);
            return word;
        }
        private string GetFrontText(int length = 1)
        {
            try
            {
                return TextEditor.Document.GetText(TextEditor.CaretOffset - length, length);
            }
            catch
            {
                return "";
            }
        }

        private string GetBehindText(int length = 1)
        {
            try
            {
                return TextEditor.Document.GetText(this.TextEditor.CaretOffset, length);
            }
            catch
            {
                return "";
            }
        }

        private void RemoveBehindText(int length = 1) => TextEditor.Document.Remove(TextEditor.CaretOffset, length);
        private void InitializeCommands()
        {
            this.CommandBindings.AddKeyGesture(new KeyGesture(Key.Oem2, ModifierKeys.Control), (sender, args) => this.Text_Input_CommentOrUnComment());
            this.CommandBindings.AddKeyGesture(new KeyGesture(Key.Up, ModifierKeys.Alt), (s, e) => this.ExchangeWithFrontLine());
            this.CommandBindings.AddKeyGesture(new KeyGesture(Key.Down, ModifierKeys.Alt), (s, e) => this.ExchangeWithNextLine());
        }

        private void Text_Input_CommentOrUnComment()
        {
            if (this.FindCommentStart(this.TextEditor.Document.GetLineByOffset(this.TextEditor.SelectionStart)) >= 0)
                this.UnComment();
            else
                this.Comment();
        }

        private void Comment()
        {
            TextDocument document = this.TextEditor.Document;
            DocumentLine lineByOffset1 = document.GetLineByOffset(this.TextEditor.SelectionStart);
            DocumentLine lineByOffset2 = document.GetLineByOffset(this.TextEditor.SelectionStart + this.TextEditor.SelectionLength);
            using (document.RunUpdate())
            {
                for (DocumentLine documentLine = lineByOffset1; documentLine != null && documentLine.LineNumber <= lineByOffset2.LineNumber; documentLine = documentLine.NextLine)
                    document.Insert(documentLine.Offset, "//");
            }
        }

        private void UnComment()
        {
            TextDocument document = this.TextEditor.Document;
            DocumentLine lineByOffset1 = document.GetLineByOffset(this.TextEditor.SelectionStart);
            DocumentLine lineByOffset2 = document.GetLineByOffset(this.TextEditor.SelectionStart + this.TextEditor.SelectionLength);
            using (document.RunUpdate())
            {
                for (DocumentLine line = lineByOffset1; line != null && line.LineNumber <= lineByOffset2.LineNumber; line = line.NextLine)
                {
                    int commentStart = this.FindCommentStart(line);
                    if (commentStart >= 0)
                        document.Remove(line.Offset + commentStart, 2);
                }
            }
        }

        private int FindCommentStart(DocumentLine line)
        {
            int num = 0;
            while (num < line.Length && char.IsWhiteSpace(this.TextEditor.Document.GetCharAt(line.Offset + num)))
                ++num;
            return num > line.Length - 2 || this.TextEditor.Document.GetCharAt(line.Offset + num) != '/' || this.TextEditor.Document.GetCharAt(line.Offset + num + 1) != '/' ? -1 : num;
        }

        public string Text
        {
            get => TextEditor.Text;
            set => TextEditor.Text = value;
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var line = TextEditor.Document.GetLineByOffset(TextEditor.CaretOffset);

            if (e.Key == Key.Enter)
            {
                if (GetFrontText() == "{")
                {
                    TextAreaInset("\r\n\t", "\r\n");
                    e.Handled = true;
                    //_Indenter.IndentLine(TextEditor.Document, line);
                    _Indenter.IndentLines(TextEditor.Document, line.LineNumber, line.LineNumber + 2);
                    //Indent();
                    TextEditor.ScrollToLine(TextEditor.Document.GetLineByOffset(TextEditor.CaretOffset).LineNumber + 1);
                    return;
                }
                _Indenter.IndentLine(TextEditor.Document, TextEditor.Document.GetLineByOffset(TextEditor.CaretOffset));
            }
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {

        }
        private void TextAreaInset(string pre, string behind)
        {
            TextEditor.Document.Insert(TextEditor.CaretOffset, pre);
            TextEditor.Document.Insert(TextEditor.CaretOffset, behind);
            TextEditor.CaretOffset -= behind.Length;

        }

        private void MenuIndent_OnClick(object sender, RoutedEventArgs e) => Indent();
        public void Indent() => _Indenter.IndentLines(TextEditor.Document, 0, TextEditor.LineCount);
        private CSharpIndentationStrategy _Indenter = new CSharpIndentationStrategy();
        private BraceFoldingStrategy foldingStrategy = new BraceFoldingStrategy();
        private FoldingManager foldingManager;
        private CompletionWindow _completionWindow;

        private void UpdateFoldings() => foldingStrategy.UpdateFoldings(foldingManager, TextEditor.Document);

    }
}
