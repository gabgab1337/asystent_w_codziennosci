using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    internal class FieldIsRequiredAsIntForTime : ValidationAttribute
    {
        private readonly string propertyDescription;

        public FieldIsRequiredAsIntForTime(string propertyDescription)
        {
            this.propertyDescription = propertyDescription;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
                if (value == null)
                {
                    return new ValidationResult($"Pole \"{propertyDescription}\" musi być ustawione.");
                }
                if (value is not int)
                {
                    return new ValidationResult($"Pole \"{propertyDescription}\" musi być liczbą naczuralną.");
                }


            return ValidationResult.Success;

        }
    }
}
