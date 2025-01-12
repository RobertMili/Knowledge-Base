using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StarPointApi.DTOs;
using StarPointApiTests.Helpers;
using Xunit;

namespace StarPointApiTests
{
    public class PutStarpointsDtoTest
    {
        public static IEnumerable<object[]> Models => new List<object[]>
        {
            ConfigValidModel(x => { }, true),
            ConfigValidModel(x => x.StarPoints = -1, false),
            ConfigValidModel(x => x.Source = null, false),
            ConfigValidModel(x => x.Source = "", false),
            ConfigValidModel(x => x.Source = "  ", false),
            ConfigValidModel(x => x.UserId = null, false),
            ConfigValidModel(x => x.UserId = " ", false),
            ConfigValidModel(x => x.UserId = "", false),
            ConfigValidModel(x => x.DatabaseId = null, true),
            ConfigValidModel(x => x.DatabaseId = "", false),
            ConfigValidModel(x => x.DatabaseId = " ", false)
        };

        [Theory]
        [MemberData(nameof(Models))]
        public void IsValidModel(PutStarPointsDTO model, bool isValid)
        {
            var context = new ValidationContext(model);
            var result = new List<ValidationResult>();
            var validation = Validator.TryValidateObject(model, context, result, true);
            Assert.Equal(isValid, validation);
        }

        private static object[] ConfigValidModel(Action<PutStarPointsDTO> config, bool isValid)
        {
            var dto = ModelBuilder.GetPutStarpointsDto(x => { });
            config.Invoke(dto);
            return new object[] {dto, isValid};
        }
    }
}