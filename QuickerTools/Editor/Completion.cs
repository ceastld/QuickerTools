using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickerTools.Utilities.Ext;
using QuickerTools.Domain.Completion;
using QuickerTools.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuickerTools.Domain;

namespace QuickerTools.Editor
{
    public static class CompletionDataBase
    {
        public static List<string> defaultNameSpace = new List<string>()
        {
            "System.IO.Path",
            "System.Text.RegularExpressions.Regex",
            "System.String",
            "System.DateTime",
            "System.Int32"
        };
        public static List<CompletionData> Completions = null;
        private static List<string> TypeKeyWords = new List<string>()
        {
            "DateTime","Path","Directory","File","Type",
            "Convert","Regex","Boolean","String","Random",
            "JToken","JObject","JArray","JsonConvert","Assembly",
            "Enumerable","List<string>","Dictionary<string,object>",
            "List<$1>","Dictionary<$1,>"
        };
        private static List<string> CodeKeyWords = new List<string>()
        {
            "int","bool","double","string","object",
            "switch","foreach","else","for","if",
            "var","new","typeof","nameof",
            "while","continue","return","break"
        };
        public static void InitCompletions()
        {
            if (Completions == null)
            {

                Completions = new List<CompletionData>();
                foreach (var x in TypeKeyWords)
                {
                    Completions.Add(new CompletionData()
                    {
                        name = x,
                        iconPath = "https://docs.microsoft.com/zh-cn/visualstudio/extensibility/ux-guidelines/media/0405-36_intellisenseclass.png?view=vs-2019"
                    });
                }
                foreach (var x in CodeKeyWords)
                {
                    Completions.Add(new CompletionData()
                    {
                        name = x,
                        iconPath = "https://files.getquicker.net/_icons/67B4FEA818AC1EA1FB17B95392DDD85DB6E1276B.png"
                    });
                }
            }
        }
        public static void SetCompletions(this IList<ICompletionData> datas, TextEditor textEditor)
        {
            InitCompletions();
            var result = textEditor.GetFrontParentAndToken();
            //MainWindow.ShowMessage(result.parent + " . " + result.token);
            InitPossibleVariable(textEditor.Document.GetText(0, textEditor.CaretOffset));
            if (string.IsNullOrEmpty(result.parent))
            {
                GetDataFromVariable(result.token, datas);
                foreach (var x in Completions)
                {
                    datas.Add(x);
                }
            }
            else
            {
                GetDataByReflection(result.parent, result.token, datas);

            }
        }
        private static Dictionary<string, Type> varDict = new Dictionary<string, Type>();

