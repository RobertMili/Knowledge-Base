using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using StarPointApi.DTOs;
using StarPointApi.Repository.Models;

namespace StarPointApi.Repository
{
    public class MockRepository : IRepository
    {
        private readonly List<StarPointEntity> _mockDB;
        private readonly List<StarPointDTO> _mockDTO;
        private readonly Random _rng = new Random();

        public MockRepository()
        {
            var rngActivity = new[] {"Walking", "Running", "Swimming", "Golf", "Tennis"};
            _mockDB = Enumerable.Range(1, 5).Select(i =>
            {
                return new StarPointEntity(
                    i.ToString(),
                    rngActivity[_rng.Next(0, rngActivity.Length)] + " from mock",
                    createdDate: DateTime.Now,
                    starPoints: _rng.Next(1, 1337),
                    sourceID: Guid.NewGuid().ToString()
                );
            }).ToList();
        }

        public async Task<TableResult> AddOrUpdateActivity(StarPointEntity starPointEntity)
        {
            await Task.Delay(1);
            _mockDB.Remove(_mockDB.FirstOrDefault(a =>
                a.PartitionKey == starPointEntity.PartitionKey && a.RowKey == starPointEntity.RowKey));

            _mockDB.Add(starPointEntity);
            var result = new TableResult
            {
                HttpStatusCode = 204,
                Result = starPointEntity
            };
            return result;
        }

        public async Task<IEnumerable<StarPointEntity>> GetActivitiesByUserID(string userID, DateTime from, DateTime to,
            string source = null)
        {
            await Task.Delay(1);

            return _mockDB.Where(x =>
                    x.UserID == userID
                    && x.CreatedDate >= from && x.CreatedDate <= to)
                .ToList();
        }

        public async Task<TableResult> GetActivityAsync(string userId, string rowKey)
        {
            await Task.Delay(1);
            var starPointEntity = _mockDB.Find(x => x.UserID == userId && x.RowKey == rowKey);
            if (starPointEntity is not null)
                return new TableResult {HttpStatusCode = 200, Result = starPointEntity};
            else
                return new TableResult {HttpStatusCode = 404, Result = starPointEntity};
        }

        public async Task<TableResult> RemoveActivity(string rowKey, string userId)
        {
            await Task.Delay(1);
            var deleted = _mockDB.Remove(_mockDB.FirstOrDefault(act => act.RowKey == rowKey));
            if (deleted)
                return new TableResult {HttpStatusCode = 204};
            return new TableResult {HttpStatusCode = 404};
        }
    }
}