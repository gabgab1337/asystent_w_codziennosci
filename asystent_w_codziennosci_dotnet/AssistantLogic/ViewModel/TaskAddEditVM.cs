using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using AssistantLogic.Validators;

namespace AssistantLogic.ViewModel
{
    public class TaskAddEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pole \"Nazwa zadania\" jest wymagane")]
        [StringLength(100)]
        public string Name { get; set; }

        [AllowNull]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public bool AnchorBegin { get; set; }

        [TimeBeginIsRequired("AnchorBegin")]
        public string? TimeBegin { get; set; }
                
        [Required]
        public bool AnchorEnd { get; set; }
        
        [TimePanedIsRequired("AnchorEnd")]
        public string? TimeEnd { get; set; }

        public string? Time { get; set; }

        [Required(ErrorMessage = "Pole \"Czas trwania: godz\" jest wymagane")]
        [Range(0, int.MaxValue, ErrorMessage = "Pole \"Czas trwania: godz\" nie może być ujemne")]
        public int Hours { get; set; }

        [TimeIntIsPosotive("Hours")]
        [Required(ErrorMessage = "Pole \"Czas trwania: min\" jest wymagane")]
        [Range(0, 59, ErrorMessage = "Pole \"Czas trwania: min\" musi być w przedziale [0,59]")]
        public int Minutes { get; set; }
    }
}
