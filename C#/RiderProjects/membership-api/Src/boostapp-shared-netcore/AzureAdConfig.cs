/*
 The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
// Modified by Church of Boost 2019

using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;

namespace BoostApp.Shared
{
    /// <summary>
    /// Description of the configuration of an AzureAD public client application (desktop/mobile application). This should
    /// match the application registration done in the Azure portal
    /// </summary>
    public class AzureAdConfig
    {
        // You can run this sample using ClientSecret or Certificate. The code will differ only when instantiating the IConfidentialClientApplication
        public bool IsUsingClientSecret => !string.IsNullOrEmpty(ClientSecret);

        // False if Audience property is set to empty string in the app config.
        public bool IsAudienceValidated => !Audience.IsEmpty();

        // True if no Audience property is present in the app config.
        public bool IsAudienceClientId => prop_audience == null;

        /// <summary>
        /// instance of Azure AD, for example public Azure or a Sovereign cloud (Azure China, Germany, US government, etc ...)
        /// </summary>
        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";

        /// <summary>
        /// The Tenant is:
        /// - either the tenant ID of the Azure AD tenant in which this application is registered (a guid)
        /// or a domain name associated with the tenant
        /// - or 'organizations' (for a multi-tenant application)
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Guid used by the application to uniquely identify itself to Azure AD
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// URL of the authority
        /// </summary>
        public string Authority
            => String.Format(CultureInfo.InvariantCulture, Instance, TenantId);

        /// <summary>
        /// Client secret (application password)
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: this property)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by the CertificateName property belows)
        /// </remarks> 
        public string ClientSecret { get; set; }

        /// <summary>
        /// Name of a certificate in the user certificate store
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: the property above)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by this CertificateName property)
        /// </remarks> 
        public string CertificateName { get; set; }

        /// <summary>
        /// Audience expected in incomming tokens (tokens from users conneting TO this service).
        /// </summary>
        /// <remarks>
        /// If Audience property is missing from the app config, ClientId is assumed to be the audience.
        /// If Audience property exists but is empty, then IsAudienceValidated will return false.
        /// </remarks>
        public string Audience
        {
            get => prop_audience ?? ClientId ?? string.Empty;
            set => prop_audience = value;
        }
        private string prop_audience;

        /// <summary>
        /// Build an AzureAdConfig from an AzureAd app config section.
        /// </summary>
        /// <param name="azureAdConfigSection">AzureAd configuration section</param>
        public static AzureAdConfig FromAzureAdAppConfigSection(IConfigurationSection azureAdConfigSection)
            => azureAdConfigSection.Get<AzureAdConfig>();

        /// <summary>
        /// Builds an AzureAdConfig from an app config; or throws if it fails to find the AzureAd section.
        /// </summary>
        /// <param name="appConfig">appconfig.json</param>
        public static AzureAdConfig FromAppConfig(IConfiguration appConfig)
            => FromAzureAdAppConfigSection(appConfig.GetAzureAdConfigSection());

        /// <summary>
        /// Tries to find (the selected) AzureAd section inside the app config and build an AzureAdConfig instance from it.
        /// </summary>
        /// <param name="appConfig">appconfig.json</param>
        /// <param name="azureAdConfig">An AzureAdConfig</param>
        /// <returns>True if an AzureAd section was found (regardless of it being well formed!); otherwise false.</returns>
        public static bool TryGetFromAppConfig(IConfiguration appConfig, out AzureAdConfig azureAdConfig)
        {
            if (appConfig.TryGetAzureAdConfigSection(out var azureAdConfigSection))
            {
                azureAdConfig = FromAzureAdAppConfigSection(azureAdConfigSection);
                return true;
            }
            azureAdConfig = null;
            return false;
        }
    }
}
