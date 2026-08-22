using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistantDatabase.Model
{
    [Table("ErrorLog")]
    public class ErrorDM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;
    }
}
