# Optimistic Concurrency Control

## Viewing the ETag Property of a Requested Resource

_The SQL API supports optimistic concurrency control (OCC) through HTTP entity tags, or ETags. Every SQL API resource has an ETag, and the ETag is set on the server every time an item is updated. In this exercise, we will view the ETag property of a resource that is requested using the SDK._

### Create a .NET Core Project

1. On your local machine, create a new folder that will be used to contain the content of your .NET Core project.

1. In the new folder, right-click the folder and select the **Open with Code** menu option.

   ![Open with Visual Studio Code](../media/05-open_with_code.jpg)

   > Alternatively, you can run a command prompt in your current directory and execute the `code .` command.

1. In the Visual Studio Code window that appears, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

   ![Open in Command Prompt](../media/05-open_command_prompt.jpg)

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet new console --output .
   ```

   > This command will create a new .NET Core 2.2 project. The project will be a **console** project and the project will be created in the current directly since you used the `--output .` option.

1. Visual Studio Code will most likely prompt you to install various extensions related to **.NET Core** or **Azure Cosmos DB** development. None of these extensions are required to complete the labs.

1. In the terminal pane, enter and execute the following command:

   ```sh
   dotnet add package Microsoft.Azure.Cosmos --version 3.0.0.18-preview
   ```

   > This command will add the [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos/) NuGet package as a project dependency. The lab instructions have been tested using the `3.0.0.18-preview` version of this NuGet package.

1) In the terminal pane, enter and execute the following command:

   ```sh
   dotnet restore
   ```

   > This command will restore all packages specified as dependencies in the project.

1) In the terminal pane, enter and execute the following command:

   ```sh
   dotnet build
   ```

   > This command will build the project.

1) Click the **🗙** symbol to close the terminal pane.

1) Observe the **Program.cs** and **[folder name].csproj** files created by the .NET Core CLI.

   ![Project files](../media/05-project_files.jpg)

1) Double-click the **[folder name].csproj** link in the **Explorer** pane to open the file in the editor.

1) Add a new **PropertyGroup** XML element to the project configuration within the **Project** element:

   ```xml
   <PropertyGroup>
       <LangVersion>latest</LangVersion>
   </PropertyGroup>
   ```

1) Your new XML should look like this:

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
           <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.0.0.18-preview" />
       </ItemGroup>
   </Project>
   ```

1) Double-click the **Program.cs** link in the **Explorer** pane to open the file in the editor.

   ![Open editor](../media/05-program_editor.jpg)

### Create CosmosClient Instance

_The CosmosClient class is the main "entry point" to using the SQL API in Azure Cosmos DB. We are going to create an instance of the **CosmosClient** class by passing in connection metadata as parameters of the class' constructor. We will then use this class instance throughout the lab._

1. Within the **Program.cs** editor tab, Add the following using blocks to the top of the editor:

   ```csharp
   using System.Threading.Tasks;
   using Microsoft.Azure.Cosmos;
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

1. For the ``_endpointUri`` variable, replace the placeholder value with the **URI** value and for the ``_primaryKey`` variable, replace the placeholder value with the **PRIMARY KEY** value from your Azure Cosmos DB account. Use [these instructions](00-account_setup.md) to get these values if you do not already have them:

    > For example, if your **uri** is ``https://cosmosacct.documents.azure.com:443/``, your new variable assignment will look like this: ``private static readonly string _endpointUri = "https://cosmosacct.documents.azure.com:443/";``.

    > For example, if your **primary key** is ``elzirrKCnXlacvh1CRAnQdYVbVLspmYHQyYrhx0PltHi8wn5lHVHFnd1Xm3ad5cn4TUcH4U0MSeHsVykkFPHpQ==``, your new variable assignment will look like this: ``private static readonly string _primaryKey = "elzirrKCnXlacvh1CRAnQdYVbVLspmYHQyYrhx0PltHi8wn5lHVHFnd1Xm3ad5cn4TUcH4U0MSeHsVykkFPHpQ==";``.

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
       private static readonly Uri _endpointUri = new Uri("<your uri>");
       private static readonly string _primaryKey = "<your key>";
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

   > We will now execute build the application to make sure our code compiles successfully.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet build
   ```

   > This command will build the console project.

1. Click the **🗙** symbol to close the terminal pane.

### Create Classes

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **New File** menu option.

   ![New File](../media/02-new_file.jpg)

1. Name the new file **Tag.cs** . The editor tab will automatically open for the new file.

1. Paste in the following code for the `Tag` class:

   ```csharp
   public class Tag
   {
       public string name { get; set; }
   }
   ```

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **New File** menu option.

1. Name the new file **Food.cs** . The editor tab will automatically open for the new file.

1. Paste in the following code for the `Food` class:

   ```csharp
    using System.Collections.Generic;

    public class Food
    {
        public string id { get; set; }
        public string description { get; set; }
        public List<Tag> tags { get; set; }
        public string foodGroup { get; set; }
    }
   ```

1. Save all of your open editor tabs.

