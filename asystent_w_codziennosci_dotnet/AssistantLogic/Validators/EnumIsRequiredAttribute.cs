using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    public class EnumIsRequiredAttribute : ValidationAttribute
    {
        private readonly object enumEmptyValue;
        private readonly object propertyName;

        public EnumIsRequiredAttribute(string propertyName, object enumEmptyValue)
        {
            this.enumEmptyValue = enumEmptyValue;
            this.propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || value.Equals(enumEmptyValue))
            {
                return new ValidationResult($"Pole \"{propertyName}\" musi być ustawione.");
            }

            return ValidationResult.Success;

        }
    }
}
