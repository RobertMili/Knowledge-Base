using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;

namespace BoostApp.Shared
{
    // DOC: https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.0&tabs=visual-studio

    public static class SwaggerServiceExtensions
    {
        internal const string
            DEFAULT_NAME = "My API",
            DEFAULT_VERSION = "1.0";

        public const string
            APPCONFIG_NAME = "Swagger:ApiTitle",
            APPCONFIG_VERSION = "Swagger:ApiVersion";

        /// <summary>
        /// Adds SwaggerGen configured for JWT auth, and using xml-documentation (if one exists).
        /// </summary>
        /// <remarks>
        /// Expects to find "Swagger:ApiTitle" and "Swagger:ApiVersion" in the <paramref name="swaggerConfig"/>.
        /// </remarks>
        public static IServiceCollection AddEasySwagger(this IServiceCollection services,
            IConfiguration swaggerConfig,
            string xmlDocumentationPath = null)
        => AddEasySwagger(services,
            swaggerConfig?[APPCONFIG_NAME] ?? DEFAULT_NAME,
            swaggerConfig?[APPCONFIG_VERSION] ?? DEFAULT_VERSION,
            xmlDocumentationPath);

        /// <summary>
        /// Adds SwaggerGen configured for JWT Bearer auth, and using xml-documentation (if one exists).
        /// </summary>
        public static IServiceCollection AddEasySwagger(this IServiceCollection services,
            string swaggerName = DEFAULT_NAME,
            string swaggerVersion = DEFAULT_VERSION,
            string xmlDocumentationPath = null)
        {
            swaggerName = swaggerName ?? DEFAULT_NAME;
            swaggerVersion = swaggerVersion ?? DEFAULT_VERSION;

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(Uri.EscapeDataString(swaggerVersion), new OpenApiInfo
                {
                    Title = swaggerName,
                    Version = swaggerVersion
                });

                try
                {
                    // Set the comments path for the Swagger JSON and UI.
                    if (xmlDocumentationPath == null)
                    {
                        var loc = Assembly.GetEntryAssembly().Location;
                        var i = loc.LastIndexOf('.');
                        if (i > 0)
                            loc = loc.Substring(0, i);
                        xmlDocumentationPath = loc + ".xml";
                    }
                    c.IncludeXmlComments(xmlDocumentationPath);
                }
                catch { } // TODO: logg

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme {
                                Reference = new OpenApiReference {
                                    Type = ReferenceType.SecurityScheme, Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
            });
            return services;
        }
    }
}
