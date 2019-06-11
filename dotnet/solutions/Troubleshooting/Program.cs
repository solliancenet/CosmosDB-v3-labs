using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

public class Program
{
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "NutritionDatabase";
    private static readonly string _collectionId = "FoodCollection";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            var database = client.GetDatabase(_databaseId);
            var container = database.GetContainer(_collectionId);

            ItemResponse<Food> response = await container.ReadItemAsync<Food>(new PartitionKey("Fast Foods"), "21083");
            await Console.Out.WriteLineAsync($"Existing eTag:\t{response.ETag}");

            response.Resource.tags.Add(new Tag{ name = "Demo" });
            response = await container.UpsertItemAsync(response.Resource);
            await Console.Out.WriteLineAsync($"New eTag:\t{response.ETag}");
        }
    }

    public class Tag
    {
        public string name { get; set; }
    }

    public class Nutrient
    {
        public string id { get; set; }
        public string description { get; set; }
        public decimal nutritionValue { get; set; }
        public string units { get; set; }
    }

    public class Serving
    {
        public decimal amount { get; set; }
        public string description { get; set; }
        public decimal weightInGrams { get; set; }
    }

    public class Food
    {
        public string id { get; set; }
        public string description { get; set; }
        public List<Tag> tags { get; set; }
        public string foodGroup { get; set; }
        public List<Nutrient> nutrients { get; set; }
        public List<Serving> servings { get; set; }
    }
}