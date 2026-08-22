using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssistantDatabase.Model
{
    [Table("DoneTasks")]
    public class DoneTaskDM
    {
        public required int TaskId { get; set; }
        public required TaskDM Task { get; set; }

        [DataType(DataType.Date)]
        public required DateTime FinishDate { get; set; }

        [DataType(DataType.Time)]
        public required DateTime FinishTime { get; set; }
    }
}
