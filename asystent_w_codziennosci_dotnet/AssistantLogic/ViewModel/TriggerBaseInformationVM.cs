using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AssistantDatabase.Model;

namespace AssistantLogic.ViewModel
{
    public class TriggerBaseInformationVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public TriggerType Type { get; set; }
        public string SubType { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public bool CanDelete { get; set; }
    }
}
