using AssistantDatabase.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssistantLogic.ViewModel
{
    public class UserAddEditVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        public UserType Type { get; set; }

        [NotMapped]
        public List<SelectListItem> Types { get; set; }

        public bool NameExists { get; set; } = false;
    }
}
