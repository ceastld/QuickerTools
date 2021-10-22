using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickerTools.Domain;
using QuickerTools.Domain.Completion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuickerTools.Utilities
{
    public static class CompletionHepler
    {
        public static (string parent,string token) GetFrontParentAndToken(this TextEditor TextEditor)
        {
            //DocumentLine docLine = TextEditor.Document.GetLineByOffset(TextEditor.CaretOffset);
            int offset = TextEditor.CaretOffset;
            int maxl = 128;
            int lenght = offset > maxl ? maxl: offset;
            offset = offset > maxl ? offset - maxl : 0;
            string text = TextEditor.Document.GetText(offset, lenght);
            string word = Regex.Match(text, @"[\w+\.]+\w*$|\w+$", RegexOptions.RightToLeft).Value;
            MainWindow.ShowMessage(word);
            int index = word.LastIndexOf(".");
            if(index < 0)
            {
                return ("", word);
            }
            else
            {
                return (word.Substring(0, index), word.Substring(index + 1));
            }
        }
        public static string GetFrontLinq(this TextEditor TextEditor)
        {
            string text = TextEditor.Document.GetText(0, TextEditor.CaretOffset);
            string partner = @"(\w+)\.(\w+)\(\s*(\w+)\s*=>";
            Match match = Regex.Match(text, partner);
            string parent = match.Groups[1].Value;
            string token = match.Groups[2].Value;
            string varName = match.Groups[3].Value;
            string result = $"{parent}|{token}|{varName}";
            MainWindow.ShowMessage(result);
            return result;
        }
        public static Type GetTypesWithString(string className)
        {
            if (string.IsNullOrEmpty(className)) return typeof(object);
            if (className == "var") return typeof(object);
            var type = AppState.QuickerConnector.ExecuteExpression($"typeof({className})") as Type;
            if(type == null)
            {
                return AppState.QuickerConnector.ExecuteExpression($"{className}.GetType()") as Type;
            }
            else
            {
                return type;
            }
        }
        public static (Type type,bool isInstance) GetTypeWithString2(string typeString)
        {
            if (string.IsNullOrEmpty(typeString)) return (typeof(object), false);
            if (typeString == "var") return (typeof(object), false);
            var type = AppState.QuickerConnector.ExecuteExpression($"typeof({typeString})") as Type;
            if (type == null)
            {
                return (AppState.QuickerConnector.ExecuteExpression($"{typeString}.GetType()") as Type, true);
            }
            else
            {
                return (type, false);
            }
        }

    }
}
