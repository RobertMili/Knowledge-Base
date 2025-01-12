using Sigma.BoostApp.Shared;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MembershipAPI.Services.UserInfo
{
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Logging;
    using Models;
    using static MsGraphProperties.Users;

    public class UserInfoService : IUserInfoService
    {
        public const string USER_AGENT
            = "UserInfoService/1.0";

        private const StringComparison IGNORE_CASE
            = StringComparison.OrdinalIgnoreCase;

        private readonly ConfidentialHttpClient _client;

        private readonly ILogger<UserInfoService> _logger;

        private readonly IDistributedCache _cache;

        // Dont forget to register AzureAdConfig Options!
        public UserInfoService(HttpClient httpClient, IOptions<AzureAdConfig> config, IDistributedCache cache, ILogger<UserInfoService> logger)
        {
            _cache = cache;
            _logger = logger;

            if (httpClient == null) throw new ArgumentNullException(paramName: nameof(httpClient));

            var adConfig = config.Value;

            // Doc: https://docs.microsoft.com/en-us/graph/api/user-get?view=graph-rest-1.0&tabs=http
            // Doc: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#typed-clients-1
            // SOf: https://stackoverflow.com/questions/23438416/why-is-httpclient-baseaddress-not-working
            httpClient.BaseAddress = new Uri($@"https://graph.microsoft.com/v1.0/users/"); // <-- MUST HAVE TRAILING '/' SLASH !!!
            httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            //httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _client =
                new ConfidentialHttpClient(
                    config: adConfig,
                    httpClient: httpClient,
                    scope: @"https://graph.microsoft.com/.default");
        }

        private async Task<(int Width, int Height)> RequestDefaultImageMeta(string userId)
        {
            const string PHOTO_NO_SIZE_META = @"/photo";

            using (var response = await _client.GetAsync(userId + PHOTO_NO_SIZE_META))
            {
                var data = await response.Content.ReadAsStringAsync();
                var meta = (JObject) JsonConvert.DeserializeObject(data);
                return (Width: meta["width"].Value<int>(), Height: meta["height"].Value<int>());
            }
        }

        private async Task<PhotoSize> GetDefaultImageSize(string userId)
        {
            var meta = await RequestDefaultImageMeta(userId);
            return new PhotoSize(width: meta.Width, height: meta.Height);
        }

        public async Task<Photo?> GetPhotoAsync(
            string userId,
            PhotoSize size = null,
            PhotoResizeCondition resizeCondition = PhotoResizeCondition.ResizeIfDefault,
            int resizeQuality = 85)
        {
            // Absolute hard limit of fallbacks & retries - we dont want to risk getting stuck in infinite calls to ms graph!
            const int MAX_ATTEMPTS = 25;

            // Maximum number of retries from statuses that are deemed retry-worthy - and associated delay.
            const int MAX_RETRIES = 2;
            const int RETRY_DELAY = 250;

            // Endpoint formats:
            const string PHOTO_NO_SIZE = @"/photo/$value";
            const string PHOTO_WITH_SIZE = @"/photos/{0}/$value";

            int retries = 0;
            var preferredSize = size;
            HttpStatusCode statusCode = default;
            try
            {
                for (int attempts = 0; attempts < MAX_ATTEMPTS; ++attempts)
                {
                    var endpoint = size == null || attempts == (MAX_ATTEMPTS - 1)
                        // if default size OR if on our last attempt; go for default size.
                        ? PHOTO_NO_SIZE
                        : string.Format(PHOTO_WITH_SIZE, size);

                    using (HttpResponseMessage response = await _client.GetAsync(userId + endpoint))
                    {
                        statusCode = response.StatusCode;

                        if (response.IsSuccessStatusCode)
                        {
                            string mimeType = response.Content.Headers.ContentType.MediaType;
                            var bytes = await response.Content.ReadAsByteArrayAsync();
                            
                            bool isDefaultSize = size == null;
                            
                            if (isDefaultSize)
                                size = await GetDefaultImageSize(userId); // Fetch size information for the default photo

                            bool isSizeOK =
                                resizeCondition == PhotoResizeCondition.NoResizeAcceptAny ||
                                preferredSize == null ||
                                preferredSize.Equals(size) ||
                                (resizeCondition == PhotoResizeCondition.ResizeIfDefault && !isDefaultSize);
                            
                            return isSizeOK
                                ? new Photo(bytes: bytes, size: size, mimeType: mimeType)
                                : PhotoExtensions.ResizeToJpeg(bytes, preferredSize);
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            if (size == null)
                                return null; // User has no photo period.
                            size = size.Fallback;
                            if (size == null && resizeCondition == PhotoResizeCondition.NoResizeNoDefault) // strict no-resize exit
                                return null;
                        }
                        else if (retries >= MAX_RETRIES || !IsRetryResponse(response.StatusCode))
                        {
                            break; // ...to the throw...
                        }
                        else
                        {
                            ++retries;
                            await Task.Delay(RETRY_DELAY);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: log here?
                throw new FailedToAquireUserInfoException(message:
                    $"Failed to get photo info from MSGraph with response {statusCode}."
                    , innerException: e, statusCode: statusCode);
            }
            // TODO: log here?
            throw new FailedToAquireUserInfoException(message:
                $"Failed to get photo info from MSGraph with response {statusCode}."
                , statusCode: statusCode);
        }

        public async Task<Name> GetNameAsync(string userId, bool silentlyFail)
        {
            var cacheKey = $"User-{userId}";
            _logger.LogDebug("Look for cached name: " + cacheKey);
            var cachedName = await _cache.GetStringAsync(cacheKey);
            if(cachedName != null)
            {
                _logger.LogDebug("Found cached name: " + cachedName);
                return new Name(displayName: cachedName, givenName: "", surName: "");
            }

            const int MAX_ATTEMPTS = 2;
            const int RETRY_DELAY = 250;
            const string SELECT_PROPERTIES = "?$select=" + GivenName + "," + SurName + "," + DisplayName;

            HttpStatusCode statusCode = default;
            try
            {
                // TODO: caching layer here (non-persistent as to appease the GDPR gods!)

                for (int attempt = 1; attempt <= MAX_ATTEMPTS; ++attempt)
                {
                    _logger.LogDebug("Try get {userId}", userId);
                    using (HttpResponseMessage response = await _client.GetAsync(userId + SELECT_PROPERTIES))
                    {
                        statusCode = response.StatusCode;

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            JObject result = JsonConvert.DeserializeObject(json) as JObject;

                            bool hasGivName = result.TryGetValue(GivenName, IGNORE_CASE, out var givenName);
                            bool hasSurName = result.TryGetValue(SurName, IGNORE_CASE, out var surName);
                            bool hasDisName = result.TryGetValue(DisplayName, IGNORE_CASE, out var displayName);
                            // TODO: log here?

                            if (hasGivName | hasSurName | hasDisName)
                            {
                                var name = new Name(
                                    givenName: givenName.ToString(),
                                    surName: surName.ToString(),
                                    displayName: displayName.ToString()
                                    );
                                await _cache.SetStringAsync(cacheKey, name.ToString());
                                return name;
                            }
                            else if (silentlyFail)
                                return default;
                            else
                                throw new FailedToAquireUserInfoException(
                                    "MSGraph response didn't contain the any of the requested properties."
                                    );
                        }

                        else if (!IsRetryResponse(response.StatusCode))
                            break;
                    }
                    await Task.Delay(RETRY_DELAY);
                }
            }
            catch (Exception e)
            {
                // TODO: log here?
                if (silentlyFail) return default;
                throw new FailedToAquireUserInfoException(message:
                    $"Failed to get user info from MSGraph with response {statusCode}."
                    , innerException: e, statusCode: statusCode);
            }
            // TODO: log here?
            if (silentlyFail) return default;
            throw new FailedToAquireUserInfoException(message:
                $"Failed to get user info from MSGraph with response {statusCode}."
                , statusCode: statusCode);
        }

        // responses where we will do a retry...
        private static bool IsRetryResponse(HttpStatusCode httpStatusCode)
            => httpStatusCode == HttpStatusCode.Processing
            || httpStatusCode == HttpStatusCode.TooManyRequests
            || httpStatusCode == HttpStatusCode.ServiceUnavailable
            || httpStatusCode == HttpStatusCode.InternalServerError;
    }
}
