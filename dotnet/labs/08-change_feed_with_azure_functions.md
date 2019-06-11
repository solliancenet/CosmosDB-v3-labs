# Change Feed with Azure Functions

In this lab, you will create a number of Azure Functions to consume data from the CosmosDB Change feed.

## Setup

> Before you start this lab, you will need to create an Azure Cosmos DB database and container, and populate it with existing data using the **Azure Data Factory (ADF)**

### Create Azure Cosmos DB Database and Container

_You will now create a database and container within your Azure Cosmos DB account._

1. On the left side of the portal, click the **Resource groups** link.

   ![Resource groups](../media/03-resource_groups.jpg)

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** _Resource Group_.

   ![Lab resource group](../media/03-lab_resource_group.jpg)

1. In the **cosmosgroup-lab** blade, select the **Azure Cosmos DB** account you recently created.

   ![Cosmos resource](../media/03-cosmos_resource.jpg)

1. In the **Azure Cosmos DB** blade, locate and click the **Overview** link on the left side of the blade.

   ![Overview pane](../media/03-overview.jpg)

1. At the top of the **Azure Cosmos DB** blade, click the **Add Container** button.

   ![Add container](../media/03-add_collection.jpg)

1. In the **Add Container** popup, perform the following actions:

   1. In the **Database id** field, select the **Create new** option and enter the value **NutritionDatabase**.

   1. Ensure the **Provision database throughput** option is not selected.

      > Provisioning throughput for a database allows you to share the throughput among all the containers that belong to that database. Within an Azure Cosmos DB database, you can have a set of containers which shares the throughput as well as containers, which have dedicated throughput.

   1. In the **Container Id** field, enter the value **FoodContainer**.

   1. In the **Partition key** field, enter the value `/foodGroup`.

   1. In the **Throughput** field, enter the value `11000`.

   1. Click the **+ Add Unique Key** link.

   1. In the new **Unique Keys** field, enter the value `/id`.

   1. Click the **OK** button.

   ![Add container](../media/04-add_collection_settings.jpg)

1. Wait for the creation of the new **database** and **container** to finish before moving on with this lab.

### Retrieve Account Credentials

_The .NET SDK requires credentials to connect to your Azure Cosmos DB account. You will collect and store these credentials for use throughout the lab._

1. On the left side of the **Azure Cosmos DB** blade, locate the **Settings** section and click the **Keys** link.

   ![Keys pane](../media/03-keys_pane.jpg)

1. In the **Keys** pane, record the values in the **CONNECTION STRING**, **URI** and **PRIMARY KEY** fields. You will use these values later in this lab.

   ![Credentials](../media/03-keys.jpg)

### Import Lab Data Into Container

You will use **Azure Data Factory (ADF)** to import the JSON array stored in the **students.json** file from Azure Blob Storage.

1. On the left side of the portal, click the **Resource groups** link.

   > To learn more about copying data to Cosmos DB with ADF, please read [ADF's documentation](https://docs.microsoft.com/en-us/azure/data-factory/connector-azure-cosmos-db).

   ![Resource groups](../media/03-resource_groups.jpg)

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** _Resource Group_.

3) Click **add** to add a new resource

   ![Add adf](../media/03-add_adf.jpg)

4) Search for **Data Factory** and select it

   ![adf-search](../media/03-adf_search.png)

5) Create a new **Data Factory**. You should name this data factory **importnutritiondata** with a unique number appended and select the relevant Azure subscription. You should ensure your existing **cosmosdblab-group** resource group is selected as well as a Version **V2**. Select **West US** as the region. Click **create**.

   ![df](../media/03-adf_selections.jpg)

6) After creation, open your newly created Data Factory. Select **Author & Monitor** and you will launch ADF. You should see a screen similar to the screenshot below. Select **Copy Data**. We will be using ADF for a one-time copy of data from a source JSON file on Azure Blob Storage to a database in Cosmos DB’s SQL API. ADF can also be used for more frequent data transfers from Cosmos DB to other data stores.
   ![](../media/03-adf_author&monitor.jpg)
   ![](../media/03-adf_copydata.jpg)

7) Edit basic properties for this data copy. You should name the task **ImportNutrition** and select to **Run once now**

   ![adf-properties](../media/03-adf_properties.jpg)

8) **Create a new connection** and select **Azure Blob Storage**. We will import data from a json file on Azure Blob Storage. In addition to Blob Storage, you can use ADF to migrate from a wide variety of sources. We will not cover migration from these sources in this tutorial.

   ![](../media/03-adf_blob.jpg)

9) Name the source **NutritionJson** and select **Use SAS URI** as the Authentication method. Please use the following SAS URI for read-only access to this Blob Storage container: https://cosmoslabs.blob.core.windows.net/?sv=2018-03-28&ss=bfqt&srt=sco&sp=rl&se=2020-04-01T13:14:14Z&st=2018-11-06T06:14:14Z&spr=https&sig=8HltMx1smolMcSmOhfVdC3drxtmTkq2%2BfJ8574uK60A%3D

   ![](../media/03-adf_connecttoblob.jpg)

