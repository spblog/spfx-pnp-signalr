namespace ProvisioningJob.Common
{
    public class PayloadMessage
    {
        public string Target { get; set; }

        public object[] Arguments { get; set; }
    }
}
