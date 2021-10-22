using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickerTools.Domain.X
{
    public enum VarType
    {
        Text = 0,
        Number = 1,
        Boolean = 2,
        Image = 3,
        List = 4,
        DateTime = 6,
        Keyboard = 7,
        Mouse = 8,
        Enum = 9,
        Dict = 10, // 0x0000000A
        Form = 11, // 0x0000000B
        Integer = 12, // 0x0000000C
        Object = 98, // 0x00000062
        Any = 99, // 0x00000063
        NA = 100, // 0x00000064
        CreateVar = 101, // 0x00000065
    }

}