<!-- 10) Select the **students** folder

    ![](../media/03-adf_choosestudents.jpg) -->

12. Do not check **Copy file recursively** and **Binary Copy**. Also ensure that **Compression Type** is "none".

    ![](../media/03-adf_source_next.jpg)

13. ADF should auto-detect the file format to be JSON. You can also select the file format as **JSON format.** You should also make sure you select **Array of Objects** as the File pattern.

    ![](../media/03-adf_source_dataset_format.jpg)

14. You have now successfully connected the Blob Storage container with the NutritionData.json file. You should select **NutritionJson** as the source and click **Next**.

    ![](../media/03-adf_SourceNext.JPG)

15. Add the Cosmos DB target data store by selecting **Create new connection** and selecting **Azure Cosmos DB (SQL API)**.

    ![](../media/03-adf_selecttarget.jpg)

16. Name the linked service **targetcosmosdb** and select your Azure subscription and Cosmos DB account. You should also select the Cosmos DB database that you created earlier.

    ![](../media/03-adf_selecttargetdb.jpg)

17. Select your newly created **targetcosmosdb** connection as the Destination date store.

    ![](../media/03-adf_destconnectionnext.jpg)

18. Select your container from the drop-down menu. You will map your Blob storage file to the correct Cosmos DB container. Select **Skip column mapping for all tables** before continuing.

    ![](../media/03-adf_correcttable.jpg)

19. You should have selected to skip column mappings in a previous step. Click through this screen.

    ![](../media/03-adf_destinationconnectionfinal.jpg)

20. There is no need to change any settings. Click **next**.

    ![](../media/03-adf_settings.jpg)

21. After deployment is complete, select **Monitor**.

    ![](../media/03-adf_deployment.jpg)

22. After a few minutes, refresh the page and the status for the ImportStudents pipeline should be listed as **Succeeded**.

    ![](../media/03-adf-succeeded.jpg)

23. Once the import process has completed, close the ADF. You will now proceed to validate your imported data.

## Executing Simple Queries

_The Azure Cosmos DB Data Explorer allows you to view documents and run queries directly within the Azure Portal. In this exercise, you will use the Data Explorer to view the data stored in our container._

### Validate Imported Data

_You will validate that the data was successfully imported into your container using the **Items** view in the **Data Explorer**._

