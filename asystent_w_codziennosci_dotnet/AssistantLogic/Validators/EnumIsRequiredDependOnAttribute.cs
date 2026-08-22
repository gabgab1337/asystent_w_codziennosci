using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    internal class EnumIsRequiredDependOnAttribute : ValidationAttribute
    {
        private readonly string propertyDescription;
        private readonly object enumEmptyValue;
        
        private readonly string dependOnEnumPropertyName;
        private readonly object dependOnEnumValue;
        public EnumIsRequiredDependOnAttribute(string propertyDescription, object enumEmptyValue,  string dependOnEnumPropertyName, object dependOnEnumValue)
        {
            this.enumEmptyValue = enumEmptyValue;
            this.propertyDescription = propertyDescription;
            this.dependOnEnumPropertyName = dependOnEnumPropertyName;
            this.dependOnEnumValue = dependOnEnumValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dependOnEnumProperty = validationContext.ObjectType.GetProperty(dependOnEnumPropertyName);

            if (dependOnEnumProperty == null)
            {
                return new ValidationResult($"Unknown property: {dependOnEnumPropertyName}");
            }
            object dependOnEnum = dependOnEnumProperty.GetValue(validationContext.ObjectInstance);

            if(dependOnEnumValue.Equals(dependOnEnum))
            {
                if (value == null || value.Equals(enumEmptyValue))
                {
                    return new ValidationResult($"Pole \"{propertyDescription}\" musi być ustawione.");
                }
            }


            return ValidationResult.Success;

        }
    }
}
