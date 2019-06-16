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

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            var database = client.GetDatabase(_databaseId);
            var peopleContainer = database.GetContainer(_peopleCollectionId);
            var transactionContainer = database.GetContainer(_transactionCollectionId);
            
        }
    }
}

public class Member
{
    public Person AccountHolder { get; set; }
    public Family Relatives { get; set; }
}

public class Family
{
    public Person Spouse { get; set; }
    public IEnumerable<Person> Children { get; set; }
}

public class Transaction
{
    public double amount { get; set; }
    public bool processed { get; set; }
    public string paidBy { get; set; }
    public string costCenter { get; set; }
}

