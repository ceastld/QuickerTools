
using System;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quicker.View.X.Controls.ParamEditors;
using Quicker.View.X.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Quicker.Domain.Actions.X.Storage;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media.Imaging;
using Z.Expressions;
using System.Runtime.CompilerServices;
using Quicker.Utilities.Ext;

namespace QuickerTools.Editor
{

    class CodeByCesaryuan
    {

        static CompletionWindow completionWindow;
        static EvalContext evalContext = null;
        static Dictionary<string, List<Type>> varTypeDict = new Dictionary<string, List<Type>>();
        static JObject QuickcerVarInfo = JObject.Parse(@"{""0"": {""name"": ""Text"",""type"": ""string""},""1"": {""name"": ""Number"",""type"": ""double""},""2"": {""name"": ""Boolean"",""type"": ""bool""},""3"": {""name"": ""Image"",""type"": ""Bitmap""},""4"": {""name"": ""List"",""type"": ""List<string>""},""6"": {""name"": ""DateTime"",""type"": ""DateTime""},""7"": ""Keyboard"",""8"": ""Mouse"",""9"": ""Enum"",""10"": {""name"": ""Dict"",""type"": ""Dictionary<string, object>""},""11"": ""Form"",""12"": {""name"": ""Integer"",""type"": ""int""},""98"": {""name"": ""Object"",""type"": ""Object""},""99"": {""name"": ""Object"",""type"": ""Object""},""100"": ""NA"",""101"": ""CreateVar""}");
        static Type[] PredefindTypes = new Type[]
        {
            typeof(List<string>),
            typeof(Dictionary<string, object>),
            typeof(int),
            typeof(double),
            typeof(bool),
            typeof(string),
            typeof(String),
            typeof(DateTime),
            typeof(Path),
            typeof(File),
            typeof(Directory),
            typeof(Regex),
            typeof(Convert),
            typeof(JObject),
            typeof(JArray),
            typeof(Bitmap),
            typeof(Enumerable),
            typeof(TimeSpan),
            typeof(JsonConvert),
            typeof(object)
        };

        static CompletionDataComparer comparer = new CompletionDataComparer();

        static List<CustomCompletionData> AllCompletionData;

