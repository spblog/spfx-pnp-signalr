using System;
using System.Threading.Tasks;
using Common;
using Common.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Identity.Client;

namespace ProvisioningJob.Common
{
    public class SignalRNotifier
    {
        private readonly ConfigReader _configReader;

        public SignalRNotifier(ConfigReader configReader)
        {
            _configReader = configReader;
        }

        public async Task NotifyProgress(ProvisioningState state)
        {
            var hubConnection = await CreateAndStartHubConnection();
            await hubConnection.SendAsync("Notify", state);
        }

        public async Task NotifyCompleted()
        {
            var hubConnection = await CreateAndStartHubConnection();
            await hubConnection.SendAsync("Completed");
        }

        private async Task<HubConnection> CreateAndStartHubConnection()
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl(_configReader.SignalrHostUrl + "/" + Consts.HubName, opts =>
                {
                    opts.AccessTokenProvider = async () =>
                    {
                        var token = await GenerateAccessToken();
                        return token;
                    };
                })
                .Build();

            await hubConnection.StartAsync();

            return hubConnection;
        }

        public async Task<string> GenerateAccessToken()
        {
            var app = ConfidentialClientApplicationBuilder.Create(_configReader.AzureClientId)
                .WithClientSecret(_configReader.AzureClientSecret)
                .WithAuthority(new Uri("https://login.microsoftonline.com/" + _configReader.AzureTenantId))
                .Build();

            // generate token for itself
            var scopes = new[] { $"api://{_configReader.AzureClientId}/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }
    }
}
