using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Melomania
{
    public static class Extensions
    {
        public static string GetDescription(this Enum enumValue) =>
            enumValue
                .GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;
    }
}
