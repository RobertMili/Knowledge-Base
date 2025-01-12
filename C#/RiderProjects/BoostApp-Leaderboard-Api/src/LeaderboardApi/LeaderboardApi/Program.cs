using System.Net.Http;
using Azure.Messaging.ServiceBus;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Messaging;
using LeaderboardApi.Models;
using LeaderboardApi.Services;
using LeaderboardApi.Services.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using BoostApp.Shared;
using Microsoft.Extensions.DependencyInjection.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Policies",
        builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add ServiceBusClient by using the AddAzureClients extension method
builder.Services.AddAzureClients(clientHelper =>
{
    clientHelper.AddServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]);
});

// Add ServiceBusProcessor by getting the ServiceBusClient and calling the CreateProcessor method
builder.Services.AddSingleton<ServiceBusProcessor>(serviceContainer =>
    serviceContainer.GetRequiredService<ServiceBusClient>()
        .CreateProcessor(builder.Configuration["ServiceBus:QueueName"]));

// Adds the HttpClientService which uses ConfidentialHttpClient from nexer.boostapp.shared nuget package. 
builder.Services.AddSingleton<IHttpClientService, HttpClientService>();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IExternalApiService, ExternalApiService>();
builder.Services.AddSingleton<IMessageHandler, MessageHandler>();
builder.Services.AddSingleton<ITeamMemberMessageService, TeamMemberMessageService>();
builder.Services.AddSingleton<ITeamMessageService, TeamMessageService>();
builder.Services.AddSingleton<IStarpointMessageService, StarpointMessageService>();
builder.Services.AddSingleton<ICompetitionMessageService, CompetitionMessageService>();

// Adds a GenericRepository<TeamLeaderboardModel> to the ServiceProvider
builder.Services.AddSingleton<IGenericRepository<TeamLeaderboardEntity>, GenericRepository<TeamLeaderboardEntity>>(
    serviceProvider =>
        new GenericRepository<TeamLeaderboardEntity>(
            serviceProvider.GetRequiredService<IConfiguration>(),
            builder.Configuration["AzureStorageAccount:TeamLeaderboardTableName"]));

// Adds a GenericRepository<TeamMemberLeaderboardModel> to the ServiceProvider
builder.Services
    .AddSingleton<IGenericRepository<TeamMemberLeaderboardEntity>, GenericRepository<TeamMemberLeaderboardEntity>>(
        serviceProvider =>
            new GenericRepository<TeamMemberLeaderboardEntity>(
                serviceProvider.GetRequiredService<IConfiguration>(),
                builder.Configuration["AzureStorageAccount:TeamMemberLeaderboardTableName"]));

// Adds a GenericRepository<StarpointModel> to the ServiceProvider
builder.Services
    .AddSingleton<IGenericRepository<StarpointEntity>, GenericRepository<StarpointEntity>>(
        serviceProvider =>
            new GenericRepository<StarpointEntity>(
                serviceProvider.GetRequiredService<IConfiguration>(),
                builder.Configuration["AzureStorageAccount:StarpointLeaderboardTableName"]));

//Adds azure ad auhentication using nexer.boostapp.shared nuget package.
builder.Services.AddEasyAzureJwtAuth(builder.Configuration);
//Adds authentication to swagger using nexer.boostapp.shared nuget package.
builder.Services.AddEasySwagger(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if
    (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //Adds the correct uri to swagger.
    app.UseSwaggerUI(setup =>
    {
        setup.SwaggerEndpoint($"{Uri.EscapeDataString(builder.Configuration["Swagger:ApiVersion"])}/swagger.json",
            $"{builder.Configuration["Swagger:ApiTitle"]} {builder.Configuration["Swagger:ApiVersion"]}");
    });
}

else
{
    // Adds the Handle method from the MessageHandler class to the ServiceBusProcessor property ProcessMessageAsync
    app.Services.GetRequiredService<ServiceBusProcessor>().ProcessMessageAsync +=
        app.Services.GetRequiredService<IMessageHandler>().Handle;

    // Adds the ErrorHandle method from the MessageHandler class to the ServiceBusProcessor property ProcessErrorAsync
    app.Services.GetRequiredService<ServiceBusProcessor>().ProcessErrorAsync +=
        app.Services.GetRequiredService<IMessageHandler>().ErrorHandle;

    // Starts processing messages
    app.Services.GetRequiredService<ServiceBusProcessor>().StartProcessingAsync();
}


app.UseCors("Policies");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();