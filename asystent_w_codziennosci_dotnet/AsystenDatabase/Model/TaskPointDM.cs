using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AssistantDatabase.Model
{
    [Table("TaskPoints")]
    public class TaskPointDM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TaskId { get; set; }
        
        [Required]
        public TaskDM Task { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        [StringLength(300)]
        public string Name{ get; set; }

        [AllowNull]
        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public int MinTime { get; set; }

        [Required]
        public int AvgTime { get; set; }

        public List<TriggerPointDM> TriggerPoints { get; set; } = new List<TriggerPointDM>();

        public List<DonePointDM> Done { get; set; } = new List<DonePointDM>();

        [AllowNull]
        [StringLength(8000)]
        public string? PhotoUrl { get; set; }
        //[Required]
        //PointType Type { get; set; }

        //[Required]
        //bool IsDone { get; set; }

    }
}
