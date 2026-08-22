using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AssistantDatabase.Model
{
    [Table("Users")]
    public class UserDM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public UserType Type { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [AllowNull]
        public int? CaregiverId { get; set; }

        [AllowNull]
        public int? SelectedASD { get; set; }

        [AllowNull]
        public string? ColorsPalete { get; set; }


        [Required]
        public DateTime JoiningDate { get; set; } = DateTime.Now;
    }
}
