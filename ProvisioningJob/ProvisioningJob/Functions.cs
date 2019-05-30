using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Common;
using Common.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Entities;
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
            _storageConnection = ConfigurationManager.ConnectionStrings["AzureWebJobsDashboard"].ConnectionString;
            _tableManager = new TableManager(Consts.TableName, _storageConnection);
        }

        public static void ProcessQueueMessage([QueueTrigger("pnp-provision")] Site siteModel, TextWriter log)
        {
            try
            {
                log.WriteLine(siteModel.WebUrl);
                var rowKey = siteModel.WebUrl.Split('/').Last();


                _tableManager.InsertEntity(new ProvisioningState
                {
                    Progress = -1,
                    Total = -1,
                    Message = "Initializing",
                    PartitionKey = Consts.PartitionKey,
                    Timestamp = DateTimeOffset.UtcNow,
                    RowKey = rowKey
                });

                var authManager = new AuthenticationManager();
                var context = authManager.GetAppOnlyAuthenticatedContext(siteModel.WebUrl, _configReader.AppId, _configReader.AppSecret);
                var web = context.Web;
                context.Load(web);
                context.ExecuteQueryRetry();

                RemoveCustomAction(web);
                AddCustomAction(web);

                Provision(web, rowKey, log);

                RemoveCustomAction(web);
            }
            catch (Exception e)
            {
                log.WriteLine(e);
                throw;
            }
        }

        private static void Provision(Web web, string rowKey, TextWriter log)
        {
            var notifier = new SignalRNotifier(_configReader.SignalRConnection);

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

                    notifier.NotifyClients(state);
                }
            };

            var provider = new XMLAzureStorageTemplateProvider(_storageConnection, "pnp-drone");

            var template = provider.GetTemplate("template.xml");
            template.Connector = new AzureStorageConnector(_storageConnection, "pnp-drone");

            web.ApplyProvisioningTemplate(template, applyingInformation);
        }

        private static void AddCustomAction(Web web)
        {
            web.AddCustomAction(new CustomActionEntity
            {
                Title = "PnP Notifier",
                ClientSideComponentId = new Guid(Consts.ClientComponentId),
                ClientSideComponentProperties = "{\"testMessage\":\"Test message\"}",
                Location = "ClientSideExtension.ApplicationCustomizer",
                Name = Consts.CustomActionName
            });

            web.Context.ExecuteQueryRetry();
        }

        private static void RemoveCustomAction(Web web)
        {
            if (web.CustomActionExists(Consts.CustomActionName))
            {
                var customAction = web.GetCustomActions().SingleOrDefault(a => a.Name == Consts.CustomActionName);
                if (customAction != null)
                {
                    customAction.DeleteObject();
                    web.Context.ExecuteQueryRetry();
                }
                

            }
        }
    }
}
