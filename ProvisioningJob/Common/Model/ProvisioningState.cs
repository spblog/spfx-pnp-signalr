using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Model
{
    public class ProvisioningState : TableEntity
    {
        public  ProvisioningState() { }

        public int Total { get; set; }
        public int Progress { get; set; }
        public string Message { get; set; }
    }
}
