using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    public class TimeBeginIsRequiredAttribute : ValidationAttribute
    {
        private readonly string anchorBeginPropertyName;

        public TimeBeginIsRequiredAttribute(string anchorBeginPropertyName)
        {
            this.anchorBeginPropertyName = anchorBeginPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var anchorBeginProperty = validationContext.ObjectType.GetProperty(anchorBeginPropertyName);

            if (anchorBeginProperty == null)
            {
                return new ValidationResult($"Unknown property: {anchorBeginProperty}");
            }
            bool anchorBegin = (bool)anchorBeginProperty.GetValue(validationContext.ObjectInstance);

            if (anchorBegin)
            {
                var startTime = (string?)value;
                if(startTime == null)
                {
                    return new ValidationResult("Pole \"Czas rozpoczęcia\" musi być ustawione.");
                }
                if(startTime.Length != 5) 
                {
                    return new ValidationResult("Pole \"Czas rozpoczęcia\" ma nieprawidłową wartość");
                }

            }
           
            return ValidationResult.Success;
            
        }
    }
}
