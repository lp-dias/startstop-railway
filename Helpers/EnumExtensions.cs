using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StartStop.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? enumValue.ToString();
        }
    }
}
