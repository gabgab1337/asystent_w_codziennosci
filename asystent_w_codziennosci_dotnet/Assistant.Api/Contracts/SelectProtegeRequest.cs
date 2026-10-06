using System.ComponentModel.DataAnnotations;

namespace Assistant.Api.Contracts
{
    public class SelectProtegeRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Pole \"protegeId\" jest wymagane")]
        public int ProtegeId { get; set; }
    }
}
