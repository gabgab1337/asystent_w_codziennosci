using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace AssistantLogic.Common
{
    public static class EnumExtensions
    {

        public static List<SelectListItem> GetTypesList(this Enum enumValue)
        {
            var type = enumValue.GetType();
            var names = Enum.GetNames(type);

            return names.Select(name => new SelectListItem
            {
                Value = name,
                Text = GetEnumDescription((Enum)Enum.Parse(type, name))
            }).ToList();
        }
      
        public static string GetEnumDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
        }
    }
}
