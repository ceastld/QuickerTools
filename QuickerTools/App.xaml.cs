using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using QuickerTools.Domain;
using Z.Expressions;

namespace QuickerTools
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static void StartUp(Func<string, object> exp, Func<string, Dictionary<string, object>, object> exp2)
        {
            if (AppState.Init())
            {
                if (Editor.ExpressionEditor.Instance == null)
                {
                    AppState.QuickerConnector.ExecuteExpressionMethod = exp;
                    AppState.QuickerConnector.ExecuteExpWithPreDefinedVarMethod = exp2;
                }
                Editor.ExpressionEditor.ShowItSelf();
                //Utilities.WindowOp.WindowHelper.ShowWindowAndWaitClose(new Editor.ExpressionEditor(), true);
            }
        }
        public static void InitUserConfig()
        {
            App.StartUp(x => x, (y, z) => Eval.Execute(y, z));
        }
        public static void test()
        {
            QuickerTools.Note.TextExtractor.ShowItSelf();
        }
    }
}
