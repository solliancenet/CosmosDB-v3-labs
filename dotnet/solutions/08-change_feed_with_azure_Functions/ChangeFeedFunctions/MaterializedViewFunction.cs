using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Linq;
using Newtonsoft.Json.Converters;
using Shared;

namespace ChangeFeedFunctions
{
    public static class MaterializedViewFunction
    {
        private static readonly string _endpointUrl = "<your-cosmosdb-endpoint-url>";
        private static readonly string _primaryKey = "<your-cosmosdb-primary-key>";
        private static readonly string _databaseId = "StoreDatabase";
        private static readonly string _containerName = "StateSales";

        [FunctionName("MaterializedViewFunction")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "StoreDatabase",
            collectionName: "CartContainerByState",
            ConnectionStringSetting = "DBConnection",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionName = "materializedViewLeases")]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                var stateDict = new Dictionary<string, List<double>>();

                foreach (var doc in input)
                {
                    var action = JsonConvert.DeserializeObject<CartAction>(doc.ToString());

                    if (action.Action != Shared.Action.Purchased)
                    {
                        continue;
                    }

                    if (stateDict.ContainsKey(action.BuyerState))
                    {
                        stateDict[action.BuyerState].Add(action.Price);
                    }
                    else
                    {
                        stateDict.Add(action.BuyerState, new List<double> { action.Price });
                    }
                }

                using (var client = new CosmosClient(_endpointUrl, _primaryKey))
                {
                    var db = client.GetDatabase(_databaseId);
                    var container = db.GetContainer(_containerName);

                    var tasks = new List<Task>();

                    foreach (var key in stateDict.Keys)
                    {
                        log.LogInformation("Checking for a doc for state - " + key);

                        var query = new CosmosSqlQueryDefinition("select * from StateSales s where s.State = @state").UseParameter("@state", key);

                        FeedIterator<StateCount> resultSet = container.CreateItemQuery<StateCount>(query, partitionKey: new Microsoft.Azure.Cosmos.PartitionKey(key), maxItemCount: 1);

                        while (resultSet.HasMoreResults)
                        {
                            var stateCount = (await resultSet.FetchNextSetAsync()).FirstOrDefault();

                            if (stateCount == null)
                            {
                                log.LogInformation("State " + key + " has no results");
                                stateCount = new StateCount();
                                stateCount.State = key;
                                stateCount.TotalSales = stateDict[key].Sum();
                                stateCount.Count = stateDict[key].Count;
                            }
                            else
                            {
                                log.LogInformation("State " + key + " has a result.");
                                stateCount.Count += stateDict[key].Count;
                                stateCount.TotalSales += stateDict[key].Sum();
                            }

                            log.LogInformation("Upserting for partition " + stateCount.State);
                            tasks.Add(container.UpsertItemAsync(stateCount, new Microsoft.Azure.Cosmos.PartitionKey(stateCount.State)));
                        }

                    }

                    await Task.WhenAll(tasks);
                }
            }
        }


    }
}
