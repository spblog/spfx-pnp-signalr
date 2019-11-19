using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;
using OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml;
using ProvisioningJob.Common;
using Site = Common.Model.Site;

namespace ProvisioningJob
{
    public class Functions
    {
        private static readonly ConfigReader _configReader;
        private static readonly string _storageConnection;
        private static readonly TableManager _tableManager;

        static Functions()
        {
            var config = new JobHostConfiguration();
            _configReader = new ConfigReader(config.IsDevelopment);
            _storageConnection = ConfigurationManager.ConnectionStrings[Consts.AzureDashboardKey].ConnectionString;
            _tableManager = new TableManager(Consts.TableName, _storageConnection);
        }

        public static async Task ProcessQueueMessage([QueueTrigger("pnp-provision")] Site siteModel, TextWriter log)
        {
            try
            {
                log.WriteLine(siteModel.WebUrl);
                var rowKey = siteModel.WebUrl.Split('/').Last();

                var entity = _tableManager.GetByKey<ProvisioningState>(rowKey);

                if (entity != null)
                {
                    log.WriteLine("Found table entity, provisioning is running right now, exiting");
                    return;
                }

                var authManager = new AuthenticationManager();
                
                // use certificate based authentication
                var context = authManager.GetAzureADAppOnlyAuthenticatedContext(siteModel.WebUrl,
                    _configReader.AzureClientId, _configReader.AzureTenantId, "cert.pfx", _configReader.CertificatePassword);
                
                var web = context.Web;
                context.Load(web);
                context.ExecuteQueryRetry();

                // main provisioning process
                await Provision(web, rowKey, log);

                // important: remove custom action added by site design, so that the top notification header is not available
                RemoveCustomAction(context.Site);

                _tableManager.DeleteEntity(rowKey);
            }
            catch (Exception e)
            {
                log.WriteLine(e);
                throw;
            }
        }

        private static async Task Provision(Web web, string rowKey, TextWriter log)
        {
            // pushes notifications to SignalR server
            // SignalR, in turn, redirects them to all connected clients
            var notifier = new SignalRNotifier(_configReader);

            var applyingInformation = new ProvisioningTemplateApplyingInformation
            {
                ProgressDelegate = (message, progress, total) =>
                {
                    log.WriteLine("{0:00}/{1:00} - {2}", progress, total, message);
                    var state = new ProvisioningState
                    {
                        Progress = progress,
                        Total = total,
                        Message = message,
                        PartitionKey = Consts.PartitionKey,
                        Timestamp = DateTimeOffset.UtcNow,
                        RowKey = rowKey
                    };
                    _tableManager.InsertEntity(state);

                    Task.Run(async () => await notifier.NotifyProgress(state)).Wait();
                }
            };

            var provider = new XMLAzureStorageTemplateProvider(_storageConnection, "pnp-drone");

            var template = provider.GetTemplate("template.xml");
            template.Connector = new AzureStorageConnector(_storageConnection, "pnp-drone");

            web.ApplyProvisioningTemplate(template, applyingInformation);

            await notifier.NotifyCompleted();
        }

        private static void RemoveCustomAction(Microsoft.SharePoint.Client.Site site)
        {
            var customActions = site.GetCustomActions().Where(a => a.Title == Consts.CustomActionName).ToList();
            var length = customActions.Count;
            for (int i = 0; i < length; i++)
            {
                customActions[i].DeleteObject();
            }

            site.Context.ExecuteQueryRetry();
        }
    }
}
