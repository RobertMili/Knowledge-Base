using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace BoostApp.Shared
{
    public static class AzureAdConfigExtensions
    {
        /// <summary>
        /// Registers AzureAdConfig for IOptions dependency injection.
        /// </summary>
        /// <param name="services">IServiceCollection, typically from inside Startup.ConfigureServices</param>
        /// <param name="appConfig">appsettings.json</param>
        /// <returns>The AzureAd config section.</returns>
        public static IConfigurationSection AddAzureAdConfigIOptions(this IServiceCollection services, IConfiguration appConfig)
        {
            var section = appConfig.GetAzureAdConfigSection();
            services.Configure<AzureAdConfig>(section);
            return section;
        }

        /// <summary>
        /// Tries to find (the selected) AzureAd section in the app config.
        /// </summary>
        /// <param name="appConfig">appconfig.json</param>
        /// <param name="azureAdConfigSection">An AzureAd section, or null.</param>
        /// <remarks>
        /// First looks for an AdConfigs section.
        /// If found it looks for the existens of a Selector property.
        /// If a Selector property is found it looks for a subsection named after the Selector property's
        /// value that contains an AzureAd section, i.e. AdConfigs:[Selector.Value]:AzureAd.
        /// If the Selector appoach fails it looks for ANY child inside AdConfig that contains an AzureAd section.
        /// Lastly it looks for an AzureAd section in the root of the config; or gives up.
        /// 
        /// DOES NOT verify that the AzureAd section is well formed!
        /// </remarks>
        /// <returns>True if found; otherwise false.</returns>
        public static bool TryGetAzureAdConfigSection(this IConfiguration appConfig, out IConfigurationSection azureAdConfigSection)
            => null != (azureAdConfigSection
                = appConfig.GetSection("AdConfigs")
                    .Declare(out var adConfigs)
                    .GetSectionOrNull(adConfigs["Selected"])
                    ?.GetSectionOrNull("AzureAd")
                //fallbacks...
                ?? adConfigs.GetChildren().Select(section => section.GetSectionOrNull("AzureAd")).FirstOrDefault(section => section != null)
                ?? appConfig.GetSectionOrNull("AzureAd"));

        /// <summary>
        /// Gets the (selected) AzureAd section from the app config, or throws trying.
        /// </summary>
        /// <seealso cref="TryGetAzureAdConfigSection(IConfiguration, out IConfigurationSection)"/>
        public static IConfigurationSection GetAzureAdConfigSection(this IConfiguration appConfig)
            => TryGetAzureAdConfigSection(appConfig, out var azureAdConfigSection)
            ? azureAdConfigSection
            : throw new Exception("Failed to find the AzureAd app config section");

        /// <summary>
        /// Attempts to bind the AzureAdConfig to an IConfigurationSection.
        /// </summary>
        public static void FromAzureAdAppConfigSection(this AzureAdConfig aacThis, IConfigurationSection azureAdConfigSection)
            => azureAdConfigSection.Bind(aacThis);

        public static void FromAppConfig(this AzureAdConfig aacThis, IConfiguration appConfig)
            => FromAzureAdAppConfigSection(aacThis, appConfig.GetAzureAdConfigSection());
    }
}
