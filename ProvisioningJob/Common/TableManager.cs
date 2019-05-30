using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common
{
    public class TableManager
    {
        private readonly CloudTable _table;

        public TableManager(string _cloudTableName, string connectionString)
        {
            if (string.IsNullOrEmpty(_cloudTableName))
            {
                throw new ArgumentNullException("Table", "Table Name can't be empty");
            }

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            _table = tableClient.GetTableReference(_cloudTableName);
            
            Task.Run(async () => await _table.CreateIfNotExistsAsync());
        }


        public void InsertEntity<T>(T entity) where T : TableEntity, new()
        {
            var insertOrMergeOperation = TableOperation.InsertOrReplace(entity);
            Task.Run(async () => await _table.ExecuteAsync(insertOrMergeOperation));
        }

        public T GetByKey<T>(string rowKey) where T : TableEntity, new()
        {
            var retrieve = TableOperation.Retrieve<T>(Consts.PartitionKey, rowKey);

            var result =  Task.Run(async () => await _table.ExecuteAsync(retrieve)).Result;

            return (T)result.Result;
        }
    }
}
