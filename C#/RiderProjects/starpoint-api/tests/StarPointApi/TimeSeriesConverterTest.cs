// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Microsoft.CodeAnalysis.CSharp;
// using StarPointApi.DTOs;
// using StarPointApi.Repository;
// using StarPointApi.Services.StarPointService;
// using Xunit;

// namespace StarPointApiTests
// {
//     public class TimeSeriesConverterTest
//     {
//         [Fact]
//         public async void Yearly_TimeSeries()
//         {
//             var SUT = new StarPointService(new MockRepository());
//             foreach (var item in Data())
//                 await SUT.PostOrEditUserActivity(item);

//             var result = await SUT.GetTimeSeriesByUserIdAsync("test", new DateTime(2019, 1, 1),
//                 new DateTime(2020, 12, 12), TimeFrame.Yearly);
//             Assert.Equal(15, result.StarPoints.Sum(x => x.StarPoints));
//             Assert.Equal(2, result.StarPoints.Count());
//         }
//         public async void Monthly_TimeSeries()
//         {
//             var SUT = new StarPointService(new MockRepository());
//             foreach (var item in Data())
//                 await SUT.PostOrEditUserActivity(item);

//             var result = await SUT.GetTimeSeriesByUserIdAsync("test", new DateTime(2019, 1, 1),
//                 new DateTime(2020, 12, 12), TimeFrame.Yearly);
//             Assert.Equal(15, result.StarPoints.Sum(x => x.StarPoints));
//             Assert.Equal(2, result.StarPoints.Count());
//         }


//         private static PostActivityDTO[] Data()
//             => new PostActivityDTO[]
//             {
//                 MockEntry(new DateTime(2019, 12, 31, 1,1,1), 1),
//                 MockEntry(new DateTime(2020, 1, 1, 1,1,1), 2),
//                 MockEntry(new DateTime(2020, 1, 1, 1,1,2), 3),
//                 MockEntry(new DateTime(2020, 1, 1, 1,1,2), 4),
//                 MockEntry(new DateTime(2020, 2, 1, 1,1,2), 5)
//             };

//         private static PostActivityDTO MockEntry(DateTime startDate, int points)
//         {
//             var dto = new PostActivityDTO {StartDate = startDate, StarPoints = points, DatabaseId = "1",Activity = "1",Description = "1",EndDate = startDate.AddSeconds(1), UserId = "test"};
//             return dto;
//         }
//     }
// }
