using AssistantDatabase.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace AssistantLogic.Validators
{
    public class UsernameExists : ValidationAttribute
    {
        private readonly string userName;
        public UsernameExists(string userName)
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
            var userNameValue = userProperty.GetValue(validationContext.ObjectInstance)?.ToString();

            bool userExists = userRepository.ExistLogin(userNameValue);

            if (userExists)
            {
                return new ValidationResult($"Podana nazwa użytkownika '{userNameValue}' już istnieje");
            }

            // Jeśli nazwa użytkownika jest unikalna
            return ValidationResult.Success;
        }
    }
}
