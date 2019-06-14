using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace ChangeFeedFunctions
{
    public static class MigrationFunction
    {
        private static readonly string _endpointUrl = "<your-endpoint-url>";
        private static readonly string _primaryKey = "<your-primary-key>";
        private static readonly string _databaseId = "StoreDatabase";
        private static readonly string _containerId = "CartContainerByState";

        [FunctionName("MigrationFunction")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "StoreDatabase",
            collectionName: "CartContainer",
            ConnectionStringSetting = "DBConnection",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionName = "migrationLeases")]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                using (var client = new CosmosClient(_endpointUrl, _primaryKey))
                {
                    var db = client.GetDatabase(_databaseId);
                    var container = db.GetContainer(_containerId);

                    foreach (var doc in input)
                    {
                        var cartAction = JsonConvert.DeserializeObject<CartAction>(doc.ToString());

                        if (cartAction == null) continue;

                        log.LogInformation("Writing Migration Data");
                        await container.CreateItemAsync(cartAction, new Microsoft.Azure.Cosmos.PartitionKey(cartAction.BuyerState));
                    }
                }
            }
        }
    }
}