        private static TextCompositionEventHandler EnteringWrapper(object paramEditor)
        {
            return (object sender, TextCompositionEventArgs e) =>
            {
                if (!String.IsNullOrEmpty(e.Text) && completionWindow != null)
                {
                    // 计划当补全窗口无可用项目时就关闭，但是好像不生效
                    if (!char.IsLetterOrDigit(e.Text[0]))
                    {
                        completionWindow.Close();
                        return;
                    }

                }
                //if (!char.IsLetterOrDigit(e.Text[0]))
                //{
                //	// Whenever a non-letter is typed while the completion window is open,
                //	// insert the currently selected element.
                //	completionWindow.CompletionList.RequestInsertion(e);
                //}
                // do not set e.Handled=true - we still want to insert the character that was typed
            };
        }
        private static TextCompositionEventHandler EnteredWrapper(object paramEditor)
        {
            return (object sender, TextCompositionEventArgs e) =>
            {
                if (paramEditor != null && GetPrivateFieid<CompletionWindow>("completionWindow", paramEditor) != null)
                    return;
                if (e.Text == "." || e.Text == "(" || char.IsLetterOrDigit(e.Text[0]))
                {
                    TextArea textArea = sender as TextArea;

                    if (e.Text == ".")
                    {
                        // 获取parent，既「.」前面的字符

                        var parent = GetParent(textArea);

                        completionWindow = new CompletionWindow(textArea);

                        completionWindow.CustomGetMatchQualityFunc = (itemText, query) =>
                        {
                            if (query == "")
                                return 1;
                            return AvalonEditExt.GetMatchQuality(itemText, query);
                        };
                        completionWindow.CloseAutomatically = true;
                        IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

                        GetDataFromSnippets(parent, "", data);
                        GetCompletionDataByReflection(parent, textArea.TextView.Document.Text, data);
                        data = data.Distinct(comparer).ToList();

                        // 补全数据不为空，则显示补全窗口
                        if (data.Count() > 0)
                        {
                            completionWindow.Show();
                        }
                        completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                    }
                    else if (e.Text == "(")
                    {
                        //计划实现括号自动补全，但不知道怎么在光标处输出文字
                        TextArea ta = sender as TextArea;
                        ta.PerformTextInput(")");
                        ta.Caret.Offset -= 1;
                    }
                    else if (char.IsLetterOrDigit(e.Text[0]))
                    {
                        // 如果已经存在completionWindow，则忽略此次事件
                        if (completionWindow == null)
                        {
                            // 获取正在输入的Token
                            string token = GetToken(textArea);
                            if (token != "")
                            {
                                // 获取token的parent
                                string parent = GetParent(textArea);

                                // 生成补全窗口实例
                                completionWindow = new CompletionWindow(textArea);
                                completionWindow.CustomGetMatchQualityFunc = (itemText, query) =>
                                {
                                    return AvalonEditExt.GetMatchQuality(itemText, e.Text + query);
                                };
                                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

                                // 从数据库里寻找匹配到的补全条目
                                GetDataFromSnippets(parent, token, data);
                                if (parent == "")
                                {
                                    GetPossibleVarNames(token, textArea.TextView.Document.Text, data);
                                    GetDataFromPredefindTypes(token, data);
                                }

                                GetCompletionDataByReflection(parent, textArea.TextView.Document.Text, data);
                                // 补全数据不为空，则显示补全窗口
                                if (data.Count() > 0)
                                {
                                    completionWindow.Show();
                                }

                                // 绑定退出事件
                                completionWindow.Closed += delegate
                                {
                                    completionWindow = null;
                                };
                            }
                        }
                    }
                }

            };
        }


