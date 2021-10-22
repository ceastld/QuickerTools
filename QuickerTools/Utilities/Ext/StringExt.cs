using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickerTools.Utilities.Ext
{
    public static class StringExt
    {
        public static bool ContainedInAnyStrings(this string filter, IEnumerable<string> values)
        {
            if (string.IsNullOrEmpty(filter))
                return true;
            if (values == null)
                return false;
            foreach (string str in values)
            {
                if (!string.IsNullOrEmpty(str) && str.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                    return true;
            }
            return false;
        }

        public static bool ContainedInAny(this string filter, params string[] values) => filter.ContainedInAnyStrings((IEnumerable<string>)values);
        public static string UrlEncode(this string text) => System.Web.HttpUtility.UrlEncode(text);
    }
}
