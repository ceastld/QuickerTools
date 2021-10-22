using Newtonsoft.Json;
using QuickerTools.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace QuickerTools.Domain
{
    public static class AppState
    {
        public static CodeData ExpressionData = new CodeData();
        public static SmartCollection<CodeItem> CodeItems = new SmartCollection<CodeItem>();
        public static SmartCollection<Variable> Variables = new SmartCollection<Variable>();
        public static QuickerConnector QuickerConnector { get; set; } = new QuickerConnector();
        public static int EditorWindowHandle
        {
            get
            {
                try
                {
                    return Convert.ToInt32(File.ReadAllText(DefaultPath.EditorWindowHandle));
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                File.WriteAllText(DefaultPath.EditorWindowHandle, value.ToString());
            }
        }
        public static string DefaultDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Quicker", "QuickerTools_Ceastld");
        public static class DefaultPath
        {
            public static string DataBase => Path.Combine(DefaultDirectory, "UserData.json");
            public static string DataBackUp => Path.Combine(DefaultDirectory, "BackUp.json");
            public static string EditorWindowHandle => Path.Combine(DefaultDirectory, "EditorWindowHandle.txt");
            public static string DataBaseFile => Path.Combine(DefaultDirectory, "UserData.db");
        }
        private static bool _isFirstStartUp = true;
        public static bool Init()
        {
            if (!_isFirstStartUp) return true;
            if (!Directory.Exists(DefaultDirectory))
            {
                Directory.CreateDirectory(DefaultDirectory);
            }
            if (File.Exists(DefaultPath.DataBase))
            {
                var temp = JsonConvert.DeserializeObject<UserData>(File.ReadAllText(DefaultPath.DataBase));
                temp.SetNew();
            }


            //检测是否有之前版本的窗口还打开着
            if(EditorWindowHandle != 0)
            {
                try
                {
                    var win = Utilities.WindowOp.WindowHelper.GetWindowByHandle(EditorWindowHandle);
                    if (win != null) win.Close();

                }
                catch
                {
                    EditorWindowHandle = 0;
                }
            }

            //清理之前版本的程序集,最后
            if (ExistsNewAssemblyAndClearOld())
            {
                MessageBox.Show("已经安装了更新版本的动作,不能使用旧版本的动作");
                return false;
            }
            _isFirstStartUp = false;
            return true;
        }
        public static void Save()
        {
            AppState.ExpressionData.CleanUpTrash();
            var data = new UserData(); data.GetCurrent();
            File.WriteAllText(DefaultPath.DataBase, data.ToString());
        }
        public static void BackUp()
        {
            var data = new UserData(); data.GetCurrent();
            if (data == null) return;
            try
            {
                File.WriteAllText(DefaultPath.DataBackUp, data.ToString());
            }
            catch
            {

            }
        }
        public static bool ExistsNewAssemblyAndClearOld()
        {
            var files = Directory.GetFiles(DefaultDirectory);
            string current = Assembly.GetExecutingAssembly().Location;


            Regex regex = new Regex(@"(\w+)(\d+\.\d+|)\.dll");
            var groups = regex.Match(Path.GetFileName(current)).Groups;

            string start = groups[1].Value;
            string str_cur_v = groups[2].Value;
            double curVersion = string.IsNullOrEmpty(str_cur_v) ? 0 : Convert.ToDouble(str_cur_v);

            bool toReturn = false;
            foreach (var x in files)
            {
                var y = Path.GetFileName(x);
                if (y.StartsWith(start) && y.EndsWith(".dll"))
                {
                    string v_str = Regex.Match(y, @"\d+\.\d+").Value;
                    double version = string.IsNullOrEmpty(v_str) ? 0 : Convert.ToDouble(v_str);
                    if (version < curVersion)
                    {
                        try
                        {
                            File.Delete(x);
                        }
                        catch { }
                    }
                    else if (version > curVersion)
                    {
                        toReturn = true;
                    }

                }
            }
            return toReturn;
        }
    }
    public class UserData
    {
        public CodeData ExpressionData { get; set; }
        public SmartCollection<Variable> Variables { get; set; } = new SmartCollection<Variable>();
        public void SetNew()
        {
            AppState.ExpressionData = ExpressionData ?? new CodeData();
            AppState.ExpressionData.Init();
            AppState.Variables.Clear();
            AppState.Variables.AddRange(Variables??new SmartCollection<Variable>());
        }
        public void GetCurrent()
        {
            ExpressionData = AppState.ExpressionData;
            Variables = AppState.Variables;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
