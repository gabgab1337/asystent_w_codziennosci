using System.Text.Json.Serialization;

namespace Assistant.Api.Contracts
{
    public class ErrorResponse
    {
        public ErrorResponse()
        {
        }

        public ErrorResponse(string error)
        {
            Error = error;
        }

        public string Error { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; set; }
    }
}
