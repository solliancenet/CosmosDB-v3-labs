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

            List<Person> people = new Faker<Person>()
                .RuleFor(p => p.firstName, f => f.Name.FirstName())
                .RuleFor(p => p.lastName, f => f.Name.LastName())
                .RuleFor(p => p.company, f => "contosofinancial")
                .Generate(25000);
            int pointer = 0;
            while (pointer < people.Count)
            {
                var result = await scripts.ExecuteStoredProcedureAsync<int, Person>(new PartitionKey("contosofinancial"), "bulkUpload", people.Skip(pointer));
                pointer += result.Response;
                await Console.Out.WriteLineAsync($"{pointer} Total Documents\t{result.Response} Documents Uploaded in this Iteration");
            }

            Uri sprocLinkDelete = UriFactory.CreateStoredProcedureUri("FinancialDatabase", "InvestorCollection", "bulkDelete");
            bool resume = true;
            do
            {
                RequestOptions options = new RequestOptions { PartitionKey = new PartitionKey("contosofinancial") };
                string query = "SELECT * FROM investors i WHERE i.company = 'contosofinancial'";
                StoredProcedureResponse<DeleteStatus> result = await scripts.ExecuteStoredProcedureAsync<DeleteStatus>(sprocLinkDelete, options, query);
                await Console.Out.WriteLineAsync($"Batch Delete Completed.\tDeleted: {result.Response.Deleted}\tContinue: {result.Response.Continuation}");
                resume = result.Response.Continuation;
            }
            while (resume);
        }
    }

    public class Person
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string company { get; set; }
    }

    public class DeleteStatus
    {
        public int Deleted { get; set; }
        public bool Continuation { get; set; }
    }
}