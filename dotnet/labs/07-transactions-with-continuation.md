# Authoring Azure Cosmos DB Stored Procedures with Continuation Tokens

In this lab, you will author and execute multiple stored procedures within your Azure Cosmos DB instance. You will explore features unique to JavaScript stored procedures such as throwing errors for transaction rollback, logging using the JavaScript console and implementing a continuation model within a bounded execution enviornment.

## Setup

> Before you start this lab, you will need to create an Azure Cosmos DB database and collection that you will use throughout the lab.

### Retrieve Account Credentials

_The Data Migration Tool and .NET SDKs both require credentials to connect to your Azure Cosmos DB account. You will collect and store these credentials for use throughout the lab._

1. On the left side of the **Azure Cosmos DB** blade, locate the **Settings** section and click the **Keys** link.

1. In the **Keys** pane, record the values in the **CONNECTION STRING**, **URI** and **PRIMARY KEY** fields. You will use these values later in this lab.

   ![Credentials](../media/04-keys.jpg)

## Author Stored Procedures using the Continuation Model

_You will now implement stored procedures that may execute longer than the bounded execution limits on the server. You will implement the continuation model so that the stored procedures can "pick up where they left off" after they ran out of time in a previous execution._

### Create Bulk Upload and Bulk Delete Stored Procedures

1. From within the **Cosmos DB** resource blade, click on the **Data Explorer** link on the left

1. Expand the **NutritionDatabase** and then click to select the **FoodCollection**.

1. Click the **New Stored Procedure** button at the top of the **Data Explorer** section.

1. In the stored procedure tab, locate the **Stored Procedure Id** field and enter the value: **bulkUpload**.

1. Replace the contents of the _stored procedure editor_ with the following JavaScript code:

   ```js
   function bulkUpload(docs) {
     var container = getContext().getCollection();
     var containerLink = container.getSelfLink();
     var count = 0;
     if (!docs) throw new Error("The array is undefined or null.");
     var docsLength = docs.length;
     if (docsLength == 0) {
       getContext()
         .getResponse()
         .setBody(0);
       return;
     }
     tryCreate(docs[count], callback);
     function tryCreate(doc, callback) {
       var isAccepted = container.createDocument(containerLink, doc, callback);
       if (!isAccepted)
         getContext()
           .getResponse()
           .setBody(count);
     }
     function callback(err, doc, options) {
       if (err) throw err;
       count++;
       if (count >= docsLength) {
         getContext()
           .getResponse()
           .setBody(count);
       } else {
         tryCreate(docs[count], callback);
       }
     }
   }
   ```

   > This stored procedure uploads an array of documents in one batch. If the entire batch is not completed, the stored procedure will set the response body to the number of documents that were imported. Your client-side code is expected to call this stored procedure multiple times until all documents are imported.

   If you are having trouble copying the stored procedure above, the full source code for this stored procedure is located here: [bulk_upload.js](../solutions/05-authoring_stored_procedures/bulk_upload.js)

1. Click the **Save** button at the top of the tab.

1. Click the **New Stored Procedure** button at the top of the **Data Explorer** section.

1. In the stored procedure tab, locate the **Stored Procedure Id** field and enter the value: **bulkDelete**.

