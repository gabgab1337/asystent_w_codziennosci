using System.ComponentModel.DataAnnotations;

namespace Assistant.Api.Contracts
{
    public class UpdateProtegeRequest
    {
        [Required(ErrorMessage = "Pole \"Login\" jest wymagane")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// When omitted or blank, the current password hash is left unchanged.
        /// </summary>
        [StringLength(100)]
        public string? Password { get; set; }
    }
}
