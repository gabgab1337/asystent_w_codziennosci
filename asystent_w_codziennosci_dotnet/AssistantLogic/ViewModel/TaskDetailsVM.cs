using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AssistantLogic.ViewModel
{
    public class TaskDetailsVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [AllowNull]
        [StringLength(2000)]
        public string? Description { get; set; }

        public List<PointBaseInformationVM> Points { get; set; }

        public PointAddEditVM Point { get; set; }

        public List<TriggerBaseInformationVM> TriggersToAdd { get; set; }

        public List<TriggerBaseInformationVM> Triggers { get; set; }

    }
}
