namespace Common
{
    public static class Consts
    {
        public static string TableName = "PnPDroneProvisioning";
        public static string PartitionKey = "pnp";
        public static string CustomActionName = "PnP Notifier";
        public static string HubName = "pnpprovisioninghub";
        public static string SignalrHostUrlKey = "SignalrHostUrl";
        public static string ClientIdKey = "AzureAd:ClientId";
        public static string ClientSecretKey = "AzureAd:ClientSecret";
        public static string TenantIdKey = "AzureAd:TenantId";
        public static string AzureDashboardKey = "AzureWebJobsDashboard";
        public static string CertificatePasswordKey = "CertificatePassword";
    }
}
