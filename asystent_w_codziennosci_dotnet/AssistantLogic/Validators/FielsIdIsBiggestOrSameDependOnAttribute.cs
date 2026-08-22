using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    public class FielsIdIsBiggestOrSameDependOnAttribute : ValidationAttribute
    {
        private readonly string propertyDescription;
        private readonly string lessPropertyName;
        private readonly string lessPropertyDescription;

        private readonly string dependOnEnumPropertyName;
        private readonly object dependOnEnumValue;
        public FielsIdIsBiggestOrSameDependOnAttribute(string propertyDescription, string lessPropertyName, string lessPropertyDescription, string dependOnEnumPropertyName, object dependOnEnumValue)
        {
            this.propertyDescription = propertyDescription;
            this.lessPropertyName = lessPropertyName;
            this.lessPropertyDescription = lessPropertyDescription;
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

            if (dependOnEnumValue.Equals(dependOnEnum))
            {
                var lessProperty = validationContext.ObjectType.GetProperty(lessPropertyName);

                if (lessProperty == null)
                {
                    return new ValidationResult($"Unknown property: {lessPropertyName}");
                }

                var less = lessProperty.GetValue(validationContext.ObjectInstance);
                if (less!=null && less is IComparable && value != null && value is IComparable)
                {
                    IComparable comparableLess = (IComparable)less;
                    IComparable comparableValue = (IComparable)value;
                    if(comparableLess.CompareTo(comparableValue)>0)
                    {
                        return new ValidationResult($"Pole \"{propertyDescription}\" musi być większe lub równe niż pole  \"{lessPropertyDescription}\".");
                    }
                }
            }


            return ValidationResult.Success;

        }
    }
}
