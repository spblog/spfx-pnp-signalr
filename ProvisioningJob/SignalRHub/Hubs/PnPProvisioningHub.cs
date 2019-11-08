using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace SignalRHub.Hubs
{
    public class PnPProvisioningHub : Hub
    {
        private readonly ILogger _logger;

        public PnPProvisioningHub()
        {
            _logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        }

        [Authorize]
        public Task Notify(object data)
        {
            return Clients.All.SendAsync("notify", data);
        }

        [Authorize]
        public Task Completed()
        {
            return Clients.All.SendAsync("completed");
        }

        public void InitialState(string webUrl)
        {
            try
            {
                var rowKey = webUrl.Split('/').Last();

                var tableManager = new TableManager(Consts.TableName, Settings.StorageConnection);
                var state = tableManager.GetByKey<ProvisioningState>(rowKey);

                Clients.Client(Context.ConnectionId).SendAsync("initial-state", new
                {
                    state.Message,
                    state.Progress,
                    state.Total
                });
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }
    }
}
