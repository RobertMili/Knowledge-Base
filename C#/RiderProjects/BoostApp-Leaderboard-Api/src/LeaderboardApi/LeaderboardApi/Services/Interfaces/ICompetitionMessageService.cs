using BoostApp.ClassLibrary;
using LeaderboardApi.Services.Interfaces;

public interface ICompetitionMessageService : IMessageService
{
    Task<bool> HandleMessageAsync(LeaderboardMessageModel message);
    Task UpdateEndDateForTeamsAsync(int competitionId, DateTime newEndDate);
}
