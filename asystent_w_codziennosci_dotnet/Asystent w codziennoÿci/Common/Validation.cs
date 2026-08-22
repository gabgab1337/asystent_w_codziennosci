using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace AsystentView.Common
{
    public class Validation
    {
        public static string GetValidationClass(ModelStateDictionary modelState, string fieldName)
        {
            var isInvalid = modelState[fieldName]?.Errors.Any() == true;
            var isValid = modelState[fieldName]?.Errors.Any() == false && modelState[fieldName] != null;
            var inputClass = "form-control custom-width";
            if (isInvalid)
            {
                inputClass += " is-invalid";
            }
            else if (isValid)
            {
                inputClass += " is-valid";
            }
            return inputClass;
        }
    }
    
}
