using System;
using System.ComponentModel;
using System.Linq;

namespace SigOpsMetrics.API.Classes.Internal
{
    public static class EnumDescriptions
    {
        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                if (attribute is DescriptionAttribute)
                {
                    var attrDescrip = (DescriptionAttribute)attribute;
                    if (attrDescrip.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            return default(T);
            // or return default(T);
        }

        public static string GetEnumDescriptionFromValue<T>(int intValue)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            var values = Enum.GetValues(typeof(T));
            foreach (int value in values)
            {
                if (value != intValue) continue;
                var enumName = Enum.GetName(typeof(T), value);
                var enumType = typeof(T);
                var fi = enumType.GetField(enumName);

                var attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

                if (attributes.Length > 0)
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            return "";
        }

        public static string GetDescriptionFromEnumValue(Enum value)
        {
            var attribute = value.GetType().GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault();
            if (attribute is DescriptionAttribute)
            {
                var attrDescrip = (DescriptionAttribute)attribute;
                return attrDescrip.Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