### Observe the ETag Property

1. Double-click the **Program.cs** link in the **Explorer** pane to open the file in the editor.

1. Locate the _using_ block within the **Main** method:

   ```csharp
   using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
   {

   }
   ```

1. Add the following code to the method to create a reference to an existing container:

   ```csharp
   var database = client.GetDatabase(_databaseId);
   var container = database.GetContainer(_containerId);
   ```

1. Add the following code to asynchronously read a single item from the container, identified by its partition key and id:

   ```csharp
   ItemResponse<Food> response = await container.ReadItemAsync<Food>("21083", new PartitionKey("Fast Foods"));
   ```

1. Add the following line of code to show the current ETag value of the item:

   ```csharp
   await Console.Out.WriteLineAsync($"ETag: {response.ETag}");
   ```

   > The ETag header and the current value are included in all response messages.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the output of the console application.

   > You should see an ETag for the item.

1. Enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the output of the console application.

   > The ETag should remain unchanged since the item has not been changed.

1. Click the **🗙** symbol to close the terminal pane.

1. Locate the _using_ block within the **Main** method:

   ```csharp
   using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
   {
   }
   ```

1. Within the **Main** method, locate the following line of code:

   ```csharp
   await Console.Out.WriteLineAsync($"ETag:\t{response.ETag}");
   ```

   Replace that line of code with the following code:

   ```csharp
   await Console.Out.WriteLineAsync($"Existing ETag:\t{response.ETag}");
   ```

1. Within the **using** block, add a new line of code to create an **ItemRequestOptions** instance that will use the **ETag** from the item and specify an **If-Match** header:

   ```csharp
   ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = response.ETag };
   ```

1. Add a new line of code to update a property of the retrieved item:

   ```csharp
   response.Resource.tags.Add(new Tag { name = "Demo" });
   ```

   > This line of code will modify a property of the item. Here we are modifying the **tags** collection property by adding a new **Tag** object.

1. Add a new line of code to invoke the **UpsertItemAsync** method passing in both the item and the options:

   ```csharp
   response = await container.UpsertItemAsync(response.Resource, requestOptions: requestOptions);
   ```

1. Add a new line of code to print out the **ETag** of the newly updated item:

   ```csharp
   await Console.Out.WriteLineAsync($"New ETag:\t{response.ETag}");
   ```

1. Your **Main** method should now look like this:

   ```csharp
   public static async Task Main(string[] args)
   {
       using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
       {
           var database = client.GetDatabase(_databaseId);
           var container = database.GetContainer(_containerId);

           ItemResponse<Food> response = await container.ReadItemAsync<Food>("21083", new PartitionKey("Fast Foods"));
           await Console.Out.WriteLineAsync($"Existing ETag:\t{response.ETag}");

           ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = response.ETag };
           response.Resource.tags.Add(new Tag { name = "Demo" });
           response = await container.UpsertItemAsync(response.Resource, requestOptions: requestOptions);
           await Console.Out.WriteLineAsync($"New ETag:\t{response.ETag}");
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

1. Observe the output of the console application.

   > You should see that the value of the ETag property has changed. The **ItemRequestOptions** class helped us implement optimistic concurrency by specifying that we wanted the SDK to use the If-Match header to allow the server to decide whether a resource should be updated. The If-Match value is the ETag value to be checked against. If the ETag value matches the server ETag value, the resource is updated. If the ETag is no longer current, the server rejects the operation with an "HTTP 412 Precondition failure" response code. The client then re-fetches the resource to acquire the current ETag value for the resource.

1. Click the **🗙** symbol to close the terminal pane.

1. Locate the _using_ block within the **Main** method:

   ```csharp
   using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
   {
   }
   ```

1. Within the **using** block, add a new line of code to again update a property of the item:

   ```csharp
   response.Resource.tags.Add(new Tag { name = "Failure" });
   ```

1. Add a new line of code to again invoke the **UpsertItemAsync** method passing in both the updated item and the same options as before:

   ```csharp
   response = await container.UpsertItemAsync(response.Resource, requestOptions: requestOptions);
   ```

   > The **ItemRequestOptions** instance has not been updated, so is still using the ETag value from the original object, which is now out of date so we should expect to now get an error.

1. Add error handling to the **UpsertItemAsync** call you just added by wrapping it with a try-catch and then output the resulting error message. The code should now look like this:

   ```csharp
   try
   {
       response = await container.UpsertItemAsync(response.Resource, requestOptions: requestOptions);
   }
   catch (Exception ex)
   {
       await Console.Out.WriteLineAsync($"Update error:\t{ex.Message}");
   }
   ```

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

   ```sh
   dotnet run
   ```

   > This command will build and execute the console project.

1. Observe the output of the console application.

   > You should see that the second update call fails because value of the ETag property has changed. The **ItemRequestOptions** class specifying the original ETag value as an If-Match header caused the server to decide to reject the update operation with an "HTTP 412 Precondition failure" response code.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

1. Close the Visual Studio Code application.

1. Close your browser application.
