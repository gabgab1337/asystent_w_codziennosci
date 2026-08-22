using System.ComponentModel.DataAnnotations.Schema;

namespace AssistantDatabase.Model
{
    [Table("TriggerPoints")]
    public class TriggerPointDM
    {
        public int PointId { get; set; }
        public required TaskPointDM Point { get; set; }

        public int TriggerId { get; set; }
        public required TriggerDM Trigger { get; set; }

        public bool Negation { get; set; } 
    }
}
