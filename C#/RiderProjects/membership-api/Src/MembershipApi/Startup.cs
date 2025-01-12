using MembershipAPI.Services.Membership;
using MembershipAPI.Services.UserInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MembershipAPI.Policies;
using MembershipAPI.Services;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Sigma.BoostApp.Shared.Authorization.Policies;
using Sigma.BoostApp.Shared;
using MembershipApi.Services;
using MembershipApi.Domain.Repositories;
using Microsoft.WindowsAzure.Storage;
using MembershipApi.Domain.Interfaces;
using MembershipApi.Domain.Repositories.Entities;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Microsoft.Extensions.Logging;
using Serilog.Formatting.Compact;
using Microsoft.Azure.Cosmos;
using MembershipAPI.Domain.Interfaces;
using MembershipAPI.Domain.Repositories;
using Microsoft.AspNetCore.Mvc.Versioning;
using MembershipAPI.Domain.Services.Interfaces;
using Microsoft.OpenApi.Models;

namespace MembershipAPI
{
    public class Startup
    {
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureLogging(services);

            //TODO: is not used
            /*string membershipConnectionString = null;

            var configKey = Configuration.GetValue<string>("MembershipDbSecretKey") ?? "MemberDBConnection";

            if (string.IsNullOrEmpty(membershipConnectionString))
            {
                membershipConnectionString = KeyVaultService.GetSecret(configKey);
            }*/

            services.AddCors(options =>
            {
                options.AddPolicy("Policies",
                builder =>
                {
                    //TODO: Add url of angular app deployed in the cloud too
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            var account = CloudStorageAccount.Parse(Configuration["Azure:StorageConnectionString"]);
            var blobClient = account.CreateCloudBlobClient();
            services.AddSingleton(blobClient.GetType(), blobClient);
            services.AddSingleton<ITableOfType<CompetitionEntity>, CompetitionRepository>();
            services.AddSingleton<ITableOfType<TeamEntity>, TeamRepository>();
            services.AddSingleton<ITableOfType<TeamMemberEntity>, TeamMemberRepository>();
            services.AddSingleton<IStarpointsRepository>(new StarpointRepository(Configuration["AzureCosmos:StorageConnectionString"]));
            services.AddSingleton<IMembershipService, MembershipService>();
            services.AddSingleton<ILeaderboardService, LeaderboardService>();
            services.AddSingleton<ILeaderboardService, LeaderboardService>();
            services.AddSingleton<CompetitionService>();
            services.AddSingleton<TeamService>();

            // NOTE: AddHttpClient<T> registers T as a Transient.
            // NOTE: services.AddHttpClient<UserInfoService>(); as per the example doc, ONLY works if you are NOT injecting it via interface!
            services.AddHttpClient<IUserInfoService, UserInfoService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            AddEasyAzureJwtAuth(services, Configuration, configureAuthorizationOptions: options =>
            {
                options.AddPolicy(nameof(TeamMemberRequirement), policy =>
                    policy.Requirements.Add(new TeamMemberRequirement()));
            })
            .AddSingleton<IAuthorizationHandler, TeamMemberHandler>();

            // Register the Swagger generator
            services.AddEasySwagger(Configuration);
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri("https://login.microsoftonline.com/02b6749b-5ce0-4853-bd5c-a05f9bd9dd3a/oauth2/v2.0/authorize"),
                            TokenUrl = new Uri("https://login.microsoftonline.com/02b6749b-5ce0-4853-bd5c-a05f9bd9dd3a/oauth2/v2.0/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid profile email api://f81b8f97-bca0-4901-9f09-d5d5c05390f7/access_as_user", "Reads the Weather forecast" }
                            }
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            },
                            Scheme = "oauth2",
                            Name = "oauth2",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            services.AddMvc(options => {
                options.EnableEndpointRouting = false;
            });
        }


        private void ConfigureLogging(IServiceCollection services)
        {
            var eventHubConfig = Configuration.GetSection("Logging").GetSection("eventHub");

            var eventHubConnectionString = eventHubConfig?.GetValue<string>("connectionString");
            var eventHubName = eventHubConfig?.GetValue<string>("eventHubName");

            var httpEndPoint = Configuration.GetValue<string>("Logging:SerilogHttpEndPoint");

            if (!string.IsNullOrEmpty(eventHubConnectionString) && !string.IsNullOrEmpty(eventHubName))
            {
                Log.Logger = new LoggerConfiguration()

                    .WriteTo.Console()

                    .WriteTo.AzureEventHub(new CompactJsonFormatter(), eventHubConnectionString, eventHubName)
                    .Enrich.WithProperty("Environment", environment.EnvironmentName)
                    .MinimumLevel.Override("Microsoft", new LoggingLevelSwitch(LogEventLevel.Error))

                    .CreateLogger();
            }
            else if (!string.IsNullOrEmpty(httpEndPoint) && httpEndPoint.Contains("localhost"))
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.Http(httpEndPoint)
                    .CreateLogger();

                Log.Logger.Information("Use Http with end point {httpEndPoint}", httpEndPoint);
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();
                Log.Logger.Information("No configuration for eventHub or Http found...");
            }

            services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory(Log.Logger, false));
        }

        private static IServiceCollection AddEasyAzureJwtAuth(IServiceCollection services,
            IConfiguration appConfig,
            Action<AuthorizationOptions> configureAuthorizationOptions = null,
            Action<AuthenticationOptions> configureAuthenticationOptions = null)
        {
            IConfigurationSection
                adConf = services.AddAzureAdConfigIOptions(appConfig);

            // TODO: improve this shit!
            string issuer = @"https://sts.windows.net/" + adConf[nameof(AzureAdConfig.TenantId)];
            services.AddSingleton(new ApprovedIssuers(issuer));

            // Must call register AddAzureAdConfigIOptions for AddAzureAdBearer to work!
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                if (configureAuthenticationOptions != null)
                    configureAuthenticationOptions(options);
            })
            .AddAzureAdBearer();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(HasApprovedUserIdRequirement), policy =>
                    policy.Requirements.Add(new HasApprovedUserIdRequirement()));
                if (configureAuthorizationOptions != null)
                    configureAuthorizationOptions(options);
            });
            services.AddSingleton<IAuthorizationHandler, HasApprovedUserIdHandler>();

            return services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // This should be configured from docker and default when deploying to azure
            var disableHttpsRedirection = Configuration.GetValue("Security:DisableHttpsRedirection", false);
            if (!disableHttpsRedirection)
                app.UseHttpsRedirection();

            app.UseCors("Policies");
            ConfigureSwagger(app, env);
        }

        private IApplicationBuilder ConfigureSwagger(IApplicationBuilder app, IWebHostEnvironment env)
        {
            string swaggerVersion = "1.0";
            string swaggerName = typeof(Startup).Namespace;

            if (environment.EnvironmentName.ToLower() == "development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();
            // app.UseHttpsRedirection();
            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(setup =>
            {
                // if (swaggerRoutePrefix != null)
                //     setup.RoutePrefix = swaggerRoutePrefix;
                setup.SwaggerEndpoint($"{Uri.EscapeDataString(swaggerVersion)}/swagger.json", $"{swaggerName} {swaggerVersion}");
                setup.OAuthClientId("f81b8f97-bca0-4901-9f09-d5d5c05390f7");
                setup.OAuthClientSecret("4sG8Q~St7~P0qm~49XTJvo~PYwX0v6~DiWkFbbWX");
                setup.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });

            return app;
        }
    }
}
