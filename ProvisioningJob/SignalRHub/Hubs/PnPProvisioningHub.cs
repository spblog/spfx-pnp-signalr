using System;
using System.Linq;
using Common;
using Common.Model;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace SignalRHub.Hubs
{
    public class PnPProvisioningHub : Hub
    {
        private ILogger _logger;

        public PnPProvisioningHub()
        {
            _logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        }
        
        public void InitialState(string webUrl)
        {
            try
            {
                var rowKey = webUrl.Split('/').Last();

                var tableManager = new TableManager(Consts.TableName, Settings.StorageConnection);
                var state = tableManager.GetByKey<ProvisioningState>(rowKey);

                Clients.Client(Context.ConnectionId).SendAsync("initial-state", state.Message, state.Progress, state.Total);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }
    }
}
