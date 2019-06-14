using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.Cosmos;

public class Program
{
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "NutritionDatabase";
    private static readonly string _collectionId = "PeopleCollection";
    private static readonly string _collectionId2 = "FoodCollection";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            Uri documentLink = UriFactory.CreateDocumentUri(_databaseId, _collectionId, "example.document");
            object doc = new
            {
                id = "example.document",
                FirstName = "Example",
                LastName = "Person"
            };
            try
            {
                ResourceResponse<Person> readResponse = await client.ReadDocumentAsync(documentLink);
                await Console.Out.WriteLineAsync($"Success: {readResponse.StatusCode}");
            }
            catch (DocumentClientException dex)
            {
                await Console.Out.WriteLineAsync($"Exception: {dex.StatusCode}");
            }

            var database = client.GetDatabase(_databaseId);
            var container = database.GetContainer(_collectionId);
            Uri collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
            var transactions = new Bogus.Faker<Transaction>()
                .RuleFor(t => t.amount, (fake) => Math.Round(fake.Random.Double(5, 500), 2))
                .RuleFor(t => t.processed, (fake) => fake.Random.Bool(0.6f))
                .RuleFor(t => t.paidBy, (fake) => $"{fake.Name.FirstName().ToLower()}.{fake.Name.LastName().ToLower()}")
                .RuleFor(t => t.costCenter, (fake) => fake.Commerce.Department(1).ToLower())
                .GenerateLazy(100);
            List<Task<ResourceResponse<Transaction>>> tasks = new List<Task<ResourceResponse<Transaction>>>();
            foreach (var transaction in transactions)
            {
                Task<ResourceResponse<Transaction>> resultTask = client.CreateDocumentAsync(collectionLink, transaction);
                tasks.Add(resultTask);
            }
            Task.WaitAll(tasks.ToArray());
            foreach (var task in tasks)
            {
                await Console.Out.WriteLineAsync($"Document Created\t{task.Result.Resource.Id}");
            }

            QueryRequestOptions options = new QueryRequestOptions
            {
                
                //EnableCrossPartitionQuery = true, 
                //PopulateQueryMetrics = true
            };
            string sql = "SELECT TOP 1000 * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
            FeedIterator<Transaction> query = container.CreateItemQuery<Transaction>(sql, 5, requestOptions: options);

        }
    }

    public class Transaction
    {
        public double amount { get; set; }
        public bool processed { get; set; }
        public string paidBy { get; set; }
        public string costCenter { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
    }

    public class Food
    {
        public string id { get; set; }
        public string description { get; set; }
        public List<Tag> tags { get; set; }
        public string foodGroup { get; set; }
    }
}