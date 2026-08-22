using AssistantDatabase.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    public class UpdateUsernameExists : ValidationAttribute
    {
        private readonly string userName;
        public UpdateUsernameExists(string userName) 
        {
            this.userName = userName;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Pobierz repozytorium użytkowników z kontekstu walidacji
            var userRepository = (IUserRepository)validationContext.GetService(typeof(IUserRepository));

            if (userRepository == null)
            {
                throw new InvalidOperationException("Nie można znaleźć IUserRepository w kontekście walidacji.");
            }

            // Pobierz właściwość nazwy użytkownika
            var userProperty = validationContext.ObjectType.GetProperty(userName);
            if (userProperty == null)
            {
                return new ValidationResult($"Nie znaleziono właściwości '{userName}' w obiekcie walidowanym.");
            }

            // Pobierz wartość tej właściwości
            var newUserName = userProperty.GetValue(validationContext.ObjectInstance)?.ToString();
            var oldUserNameProperty = validationContext.ObjectType.GetProperty("UsernameBefore");
            if (oldUserNameProperty == null)
            {
                return new ValidationResult("Nie znaleziono właściwości 'UsernameBefore' w obiekcie walidowanym.");
            }
            var oldUserName = oldUserNameProperty.GetValue(validationContext.ObjectInstance)?.ToString();
            bool userExists = userRepository.ExistLogin(newUserName);

            if (userExists && newUserName != oldUserName)
            {
                return new ValidationResult($"Podana nazwa użytkownika '{newUserName}' już istnieje");
            }

            // Jeśli nazwa użytkownika jest unikalna
            return ValidationResult.Success;
        }
    }
}
