## Build A Simple .NET Console App

_After using the Azure Portal's **Data Explorer** to query an Azure Cosmos DB container. You are now going to use the .NET SDK to issue similar queries._

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

   > This command will create a new .NET Core 2.2 project. The project will be a **console** project and the project will be created in the current directly since you used the `--output .` option.

1. Visual Studio Code will most likely prompt you to install various extensions related to **.NET Core** or **Azure Cosmos DB** development. None of these extensions are required to complete the labs.

1. In the terminal pane, enter and execute the following command:

   ```sh
   dotnet add package Microsoft.Azure.Cosmos --version 3.0.0.17-preview
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
            <TargetFramework>netcoreapp2.2</TargetFramework>
        </PropertyGroup>
        <ItemGroup>
            <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.0.0.17-preview" />
        </ItemGroup>
    </Project>

   ```

1. Double-click the **Program.cs** link in the **Explorer** pane to open the file in the editor.

   ![Open editor](../media/03-program_editor.jpg)

### Create CosmosClient Instance

_The CosmosClient class is the main "entry point" to using the SQL API in Azure Cosmos DB. We are going to create an instance of the **CosmosClient** class by passing in connection metadata as parameters of the class' constructor. We will then use this class instance throughout the lab._

1. Within the **Program.cs** editor tab, Add the following using blocks to the top of the editor:

   ```csharp
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using System.Collections.Generic;
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

1. Following the **Program** class definition, add the following class definitions that represent that data stored in Cosmos DB:

   ```csharp
   public class Tag
   {
       public string Name { get; set; }
   }

   public class Nutrient
   {
       public string Id { get; set; }
       public string Description { get; set; }
       public decimal NutritionValue { get; set; }
       public string Units { get; set; }
   }

   public class Serving
   {
       public decimal Amount { get; set; }
       public string Description { get; set; }
       public decimal WeightInGrams { get; set; }
   }

   public class Food
   {
       public string Id { get; set; }
       public string Description { get; set; }
       public string ManufacturerName { get; set; }
       public List<Tag> Tags { get; set; }
       public string FoodGroup { get; set; }
       public List<Nutrient> Nutrients { get; set; }
       public List<Serving> Servings { get; set; }

       public Food()
       {
            Tags = new List<Tag>();
            Nutrients = new List<Nutrient>();
            Servings = new List<Serving>();
       }
   }
   ```

1. Within the **Program** class, add the following lines of code to create variables for your connection information:

   ```csharp
    private static readonly string _endpointUri = "";
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "NutritionDatabse";
    private static readonly string _containerId = "FoodCollection";
   ```

1. For the `_endpointUri` variable, replace the placeholder value with the **URI** value from your Azure Cosmos DB account that you recorded earlier in this lab:

   > For example, if your **uri** is `https://cosmosacct.documents.azure.com:443/`, your new variable assignment will look like this: `private static readonly Uri _endpointUri = "https://cosmosacct.documents.azure.com:443/";`.

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
        private static readonly string _endpointUri = "";
        private static readonly string _primaryKey = "";
        private static readonly string _databaseId = "NutritionDatabase";
        private static readonly string _containerId = "FoodCollection";

        public static async Task Main(string[] args)
        {
            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
            {

            }
        }
    }
   ```

   > We are now going to implement a sample query to make sure our client connection code works.

### Execute a Query against Cosmos DB Using ReadItemAsync

_ReadItemAsync allows a single item to be retrieved from Cosmos DB by its ID_

1. Locate the using block within the **Main** method:

   ```csharp
   using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
   {

   }
   ```

1. Add the following lines of code to create a variable named `container` that references the Cosmos DB container identified as `_containerId`

   ```csharp
   var database = client.GetDatabase(_databaseId);
   var container = database.GetContainer(_containerId);
   ```

1. Add the following lines of code to use the **ReadItemAsync** function to retrieve a single item from your Cosmos DB by its `id` and write its description to the console.

   ```csharp
    ItemResponse<Food> candyResponse = await container.ReadItemAsync<Food>(new PartitionKey("Sweets"), "19130");
    Food candy = candyResponse.Resource;
    Console.Out.WriteLine($"Read {candy.Description}");
   ```

1. Save all of your open tabs in Visual Studio Code

1. Return to your terminal pane

   > If you've closed the terminal right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

1. You should see the following line output in the console, indiciating that **ReadItemAsync** completed successfully:

   ```sh
   Read Candies, HERSHEY''S POT OF GOLD Almond Bar
   ```

### Executing a Query Against a Single Cosmos DB Partition Using a SQL Item Query

1.  Return to Visual Studio Code

    > If you've closed Visual Studio code, re-open it from the command line with the following command:

        ```sh
        code .
        ```

1.  Find the last line of code you wrote:

    ```csharp
    Console.Out.WriteLine($"Read {candy.Description}");
    ```

1.  Create a SQL Query against your data, as follows:

    ```csharp
    string sqlA = "SELECT f.description, f.manufacturerName, f.servings FROM foods f WHERE f.foodGroup = 'Sweets'";
    ```

    > This query will select all food where the foodGroup is set to the value `sweets` you'll note that the syntax is very familiar if you've done work with SQL before. Also note that because this query has the partition key in the WHERE clause, this will be a single partition query.

1.  Add the following code to execute and read the results of this query

    ```csharp
    FeedIterator<Food> queryA = container.CreateItemQuery<Food>(new CosmosSqlQueryDefinition(sqlA), 1);
    foreach (Food food in await queryA.FetchNextSetAsync())
    {
        await Console.Out.WriteLineAsync($"{food.Description} by {food.ManufacturerName}");
        foreach (Serving serving in food.Servings)
        {
            await Console.Out.WriteLineAsync($"\t{serving.Amount} {serving.Description}");
        }
        await Console.Out.WriteLineAsync();
    }
    ```

    > Note the `1` being used as a parameter to **CosmosSqlQueryDefintion**. This parameter limits the maximum parallel query executions in the Cosmos DB service to 1.

1.  Save all of your open tabs in Visual Studio Code

1.  Return to your terminal pane

    > If you've closed the terminal right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1.  In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

1.  The code will loop through each result of the SQL query and output a message to the console similar to the following:

    ```sh
    ...

    Puddings, coconut cream, dry mix, instant by
        1 package (3.5 oz)
        1 portion, amount to make 1/2 cup

    ...
    ```

### Executing a Query Against Multiple Cosmos DB Partitions Using a SQL Item Query With Paging

1.  Return to Visual Studio Code

    > If you've closed Visual Studio code, re-open it from the command line with the following command:

        ```sh
        code .
        ```

1.  Following your `foreach` loop, create a SQL Query against your data, as follows:

    ```csharp
    string sqlB = @"SELECT f.id, f.description, f.manufacturerName, f.servings FROM foods f WHERE f.manufacturerName != null";
    ```

1.  Add the following line of code after the definition of `sqlB` to create your next item query:

    ```csharp
    FeedIterator<Food> queryB = container.CreateItemQuery<Food>(sqlB, 5, maxItemCount: 100);
    ```

    > Take note of the differences in this call to **CreateItemQuery** as compared to the previous section. **maxConcurrency** is set to `5` and we are limited the **MaxItemCount** to 100 items. This will result in paging if there are more than 100 items that match the query.

1.  Add the following lines of code to page through the results of this query using a while loop.

    ```csharp
    int pageCount = 0;
    while (queryB.HasMoreResults)
    {
        Console.Out.WriteLine($"---Page #{++pageCount:0000}---");
        foreach (var food in await queryB.FetchNextSetAsync())
        {
            Console.Out.WriteLine($"\t[{food.Id}]\t{food.Description,-20}\t{food.ManufacturerName,-40}");
        }
    }
    ```

1.  Save all of your open tabs in Visual Studio Code

1.  Return to your terminal pane

    > If you've closed the terminal right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1.  In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

1.  You should see a number of new resuls, each separated by the a line indicating the page, as follows:

    ```
        [19067] Candies, TWIZZLERS CHERRY BITES Hershey Food Corp.
    ---Page #0016---
        [19065] Candies, ALMOND JOY Candy Bar   Hershey Food Corp.
    ```

    > Note that the results are coming form multiple partitions.

### Projecting Query Results

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **New File** menu option.

1. Name the new file **StudentProfile.cs** . The editor tab will automatically open for the new file.

1. Paste in the following code for the `StudentProfile` and `StudentProfileEmailInformation` classes:

   ```csharp
   public class StudentProfile
   {
       public string Id { get; set; }
       public string Name { get; set; }
       public StudentProfileEmailInformation Email { get; set; }
   }

   public class StudentProfileEmailInformation
   {
       public string Home { get; set; }
       public string School { get; set; }
   }
   ```

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

1. Within the **Program.cs** editor tab, locate the **Main** method.

1. Within the **Main** method, locate the following line of code:

   ```csharp
   string sql = "SELECT VALUE activity FROM students s JOIN activity IN s.clubs WHERE s.enrollmentYear = 2018";
   ```

   Replace that line of code with the following code:

   ```csharp
   string sql = "SELECT VALUE { 'id': s.id, 'name': CONCAT(s.firstName, ' ', s.lastName), 'email': { 'home': s.homeEmailAddress, 'school': CONCAT(s.studentAlias, '@contoso.edu') } } FROM students s WHERE s.enrollmentYear = 2018";
   ```

   > This query will get relevant information about a student and format it to a specific JSON structure that our application expects. For your information, here's the query that we are using:

   ```sql
   SELECT VALUE {
       "id": s.id,
       "name": CONCAT(s.firstName, " ", s.lastName),
       "email": {
           "home": s.homeEmailAddress,
           "school": CONCAT(s.studentAlias, '@contoso.edu')
       }
   } FROM students s WHERE s.enrollmentYear = 2018
   ```

1. Locate the following line of code:

   ```csharp
   IQueryable<string> query = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql));
   ```

   Replace that code with the following code:

   ```csharp
   IQueryable<StudentProfile> query = client.CreateDocumentQuery<StudentProfile>(collectionLink, new SqlQuerySpec(sql));
   ```

1. Locate the following lines of code:

   ```csharp
   foreach(string activity in query)
   {
       await Console.Out.WriteLineAsync(activity);
   }
   ```

   Replace that code with the following code:

   ```csharp
   foreach(StudentProfile profile in query)
   {
       await Console.Out.WriteLineAsync($"[{profile.Id}]\t{profile.Name,-20}\t{profile.Email.School,-50}\t{profile.Email.Home}");
   }
   ```

   > This code uses the special alignment features of C# string formatting so you can see all properties of the `StudentProfile` instances.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the results of the execution.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

## Implement Pagination using the .NET SDK

_You will use the **HasMoreResults** boolean property and **ExecuteNextAsync** method of the **ResourceResponse** class to implement paging of your query results. Behind the scenes, these properties use a continuation token. A continuation token enables a client to retrieve the ‘next’ set of data in a follow-up query._

### Use the **HasMoreResults** and **ExecuteNextAsync** Members to Implement Pagination

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

1. Within the **Program.cs** editor tab, locate the **Main** method.

1. Within the **Main** method, locate the following line of code:

   ```csharp
   IQueryable<StudentProfile> query = client.CreateDocumentQuery<StudentProfile>(collectionLink, new SqlQuerySpec(sql));
   ```

   Replace that code with the following code:

   ```csharp
   IDocumentQuery<StudentProfile> query = client.CreateDocumentQuery<StudentProfile>(collectionLink, new SqlQuerySpec(sql), new FeedOptions { MaxItemCount = 100 }).AsDocumentQuery();
   ```

   > The DocumentQuery class will allow us to determine if there are more results available and page through results.

1. Locate the following lines of code:

   ```csharp
   foreach(StudentProfile profile in query)
   {
       await Console.Out.WriteLineAsync($"[{profile.Id}]\t{profile.Name,-20}\t{profile.Email.School,-50}\t{profile.Email.Home}");
   }
   ```

   Replace that code with the following code:

   ```csharp
   int pageCount = 0;
   while(query.HasMoreResults)
   {
       await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
       foreach(StudentProfile profile in await query.ExecuteNextAsync())
       {
           await Console.Out.WriteLineAsync($"\t[{profile.Id}]\t{profile.Name,-20}\t{profile.Email.School,-50}\t{profile.Email.Home}");
       }
   }
   ```

   > First we check if there are more results using the `HasMoreResults` property of the `IDocumentQuery<>` interface. If this value is set to true, we invoke the `ExecuteNextAsync` method to get the next batch of results and enumerate them using a `foreach` block.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the results of the execution.

   > You can view the current page count by looking at the headers in the console output.

1. Click the **🗙** symbol to close the terminal pane.

### Implement Continuation Token

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
       using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
       {
           await client.OpenAsync();

           Uri collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);

           string continuationToken = String.Empty;
           do
           {
               FeedOptions options = new FeedOptions
               {
                   EnableCrossPartitionQuery = true,
                   RequestContinuation = continuationToken
               };
               IDocumentQuery<Student> query = client
                   .CreateDocumentQuery<Student>(collectionLink, options)
                   .Where(student => student.age < 18)
                   .AsDocumentQuery();

               FeedResponse<Student> results = await query.ExecuteNextAsync<Student>();
               continuationToken = results.ResponseContinuation;

               await Console.Out.WriteLineAsync($"ContinuationToken:\t{continuationToken}");
               foreach(Student result in results)
               {
                   await Console.Out.WriteLineAsync($"[Age: {result.age}]\t{result.studentAlias}@consoto.edu");
               }
               await Console.Out.WriteLineAsync();
           }
           while (!String.IsNullOrEmpty(continuationToken));
       }
   }
   ```

   > A continuation token allows us to resume a paginated query either immediately or later. When creating a query, the results are automatically paged. If there are more results, the returned page of results will also include a continuation token. This token should be passed back in This implementation creates a **do-while** loop that will continue to get pages of results as long as the continuation token is not null.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the output of the console application.

   > You should see a list of documents grouped by "pages" of results. You should also see a continuation token associated with each page of results. This token can be used if you are in a client-server scenario where you need to continue a query that was executed earlier.

1. Click the **🗙** symbol to close the terminal pane.