        /// <summary>
        /// 初始化所有可能的变量,并写入到词典中
        /// </summary>
        /// <param name="allCode"></param>
        private static void InitPossibleVariable(string allCode)
        {
            varDict.Clear();
            varDict.Add("_eval", CompletionHepler.GetTypesWithString("EvalContext") ?? typeof(object));
            foreach (var x in AppState.Variables)
            {
                varDict[x.Key] = x.Type;
            }
            string typePattern = @"(?<type>(?<![<>,\w])[a-zA-Z][<>\[\],\w,^\s]+)";
            string varPattern = @"(?<!\w)(?<var>[a-zA-Z]\w*)";
            string normalVar = String.Format(@"(?:{0}\s+{1}\s*(?:;|=))", typePattern, varPattern);
            string linqVar = @"(?:(\w+)\.(\w+)\(\s*(\w+)\s*=>)";
            var pattern = new Regex($@"{normalVar}|{linqVar}", RegexOptions.Multiline);
            var matches = pattern.Matches(allCode);
            foreach (var match in matches.OfType<Match>())
            {
                string varTypeString = match.Groups["type"].Value;
                if (!string.IsNullOrEmpty(varTypeString))
                {
                    string varName = match.Groups["var"].Value;
                    var type = CompletionHepler.GetTypesWithString(varTypeString);
                    if (type == null) continue;
                    varDict[varName] = type;
                }
                else
                {
                    string parent = match.Groups[1].Value;
                    Type type;
                    if (varDict.ContainsKey(parent))
                    {
                        type = varDict[parent];
                    }
                    else
                    {
                        type = CompletionHepler.GetTypesWithString(parent);
                    }
                    string token = match.Groups[2].Value;
                    string varName = match.Groups[3].Value;
                    if (type == null)
                    {
                        continue;
                    }
                    else
                    {
                        //MethodInfo method = typeof(Enumerable).GetMethod(token, BindingFlags.Static | BindingFlags.Public);
                        var paramss = type.GetInterfaces().Where(x => x.Name.StartsWith("IEnumerable"))
                            .Select(x => x.GetGenericArguments().ToList()).Where(x => x.Count == 1).ToList();
                        try
                        {
                            Type varType = paramss[0][0];
                            varDict[varName] = varType;
                        }
                        catch
                        {
                            continue;
                        }

                    }

                }
            }
        }
        private static void GetDataFromVariable(string token, IList<ICompletionData> datas)
        {
            foreach (var x in varDict.Keys.ToArray())
            {
                if (varDict[x] == null) continue;
                datas.Add(new CompletionData()
                {
                    name = x,
                    actualText = x,
                    description = varDict[x].Name ?? "",
                    iconPath = "https://files.getquicker.net/_icons/9CEDE326357A8F717B40DCAB4DC3AB875DDFF0B3.png",
                });
            }
        }
        private static void GetDataByReflection(string parent, string token, IList<ICompletionData> datas)
        {
            bool isInstance = true;
            var types = new List<Type>();
            if (varDict.ContainsKey(parent))
            {
                types.Add(varDict[parent]);
            }
            else
            {
                var typeResult = CompletionHepler.GetTypeWithString2(parent);
                var type = typeResult.type;
                isInstance = typeResult.isInstance;
                if (type != null)
                    types.Add(type);
            }
            types = types.Where(x => x != null).ToList();
            var list = GetMethods(types, isInstance: isInstance).Concat(GetPropertys(types, isInstance)).Concat(GetFields(types, isInstance));
            if (!string.IsNullOrEmpty(token))
            {
                list = list.Where(x => token.ContainedInAny(x.name)).Select(x => { x.replaceOffset = token.Length; return x; }).ToList();
            }
            foreach (var x in list) datas.Add(x);
        }
        private static List<CompletionData> GetPropertys(List<Type> types, bool isInstance = true)
        {
            var data = new List<CompletionData>();
            BindingFlags flag;
            if (isInstance)
            {
                flag = BindingFlags.Instance | BindingFlags.Public;
            }
            else
            {
                flag = BindingFlags.Public | BindingFlags.Static;
            }
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                var props = type.GetProperties(flag);

                foreach (var prop in props)
                {

                    if (!prop.Name.Contains("_"))
                    {
                        var onedata = new CompletionData()
                        {
                            name = prop.Name,
                            actualText = prop.Name,
                            priority = types.Count() - i,
                            description = type.Name + "." + prop.Name + ": " + prop.PropertyType.Name,
                            iconPath = "https://docs.microsoft.com/zh-cn/visualstudio/extensibility/ux-guidelines/media/0405-40_field.png?view=vs-2019"
                        };
                        data.Add(onedata);
                    }
                }
            }
            return data;
        }
        private static List<CompletionData> GetFields(List<Type> types, bool isInstance = true)
        {
            var data = new List<CompletionData>();
            BindingFlags flag;
            if (isInstance)
            {
                flag = BindingFlags.Instance | BindingFlags.Public;
            }
            else
            {
                flag = BindingFlags.Public | BindingFlags.Static;
            }
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                var fields = type.GetFields(flag);

                foreach (var field in fields)
                {
                    if (!field.Name.Contains("_"))
                    {
                        var onedata = new CompletionData()
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
            List<string> list = new List<string>();
            return data;
        }
        private static Dictionary<string, List<MethodInfo>> GetDistinctMethods(IEnumerable<MethodInfo> methods)
        {
            Dictionary<string, List<MethodInfo>> distinctMethods = new Dictionary<string, List<MethodInfo>>();
            foreach (var method in methods)
            {
                if (method.Name.Contains("_")) continue;
                if (!distinctMethods.ContainsKey(method.Name))
                {
                    distinctMethods.Add(method.Name, new List<MethodInfo>() { method });
                }
                else
                {
                    distinctMethods[method.Name].Add(method);
                }
            }
            return distinctMethods;
        }
        private static List<CompletionData> GetMethods(List<Type> types, bool isInstance = true)
        {
            var data = new List<CompletionData>();//return data;
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type == null) continue;
                Type[] interfaces = type.GetInterfaces();
                BindingFlags flag;
                if (isInstance)
                {
                    flag = BindingFlags.Instance | BindingFlags.Public;
                }
                else
                {
                    flag = BindingFlags.Public | BindingFlags.Static;
                }
                var methods = type.GetMethods(flag);
                var extensionMethod = type.GetMethods(flag).Where(t => t.IsDefined(typeof(ExtensionAttribute), false));

                foreach (var methodPair in GetDistinctMethods(methods.Concat(extensionMethod)))
                {
                    string name = methodPair.Key;
                    string textEnd = "($1)";
                    if (methodPair.Value.Count == 1 && methodPair.Value[0].GetParameters().Length == 0)
                    {
                        textEnd = "()";
                    }
                    var onedata = new CompletionData()
                    {
                        name = name,
                        actualText = name + textEnd,
                        description = string.Join("\r\n", methodPair.Value.Select(method =>
                         {
                             IEnumerable<ParameterInfo> paramss = null;
                             bool isExtension = method.IsDefined(typeof(ExtensionAttribute), false);
                             if (isExtension && isInstance)
                             {
                                 paramss = method.GetParameters().Skip(1);
                             }
                             else
                                 paramss = method.GetParameters();
                             return ((!isInstance) ? "static: " : "")
                                    + GetGenericTypeName(type)
                                    + "."
                                    + method.Name
                                    + "("
                                    + String.Join(", ", paramss.Select(x => GetGenericTypeName(x.ParameterType) + " " + x.Name))
                                    + "): "
                                    + method.ReturnType.Name;
                         })),
                        iconPath = "https://files.getquicker.net/_icons/844EEFDA8D988E83A6BC89FE8513AADBE72930C4.png"

                    };
                    data.Add(onedata);
                }

                //foreach (var method in methods.Concat(extensionMethod))
                //{
                //    IEnumerable<ParameterInfo> paramss = null;
                //    bool isExtension = method.IsDefined(typeof(ExtensionAttribute), false);
                //    if (isExtension && isInstance)
                //    {
                //        paramss = method.GetParameters().Skip(1);
                //    }
                //    else
                //        paramss = method.GetParameters();
                //    if (!method.Name.Contains("_"))
                //    {
                //        var onedata = new CompletionData()
                //        {
                //            name = method.Name,
                //            actualText = method.Name + (paramss.Count() > 0 ? "($1" : "(") + String.Join(", ", paramss.Select(x => "")) + ")",
                //            priority = types.Count() - i + 17 - paramss.Count(),
                //            description = ((!isInstance) ? "static: " : "") + GetGenericTypeName(type)
                //                         + "."
                //                         + method.Name
                //                         + "("
                //                         + String.Join(", ", paramss.Select(x => GetGenericTypeName(x.ParameterType) + " " + x.Name))
                //                         + "): "
                //                         + method.ReturnType.Name,
                //            iconPath = "https://files.getquicker.net/_icons/844EEFDA8D988E83A6BC89FE8513AADBE72930C4.png"
                //        };
                //        data.Add(onedata);
                //    }
                //}

                if (interfaces.Any(x => x.Name.StartsWith("IEnumerable")))
                    foreach (var method in typeof(Enumerable).GetMethods())
                    {
                        var ts = type.GetInterfaces().Where(x => x.Name.StartsWith("IEnumerable`"));
                        if (!method.Name.Contains("_"))
                        {
                            var paramss = method.GetParameters().Skip(1);
                            //string genericPart = (method.IsGenericMethod ? "<>" : "");
                            var onedata = new CompletionData()
                            {
                                name = method.Name,   // + genericPart,
                                //actualText = method.Name + (paramss.Count() > 0 ? "($1" : "(") + String.Join(", ", paramss.Select(x => "")) + ")",
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
    }
}
