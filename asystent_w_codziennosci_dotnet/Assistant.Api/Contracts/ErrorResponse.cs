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
    }
}
