using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AssistantLogic.ViewModel
{
    public class PlanDayPointVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        [AllowNull]
        [StringLength(2000)]
        public string Description { get; set; }

        public RealizationState State { get; set; }
    }
}
