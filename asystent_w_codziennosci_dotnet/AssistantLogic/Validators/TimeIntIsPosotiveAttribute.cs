using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    public class TimeIntIsPosotiveAttribute : ValidationAttribute
    {
        private readonly string hoursPropertyName;

        public TimeIntIsPosotiveAttribute(string hoursPropertyName)
        {
            this.hoursPropertyName = hoursPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var hoursProperty = validationContext.ObjectType.GetProperty(hoursPropertyName);

            if (hoursProperty == null)
            {
                return new ValidationResult($"Unknown property: {hoursPropertyName}");
            }
            int hours = (int)hoursProperty.GetValue(validationContext.ObjectInstance);
            int minutes = (int)value;

            if(hours == 0 &&  minutes == 0)
            {
                return new ValidationResult("Pole \"Czas trwania\" musi być większe od zera");
            }
            
            return ValidationResult.Success;
        }

    }
}