1. Replace the contents of the _stored procedure editor_ with the following JavaScript code:

   ```js
   function bulkDelete(query) {
     var container = getContext().getCollection();
     var containerLink = container.getSelfLink();
     var response = getContext().getResponse();
     var responseBody = {
       deleted: 0,
       continuation: true
     };
     if (!query) throw new Error("The query is undefined or null.");
     tryQueryAndDelete();
     function tryQueryAndDelete(continuation) {
       var requestOptions = { continuation: continuation };
       var isAccepted = container.queryDocuments(
         containerLink,
         query,
         requestOptions,
         function(err, retrievedDocs, responseOptions) {
           if (err) throw err;
           if (retrievedDocs.length > 0) {
             tryDelete(retrievedDocs);
           } else if (responseOptions.continuation) {
             tryQueryAndDelete(responseOptions.continuation);
           } else {
             responseBody.continuation = false;
             response.setBody(responseBody);
           }
         }
       );
       if (!isAccepted) {
         response.setBody(responseBody);
       }
     }
     function tryDelete(documents) {
       if (documents.length > 0) {
         var isAccepted = container.deleteDocument(
           documents[0]._self,
           {},
           function(err, responseOptions) {
             if (err) throw err;
             responseBody.deleted++;
             documents.shift();
             tryDelete(documents);
           }
         );
         if (!isAccepted) {
           response.setBody(responseBody);
         }
       } else {
         tryQueryAndDelete();
       }
     }
   }
   ```

   > This stored procedure iterates through all documents that match a specific query and deletes the documents. If the stored procedure is unable to delete all documents, it will return a continuation token. Your client-side code is expected to repeatedly call the stored procedure passing in a continuation token until the stored procedure does not return a continuation token.

   If you are having trouble copying the stored procedure above, the full source code for this stored procedure is located here: [bulk_delete.js](../solutions/05-authoring_stored_procedures/bulk_delete.js)

1. Click the **Save** button at the top of the tab.

### Create a .NET Core Project

1. On your local machine, create a new folder that will be used to contain the content of your .NET Core project.

1. In the new folder, right-click the folder and select the **Open with Code** menu option.

   ![Open with Visual Studio Code](../media/04-open_with_code.jpg)

   > Alternatively, you can run a command prompt in your current directory and execute the `code .` command.

1. In the Visual Studio Code window that appears, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

   ![Open in Command Prompt](../media/04-open_command_prompt.jpg)

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet new console --output .
   ```

   > This command will create a new .NET Core 2.2 project. The project will be a **console** project and the project will be created in the current directly since you used the `--output .` option.

1. Visual Studio Code will most likely prompt you to install various extensions related to **.NET Core** or **Azure Cosmos DB** development. None of these extensions are required to complete the labs.

1. In the terminal pane, enter and execute the following command:

   ```sh
   dotnet add package Microsoft.Azure.Cosmos --version 3.0.0
   ```

   > This command will add the [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos/) NuGet package as a project dependency. The lab instructions have been tested using the `3.0.0` version of this NuGet package.

1. In the terminal pane, enter and execute the following command:

   ```sh
   dotnet add package Bogus --version 22.0.8
   ```

   > This command will add the [Bogus](https://www.nuget.org/packages/Bogus/) NuGet package as a project dependency. This library will allow us to quickly generate test data using a fluent syntax and minimal code. We will use this library to generate test documents to upload to our Azure Cosmos DB instance. The lab instructions have been tested using the `22.0.8` version of this NuGet package.

1. In the terminal pane, enter and execute the following command:

   ```sh
   dotnet restore
   ```

   > This command will restore all packages specified as dependencies in the project.

1. In the terminal pane, enter and execute the following command:

   ```sh
   dotnet build
   ```

   > This command will build the project.

1. Click the **🗙** symbol to close the terminal pane.

1. Observe the **Program.cs** and **[folder name].csproj** files created by the .NET Core CLI.

1. Double-click the **[folder name].csproj** link in the **Explorer** pane to open the file in the editor.

1. Add a new **PropertyGroup** XML element to the project configuration within the **Project** element:

   ```xml
   <PropertyGroup>
       <LangVersion>latest</LangVersion>
   </PropertyGroup>
   ```

1. Your new XML should look like this:

   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
       <PropertyGroup>
           <LangVersion>latest</LangVersion>
       </PropertyGroup>
       <PropertyGroup>
           <OutputType>Exe</OutputType>
           <TargetFramework>netcoreapp2.2</TargetFramework>
       </PropertyGroup>
       <ItemGroup>
           <PackageReference Include="Bogus" Version="22.0.7" />
           <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.0.0" />
       </ItemGroup>
   </Project>
   ```

1. Double-click the **Program.cs** link in the **Explorer** pane to open the file in the editor.

### Create CosmosClient Instance

_The CosmosClient class is the main "entry point" to using the SQL API in Azure Cosmos DB. We are going to create an instance of the **CosmosClient** class by passing in connection metadata as parameters of the class' constructor. We will then use this class instance throughout the lab._

