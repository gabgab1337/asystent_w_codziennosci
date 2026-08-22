using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    internal class TimePanedIsRequiredAttribute : ValidationAttribute
    {
        private readonly string anchorEndPropertyName;

        public TimePanedIsRequiredAttribute(string anchorEndPropertyName)
        {
            this.anchorEndPropertyName = anchorEndPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var anchorEndProperty = validationContext.ObjectType.GetProperty(anchorEndPropertyName);

            if (anchorEndProperty == null)
            {
                return new ValidationResult($"Unknown property: {anchorEndProperty}");
            }
            bool anchorEnd = (bool)anchorEndProperty.GetValue(validationContext.ObjectInstance);

            if (anchorEnd)
            {
                var time = (string?)value;
                if (time == null)
                {
                    return new ValidationResult("Pole \"Czas zakończenia\" musi być ustawione.");
                }
                if (time.Length != 5)
                {
                    return new ValidationResult("Pole \"Czas zakończenia\" ma nieprawidłową wartość");
                }

            }

            return ValidationResult.Success;

        }
    }
}