        private static void GetDataFromPredefindTypes(string token, IList<ICompletionData> data)
        {
            foreach (var item in PredefindTypes.Where(x => x.Name.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                .Select(x =>
                    new CustomCompletionData()
                    {
                        name = x.Name,
                        actualText = x.IsGenericType ? Regex.Replace(x.Name, @"`\d+$", "<$1>") : x.Name,
                        priority = 10,
                        description = x.FullName,
                        iconPath = "https://files.getquicker.net/_icons/63C96D04CEC05D6370F98EF63E3B3F10E6F7E349.png"
                    }))
            {
                if (item != null)
                {
                    item.replaceOffset = token.Length;
                    data.Add(item);
                }
            }
        }

        private static void GetDataFromSnippets(string parent, string token, IList<ICompletionData> data)
        {
            var completionData = AllCompletionData.Where(x => x.name.StartsWith(token, StringComparison.OrdinalIgnoreCase) && (x.parent.Split('|').Any(y => y.Equals(parent, StringComparison.OrdinalIgnoreCase)) || (parent != "" && x.parent == ".")));
            foreach (var item in completionData)
            {
                if (item != null)
                {
                    item.replaceOffset = token.Length;
                    data.Add(item);
                }
            }
        }

        private static void GetCompletionDataByReflection(string parent, string allCode, IList<ICompletionData> data)
        {
            List<Type> temp = new List<Type>();
            var varName = parent.TrimStart('{').TrimEnd('}', '.');
            temp.AddRange(GetParentPossibleTypes(varName, allCode));

            if (varTypeDict.ContainsKey(varName))
            {
                temp = varTypeDict[varName];
            }
            foreach (var item in GetMethods(temp, BindingFlags.Instance | BindingFlags.Public, true).Concat(GetPropertys(temp)))
            {
                data.Add(item);
            }

            // 获取静态方法
            temp = GetTypeWithString(parent.TrimEnd('.'));
            //AppHelper.ShowInformation(parent);
            foreach (var item in GetMethods(temp, BindingFlags.Static | BindingFlags.Public, false).Concat(GetPropertys(temp)).Concat(GetFields(temp)))
            {
                data.Add(item);
            }
        }

        private static bool GetPossibleVarNames(string token, string allCode, IList<ICompletionData> data)
        {
            string typePattern = @"(?<=^(\$=)?\s*)(?<type>(?<![<>,\w])[a-zA-Z][<>, \w]+)";
            string space = @" +";
            string varPattern = @"(?<!\w)(?<var>[a-zA-Z]\w*)";
            var pattern = new Regex(String.Format(@"{0}{2}{1}\s*(;|=)", typePattern, varPattern, space), RegexOptions.Multiline);
            var matches = pattern.Matches(allCode);
            foreach (var item in matches.OfType<Match>())
            {
                string varName = item.Groups["var"].Value;
                if (varName.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                {
                    data.Add(new CustomCompletionData()
                    {
                        name = varName,
                        replaceOffset = token.Length,
                        description = item.Groups["type"].Value,
                        iconPath = "https://files.getquicker.net/_icons/9CEDE326357A8F717B40DCAB4DC3AB875DDFF0B3.png"
                    });
                }
            }
            if (data.Count > 0)
                return true;
            return false;
        }

        private static List<Type> GetParentPossibleTypes(string parent, string allCode)
        {
            var list = new List<Type>();
            if (parent == "")
                return list;
            string typePattern = @"(?<type>(?<![<>,\w])[a-zA-Z][<>, \w]+)";
            string space = @" +";
            var pattern = new Regex(String.Format(@"{0}{2}{1}|{1}{2}={2}new{2}{0}", typePattern, parent, space));
            var match = pattern.Match(allCode);
            if (match.Success)
            {
                list.AddRange(GetTypeWithString(match.Groups["type"].Value));
            }
            return list;
        }



        private static List<CustomCompletionData> GetMethods(List<Type> types, BindingFlags flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static, bool isInstance = true)
        {
            var data = new List<CustomCompletionData>();
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                Type[] interfaces = type.GetInterfaces();
                var methods = type.GetMethods(flag);
                var extensionMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(t => t.IsDefined(typeof(ExtensionAttribute), false));
                foreach (var method in methods.Concat(extensionMethod))
                {
                    IEnumerable<ParameterInfo> paramss = null;
                    bool isExtension = method.IsDefined(typeof(ExtensionAttribute), false);
                    if (isExtension && isInstance)
                    {
                        paramss = method.GetParameters().Skip(1);
                    }
                    else
                        paramss = method.GetParameters();

                    if (!method.Name.Contains("_"))
                    {
                        string genericPart = (method.IsGenericMethod ? "<>" : "");
                        var onedata = new CustomCompletionData()
                        {
                            name = method.Name + genericPart,
                            actualText = method.Name + (method.IsGenericMethod ? "<$1>" : "") + (paramss.Count() > 0 ? "($1" : "(") + String.Join(", ", paramss.Select(x => "")) + ")",
                            priority = types.Count() - i + 10 - paramss.Count(),
                            description = ((!isInstance) ? "static: " : "") + GetGenericTypeName(type)
                                         + "."
                                         + method.Name
                                         + "("
                                         + String.Join(", ", paramss.Select(x => GetGenericTypeName(x.ParameterType) + " " + x.Name))
                                         + "): "
                                         + method.ReturnType.Name,
                            iconPath = "https://files.getquicker.net/_icons/844EEFDA8D988E83A6BC89FE8513AADBE72930C4.png"
                        };
                        data.Add(onedata);
                    }

                }
                if (interfaces.Any(x => x.Name.StartsWith("IEnumerable")))
                    foreach (var method in typeof(Enumerable).GetMethods())
                    {
                        var ts = type.GetInterfaces().Where(x => x.Name.StartsWith("IEnumerable`"));
                        if (!method.Name.Contains("_"))
                        {
                            var paramss = method.GetParameters().Skip(1);
                            string genericPart = (method.IsGenericMethod ? "<>" : "");
                            var onedata = new CustomCompletionData()
                            {
                                name = method.Name + genericPart,
                                actualText = method.Name + (method.IsGenericMethod ? "<$1>" : "") + (paramss.Count() > 0 ? "($1" : "(") + String.Join(", ", paramss.Select(x => "")) + ")",
                                priority = types.Count() - i + 10 - paramss.Count(),
                                description = String.Join(", ", ts.Select(x => GetGenericTypeName(x)))
                                             + "\r\n"
                                             + method.Name
                                             + "("
                                             + String.Join(", ", paramss.Select(x => x.ParameterType.Name + " " + x.Name))
                                             + "):  "
                                             + method.ReturnType.Name,
                                iconPath = "https://files.getquicker.net/_icons/844EEFDA8D988E83A6BC89FE8513AADBE72930C4.png"
                            };
                            data.Add(onedata);
                        }

                    }
            }
            return data;
        }

        private static List<CustomCompletionData> GetPropertys(List<Type> types)
        {
            var data = new List<CustomCompletionData>();
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                var props = type.GetProperties();

                foreach (var prop in props)
                {

                    if (!prop.Name.Contains("_"))
                    {
                        var onedata = new CustomCompletionData()
                        {
                            name = prop.Name,
                            actualText = prop.Name,
                            priority = types.Count() - i,
                            description = type.Name + "." + prop.Name + ": " + prop.PropertyType.Name,
                            iconPath = "https://files.getquicker.net/_icons/8606B55A3EA3EA7E76D5511CC6FC247354E49E99.png"
                        };
                        data.Add(onedata);
                    }
                }
            }
            return data;
        }

