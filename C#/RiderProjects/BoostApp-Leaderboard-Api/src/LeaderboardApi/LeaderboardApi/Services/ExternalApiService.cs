using System;
using System.Text.Json;
using LeaderboardApi.Models;
using LeaderboardApi.Services.Interfaces;

namespace LeaderboardApi.Services
{
	public class ExternalApiService : IExternalApiService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<ExternalApiService> _logger;


        public ExternalApiService()
        {
        }

        public ExternalApiService(IHttpClientService httpClientService, ILogger<ExternalApiService> logger)
        {
            _httpClientService = httpClientService;
            _logger = logger;
        }

        // Makes a request to competitionAPI, returns the responseObject so you can use whatever value you need. 
        public async Task<CompetitionResponseModel> GetCompetition(int competitionId)
        {
            try
            {
                // Construct the endpoint URL with the specified competitionId
                var competitionEndpoint = $"https://boostappcompetitionapi.azurewebsites.net/competition/{competitionId}";

                // Send the HTTP GET request to the endpoint URL
                var httpClient = _httpClientService.GetHttpClient();
                var response = await httpClient.GetAsync(competitionEndpoint);

                // Check the response status code
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error getting competition with ID {competitionId}, status code:{response.StatusCode}");
                    return null;
                }

                // Deserialize the response JSON into a C# object
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var responseJson = await response.Content.ReadAsStringAsync();
                if (responseJson == null)
                {
                    _logger.LogError($"Error getting competition with ID {competitionId}: response JSON is null");
                    return null;
                }
                var responseObject = JsonSerializer.Deserialize<CompetitionResponseModel>(responseJson, options);

                // Return the responseObject
                return responseObject;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting competition with ID {competitionId}, {ex.Message}");
                return null;
            }
        }


        // Make a request to starpointAPI to be used in teamMemberMessageService to check if a teamMember have som existing starpoints when posting it to a team
        public async Task<int> GetExistingStarpointsAsync(DateTime startDate, string userID)
        {
            try
            {
                // Construct the endpoint URL with the specified start date and user ID
                var starpointEndpoint = $"https://starpoint.azurewebsites.net/StarPoint/User/Total/{startDate:yyyy-MM-dd}?userID={userID}";

                // Send the HTTP GET request to the endpoint URL
                var httpClient = _httpClientService.GetHttpClient();
                var response = await httpClient.GetAsync(starpointEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error getting Starpoints with startDate: {startDate} and userId:{userID}, status code: {response.StatusCode}");
                    throw new Exception($"Error getting Starpoints with startDate: {startDate} and userId:{userID}, status code: {response.StatusCode}");
                }

                // Deserialize the response JSON into a C# object
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var responseJson = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseJson))
                {
                    // Return 0 to indicate that there are no existing starpoints
                    return 0;
                }
                var responseObject = JsonSerializer.Deserialize<StarpointResponseModel>(responseJson, options);

                // Extract and return the totalStarPoints value
                return responseObject.TotalStarPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting Starpoints with startDate: {startDate} and userId:{userID}, {ex.Message}");
                throw new Exception($"Error getting existing starpoints with startDate: {startDate} and userId:{userID}");
            }
        }

    }
}

