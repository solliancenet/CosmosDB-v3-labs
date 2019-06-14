﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

public class Program
{
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "NutritionDatabase";
    private static readonly string _containerId = "FoodCollection";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            var database = client.GetDatabase(_databaseId);
            var container = database.GetContainer(_containerId);

            await container.ReadItemAsync<Food>(new PartitionKey("Sweets"), "19130");

            string sqlA = "SELECT f.description, f.manufacturerName, s.servings FROM foods f WHERE f.foodGroup = 'Sweets'";
            FeedIterator<Food> queryA = container.CreateItemQuery<Food>(new CosmosSqlQueryDefinition(sqlA), 1);
            foreach (Food food in await queryA.FetchNextSetAsync())
            {
                await Console.Out.WriteLineAsync($"{food.Description} by {food.ManufacturerName}");
                foreach (Serving serving in food.Servings)
                {
                    await Console.Out.WriteLineAsync($"\t{serving.Amount} {serving.Description}");
                }
                await Console.Out.WriteLineAsync();
            }


            IOrderedQueryable<Food> sweets = container.CreateItemQuery<Food>("Sweets", requestOptions: new QueryRequestOptions { MaxItemCount = 100, ConsistencyLevel = ConsistencyLevel.Session });
            var healthySweets = sweets.Where(f => f.Nutrients.Where(n => n.Description == "Sugars, total").Any(n => n.NutritionValue < 30));


            string sqlB = "SELECT VALUE { 'id': f.id, 'productName': f.description, 'company': f.manufacturerName 'package': { 'name': f.servings[0].description, 'weight': CONCAT(f.servings[0].weightInGrams, 'g') } } FROM foods f WHERE f.manufacturerName IS NOT NULL";
            FeedIterator<GroceryProduct> queryB = container.CreateItemQuery<GroceryProduct>(sqlB, 5, requestOptions: new QueryRequestOptions{ MaxItemCount = 100, ConsistencyLevel = ConsistencyLevel.Session });

            FeedResponse<GroceryProduct> feedResponse = await queryB.FetchNextSetAsync();
            string continuation = feedResponse.Continuation;
            foreach (var product in feedResponse)
            {
                Console.Out.WriteLine($"\t[{product.Id}]\t{product.ProductName,-20}\t{product.Company,-40}\t{product.Package.Name}");
            }
            FeedIterator<GroceryProduct> queryB2 = container.CreateItemQuery<GroceryProduct>(sqlB, 5, continuationToken: continuation);
            FeedResponse<GroceryProduct> feedResponse2 = await queryB2.FetchNextSetAsync();
            Console.Out.WriteLine($"Continuing with {feedResponse2.Count} items");


            string sqlC = "SELECT f.description, f.manufacturerName, s.servings FROM foods f WHERE f.manufacturerName IS NOT NULL";
            FeedIterator<Food> queryC = container.CreateItemQuery<Food>(sqlC, 5, maxItemCount: 100);
            int pageCount = 0;
            while (queryC.HasMoreResults)
            {
                Console.Out.WriteLine($"---Page #{++pageCount:0000}---");
                foreach (var food in await queryC.FetchNextSetAsync())
                {
                    Console.Out.WriteLine($"\t[{food.Id}]\t{food.Description,-20}\t{food.ManufacturerName,-40}");
                }
            }
        }
    }
}

public class Tag
{
    public string Name { get; set; }
}

public class Nutrient
{
    public string Id { get; set; }
    public string Description { get; set; }
    public decimal NutritionValue { get; set; }
    public string Units { get; set; }
}

public class Serving
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public decimal WeightInGrams { get; set; }
}

public class Food
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string ManufacturerName { get; set; }
    public List<Tag> Tags { get; set; }
    public string FoodGroup { get; set; }
    public List<Nutrient> Nutrients { get; set; }
    public List<Serving> Servings { get; set; }
}

public class GroceryProduct
{
    public string Id { get; set; }
    public string ProductName { get; set; }
    public string Company { get; set; }
    public RetailPackage Package { get; set; }
}

public class RetailPackage
{
    public string Name { get; set; }
    public int Weight { get; set; }
}
