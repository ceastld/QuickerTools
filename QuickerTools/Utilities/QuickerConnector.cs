using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QuickerTools.Utilities
{
    public class QuickerConnector
    {

        public Action<string, string, bool> RunActionMethod { get; set; }
        public Action<string> EditActionMethod { get; set; }
        public Func<string, object> ExecuteExpressionMethod { get; set; }
        public Func<string,Dictionary<string,object>,object> ExecuteExpWithPreDefinedVarMethod { get; set; }
        /// <summary>
        /// 方便判断表达式是否合法, 
        /// 例如,判断一个变量是否为合法的变量名称
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public bool TryExecute(string expression)
        {

            try
            {
                if (ExecuteExpressionMethod != null)
                    ExecuteExpressionMethod(expression);
                //else
                    //Eval.Execute(expression);
                return true;
            }
            catch { return false; }
        }
        public object ExecuteExpression(string expression)
        {
            try
            {
                if (ExecuteExpressionMethod != null)
                    return ExecuteExpressionMethod(expression);
                else
                    //return Eval.Execute(expression);
                    return "";
            }
            catch (Exception e)
            {
                return $"解析表达式出错\r\n\r\n{e.Message}";
            }
        }
        public object ExecuteExpression(string expression,Dictionary<string,object> varDict)
        {
            try
            {
                if (ExecuteExpWithPreDefinedVarMethod != null)
                    return ExecuteExpWithPreDefinedVarMethod(expression, varDict);
                else
                    //return Eval.Execute(expression, varDict);
                    return "";
            }
            catch(Exception e)
            {
                return $"解析表达式出错\r\n\r\n{e.Message}";
            }
        }

    }

}