1. Return to the **Azure Portal** (<http://portal.azure.com>).

1. On the left side of the portal, click the **Resource groups** link.

   ![Resource groups](../media/03-resource_groups.jpg)

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** _Resource Group_.

   ![Lab resource group](../media/03-lab_resource_group.jpg)

1. In the **cosmosgroup-lab** blade, select the **Azure Cosmos DB** account you recently created.

   ![Cosmos resource](../media/03-cosmos_resource.jpg)

1. In the **Azure Cosmos DB** blade, locate and click the **Data Explorer** link on the left side of the blade.

   ![Data Explorer pane](../media/03-data_explorer_pane.jpg)

1. In the **Data Explorer** section, expand the **NutritionDatabase** database node and then expand the **FoodContainer** container node.

   ![Container node](../media/03-collection_node.jpg)

1. Within the **FoodContainer** node, click the **Items** link to view a subset of the various documents in the container. Select a few of the documents and observe the properties and structure of the documents.

   ![Documents](../media/03-documents.jpg)

   ![Example document](../media/03-example_document.jpg)

### Create a .NET Core Project

1. On your local machine, create a new folder that will be used to contain the content of your .NET Core project.

1. In the new folder, right-click the folder and select the **Open with Code** menu option.

   ![Open with Visual Studio Code](../media/03-open_with_code.jpg)

   > Alternatively, you can run a command prompt in your current directory and execute the `code .` command.

1. In the Visual Studio Code window that appears, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

   ![Open in Command Prompt](../media/03-open_command_prompt.jpg)

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet new console --output .
   ```

   > This command will create a new .NET Core 2.1 project. The project will be a **console** project and the project will be created in the current directly since you used the `--output .` option.

1. Visual Studio Code will most likely prompt you to install various extensions related to **.NET Core** or **Azure Cosmos DB** development. None of these extensions are required to complete the labs.

1. In the terminal pane, enter and execute the following command:

   ```sh
   dotnet add package Microsoft.Azure.Cosmos --version 3.0.0
   ```

   > This command will add the [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos/) NuGet package as a project dependency. The lab instructions have been tested using the `3.0.0` version of this NuGet package.

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

   ![Project files](../media/03-project_files.jpg)

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
           <TargetFramework>netcoreapp2.0</TargetFramework>
       </PropertyGroup>
       <ItemGroup>
           <PackageReference Include="Microsoft.Azure.DocumentDB.Core" Version="1.9.1" />
       </ItemGroup>
   </Project>
   ```

1. Double-click the **Program.cs** link in the **Explorer** pane to open the file in the editor.

   ![Open editor](../media/03-program_editor.jpg)

### Create CosmosClient Instance

_The CosmosClient class is the main "entry point" to using the SQL API in Azure Cosmos DB. We are going to create an instance of the **CosmosClient** class by passing in connection metadata as parameters of the class' constructor. We will then use this class instance throughout the lab_

1. Within the **Program.cs** editor tab, Add the following using blocks to the top of the editor:

   ```csharp
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
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
   private static readonly string _containerId = "FoodContainer";
   ```

1. For the `_endpointUri` variable, replace the placeholder value with the **URI** value from your Azure Cosmos DB account that you recorded earlier in this lab:

   > For example, if your **uri** is `https://cosmosacct.documents.azure.com:443/`, your new variable assignment will look like this: `private static readonly Uri _endpointUri = new Uri("https://cosmosacct.documents.azure.com:443/");`.

1. For the `_primaryKey` variable, replace the placeholder value with the **PRIMARY KEY** value from your Azure Cosmos DB account that you recorded earlier in this lab:

   > For example, if your **primary key** is `NAye14XRGsHFbhpOVUWB7CMG2MOTAigdei5eNjxHNHup7oaBbXyVYSLW2lkPGeKRlZrCkgMdFpCEnOjlHpz94g==`, your new variable assignment will look like this: `private static readonly string _primaryKey = "NAye14XRGsHFbhpOVUWB7CMG2MOTAigdei5eNjxHNHup7oaBbXyVYSLW2lkPGeKRlZrCkgMdFpCEnOjlHpz94g==";`.

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

1. Your `Program` class definition should now look like this:

   ```csharp
   public class Program
   {
       private static readonly string _endpointUri = ("<your uri>";
       private static readonly string _primaryKey = "<your key>";
       private static readonly string _databaseId = "NutritionDatabase";
       private static readonly string _containerId = "FoodContainer";

       public static async Task Main(string[] args)
       {
           using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
           {
           }
       }
   }
   ```

   > We are now going to implement a sample query to make sure our client connection code works.

1. Locate the using block within the **Main** method:

   ```csharp
   using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
   {

   }
   ```

1. Add the following lines of code to create a variable named `database` that references the **NutritionDatabase** and `container` that references the **FoodCollection**:

   ```csharp
   CosmosDatabase database = client.GetDatabase(_databaseId);
   CosmosContainer container = database.GetContainer(_containerName);
   ```

1. Add the following line of code to create a string variable named `querySpec` that contains a sample SQL query:

   ```csharp
   var querySpec = new CosmosSqlQueryDefinition("SELECT TOP 5 * from f");
   ```

   > This query will get the top 5 items from the **FoodCollection**

1. After the closing bracket of your Main function, add the following class definitions:

   ```csharp
   internal sealed class Nutrient
       {
           [JsonProperty("id")]
           public string Id { get; set; }
           [JsonProperty("description")]
           public string Description { get; set; }
           [JsonProperty("nutritionValue")]
           public decimal NutritionValue { get; set; }
           [JsonProperty("units")]
           public string Units { get; set; }
       }

       internal sealed class Serving
       {
           [JsonProperty("amount")]
           public decimal Amount { get; set; }
           [JsonProperty("description")]
           public string Description { get; set; }
           [JsonProperty("weightInGrams")]
           public decimal WeightInGrams { get; set; }
       }

       internal sealed class Food
       {
           [JsonProperty("id")]
           public string Id { get; set; }
           [JsonProperty("description")]
           public string Description { get; set; }
           [JsonProperty("tags")]
           public List<Tag> Tags { get; set; }
           [JsonProperty("foodGroup")]
           public string FoodGroup { get; set; }
           [JsonProperty("nutrients")]
           public List<Nutrient> Nutrients { get; set; }
           [JsonProperty("servings")]
           public List<Serving> Servings { get; set; }
       }
   ```

1. Add the following line to create a FeedIterator for the query.

   ```csharp
   FeedIterator<Food> query = container.CreateItemQuery<Food>(querySpec, partitionKey: new PartiionKey("Baby Foods"));
   ```

1. Add the following lines of code to enumerate over the results and print the strings to the console:

   ```csharp
   while (query.HasMoreResults)
   {
       var resultSet = await query.FetchNextSetAsync();
       var results = resultSet.ToList();

       foreach (var result in results)
       {
           await Console.Out.WriteLineAsync(result.Description);
       }
   }
   ```

1. Your **Main** method should now look like this:

   ```csharp
   public static async Task Main(string[] args)
   {
       using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
       {
               CosmosDatabase database = client.GetDatabase(_databaseId);
               CosmosContainer container = database.GetContainer(_containerName);

               var queryText = "SELECT TOP 5 * FROM f";
               var querySpec = new CosmosSqlQueryDefinition(queryText);//.UseParameter("@description", "%formula%");

               FeedIterator<Food> query = container.CreateItemQuery<Food>(querySpec, partitionKey: new PartitionKey("Baby Foods"));

               while (query.HasMoreResults)
               {
                   var resultSet = await query.FetchNextSetAsync();
                   var results = resultSet.ToList();

                   foreach (var result in results)
                   {
                       await Console.Out.WriteLineAsync(result.Description);
                   }
               }
           }
       }
   }
   ```

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the results of the console project.

   > You should see five descriptions printed to the console window.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.
