using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AssistantLogic.ViewModel
{
    public class PlanDayTaskVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [AllowNull]
        [StringLength(1000)]
        public string? Description { get; set; }


        [Required]
        public bool AnchorBegin { get; set; }

        public string TimeBegin { get; set; }

        public int Time { get; set; }

        [Required]
        public bool AnchorEnd { get; set; }

        public string TimeEnd { get; set; }

        public int TimeBuffor { get; set; }

        public List<PlanDayPointVM> Points { get; set; }

        public string? Message { get; set; } = string.Empty;

        public bool MessagePositiv { get; set; }

        public RealizationState State { get; set; }
    }
}