        private static List<CustomCompletionData> GetFields(List<Type> types)
        {
            var data = new List<CustomCompletionData>();
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                var fields = type.GetFields();

                foreach (var field in fields)
                {
                    if (!field.Name.Contains("_"))
                    {
                        var onedata = new CustomCompletionData()
                        {
                            name = field.Name,
                            actualText = field.Name,
                            priority = types.Count() - i,
                            description = type.Name + "." + field.Name + ": " + field.FieldType.Name,
                            iconPath = "https://files.getquicker.net/_icons/9CEDE326357A8F717B40DCAB4DC3AB875DDFF0B3.png"
                        };
                        data.Add(onedata);
                    }
                }
            }
            return data;
        }

        private static string GetGenericTypeName(Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetGenericTypeName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName;
        }

        private static List<Type> GetTypeWithString(string typestring)
        {
            List<Type> types = new List<Type>();
            if (typestring != "")
            {
                switch (typestring)
                {
                    case "List<string>":
                        types.Add(typeof(List<string>));
                        break;
                    case "Dictionary<string, object>":
                        types.Add(typeof(Dictionary<string, object>));
                        break;
                    case "int":
                        types.Add(typeof(int));
                        break;
                    case "double":
                        types.Add(typeof(double));
                        break;
                    case "bool":
                        types.Add(typeof(bool));
                        break;
                    case "string":
                        types.Add(typeof(string));
                        break;
                    case "String":
                        types.Add(typeof(string));
                        break;
                    case "DateTime":
                        types.Add(typeof(DateTime));
                        break;
                    case "Path":
                        types.Add(typeof(Path));
                        break;
                    case "File":
                        types.Add(typeof(File));
                        break;
                    case "Directory":
                        types.Add(typeof(Directory));
                        break;
                    case "Regex":
                        types.Add(typeof(Regex));
                        break;
                    case "Convert":
                        types.Add(typeof(Convert));
                        break;
                    case "JObject":
                        types.Add(typeof(JObject));
                        break;
                    case "JArray":
                        types.Add(typeof(JArray));
                        break;
                    case "Bitmap":
                        types.Add(typeof(Bitmap));
                        break;
                    case "Enumerable":
                        types.Add(typeof(Enumerable));
                        break;
                    case "TimeSpan":
                        types.Add(typeof(TimeSpan));
                        break;
                    case "JsonConvert":
                        types.Add(typeof(JsonConvert));
                        break;
                    default:
                        try
                        {
                            if (evalContext == null)
                            {
                                evalContext = new EvalContext();
                                evalContext.UseLocalCache = true;
                                evalContext.RegisterType(new Type[]
                                {
                                    typeof(Regex),
                                    typeof(Path),
                                    typeof(Enumerable),
                                    typeof(JsonConvert),
                                    typeof(JArray),
                                    typeof(JObject),
                                    typeof(JToken),
                                    typeof(DateTime),
                                    typeof(StringExt),
                                });
                            }

                            //AppHelper.ShowInformation(typestring);
                            var type = evalContext.Execute<Type>("typeof(" + typestring + ")");
                            if (type != null)
                                types.Add(type);
                        }
                        catch (Exception e) { }
                        break;
                }
            }

            return types;
        }

