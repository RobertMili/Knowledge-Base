using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BoostApp.Shared
{
    // Inherit from or compose with this to make usefull stuff
    // Doc: https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-credential-flows
    public class ConfidentialHttpClient
    {
        public string Scope { get; }

        protected IEnumerable<string> Scopes { get; }

        protected IConfidentialClientApplication ClientApp { get; }

        protected HttpClient HttpClient { get; }

        public ConfidentialHttpClient(IConfiguration appConfig, HttpClient httpClient, string scope = null)
            : this(AzureAdConfig.FromAppConfig(appConfig), httpClient, scope)
        { }

        /// <summary>
        /// Creates a ConfidentialClientService for making requests that require ClientCredentials Flow authentication.
        /// </summary>
        /// <param name="config">AzureAdConfig</param>
        /// <param name="httpClient">HttpClient for calling web api</param>
        /// <param name="scope">The scope to request auth for, or null to use the <paramref name="httpClient"/>.BaseAddress + ".default" as scope</param>
        /// <remarks>
        /// Dont forget to configure the <paramref name="httpClient"/> BEFORE sending it into this ctor!
        /// For example to user MsGraph the BaseAddress should be "https://graph.microsoft.com/v1.0/".
        /// </remarks>
        public ConfidentialHttpClient(AzureAdConfig config, HttpClient httpClient, string scope = null)
        {
            if (config == null) throw new ArgumentNullException(paramName: nameof(config));
            HttpClient = httpClient ?? throw new ArgumentNullException(paramName: nameof(httpClient));

            // Scope in a Client Credentials Flow must be of the shape "resource/.default"
            // Actual permissions are set statically in Azure Portal (by a tenant admin)
            scope = scope ??
                (httpClient.BaseAddress.IsAbsoluteUri
                ? httpClient.BaseAddress.AbsoluteUri
                : httpClient.BaseAddress.ToString()
                );
            Scope = !scope.EndsWith(".default", StringComparison.OrdinalIgnoreCase)
                ? scope + (scope.EndsWith('/') ? ".default" : "/.default")
                : scope;

            Scopes = new[] { Scope };

            var appBuilder = ConfidentialClientApplicationBuilder
                .Create(config.ClientId)
                .WithAuthority(new Uri(config.Authority));

            appBuilder = config.IsUsingClientSecret
                    ? appBuilder.WithClientSecret(config.ClientSecret)
                    : appBuilder.WithCertificate(X509CertHelper.FromCurrentUser(config.CertificateName));

            ClientApp = appBuilder.Build();
        }

        public Uri BaseAddress => HttpClient.BaseAddress;

        protected async Task<AuthenticationResult> AquireAuthenticationAsync(bool forceRefresh = false)
        {
            try
            {
                /* REMARKS:
                 * AcquireTokenForClient uses the application token cache (not the
                 * user token cache) Don't call AcquireTokenSilent before calling
                 * AcquireTokenForClient as AcquireTokenSilent uses the user token
                 * cache. AcquireTokenForClient checks the application token cache
                 * itself and updates it.
                 */
                return await ClientApp
                    .AcquireTokenForClient(Scopes)
                    .WithForceRefresh(forceRefresh) // bypass cache and request a new token from authorty?
                    .ExecuteAsync();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw new MsalServiceException(ex.ErrorCode, "Scope provided is not supported - Ensure the resource URI was configured correctly.");
            }
        }

        public async Task<HttpResponseMessage> GetAsync(Uri relativeRequestUri)
            => await GetAsync(relativeRequestUri?.ToString());

        public async Task<HttpResponseMessage> GetAsync(string relativeRequestUri = null)
        {
            AuthenticationResult auth = await AquireAuthenticationAsync();
            return await GetAsync(httpClient: HttpClient, accessToken: auth.AccessToken, resourceUri: relativeRequestUri);
        }

        // Tries to add the specified mediaType to the Accept value collection, and adds the accessToken as a bearer Authorization header.
        protected static Task<HttpResponseMessage> GetAsync(HttpClient httpClient, string accessToken, string resourceUri = null, string mediaType = null)
        {
            if (httpClient == null)
                throw new ArgumentNullException(paramName: nameof(httpClient));
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("AccessToken not supplied.", paramName: nameof(accessToken));

            var requetHeaders = httpClient.DefaultRequestHeaders;
            requetHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

            if (mediaType != null)
            {
                if (requetHeaders.Accept == null)
                    requetHeaders.Add("Accept", mediaType);
                else if (!requetHeaders.Accept.Any(m => m.MediaType == mediaType))
                    requetHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            }

            return httpClient.GetAsync(requestUri: resourceUri ?? string.Empty);
        }
    }
}
