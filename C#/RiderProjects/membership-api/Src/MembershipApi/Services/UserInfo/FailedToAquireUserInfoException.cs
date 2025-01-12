using System;
using System.Net;

namespace MembershipAPI.Services.UserInfo
{
    public class FailedToAquireUserInfoException: Exception
    {
        const string DEFAULT_MESSAGE = nameof(FailedToAquireUserInfoException);

        public FailedToAquireUserInfoException (string message = DEFAULT_MESSAGE, HttpStatusCode statusCode = default, Exception innerException = null)
            : base(message: message ?? DEFAULT_MESSAGE, innerException: innerException)
        {
            HttpStatusCode = statusCode;
        }

        public HttpStatusCode HttpStatusCode { get; }
    }
}
