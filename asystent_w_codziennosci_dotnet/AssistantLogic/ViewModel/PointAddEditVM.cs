using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace AssistantLogic.ViewModel
{
    public class PointAddEditVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        [AllowNull]
        [StringLength(2000)]
        public string Description { get; set; }

        [AllowNull]
        [StringLength(8000)]
        public string? PhotoUrl { get; set; }

        public string? Image { get; set; }

        [AllowNull]
        public IFormFile? ImageFileUpload { get; set; }

    }
}
