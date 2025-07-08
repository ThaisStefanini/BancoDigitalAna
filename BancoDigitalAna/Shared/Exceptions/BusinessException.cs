using System.Net;

namespace BancoDigitalAna.Shared.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorType { get; }
        public HttpStatusCode StatusCode { get; }

        public BusinessException(string message, string errorType, HttpStatusCode statusCode)
            : base(message)
        {
            ErrorType = errorType;
            StatusCode = statusCode;
        }
    }
}
