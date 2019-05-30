using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddMvc();
            services.AddSignalR().AddAzureSignalR();
            Settings.StorageConnection = Configuration["AzureWebJobsDashboard"];
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            {
                builder.WithOrigins("https://localhost:4321", "http://localhost:5000", "https://mastaq.sharepoint.com")
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST")
                    .AllowCredentials();
            });

            app.UseHttpsRedirection();

            app.UseMvc();
            app.UseFileServer();
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<PnPProvisioningHub>("/" + Consts.HubName);
            });
        }
    }
}
