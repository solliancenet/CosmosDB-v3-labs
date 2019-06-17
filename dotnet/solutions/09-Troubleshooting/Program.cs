using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.Cosmos;

public class Program
{
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "FinancialDatabase";
    private static readonly string _peopleCollectionId = "PeopleCollection";
    private static readonly string _transactionCollectionId = "TransactionCollection";

    internal static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            var database = client.GetDatabase(_databaseId);
            var peopleContainer = database.GetContainer(_peopleCollectionId);
            var transactionContainer = database.GetContainer(_transactionCollectionId);

        }
    }

    private static async Task IndexTuning(CosmosContainer peopleContainer)
    {
        object member = new Member
        {
            accountHolder = new Bogus.Person(),
            relatives = new Family
            {
                spouse = new Bogus.Person(),
                children = Enumerable.Range(0, 4).Select(r => new Bogus.Person())
            }
        };
        ItemResponse<object> response = await peopleContainer.CreateItemAsync(member);
        await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
    }

    private static async Task ObserveThrottling(CosmosContainer transactionContainer)
    {
        var transactions = new Bogus.Faker<Transaction>()
            .RuleFor(t => t.id, (fake) => Guid.NewGuid().ToString())
            .RuleFor(t => t.amount, (fake) => Math.Round(fake.Random.Double(5, 500), 2))
            .RuleFor(t => t.processed, (fake) => fake.Random.Bool(0.6f))
            .RuleFor(t => t.paidBy, (fake) => $"{fake.Name.FirstName().ToLower()}.{fake.Name.LastName().ToLower()}")
            .RuleFor(t => t.costCenter, (fake) => fake.Commerce.Department(1).ToLower())
            .GenerateLazy(5000);

        List<Task<ItemResponse<Transaction>>> tasks = new List<Task<ItemResponse<Transaction>>>();
        foreach (var transaction in transactions)
        {
            Task<ItemResponse<Transaction>> resultTask = transactionContainer.CreateItemAsync(transaction);
            tasks.Add(resultTask);
        }
        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            await Console.Out.WriteLineAsync($"Item Created\t{task.Result.Resource.id}");
        }
    }

    private static async Task MeasuringRuCharge(CosmosContainer transactionContainer)
    {
        //string sql = "SELECT TOP 1000 * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
        //string sql = "SELECT * FROM c WHERE c.processed = true";
        //string sql = "SELECT * FROM c";
        string sql = "SELECT c.id FROM c";
        FeedIterator<Transaction> query = transactionContainer.CreateItemQuery<Transaction>(sql, 1);
        var result = await query.FetchNextSetAsync();
        await Console.Out.WriteLineAsync($"Request Charge: {result.RequestCharge} RUs");
    }

    private static async Task QueryOptions(CosmosContainer transactionContainer)
    {
        int maxItemCount = 1000;
        int maxDegreeOfParallelism = -1;
        int maxBufferedItemCount = 50000;

        QueryRequestOptions options = new QueryRequestOptions
        {
            MaxItemCount = maxItemCount,
            MaxBufferedItemCount = maxBufferedItemCount
        };

        await Console.Out.WriteLineAsync($"MaxItemCount:\t{maxItemCount}");
        await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{maxDegreeOfParallelism}");
        await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{maxBufferedItemCount}");

        string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";

        Stopwatch timer = Stopwatch.StartNew();

        FeedIterator<Transaction> query = transactionContainer.CreateItemQuery<Transaction>(sql, maxDegreeOfParallelism, requestOptions: options);
        while (query.HasMoreResults)
        {
            var result = await query.FetchNextSetAsync();
        }
        timer.Stop();
        await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
    }

    private static async Task DirectQuery(CosmosContainer peopleContainer)
    {
        string sql = "SELECT TOP 1 * FROM c WHERE c.id = 'example.document'";
        FeedIterator<object> query = peopleContainer.CreateItemQuery<object>(sql, -1);
        FeedResponse<object> response = await query.FetchNextSetAsync();
        await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
        await Console.Out.WriteLineAsync($"{response.Resource.First()}");

        ItemResponse<object> itemResponse = await peopleContainer.ReadItemAsync<object>(new PartitionKey("Cremin"), "example.document");
        await Console.Out.WriteLineAsync($"{itemResponse.RequestCharge} RUs");
    }
}

public class Member
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public Person accountHolder { get; set; }
    public Family relatives { get; set; }
}

public class Family
{
    public Person spouse { get; set; }
    public IEnumerable<Person> children { get; set; }
}

public class Transaction
{
    public string id { get; set; }
    public double amount { get; set; }
    public bool processed { get; set; }
    public string paidBy { get; set; }
    public string costCenter { get; set; }
}

