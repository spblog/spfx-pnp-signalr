using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SignalRHub.Hubs;

namespace SignalRHub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; 
            Settings.StorageConnection = Configuration[Consts.AzureDashboardKey];
            var tenantId = Configuration["AzureAd:TenantId"];

            // use Azure AD JWT Bearer authentication by default
            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                .AddAzureADBearer(options => { Configuration.Bind("AzureAd", options); });

            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                options.Authority += "/v2.0";

                options.TokenValidationParameters.ValidAudiences = new []
                {
                    options.Audience, $"api://{options.Audience}"
                };

                options.TokenValidationParameters.IssuerValidator = (issuer, token, parameters) =>
                {
                    if (!issuer.Contains(tenantId))
                    {
                        throw new SecurityTokenInvalidIssuerException("Issuer received doesn't match Tenant id");
                    }

                    return issuer;
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = AttachAccessToken
                };

            });

            services.AddSignalR();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // web sockets don't transport headers, so an access token is attached throught the query string
        // this method reads token from the query string and adds to the context, so that request becomes authenticated
        private static Task AttachAccessToken(MessageReceivedContext context)
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = context.Request.Query["access_token"];
            }

            return Task.CompletedTask;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseFileServer();

            app.UseSignalR(routes =>
            {
                routes.MapHub<PnPProvisioningHub>("/" + Consts.HubName);
            });
            
            app.UseMvc();
        }
    }
}
