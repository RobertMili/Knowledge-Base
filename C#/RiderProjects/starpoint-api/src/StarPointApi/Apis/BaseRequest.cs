using Collector.Common.RestContracts;
using Collector.Common.RestContracts.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StarPointApi.Apis
{
    // This is an abstract base class for making RESTful API requests. It inherits from RequestBase and implements
    // ISuccessfulResponseParser and IErrorResponseParser. It is a generic class that takes two type parameters: 
    // TResourceIdentifier and TResponse, which must both implement certain interfaces. 
    public abstract class BaseRequest<TResourceIdentifier, TResponse>
        : RequestBase<TResourceIdentifier, TResponse>,
            ISuccessfulResponseParser<TResponse>,
            IErrorResponseParser
        where TResourceIdentifier : class, IResourceIdentifier
        where TResponse : class
    {
        protected BaseRequest(TResourceIdentifier resourceIdentifier)
            : base(resourceIdentifier)
        {
        }

        // This method returns the namespace of the current class.
        public override string GetConfigurationKey() => GetType().Namespace;

        public Error ParseError(string content)
        {
            // Plain JSON parser
            //var myError = JsonConvert.DeserializeObject<MyApiErrorResponse>(content);

            return new Error(
                "", //myError.ErrorCode.ToString(),
                content //myError.ErrorMessage
            );
        }

        // Method that parses a successful response from the API server
        public TResponse ParseResponse(string content)
        {
            return JsonConvert.DeserializeObject<TResponse>(content , new JsonSerializerSettings(){ContractResolver = new CamelCasePropertyNamesContractResolver()});
        }

    }
}