using System;
using Bogus;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;

public class Program
{
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            CosmosDatabase database = client.GetDatabase("NutritionDatabase");
            CosmosContainer container = database.GetContainer("FoodCollection");
            CosmosScripts scripts = container.GetScripts();

            List<Food> foods = new Faker<Food>()
                .RuleFor(p => p.id, f => (-1 - f.IndexGlobal).ToString())
                .RuleFor(p => p.description, f => f.Commerce.ProductName())
                .RuleFor(p => p.manufacturerName, f => f.Company.CompanyName())
                .RuleFor(p => p.foodGroup, f => "Energy Bars")
                .Generate(25000);
            int pointer = 0;
            while (pointer < foods.Count)
            {
                StoredProcedureExecuteResponse<int> result = await scripts.ExecuteStoredProcedureAsync<IEnumerable<Food>, int>(new PartitionKey("Energy Bars"), "bulkUpload", foods.Skip(pointer));
                pointer += result.Resource;
                await Console.Out.WriteLineAsync($"{pointer} Total Items\t{result.Resource} Items Uploaded in this Iteration");
            }

            bool resume = true;
            do
            {
                string query = "SELECT * FROM foods f WHERE f.foodGroup = 'Energy Bars'";
                StoredProcedureExecuteResponse<DeleteStatus> result = await scripts.ExecuteStoredProcedureAsync<string, DeleteStatus>(new PartitionKey("Energy Bars"), "bulkDelete", query);
                await Console.Out.WriteLineAsync($"Batch Delete Completed.\tDeleted: {result.Resource.Deleted}\tContinue: {result.Resource.Continuation}");
                resume = result.Resource.Continuation;
            }
            while (resume);
        }
    }

    public class Food
    {
        public string id { get; set; }
        public string description { get; set; }
        public string manufacturerName { get; set; }
        public string foodGroup { get; set; }
    }

    public class DeleteStatus
    {
        public int Deleted { get; set; }
        public bool Continuation { get; set; }
    }
}