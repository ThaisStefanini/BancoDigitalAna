namespace BancoDigitalAna.Shared.Helpers
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string ErrorType { get; set; }

        public ErrorResponse()
        {
            Message = "";
            ErrorType = "";
        }

        public ErrorResponse(string message, string errorType)
        {
            Message = message;
            ErrorType = errorType;
        }
    }
}
