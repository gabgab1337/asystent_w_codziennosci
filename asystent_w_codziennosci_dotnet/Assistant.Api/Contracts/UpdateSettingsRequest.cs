using System.ComponentModel.DataAnnotations;

namespace Assistant.Api.Contracts
{
    /// <summary>
    /// Body of <c>PUT /api/v1/me/settings</c>. Mirrors the MVC <c>SelectColor</c> form,
    /// which posts one palette name (White, Blue, Green, and the rest of the picker).
    /// </summary>
    public class UpdateSettingsRequest
    {
        [Required(ErrorMessage = "Pole \"Paleta kolorów\" jest wymagane")]
        [StringLength(50)]
        public string ColorPalette { get; set; } = string.Empty;
    }
}
