using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

public class Program
{
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";

    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            CosmosDatabase targetDatabase = await client.Databases.CreateDatabaseIfNotExistsAsync("EntertainmentDatabase");
            await Console.Out.WriteLineAsync($"Database Id:\t{targetDatabase.Id}");

            CosmosContainerResponse response = await targetDatabase.Containers.CreateContainerIfNotExistsAsync("DefaultCollection", "/id");
            CosmosContainer defaultContainer = response.Container;
            await Console.Out.WriteLineAsync($"Default Collection Id:\t{defaultContainer.Id}");

            IndexingPolicy indexingPolicy = new IndexingPolicy
            {
                IndexingMode = IndexingMode.Consistent,
                Automatic = true,
                IncludedPaths = new Collection<IncludedPath>
                {
                    new IncludedPath
                    {
                        Path = "/*",
                        //Indexes = new Collection<Index>
                        //{
                        //    new RangeIndex(DataType.Number, -1),
                        //    new RangeIndex(DataType.String, -1)
                        //}
                    }
                }
            };

            CosmosContainerSettings containerSettings = new CosmosContainerSettings("CustomCollection", $"/{nameof(IInteraction.type)}")
            {
                IndexingPolicy = indexingPolicy
            };
            var containerResponse = await targetDatabase.Containers.CreateContainerIfNotExistsAsync(containerSettings, 10000);
            var customCollection = containerResponse.Container;
            await Console.Out.WriteLineAsync($"Custom Collection Id:\t{customCollection.Id}");

            var foodInteractions = new Bogus.Faker<PurchaseFoodOrBeverage>()
                .RuleFor(i => i.id, (fake) => Guid.NewGuid().ToString())
                .RuleFor(i => i.type, (fake) => nameof(PurchaseFoodOrBeverage))
                .RuleFor(i => i.unitPrice, (fake) => Math.Round(fake.Random.Decimal(1.99m, 15.99m), 2))
                .RuleFor(i => i.quantity, (fake) => fake.Random.Number(1, 5))
                .RuleFor(i => i.totalPrice, (fake, user) => Math.Round(user.unitPrice * user.quantity, 2))
                .GenerateLazy(500);

            foreach (var interaction in foodInteractions)
            {
                CosmosItemResponse<PurchaseFoodOrBeverage> result = await customCollection.Items.CreateItemAsync(interaction.type, interaction);
                await Console.Out.WriteLineAsync($"Document Created\t{result.Resource.id}");
            }

            var tvInteractions = new Bogus.Faker<WatchLiveTelevisionChannel>()
                .RuleFor(i => i.id, (fake) => Guid.NewGuid().ToString())
                .RuleFor(i => i.type, (fake) => nameof(WatchLiveTelevisionChannel))
                .RuleFor(i => i.minutesViewed, (fake) => fake.Random.Number(1, 45))
                .RuleFor(i => i.channelName, (fake) => fake.PickRandom(new List<string> { "NEWS-6", "DRAMA-15", "ACTION-12", "DOCUMENTARY-4", "SPORTS-8" }))
                .GenerateLazy(500);

            foreach (var interaction in tvInteractions)
            {
                CosmosItemResponse<WatchLiveTelevisionChannel> result = await customCollection.Items.CreateItemAsync(interaction.type, interaction);
                await Console.Out.WriteLineAsync($"Document Created\t{result.Resource.id}");
            }

            var mapInteractions = new Bogus.Faker<ViewMap>()
                .RuleFor(i => i.id, (fake) => Guid.NewGuid().ToString())
                .RuleFor(i => i.type, (fake) => nameof(ViewMap))
                .RuleFor(i => i.minutesViewed, (fake) => fake.Random.Number(1, 45))
                .GenerateLazy(500);

            foreach (var interaction in mapInteractions)
            {
                CosmosItemResponse<ViewMap> result = await customCollection.Items.CreateItemAsync(interaction.type, interaction);
                await Console.Out.WriteLineAsync($"Document Created\t{result.Resource.id}");
            }

            CosmosResultSetIterator<GeneralInteraction> query = customCollection.Items.CreateItemQuery<GeneralInteraction>("SELECT * FROM c", 2);
            while (query.HasMoreResults)
            {
                foreach (GeneralInteraction interaction in await query.FetchNextSetAsync())
                {
                    Console.Out.WriteLine($"[{interaction.type}]\t{interaction.id}");
                }
            }
        }
    }
}

public interface IInteraction
{
    string type { get; }
}

public class GeneralInteraction : IInteraction
{
    public string id { get; set; }

    public string type { get; set; }
}

public class PurchaseFoodOrBeverage : IInteraction
{
    public string id { get; set; }
    public decimal unitPrice { get; set; }
    public decimal totalPrice { get; set; }
    public int quantity { get; set; }
    public string type { get; set; }
}

public class ViewMap : IInteraction
{
    public string id { get; set; }
    public int minutesViewed { get; set; }
    public string type { get; set; }
}

public class WatchLiveTelevisionChannel : IInteraction
{
    public string id { get; set; }
    public string channelName { get; set; }
    public int minutesViewed { get; set; }
    public string type { get; set; }
}