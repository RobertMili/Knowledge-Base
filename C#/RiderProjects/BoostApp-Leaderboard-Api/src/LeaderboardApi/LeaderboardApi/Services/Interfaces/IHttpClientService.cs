using System;
using BoostApp.Shared;

namespace LeaderboardApi.Services.Interfaces
{
	public interface IHttpClientService
	{
        public ConfidentialHttpClient GetHttpClient(); 
    }
}

