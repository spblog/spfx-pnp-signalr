
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

        public string SignalrHostUrl
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable(Consts.SignalrHostUrlKey, EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[Consts.SignalrHostUrlKey];

            }
        }

        public string AzureClientId
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable(Consts.ClientIdKey, EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[Consts.ClientIdKey];

            }
        }

        public string AzureClientSecret
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable(Consts.ClientSecretKey, EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[Consts.ClientSecretKey];

            }
        }

        public string AzureTenantId
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable(Consts.TenantIdKey, EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[Consts.TenantIdKey];

            }
        }

        public string CertificatePassword
        {
            get
            {
                if (_isDevelopment)
                {
                    return Environment.GetEnvironmentVariable(Consts.CertificatePasswordKey, EnvironmentVariableTarget.User);
                }

                return ConfigurationManager.AppSettings[Consts.CertificatePasswordKey];

            }
        }
    }
}
