using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AssistantDatabase.Model
{
    [Table("Tasks")]
    public class TaskDM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [AllowNull]
        [StringLength(1000)]
        public string? Description { get; set; }

        public List<TaskPointDM> Points { get; set; } = new List<TaskPointDM>();

        [Required]
        public bool AnchorBegin { get; set; }

        public int? TimeBegin { get; set; }

        public int Time { get; set; }

        [Required]
        public bool AnchorEnd { get; set; }

        public int? TimeEnd { get; set; }

        [Required]
        public UserDM Caregiver { get; set; }

        [Required]
        public UserDM AsdPerson { get; set; }

        public List<TriggerTaskDM> TriggerTasks { get; set; } = new List<TriggerTaskDM>();

        [Required]
        public int Number { get; set; }

        public List<DoneTaskDM> Done { get; set; } = new List<DoneTaskDM>();

    }
}
