using QuickerTools.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickerTools.Editor
{
    public class SortByTitle : IComparer
    {
        public int Compare(CodeItem x, CodeItem y) => string.Compare(x.Title, y.Title);
        public int Compare(object x, object y) => x == null || y == null ? 0 : this.Compare((CodeItem)x, (CodeItem)y);
        
    }
    public class SortByTime : IComparer
    {
        public int Compare(CodeItem x, CodeItem y) => -DateTime.Compare(x.EditTime, y.EditTime);
        public int Compare(object x, object y) => x == null || y == null ? 0 : this.Compare((CodeItem)x, (CodeItem)y);
    }
}
