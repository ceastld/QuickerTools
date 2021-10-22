using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickerTools.Domain.Completion
{
    public class CompletionData : ICompletionData
    {
        public CompletionData()
        {

        }
        public CompletionData(string text)
        {
            name = text;
        }
        public ImageSource Image => GetImage(iconPath);
        public string Text => this.name;
        public object Content => Text;
        public object Description { get => description ?? "Description for : " + Text; set => description = (string)value; }
        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if (String.IsNullOrEmpty(this.actualText))
                this.actualText = this.Text;
            var replaceSegment = new SelectionSegment(completionSegment.Offset - this.replaceOffset, completionSegment.EndOffset);
            textArea.Document.Replace(replaceSegment, GetActualTextAndSetOffset(this.actualText));
            // 输入补全条目后进行光标位置偏移
            textArea.Caret.Offset = textArea.Caret.Offset - this.completeOffset;
        }
        private string GetActualTextAndSetOffset(string text)
        {
            var index = text.IndexOf("$1");
            if (index != -1)
            {
                int offset = text.Length - "$1".Length * (text.Split(new string[] { "$1" }, 0).Count() - 1) - index;
                this.completeOffset = offset;
                return text.Replace("$1", "");
            }
            else
            {
                return text;
            }

        }

        public ImageSource GetImage(string path)
        {
            string text = path;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(text);
            bitmapImage.EndInit();
            if (bitmapImage.CanFreeze)
            {
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }
        public int replaceOffset = 0;
        public int completeOffset = 0;
        public string description = "";
        public string name; // 
        public string actualText;
        public string parent = "";
        public int priority = 0;
        public string iconPath = "https://files.getquicker.net/_icons/3EE2EC560B20DAB8D47A981922038717B5A6B703.png";
    }
}
