using System.ComponentModel.DataAnnotations;

namespace Assistant.Api.Contracts
{
    public class CreateProtegeRequest
    {
        [Required(ErrorMessage = "Pole \"Login\" jest wymagane")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pole \"Hasło\" jest wymagane")]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
