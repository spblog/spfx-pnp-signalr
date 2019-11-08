using System.Threading.Tasks;
using Common;
using Common.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace ProvisioningJob.Common
{
    public class SignalRNotifier
    {
        private readonly ServiceUtils _serviceUtils;

        public SignalRNotifier(string connectionString)
        {
            _serviceUtils = new ServiceUtils(connectionString);
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
                .WithUrl(_serviceUtils.Endpoint + "/" + Consts.HubName, opts =>
                {
                    opts.AccessTokenProvider = async () => await _serviceUtils.GenerateAccessToken();
                })
                .Build();

            await hubConnection.StartAsync();

            return hubConnection;
        }
    }
}
