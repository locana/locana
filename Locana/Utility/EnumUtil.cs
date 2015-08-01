using System;
using System.Collections.Generic;
using System.Linq;

namespace Kazyx.Uwpmm.Utility
{
    public static class EnumUtil<T>
    {
        public static IEnumerable<T> GetValueEnumerable()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static bool IsDefined(int number)
        {
            return Enum.IsDefined(typeof(T), number);
        }
    }
}
