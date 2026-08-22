using System.ComponentModel.DataAnnotations.Schema;

namespace AssistantDatabase.Model
{
    [Table("TriggerTasks")]
    public class TriggerTaskDM
    {
        public int TaskId { get; set; }
        public required TaskDM Task { get; set; }

        public int TriggerId { get; set; }
        public required TriggerDM Trigger { get; set; } //public required ważne !! on mówi że not null
        public bool Negation { get; set; }

    }
}
