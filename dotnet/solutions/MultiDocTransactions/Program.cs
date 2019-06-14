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

    const string FoodGroup = "Snacks";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            CosmosDatabase database = client.GetDatabase("NutritionDatabase");
            CosmosContainer container = database.GetContainer("FoodCollection");
            CosmosScripts scripts = container.GetScripts();

            List<Food> people = new Faker<Food>()
                .RuleFor(p => p.Id, f => f.Random.Int().ToString())
                .RuleFor(p => p.Description, f => f.Commerce.ProductName())
                .RuleFor(p => p.ManufacturerName, f => f.Company.CompanyName())
                .RuleFor(p => p.FoodGroup, f => FoodGroup)
                .Generate(25000);
            int pointer = 0;
            while (pointer < people.Count)
            {
                StoredProcedureExecuteResponse<int> result = await scripts.ExecuteStoredProcedureAsync<IEnumerable<Food>, int>(new PartitionKey(FoodGroup), "bulkUpload", people.Skip(pointer));
                pointer += result.Resource;
                await Console.Out.WriteLineAsync($"{pointer} Total Items\t{result.Resource} Items Uploaded in this Iteration");
            }

            bool resume = true;
            do
            {
                string query = $"SELECT * FROM investors i WHERE i.company = '{FoodGroup}'";
                StoredProcedureExecuteResponse<DeleteStatus> result = await scripts.ExecuteStoredProcedureAsync<string, DeleteStatus>(new PartitionKey(FoodGroup), "bulkDelete", query);
                await Console.Out.WriteLineAsync($"Batch Delete Completed.\tDeleted: {result.Resource.Deleted}\tContinue: {result.Resource.Continuation}");
                resume = result.Resource.Continuation;
            }
            while (resume);
        }
    }

    public class Food
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string ManufacturerName { get; set; }
        public string FoodGroup { get; set; }
    }

    public class DeleteStatus
    {
        public int Deleted { get; set; }
        public bool Continuation { get; set; }
    }
}