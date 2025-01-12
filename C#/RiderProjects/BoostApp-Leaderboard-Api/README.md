<H1> BoostApp Leaderboard API </H1> (Last edited 2023-04-19)

[GO TO TEAMS-WIKI](https://teams.microsoft.com/l/entity/com.microsoft.teamspace.tab.wiki/tab::054cef1a-c03a-40ab-809e-ac6e559f6a21?context=%7B%22subEntityId%22%3A%22%7B%5C%22pageId%5C%22%3A134%2C%5C%22sectionId%5C%22%3A177%2C%5C%22origin%5C%22%3A2%7D%22%2C%22channelId%22%3A%2219%3AvC6N9zsWFL8bLUQaK5rngbcsW8qw3OkYrolo8Ls_Blk1%40thread.tacv2%22%7D&tenantId=02b6749b-5ce0-4853-bd5c-a05f9bd9dd3a)

<H2> Content </H2>

- Introduction
- Architecture
- Nuget
- Azure Storage Account / Database
- Azure Service Bus
- Message Handling
- API
- Azure Cloud
- Left to do task:
  <br>

<H2> Introduction </H2>
This is the Leaderboard API section for the BoostApp project. 
In earlier designs the API handling teams and teammember was called membership, this is now split into two seperate API:s. Membership was also responsible for fetching data to the leaderboard using a GET Team and GET TeamMember endpoint. 
Since the membership API is discontinued this will be the API handling the data that the leaderboard is using.

When creating this API main focus have been on creating a Get Team and Get TeamMember endpoint, and also to handle the consuming of messages recieved from azure service bus that will update the leaderboard DB.
You can try it out in the swagger UI when running the program on "http://localhost :[yourLocalPort]/swagger/index.html".
This API is built with a service repository pattern. The repository layer is the layer closest to the DB and is the layer resposible for handling the communication between the business logic and the data storage.
In the service layer the business logic is implemeted.  
In this project we are using a Generic Repository where you can create different repositories depending on which entity you would like to use.

<H2>  Architecture </H2>

![Flowshart Image](/img/leaderboardimg.jpg "Flowshart image")

[LINK TO DIAGRAM](https://www.figma.com/file/cQc87f7Th9Cy5OjGn4DwSy/BoostApp-Dokumentation?node-id=0%3A1)

<H2> NuGet </H2> 
To Be able to use the Nexer Nuget package you need to authenticate the nuget feed.
The easiest way to do that is to open Visual Studio -> Preferences -> Manage nuget Packages -> Nuget - sources. Add source and enter the endpoint found in project/nuget.config. At the moment: https://pkgs.dev.azure.com/SigmaOpenSource/BoostApp/_packaging/boostapp/nuget/v3/index.json

If you need to update the nuget package the repo incuding nuget package is located here:
https://dev.azure.com/SigmaOpenSource/BoostApp/_git/sigma-boostapp-common

BoostApp NuGet implementations: 
In the messageHandler and the messageServices the API is using the BoostApp Classlibrary to fetch enums of Sender and Request type when receiving a message. 
The HttpClientService is using the ConfidentialHttpClient from BoostApp Shared. 
<br>

<H2>  Azure Storage Account / Database </H2>  
We are using a storage account hosted on Microsoft Azure with the account name "boostappleaderboard" for storing data. 
It contains two tables, one for Teams and one for TeamMembers. The tables are named TeamLeaderboard and TeamMemberLeaderboard.
 
The TeamLeaderboard tables PartitionKey column is equal to the Teams competitionId property in the Team DB and the RowKey column is equal to the Teams id property in the Team DB.
The TeamMemberLeaderboard tables PartitionKey column is equal to the  TeamMembers teamId property in the TeamMember DB and the RowKey column is equal to the TeamMembers id property in the TeamMember DB.

Link to database: [HERE](https://portal.azure.com/#@nexergroup.com/resource/subscriptions/562e0016-4fd3-4756-83aa-8596af5e90a4/resourceGroups/boostappgroup-nexer/providers/Microsoft.Storage/storageAccounts/boostappleaderboard/overview)

<H2>Azure Service Bus </H2>
We are using azure service bus to handle the communication between Team, TeamMember, Starpoint, Competition and Leaderboard APIs. It contains one queue that all of the services use to send messages to the Leaderboard API.
 
When Team, TeamMember or Starpoint add/modify/delete data in their own database they will publish a message to the queue containing data about what has been done as well as the object that has been added/modified/deleted. The Competition API will only send a message when EndDate has been changed on a Competition.
Leaderboard will consume these message queues and add/modify/delete the data in its own database based on the data inside of the message.

Link to service bus queue: [HERE](https://portal.azure.com/#@nexergroup.com/resource/subscriptions/562e0016-4fd3-4756-83aa-8596af5e90a4/resourceGroups/boostappgroup-nexer/providers/Microsoft.ServiceBus/namespaces/boostapp/queues)

<H2>Message handling </H2>
The first stop when a message is recieved is the MessageHandler class. Its job is to decode the message body and figure out who sent the message, it will then pass the model to the correct MessageService.
The MessageService will then check the Request property and based on that value call the correct method in the GenericRepository to do the operation/operations on the database.

Logging/Error handling
The LeaderboardAPI uses the .NET ILogger interface to log. When something goes wrong in the message handling and the message can no longer be processed it logs an error describing why it couldn't be processed. There is also a ErrorHandle class that catches exceptions and logs them.

Apart from logging when something goes wrong it also logs information when a new message starts being processed and when a message is being abandoned. Both of these log messages contains the messageId to make it easier to troubleshoot.

If a message gets sent to the Dead Letter queue you can go to the App Service in Azure Portal and read the logs to find out why it couldn't be processed. To find the correct log messages just look for the logs with the same messageId as the message you are troubleshooting,

<H2>External Api Requests​​​​​​​ </H2>
In the leaderboardAPI we need to do some GET requests to external apis (still inside the boostapp scope). We do this by using a ConfidentialHttpClient from the boostapp.shared NuGet package. The ConfidentialHttpClient is created and owned by a service class named HttpClientService, this service is then injected into the ExternalApiService class.

The ExternalApiService handles all of the requests to external APIs and is being used by multiple services within the leaderboardAPI.

<H2> API </H2>

- Leaderboard API: https://dev.azure.com/SigmaOpenSource/BoostApp/_git/BoostApp-Leaderboard-Api
<br>

<H2> Azure Cloud </H2> 
All recources can be found on below links:

- Azure Portal: https://portal.azure.com/#home
- Azure Active Directory: https://dev.azure.com/SigmaOpenSource/BoostApp
  <br>

<h3> Left to do tasks: (Make sure to delete this section after your are done with everything) </h3>

- Create and maintain a copy of the competition DB with the relevant data to make sure that we always have the latest data. 
- Make sure CompetitionAPI is providing the correct data in a message. 
- Create and maintain a copy of the starpoint DB with the relevant data to make sure that we always have the latest data. The messaging is already setup, we need to add some extra code to the StarpointMessageService class to add the data to the new Starpoint DB copy. RELEVANT DATA: CreatedDate, Starpoints, UserID == PartitionKey, ID == RowKey 
- Create a Release pipeline. 
