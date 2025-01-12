using System;
using BoostApp.Shared;
using LeaderboardApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Azure.Core;
using System.Net.Http.Headers;

namespace LeaderboardApi.Services
{
    public class HttpClientService : IHttpClientService
    {
        // The client object used to send requests
        private readonly ConfidentialHttpClient _client;
        // Logger that logs eror messages
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(IHttpClientFactory httpClientFactory, IOptions<AzureAdConfig> config, ILogger<HttpClientService> logger)
        {
            _logger = logger;

            // Check that the dependencies are not null
            if (httpClientFactory == null) throw new ArgumentNullException(paramName: nameof(httpClientFactory));
            if (config == null) throw new ArgumentNullException(paramName: nameof(config));
            var adConfig = config.Value;

            // Create a new HttpClient 
            var httpClient = httpClientFactory.CreateClient();

            // Create a new ConfidentialHttpClient with the specified Azure AD configuration, HttpClient, and scope
            try
            {
                _client = new ConfidentialHttpClient(
                    config: adConfig,
                    httpClient: httpClient,
                    scope: @"api://a38408ce-d91c-4010-9204-448b813e0e00"); // This is the scope where you want to make requests for, atm this is the stapoint API scope
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating ConfidentialHttpClient object: {ex.Message}");
                throw new Exception($"Error creating ConfidentialHttpClient object: {ex.Message}");
            }
        }

        // Return the ConfidentialHttpClient object
        public ConfidentialHttpClient GetHttpClient()
        {
            try
            {
                return _client;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting HttpClient object: {ex.Message}");
                throw new Exception($"Error getting HttpClient object: {ex.Message}");
            }
        }
    }
}



