using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BoostApp.Shared
{
    public static class AzureDefaultServiceExtensions
    {
        /// <summary>
        /// Adds AzureAdConfig IOptions, HasApprovedUserIdRequirement, and default JWT Bearer Auth
        /// (with optional further configuration options, e.g. for adding more Auth Policies). 
        /// </summary>
        public static IServiceCollection AddEasyAzureJwtAuth(this IServiceCollection services,
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
                options.AddPolicy(nameof(Policies.HasApprovedUserIdRequirement), policy =>
                    policy.Requirements.Add(new Policies.HasApprovedUserIdRequirement()));
                if (configureAuthorizationOptions != null)
                    configureAuthorizationOptions(options);
            });
            services.AddSingleton<IAuthorizationHandler, Policies.HasApprovedUserIdHandler>();

            return services;
        }
    }
}
