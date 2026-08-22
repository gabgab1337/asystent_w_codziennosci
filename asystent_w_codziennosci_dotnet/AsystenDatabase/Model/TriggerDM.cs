using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AssistantDatabase.Model
{
    [Table("Trigers")]
    public class TriggerDM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public UserDM Caregiver { get; set; }

        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public TriggerType Type { get; set; }
        
        [Required]
        public int SubType { get; set; }
        public string? Data { get; set; }

        public ICollection<TriggerTaskDM> TriggerTasks { get; set; } = new List<TriggerTaskDM>();

        public ICollection<TriggerPointDM> TriggerPoints { get; set; } = new List<TriggerPointDM>();

        [NotMapped]
        public bool Negation { get; set; }


    }
}