1. Within the **Program.cs** editor tab, Add the following using blocks to the top of the editor:

   ```csharp
   using Bogus;
   using System.Threading.Tasks;
   using Microsoft.Azure.Cosmos;
   using Microsoft.Azure.Cosmos.Scripts;
   using System.Collections.Generic;
   using System.Linq;
   using Newtonsoft.Json;
   ```

1. Locate the **Program** class and replace it with the following class:

   ```csharp
   public class Program
   {
       public static async Task Main(string[] args)
       {
       }
   }
   ```

1. Within the **Program** class, add the following lines of code to create variables for your connection information:

   ```csharp
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "NutritionDatabase";
    private static readonly string _containerId = "FoodCollection";
   ```

1. For the `_endpointUri` variable, replace the placeholder value with the **URI** value from your Azure Cosmos DB account that you recorded earlier in this lab:

   > For example, if your **uri** is `https://cosmosacct.documents.azure.com:443/`, your new variable assignment will look like this: `private static readonly string _endpointUri = "https://cosmosacct.documents.azure.com:443/";`.

1. For the `_primaryKey` variable, replace the placeholder value with the **PRIMARY KEY** value from your Azure Cosmos DB account that you recorded earlier in this lab:

   > For example, if your **primary key** is `elzirrKCnXlacvh1CRAnQdYVbVLspmYHQyYrhx0PltHi8wn5lHVHFnd1Xm3ad5cn4TUcH4U0MSeHsVykkFPHpQ==`, your new variable assignment will look like this: `private static readonly string _primaryKey = "elzirrKCnXlacvh1CRAnQdYVbVLspmYHQyYrhx0PltHi8wn5lHVHFnd1Xm3ad5cn4TUcH4U0MSeHsVykkFPHpQ==";`.

1. Locate the **Main** method:

   ```csharp
   public static async Task Main(string[] args)
   {
   }
   ```

1. Within the **Main** method, add the following lines of code to author a using block that creates and disposes a **CosmosClient** instance:

   ```csharp
   using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
   {

   }
   ```

1. Within the using block, add the following code to the method to obtain a reference to the `FoodCollection` container:

   ```csharp
    CosmosDatabase database = client.GetDatabase(_databaseId);
    CosmosContainer container = database.GetContainer(_containerId);
    CosmosScripts scripts = container.GetScripts();
   ```

