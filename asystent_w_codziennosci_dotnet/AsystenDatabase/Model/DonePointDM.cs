using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssistantDatabase.Model
{
    [Table("DonePoints")]
    public class DonePointDM
    {
        public required int PointId { get; set; }
        public required TaskPointDM Point { get; set; }

        [DataType(DataType.Date)]
        public required DateTime FinishDate { get; set; }
    }
}
