﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Bogus;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared;

namespace DataGenerator
{
    public class Program
    {
        private static readonly string _endpointUrl = "<your-cosmosdb-endpoint-url>";
        private static readonly string _primaryKey = "<your-cosmosdb-primary-key>";
        private static readonly string _databaseId = "StoreDatabase";
        private static readonly string _containerName = "CartContainer";

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Press any key to stop the console app...");

            Randomizer random = new Randomizer();
            var items = new string[]
            {
                "Unisex Socks", "Women's Earring", "Women's Necklace", "Unisex Beanie",
                "Men's Baseball Hat", "Unisex Gloves", "Women's Flip Flop Shoes", "Women's Silver Necklace",
                "Men's Black Tee", "Men's Black Hoodie", "Women's Blue Sweater", "Women's Sweatpants",
                "Men's Athletic Shorts", "Women's Athletic Shorts", "Women's White Sweater", "Women's Green Sweater",
                "Men's Windbreaker Jacket", "Women's Sandal", "Women's Rainjacket", "Women's Denim Shorts",
                "Men's Fleece Jacket", "Women's Denim Jacket", "Men's Walking Shoes", "Women's Crewneck Sweater",
                "Men's Button-Up Shirt", "Women's Flannel Shirt", "Women's Light Jeans", "Men's Jeans",
                "Women's Dark Jeans", "Women's Red Top", "Men's White Shirt", "Women's Pant", "Women's Blazer Jacket", "Men's Puffy Jacket",
                "Women's Puffy Jacket", "Women's Athletic Shoes", "Men's Athletic Shoes", "Women's Black Dress", "Men's Suit Jacket", "Men's Suit Pant",
                "Women's High Heel Shoe", "Women's Cardigan Sweater", "Men's Dress Shoes", "Unisex Puffy Jacket", "Women's Red Dress", "Unisex Scarf",
                "Women's White Dress", "Unisex Sandals", "Women's Bag"
            };

            var states = new string[]
            {
                "AL","AK","AS","AZ","AR","CA","CO","CT","DE","DC","FM","FL","GA","GU","HI","ID","IL","IN",
                "IA","KS","KY","LA","ME","MH","MD","MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ","NM",
                "NY","NC","ND","MP","OH","OK","OR","PW","PA","PR","RI","SC","SD","TN","TX","UT","VT","VI",
                "VA","WA","WV","WI","WY"
            };

            var prices = new double[]
            {
               3.75, 8.00, 12.00, 10.00,
                17.00, 20.00, 14.00, 15.50,
                9.00, 25.00, 27.00, 21.00, 22.50,
                22.50, 32.00, 30.00, 49.99, 35.50,
                55.00, 50.00, 65.00, 31.99, 79.99,
                22.00, 19.99, 19.99, 80.00, 85.00,
                90.00, 33.00, 25.20, 40.00, 87.50, 99.99,
                95.99, 75.00, 70.00, 65.00, 92.00, 95.00,
                72.00, 25.00, 120.00, 105.00, 130.00, 29.99,
                84.99, 12.00, 37.50
            };

            Console.WriteLine("Starting Data Generation");

            while (!Console.KeyAvailable)
            {
                var itemIndex = random.Number(0, items.Length - 1);
                var stateIndex = random.Number(0, states.Length - 1);

                var item = new CartAction
                {
                    CartId = random.Number(1000, 99999),
                    Action = random.Enum<Shared.Action>(),
                    Item = items[itemIndex],
                    Price = prices[itemIndex],
                    BuyerState = states[stateIndex]
                };

                if (item.Action != Shared.Action.Viewed)
                {
                    var previousActions = new List<Shared.Action> { Shared.Action.Viewed };

                    if (item.Action == Shared.Action.Purchased)
                    {
                        previousActions.Add(Shared.Action.Added);
                    }

                    foreach (var action in previousActions)
                    {
                        var previousItem = new CartAction
                        {
                            CartId = item.CartId,
                            Action = action,
                            Item = item.Item,
                            Price = item.Price,
                            BuyerState = item.BuyerState
                        };

                        AddItem(previousItem);
                        Console.Write("*");
                    }
                }

                AddItem(item);
                Console.Write("*");
            }
        }

        private static async Task AddItem(CartAction item)
        {
            using (var client = new CosmosClient(_endpointUrl, _primaryKey))
            {
                var db = client.GetDatabase(_databaseId);
                var container = db.GetContainer(_containerName);

                await container.CreateItemAsync(item, new PartitionKey(item.Item));
            }
        }
    }
}