1. Your `Program` class definition should now look like this:

   ```csharp
   public class Program
    {
        private static readonly string _endpointUri = "";
        private static readonly string _primaryKey = "";
        private static readonly string _databaseId = "NutritionDatabase";
        private static readonly string _containerId = "FoodCollection";

        public static async Task Main(string[] args)
        {
            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
            {
                CosmosDatabase database = client.GetDatabase(_databaseId);
                CosmosContainer container = database.GetContainer(_containerId);
            }
        }
    }
   ```

   > We will now execute a build of the application to make sure our code compiles successfully.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet build
   ```

   > This command will build the console project.You'll likely see warnings related to the lack of the `await` operator. You can ignore these for now.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

### Execute Bulk Upload Stored Procedure from .NET Core SDK

1. In the Visual Studio Code window, double click to open the **Program.cs** file

1. Following the definition of the **Program** class, add the following code to create a `Food` class:

   ```csharp
   public class Food
   {
       [JsonProperty("id")]
       public string Id { get; set; }
       [JsonProperty("description")]
       public string Description { get; set; }
       [JsonProperty("manufacturerName")]
       public string ManufacturerName { get; set; }
       [JsonProperty("foodGroup")]
       public string FoodGroup { get; set; }
   }
   ```

1. Save all of your open editor tabs.

1. Locate the **Main** method within the **Program** class:

   ```csharp
   public static async Task Main(string[] args)
   ```

1. Within the **using** block, add the following line of code to create a `scripts` variable that references the location of your stored procedures:

   ```csharp
   CosmosScripts scripts = container.GetScripts();
   ```

1. Still within the **using** block, add the following block of code to create a collection of 25,000 "fake" instances of the **Food** class.

   ```csharp
   List<Food> foods = new Faker<Food>()
                .RuleFor(p => p.Id, f => (-1 - f.IndexGlobal).ToString())
                .RuleFor(p => p.Description, f => f.Commerce.ProductName())
                .RuleFor(p => p.ManufacturerName, f => f.Company.CompanyName())
                .RuleFor(p => p.FoodGroup, f => "Energy Bars")
                .Generate(10000);
   ```

   > As a reminder, the Bogus library generates a set of test data. In this example, you are creating 10,000 items using the Bogus library and the rules listed above. The **Generate** method tells the Bogus library to use the rules to create the specified number of entities and store them in a generic **List<T>**.

1. Still within the **using** block, add the following line of code to create a variable named **pointer** with a default value of **zero**.

   ```csharp
   int pointer = 0;
   ```

   > We are going to use this variable to determine how many documents were uploaded by our stored procedure.

1. Still within the **using** block, add the following **while** block to continue to iterate code as long as the value of the **pointer** field is _less than_ the amount of items in the **food** collection:

   ```js
   while (pointer < foods.Count) {}
   ```

   > We are going to create a while loop that will keep uploading documents until the pointer's value greater than or equal to the amount of food objects in our object set.

1. Within the **while** block, add the following lines of code to execute the stored procedure:

   ```csharp
   StoredProcedureExecuteResponse<int> result = await scripts.ExecuteStoredProcedureAsync<IEnumerable<Food>, int>(new PartitionKey("Energy Bars"), "bulkUpload", foods.Skip(pointer));
   ```

   > This line of code will execute the stored procedure using three parameters; the partition key for the data set you are executing against, the name of the stored procedure, and a list of **food** objects to send to the stored procedure.

1. Still within the **while** block, add the following line of code to store the number returned by the stored procedure in the **pointer** variable:

   ```csharp
   pointer += result.Response;
   ```

   > Everytime the stored procedure returns how many documents were processed, we will increment the counter.

1. Still within the **while** block, add the following line of code to print out the amount of documents uploaded in the current iteration:

   ```csharp
   await Console.Out.WriteLineAsync($"{pointer} Total Items\t{result.Resource} Items Uploaded in this Iteration");
   ```

1. Your **Main** method should now look like this:

   ```csharp
    public static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
        {
            CosmosDatabase database = client.GetDatabase(_databaseId);
            CosmosContainer container = database.GetContainer(_containerId);
            CosmosScripts scripts = container.GetScripts();

            List<Food> foods = new Faker<Food>()
            .RuleFor(p => p.Id, f => (-1 - f.IndexGlobal).ToString())
            .RuleFor(p => p.Description, f => f.Commerce.ProductName())
            .RuleFor(p => p.ManufacturerName, f => f.Company.CompanyName())
            .RuleFor(p => p.FoodGroup, f => "Energy Bars")
            .Generate(10000);

            int pointer = 0;
            while (pointer < foods.Count)
            {
                StoredProcedureExecuteResponse<int> result = await scripts.ExecuteStoredProcedureAsync<IEnumerable<Food>, int>(new PartitionKey("Energy Bars"), "bulkUpload", foods.Skip(pointer));
                pointer += result.Resource;
                await Console.Out.WriteLineAsync($"{pointer} Total Items\t{result.Resource} Items Uploaded in this Iteration");
            }

        }
    }
   ```

   > You will notice that our C# code using the **Skip** method of the LINQ library to submit only the subset of our documents that are not yet uploaded. On the first execution of the while loop, we will skip **0** documents and attempt to upload all documents. When the stored procedure has finished executing, we will get a response indicating how many documents were uploaded. As an example, let's say **5000** documents were uploaded. The pointer will now be incremented to a value of **5000**. On the next check of the while loop's condition, **5000** will be evaluated to be less than **25000** causing another execution of the code in the while loop. The LINQ method will now skip **5000** documents and send the remaining **20000** documents to the stored procedure to upload. This loop will continue until all documents are uploaded. Also keep in mind that as of this writing, Cosmos DB has a 2 MB request limit on all calls. If your data is bigger than this test data, consider chaining `.Take()` to `foods.Skip(point)` to send a smaller payload with each request.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the results of the console project.

   > This stored procedure will batch upload 25,000 documents to your collection within the specified partition key.

1. Click the **🗙** symbol to close the terminal pane.

### Observe the Uploaded Documents in the Azure Portal

1. Return to the **Azure Portal** (<http://portal.azure.com>).

1. On the left side of the portal, click the **Resource groups** link.

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** _Resource Group_.

1. In the **cosmosgroup-lab** blade, select the **Azure Cosmos DB** account you recently created.

1. In the **Azure Cosmos DB** blade, locate and click the **Data Explorer** link on the left side of the blade.

1. In the **Data Explorer** section, expand the **NutritionDatabase** database node and then observe select the **FoodCollection** node.

1. Click the **New SQL Query** button at the top of the **Data Explorer** section.

1. In the query tab, replace the contents of the _query editor_ with the following SQL query:

   ```sql
   SELECT * FROM foods f WHERE f.foodGroup = "Energy Bars"
   ```

   > To validate that our documents were uploaded, we will issue a query to select all documents with the partition key we used earlier for the stored procedure's execution.

1. Click the **Execute Query** button in the query tab to run the query.

1. In the **Results** pane, observe the results of your query.

1. In the query tab, replace the contents of the _query editor_ with the following SQL query:

   ```sql
   SELECT COUNT(1) FROM foods f WHERE f.foodGroup = "Energy Bars"
   ```

   > This query will return a count of the documents that are in the **Energy Bars** partition key.

1. Click the **Execute Query** button in the query tab to run the query.

1. In the **Results** pane, observe the results of your query.

### Execute Bulk Delete Stored Procedure from .NET Core SDK

1. In the Visual Studio Code pane, double click the **Program.cs** file to open it in the editor.

1. Following the definition of the `Food` class, add the following code to create a `DeleteStatus` class:

   ```csharp
   public class DeleteStatus
   {
       public int Deleted { get; set; }
       public bool Continuation { get; set; }
   }
   ```

   > The next stored procedure returns a complex JSON object instead of a simple typed value. We will need to create a C# class to deserialize the JSON object so we can use it's data in our C# code.

1. Locate the **Main** method and delete any existing code:

   ```csharp
   public static async Task Main(string[] args)
   {

   }
   ```

1. Replace the **Main** method with the following implementation:

   ```csharp
   public static async Task Main(string[] args)
        {
            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
            {
                CosmosDatabase database = client.GetDatabase(_databaseId);
                CosmosContainer container = database.GetContainer(_containerId);
                CosmosScripts scripts = container.GetScripts();

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
   ```

   > This code will execute the stored procedure that deletes documents as long as the **resume** variable is set to true. The stored procedure itself always returns an object, serialized as **DeleteStatus**, that has a boolean indicating whether we should continue deleting documents and a number indicating how many documents were deleted as part of this execution. Within the do-while loop, we simply store the value of the boolean returned from the stored procedure in our **resume** variable and continue executing the stored procedure until it returns a false value indicating that all documents were deleted.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the results of the console project.

   > This stored procedure will delete all of the documents associated with the specified partition key. In this demo, this means we will delete the documents we batch uploaded earlier.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

1. Close the Visual Studio Code application.

### Query for Documents Within a Partition Key in the Azure Portal

1. Return to the **Azure Portal** (<http://portal.azure.com>).

1. On the left side of the portal, click the **Resource groups** link.

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** _Resource Group_.

1. In the **cosmosgroup-lab** blade, select the **Azure Cosmos DB** account you recently created.

1. In the **Azure Cosmos DB** blade, locate and click the **Data Explorer** link on the left side of the blade.

1. In the **Data Explorer** section, expand the **NutritionDatabase** database node and then observe select the **FoodCollection** node.

1. Click the **New SQL Query** button at the top of the **Data Explorer** section.

1. In the query tab, replace the contents of the _query editor_ with the following SQL query:

   ```sql
   SELECT COUNT(1) FROM foods f WHERE f.foodGroup = "Energy Bars"
   ```

   > This query will return a count of the documents that are in the **Energy Bars** partition key. This count should verify that all documents were deleted.

1. Click the **Execute Query** button in the query tab to run the query.

1. In the **Results** pane, observe the results of your query.

1. Close your browser application.
