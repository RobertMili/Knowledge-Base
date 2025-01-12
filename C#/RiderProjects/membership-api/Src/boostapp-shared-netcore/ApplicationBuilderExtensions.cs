using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace BoostApp.Shared
{
    using static SwaggerServiceExtensions;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseEasyAuthSwaggerSetup(this IApplicationBuilder app, IHostingEnvironment env,
            IConfiguration appConfig)
        => UseEasyAuthSwaggerSetup(app, env, swaggerName: appConfig[APPCONFIG_NAME], swaggerVersion: appConfig[APPCONFIG_VERSION]);

        /// <summary>
        /// Uses a common set of default settings... (exception page settings, auth, https, mvc, swagger, swagger-ui)
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env">IHostingEnvironment to determine exception page settings</param>
        /// <param name="swaggerName">Name the SwaggerUI page (sans version)</param>
        /// <param name="swaggerVersion">Version string for Swagger (used both for endpoint url and SwaggerUI page)</param>
        /// <remarks>
        /// Swagger endpoint url is currently hard-coded as "/swagger/v1/swagger.json" (TODO)
        /// </remarks>
        public static IApplicationBuilder UseEasyAuthSwaggerSetup(this IApplicationBuilder app, IHostingEnvironment env,
            string swaggerName = DEFAULT_NAME, string swaggerVersion = DEFAULT_VERSION, string swaggerRoutePrefix = null)
        {
            swaggerName = swaggerName ?? DEFAULT_NAME;
            swaggerVersion = swaggerVersion ?? DEFAULT_VERSION;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(setup =>
            {
                if (swaggerRoutePrefix != null)
                    setup.RoutePrefix = swaggerRoutePrefix;
                setup.SwaggerEndpoint($"{Uri.EscapeDataString(swaggerVersion)}/swagger.json", $"{swaggerName} {swaggerVersion}");
                //setup.SwaggerEndpoint($"/swagger/{Uri.EscapeDataString(swaggerVersion)}/swagger.json", $"{swaggerName} {swaggerVersion}");
            });

            return app;
        }
    }
}
