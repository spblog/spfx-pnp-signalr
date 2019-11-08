using System.Threading.Tasks;
using Common;
using Common.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace ProvisioningJob.Common
{
    public class SignalRNotifier
    {
        private readonly SignalrUtils _signalrUtils;

        public SignalRNotifier(string connectionString)
        {
            _signalrUtils = new SignalrUtils(connectionString);
        }

        public void NotifyProgress(ProvisioningState state)
        {
            var hubConnection = Task.Run(async () => await CreateAndStart()).Result;
            Task.Run(async () => await hubConnection.SendAsync("Notify", state)).Wait();
        }
        
        public void NotifyCompleted()
        {
            var hubConnection = Task.Run(async () => await CreateAndStart()).Result;
            Task.Run(async () => await hubConnection.SendAsync("Completed")).Wait();
        }

        private async Task<HubConnection> CreateAndStart()
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl(_signalrUtils.Endpoint + "/" + Consts.HubName, opts =>
                {
                    opts.AccessTokenProvider = async () => await _signalrUtils.GenerateAccessToken();
                })
                .Build();

            await hubConnection.StartAsync();

            return hubConnection;
        }
    }
}
