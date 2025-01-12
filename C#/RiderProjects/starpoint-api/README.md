# BoostApp-StarPoint-API

[GO TO TEAMS-WIKI](https://teams.microsoft.com/l/channel/19%3Ad22e2e83ba2b406485158793a32546b3%40thread.skype/tab%3A%3Aa97468d8-bd7e-40a6-af7c-fdd0ec5e5a4e?groupId=cc886c7c-07d2-4c22-a2d0-c7d427c2ea19&tenantId=f82b0fb7-0101-410d-8e87-0efa7c1d3978)

## API overview

Repo: [Azure DevOps StarPoint-API](https://dev.azure.com/SigmaOpenSource/BoostApp/_git/starpoint-api)

Azure Portal [StarPointAPI App Service](https://portal.azure.com/#@onlinesigma.onmicrosoft.com/resource/subscriptions/e552509c-6e9f-488c-8b63-977cad176933/resourceGroups/boostappgroup/providers/Microsoft.Web/sites/starpoint/appServices)

Azure Active Directory [App Registration (BoostApp-Starpoint-Api)](https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/a38408ce-d91c-4010-9204-448b813e0e00/isMSAApp~/false) - to view client id, scopes and AD settings

(NEW) Database [BoostAppStarPoints](https://portal.azure.com/#@nexergroup.com/resource/subscriptions/562e0016-4fd3-4756-83aa-8596af5e90a4/resourceGroups/boostappgroup-nexer/providers/Microsoft.Storage/storageAccounts/boostappstarpoint/storagebrowser)

(OLD) Database-ActivityTable [BoostAppStorageAccount](https://portal.azure.com/#@onlinesigma.onmicrosoft.com/resource/subscriptions/e552509c-6e9f-488c-8b63-977cad176933/resourceGroups/boostappgroup/providers/Microsoft.Storage/storageAccounts/boostappstorageaccount/overview) - database for Star Points.

StarPointAPI: <https://starpoint.azurewebsites.net/swagger/index.html>

LocalHost: <https://localhost:5001/swagger/index.html>

Http Request Examples: [API-Requests.http](API-Requests.http)

## Run Locally

You need:

- Azure AD Sigma Account

- Jason Web Token\*, see [GO TO TEAMS-WIKI](https://teams.microsoft.com/l/channel/19%3Ad22e2e83ba2b406485158793a32546b3%40thread.skype/tab%3A%3Aa97468d8-bd7e-40a6-af7c-fdd0ec5e5a4e?groupId=cc886c7c-07d2-4c22-a2d0-c7d427c2ea19&tenantId=f82b0fb7-0101-410d-8e87-0efa7c1d3978)

- Storage Emulator [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio)

- Configure 2 settings

\*The Jason Web Token is only needed when sending requests to the API.

There is two settings that makes it simpler to run the application locally and debug.

```shell
ENVIRONMENT=Development
    #Default with Visual Studio

CRONJOB=Disable
    #Needs to be added manually
    #Not required but recommended
```

(Above settings are placed in the launchSettings.Json file or use the command located in the Run-Dev file in repo)

## Cronjob

The API also contains a cronjob that runs as a hosted service,

The CronJob runs as a hosted service inside the API it fetches all StarPoints from 'Step API' on time based intervalls.

Configure the CronJob in [Startup.cs.](src\StarPointApi\Startup.cs)

```csharp
            var cronExpression =
                CurrentEnvironment.IsDevelopment()
                    ? @"*/5 * * * * *"
                    : @"0 0 3 * * *"; // Every 5 sec or every day at 3am

            services.AddCronJob<GetStepsJob>(x =>
            {
               x.TimeZoneInfo = TimeZoneInfo.Utc;
               x.CronExpression = cronExpression;
            });
```


## BoostApp Nuget package

Starpoint is using the BoostApps nuget package. Which you can find in the repo: [Here](https://dev.azure.com/SigmaOpenSource/BoostApp/_git/sigma-boostapp-common). 

## Service Bus

When making a POST, PUT or DELETE request the StarPoint-API publishes a message to Azure Service Bus with information regarding what has been updated.
This is implemented in the MessageService where a MessagePublisher class is being injected. The MessagePublisher has a Publish method that sends a LeaderBoardMessageModel object to service bus with different information regarding if it's a POST, PUT or DELETE request. This MessageService is then injected into the StarPointService.

The messagPublisher class is fetched from the Nuget package: BoostApp.Shared.Messaging and the LeaderBoardMessageModel is fetched from BoostApp.ClassLibrary.

## Left todo: 

* Consume the message queue “starpoint-queue”. The messages are sent by the challenge service when a challenge is completed or not completeted. When a challenge is completed the starpoints should be added to the DB. When a challenge is not completed it should remove the starpoints from the DB. 

* Update the service so it dosen’t send messages to the service bus que when the app is in development enviorment. Messages should only be sent by “real requests” not in development and testing enviorment. (See example in LeaderboardAPI) 