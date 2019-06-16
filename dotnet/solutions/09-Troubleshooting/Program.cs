using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.Cosmos;

public partial class Program
{
    //private static readonly string _endpointUri = "";
    //private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "FinancialDatabase";
    private static readonly string _peopleCollectionId = "PeopleCollection";
    private static readonly string _transactionCollectionId = "TransactionCollection";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            var database = client.GetDatabase(_databaseId);
            var peopleContainer = database.GetContainer(_peopleCollectionId);
            var transactionContainer = database.GetContainer(_transactionCollectionId);
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
    public double amount { get; set; }
    public bool processed { get; set; }
    public string paidBy { get; set; }
    public string costCenter { get; set; }
}

