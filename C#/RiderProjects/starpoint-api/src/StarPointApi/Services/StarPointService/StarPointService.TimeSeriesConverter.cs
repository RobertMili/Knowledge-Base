using System;
using System.Collections.Generic;
using System.Linq;
using StarPointApi.DTOs;
using StarPointApi.Repository.Models;
using StarPointApi.Shared;

namespace StarPointApi.Services.StarPointService
{
    public partial class StarPointService
    {
        public class TimeSeriesConverter
        {
            public static TimeSeriesDTO Convert(string userId, StarPointEntity[] starPoints, TimeFrame timeFrame,
                DateTime startDate, DateTime endDate)
            {
                if (starPoints == null) throw new ArgumentNullException(nameof(starPoints));

                var tempPoints = new List<StarPointEntity>();
                tempPoints.AddRange(starPoints);
                var result = new TimeSeriesDTO()
                {
                    UserId = userId,
                    TimeFrame = timeFrame
                };

                switch (timeFrame)
                {
                    case TimeFrame.Yearly:
                        Yearly();
                        break;
                    case TimeFrame.Monthly:
                        Monthly();
                        break;
                    case TimeFrame.Weekly:
                        Weekly();
                        break;
                    case TimeFrame.Daily:
                        Daily();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(timeFrame), timeFrame, null);
                }

                void Yearly()
                {
                    var fillers = startDate.AllYearsBetween(endDate)
                        .Select(x => new StarPointEntity {CreatedDate = x.Date});
                    tempPoints.AddRange(fillers);
                    result.StarPoints = tempPoints.GroupBy(x => x.CreatedDate.Year).Select(x => new StarPoint()
                    {
                        Date = DateTime.SpecifyKind(new DateTime(x.Key, 1, 1), DateTimeKind.Utc),
                        StarPoints = x.Sum(y => y.StarPoints)
                    });
                }

                void Monthly()
                {
                    var fillers = startDate.AllMonthsBetween(endDate)
                        .Select(x => new StarPointEntity {CreatedDate = x.Date});
                    tempPoints.AddRange(fillers);

                    result.StarPoints = tempPoints.GroupBy(x => new {x.CreatedDate.Year, x.CreatedDate.Month}).Select(
                        x =>
                            new StarPoint()
                            {
                                Date = DateTime.SpecifyKind(new DateTime(x.Key.Year, x.Key.Month, 1), DateTimeKind.Utc),
                                StarPoints = x.Sum(y => y.StarPoints)
                            });
                }

                void Weekly()
                {
                    var fillers = startDate.AllWeeksBetween(endDate)
                        .Select(x => new StarPointEntity {CreatedDate = x.Date});
                    tempPoints.AddRange(fillers);

                    result.StarPoints = tempPoints.GroupBy(x => x.CreatedDate.FirstDateOfWeek()).Select(x =>
                        new StarPoint
                        {
                            Date = DateTime.SpecifyKind(x.Key.Date, DateTimeKind.Utc),
                            StarPoints = x.Sum(y => y.StarPoints)
                        });
                }

                void Daily()
                {
                    var fillers = startDate.AllDaysBetween(endDate)
                        .Select(x => new StarPointEntity {CreatedDate = x.Date});
                    tempPoints.AddRange(fillers);

                    result.StarPoints = tempPoints.GroupBy(x => x.CreatedDate.Date).Select(x => new StarPoint
                    {
                        Date = DateTime.SpecifyKind(x.Key.Date, DateTimeKind.Utc),
                        StarPoints = x.Sum(y => y.StarPoints)
                    });
                }

                result.StarPoints = result.StarPoints.OrderBy(x => x.Date);
                return result;
            }
        }
    }
}