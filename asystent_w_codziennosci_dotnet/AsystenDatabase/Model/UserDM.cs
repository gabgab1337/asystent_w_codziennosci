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

        /// <summary>
        /// Task this ASD person is doing right now. Replaces the MVC session value.
        /// Null means free time. Do not store the old sentinel id -1.
        /// </summary>
        [AllowNull]
        public int? CurrentTaskId { get; set; }

        [AllowNull]
        public string? ColorsPalete { get; set; }


        [Required]
        public DateTime JoiningDate { get; set; } = DateTime.Now;
    }
}
