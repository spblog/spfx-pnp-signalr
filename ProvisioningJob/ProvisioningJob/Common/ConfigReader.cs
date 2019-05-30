
using System;
using System.Configuration;

namespace ProvisioningJob.Common
{
    public class ConfigReader
    {
        private bool _isDevelopment = false;

        public ConfigReader(bool isDevelopment)
        {
            _isDevelopment = isDevelopment;
        }

        public string AppId
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable(nameof(AppId), EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[nameof(AppId)];
            }
        }

        public string AppSecret
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable(nameof(AppSecret), EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[nameof(AppSecret)];
            }
        }

        public string SignalRConnection
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable("Azure:SignalR:ConnectionString", EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings["Azure:SignalR:ConnectionString"];

            }
        }
    }
}
