using Microsoft.Azure.WebJobs;

namespace ProvisioningJob
{
    class Program
    {
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
