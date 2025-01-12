using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StarPointApi.Controllers;
using StarPointApi.DTOs;
using StarPointApi.Repository;
using StarPointApi.Services.MessageService;
using StarPointApi.Services.StarPointService;
using StarPointApi.Shared;
using StarPointApiTests.Helpers;
using Xunit;

namespace StarPointApiTests
{
    public class StarPointBackEndControllerTest
    {
        [Fact]
        public async Task AddUser_ModelStateNotOK()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var controller = new StarPointBackEndController(service);
            controller.ModelState.AddModelError("activity", "null");

            var userInput = new PutStarPointsDTO
            {
                CreatedDate = DateTime.Today.AddDays(-1),
                Source = "Spinning",
                StarPoints = 500,
                UserId = "TestUserId",
            };

            var input = await controller.Put(userInput);
            Assert.NotNull(input);
            Assert.IsType<BadRequestObjectResult>(input.Result);
        }

        [Fact]
        public async Task AddUser_ModelstateOK()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var controller = new StarPointBackEndController(service);
            var input = ModelBuilder.GetPutStarpointsDto(x => { });
            var response = await controller.Put(input);
            Assert.NotNull(input);
            Assert.IsType<AddUserResponseDTO>(response.Value);
        }

        [Fact]
        public async Task DeleteBadRequest()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var controller = new StarPointBackEndController(service);
            var badDelete1 = await controller.Delete("1337", " ");
            var badDelete2 = await controller.Delete("1337", null);
            Assert.IsType<BadRequestObjectResult>(badDelete1);
            Assert.IsType<BadRequestObjectResult>(badDelete2);
        }

        [Fact]
        public async Task DeleteExistingActivity()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var controller = new StarPointBackEndController(service);
            var response = await controller.Put(ModelBuilder.GetPutStarpointsDto(x => { x.UserId = "UserId"; }));
            var okDelete = await controller.Delete(response.Value.DatabaseID, "UserId");
            Assert.IsType<OkObjectResult>(okDelete);
        }

        [Fact]
        public async Task DeleteNonExistingActivity()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var controller = new StarPointBackEndController(service);
            var badDelete = await controller.Delete("1337", "UserId");
            Assert.IsType<BadRequestObjectResult>(badDelete);
        }
    }
}