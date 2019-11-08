
using System;
using System.Configuration;
using Common;

namespace ProvisioningJob.Common
{
    public class ConfigReader
    {
        private readonly bool _isDevelopment;

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
                    return Environment.GetEnvironmentVariable(Consts.SignalrConnectionKey, EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[Consts.SignalrConnectionKey];

            }
        }
    }
}
