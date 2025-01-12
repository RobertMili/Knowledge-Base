using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BoostApp.Shared
{
    public static class AzureAdAuthenticationBuilderExtensions
    {
        // REQUIRES THAT AzureAdConfigure has been registered:  services.AddAzureAdConfigIOptions(Configuration)
        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder)
        {
            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureAzureOptions>();
            builder.AddJwtBearer();
            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly AzureAdConfig config;

            public ConfigureAzureOptions(IOptions<AzureAdConfig> azureOptions)
            {
                config = azureOptions.Value;
            }

            public void Configure(string name, JwtBearerOptions options)
            {
                options.Authority = config.Authority;

                if (options.TokenValidationParameters.ValidateAudience = config.IsAudienceValidated)
                {
                    if (config.IsAudienceClientId)
                    {
                        // The valid audiences are both the Client ID(options.Audience) and api://{ClientID}
                        options.TokenValidationParameters.ValidAudiences = new string[] { config.ClientId, $"api://{config.ClientId}" };
                    }
                    else
                    {
                        // The valid audience is explicitly 
                        options.Audience = config.Audience;
                    }
                }

                // If you want to debug, or just understand the JwtBearer events, uncomment the following line of code
                // options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);
            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}
