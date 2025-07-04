using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinSonic.Model.Util
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        public static List<EnumDisplayItem<T>> GetDisplayItems<T>() where T : struct, Enum
        {
            return [.. Enum.GetValues<T>().Select(value => new EnumDisplayItem<T>(value, value.GetDescription()))];
        }
    }
}
