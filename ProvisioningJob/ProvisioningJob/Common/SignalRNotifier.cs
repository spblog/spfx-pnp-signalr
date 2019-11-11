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
        private readonly AzureAppInfo _appInfo;

        public SignalRNotifier(ConfigReader configReader)
        {
            _appInfo = new AzureAppInfo
            {
                ClientId = configReader.AzureClientId,
                ClientSecret = configReader.AzureClientSecret,
                TenantId = configReader.AzureTenantId
            };
            _configReader = configReader;
        }

        public void NotifyProgress(ProvisioningState state)
        {
            var hubConnection = Task.Run(async () => await CreateAndStartHubConnection()).Result;
            Task.Run(async () => await hubConnection.SendAsync("Notify", state)).Wait();
        }
        
        public void NotifyCompleted()
        {
            var hubConnection = Task.Run(async () => await CreateAndStartHubConnection()).Result;
            Task.Run(async () => await hubConnection.SendAsync("Completed")).Wait();
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
            var app = ConfidentialClientApplicationBuilder.Create(_appInfo.ClientId)
                .WithClientSecret(_appInfo.ClientSecret)
                .WithAuthority(new Uri("https://login.microsoftonline.com/" + _appInfo.TenantId))
                .Build();

            // generate token for itself
            var scopes = new [] { $"api://{_appInfo.ClientId}/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }
    }
}
