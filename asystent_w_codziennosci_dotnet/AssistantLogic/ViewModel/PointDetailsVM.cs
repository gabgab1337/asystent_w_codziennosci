using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AssistantLogic.ViewModel
{
    public class PointDetailsVM
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

        public List<TriggerBaseInformationVM> TriggersToAdd { get; set; }

        public List<TriggerBaseInformationVM> Triggers { get; set; }
        public string? Image { get; set; }
    }
}
