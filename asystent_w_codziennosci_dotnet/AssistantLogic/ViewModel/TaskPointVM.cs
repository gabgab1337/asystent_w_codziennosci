using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using AssistantDatabase.Model;

namespace AssistantLogic.ViewModel
{
    public class TaskPointVM
    {
        public TaskPointVM() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [AllowNull]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public int MinTime { get; set; }

        [Required]
        public int AvgTime { get; set; }

        [Required]
        PointType Type { get; set; }

        [Required]
        bool IsDone { get; set; }
    }
}
