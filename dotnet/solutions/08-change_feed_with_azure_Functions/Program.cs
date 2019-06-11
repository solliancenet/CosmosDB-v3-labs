using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

public class Program
{
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "NutritionDatabase";
    private static readonly string _containerName = "FoodCollection";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            CosmosDatabase database = client.GetDatabase(_databaseId);
            CosmosContainer container = database.GetContainer(_containerName);

            var queryText = "SELECT TOP 5 * FROM f";
            var querySpec = new CosmosSqlQueryDefinition(queryText);//.UseParameter("@description", "%formula%");

            FeedIterator<Food> query = container.CreateItemQuery<Food>(querySpec, partitionKey: new PartitionKey("Baby Foods"));

            while (query.HasMoreResults)
            {
                var resultSet = await query.FetchNextSetAsync();
                var results = resultSet.ToList();

                foreach (var result in results)
                {
                    await Console.Out.WriteLineAsync(result.Description);
                }
            }
        }
    }
}

internal sealed class Tag
{
    [JsonProperty("name")]
    public string Name { get; set; }
}

internal sealed class Nutrient
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("nutritionValue")]
    public decimal NutritionValue { get; set; }
    [JsonProperty("units")]
    public string Units { get; set; }
}

internal sealed class Serving
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("weightInGrams")]
    public decimal WeightInGrams { get; set; }
}

internal sealed class Food
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("tags")]
    public List<Tag> Tags { get; set; }
    [JsonProperty("foodGroup")]
    public string FoodGroup { get; set; }
    [JsonProperty("nutrients")]
    public List<Nutrient> Nutrients { get; set; }
    [JsonProperty("servings")]
    public List<Serving> Servings { get; set; }
}
    }