        private static string GetToken(TextArea sender)
        {
            // 获取光标位置前面的至多30个字符
            var po = sender.Caret.Position;

            var selection = new RectangleSelection(
                sender,
                new TextViewPosition(po.Line, po.Column - 30 >= 0 ? po.Column - 30 : 0),
                po);
            // 正则获取Parent
            var tokenMatch = Regex.Match(selection.GetText(), @"(?<=([^\w]|^))\w*$", RegexOptions.RightToLeft);
            if (tokenMatch.Success)
            {
                return tokenMatch.Value;
            }
            else
            {
                return "";
            }
        }

        private static string GetParent(TextArea sender)
        {
            // 获取光标位置前面的至多30个字符
            var currentCursorPosition = sender.Caret.Position;
            var selection = new RectangleSelection(sender, new TextViewPosition(currentCursorPosition.Line, currentCursorPosition.Column - 30 >= 0 ? currentCursorPosition.Column - 30 : 0), currentCursorPosition);
            // 正则获取Parent
            var parentMatch = Regex.Match(selection.GetText(), @"(?<=([^\w{}]|^))[^.]*?\.(?=\w*$)", RegexOptions.RightToLeft);
            if (parentMatch.Success)
            {
                return parentMatch.Value;
            }
            else
            {
                return "";
            }
        }

        public static T GetPrivateFieid<T>(string name, object instance) where T : class
        {
            if (instance == null)
                return null;
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
                return field.GetValue(instance) as T;
            return null;
        }

        public class CustomCompletionData : ICompletionData
        {
            public CustomCompletionData()
            {

            }

            public System.Windows.Media.ImageSource Image
            {
                get
                {

                    return GetImage(this.iconPath);
                    //return null; 
                }
            }

            public string Text
            {
                get { return name; }
            }

            // Use this property if you want to show a fancy UIElement in the drop down list.
            public object Content
            {
                get { return this.Text; }
            }

            public object Description
            {
                get { return description; }
            }

            public double Priority { get { return priority; } }

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

            private ImageSource GetImage(string path)
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
            public int replaceOffset = 0; // 替换选中补全项时的偏移量，默认为0，当非.触发时为token的长度
            public int completeOffset = 0; // 替换完成后光标的偏移量，用来将光标定位到括号内等位置
            public string description = ""; // 数据的介绍
            public string name; // 数据在补全窗口中的文字
            public string actualText; // 实际替换时的文字
            public string parent = ""; // 数据的父类，比如 ToInt32() 的父类是「Convert.」
            public int priority = 0; // 当有多条数据时展示的优先级，优先级越高越靠上
            public string iconPath = "https://files.getquicker.net/_icons/3EE2EC560B20DAB8D47A981922038717B5A6B703.png";
        }

        class CompletionDataComparer : IEqualityComparer<ICompletionData>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(ICompletionData x, ICompletionData y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.Text == y.Text;
            }

            // If Equals() returns true for a pair of objects
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(ICompletionData product)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(product, null)) return 0;

                //Get hash code for the Code field.
                int hashProductCode = product.Text.GetHashCode();

                //Calculate the hash code for the product.
                return hashProductCode;
            }
        }


    }
}
