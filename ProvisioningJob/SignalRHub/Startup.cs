using System;
using System.Text;
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
            var serviceUtils = new ServiceUtils(Configuration[Consts.SignalrConnectionKey]);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serviceUtils.AccessKey));
            var tenantId = Configuration["AzureAd:TenantId"];

            services.AddAuthentication()
                .AddAzureADBearer(options =>
                {
                    Configuration.Bind("AzureAd", options);
                })
                .AddJwtBearer("WebJobBearerAuth", options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = securityKey
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = AttachAccessToken
                    };
                });

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

            services.AddCors();
            services.AddSignalR();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        private static Task AttachAccessToken(MessageReceivedContext context)
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) &&
                (context.HttpContext.WebSockets.IsWebSocketRequest || context.Request.Headers["Accept"] == "text/event-stream"))
            {
                context.Token = context.Request.Query["access_token"];
            }


            return Task.CompletedTask;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            app.UseCors(builder =>
            {
                builder.WithOrigins("https://localhost:4321", "https://mastaq.sharepoint.com")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });

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
