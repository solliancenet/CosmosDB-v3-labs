﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using shared;

namespace ChangeFeedConsole
{
    class Program
    {
        private static readonly string _endpointUrl = "";
        private static readonly string _primaryKey = "";
        private static readonly string _databaseId = "StoreDatabase";
        private static readonly string _containerId = "CartContainer";

        private static readonly string _destinationContainerId = "CartContainerByState";

        static async Task Main(string[] args)
        {
            using (var client = new CosmosClient(_endpointUrl, _primaryKey))
            {
                var db = client.GetDatabase(_databaseId);
                var container = db.GetContainer(_containerId);
                var destinationContainer = db.GetContainer(_destinationContainerId);

            }
        }
    }
}