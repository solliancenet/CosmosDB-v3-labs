## Build A Simple .NET Console App

*After using the Azure Portal's **Data Explorer** to query an Azure Cosmos DB collection, you are now going to use the .NET SDK to issue similar queries.*

### Create a .NET Core Project

1. On your local machine, create a new folder that will be used to contain the content of your .NET Core project.

1. In the new folder, right-click the folder and select the **Open with Code** menu option.

    ![Open with Visual Studio Code](../media/03-open_with_code.jpg)

    > Alternatively, you can run a command prompt in your current directory and execute the ``code .`` command.

1. In the Visual Studio Code window that appears, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

    ![Open in Command Prompt](../media/03-open_command_prompt.jpg)

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet new console --output .
    ```

    > This command will create a new .NET Core 2.1 project. The project will be a **console** project and the project will be created in the current directly since you used the ``--output .`` option.

1. Visual Studio Code will most likely prompt you to install various extensions related to **.NET Core** or **Azure Cosmos DB** development. None of these extensions are required to complete the labs.

1. In the terminal pane, enter and execute the following command:

    ```sh
    dotnet add package Microsoft.Azure.DocumentDB.Core --version 1.9.1
    ```

    > This command will add the [Microsoft.Azure.DocumentDB.Core](https://www.nuget.org/packages/Microsoft.Azure.DocumentDB.Core/) NuGet package as a project dependency. The lab instructions have been tested using the ``1.9.1`` version of this NuGet package.

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

### Create DocumentClient Instance

*The DocumentClient class is the main "entry point" to using the SQL API in Azure Cosmos DB. We are going to create an instance of the **DocumentClient** class by passing in connection metadata as parameters of the class' constructor. We will then use this class instance throughout the lab.*

1. Within the **Program.cs** editor tab, Add the following using blocks to the top of the editor:

    ```csharp
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
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
    private static readonly Uri _endpointUri = new Uri("");
    private static readonly string _primaryKey = "";
    private static readonly string _databaseId = "UniversityDatabase";
    private static readonly string _collectionId = "StudentCollection";  
    ```

1. For the ``_endpointUri`` variable, replace the placeholder value with the **URI** value from your Azure Cosmos DB account that you recorded earlier in this lab: 

    > For example, if your **uri** is ``https://cosmosacct.documents.azure.com:443/``, your new variable assignment will look like this: ``private static readonly Uri _endpointUri = new Uri("https://cosmosacct.documents.azure.com:443/");``.

1. For the ``_primaryKey`` variable, replace the placeholder value with the **PRIMARY KEY** value from your Azure Cosmos DB account that you recorded earlier in this lab: 

    > For example, if your **primary key** is ``NAye14XRGsHFbhpOVUWB7CMG2MOTAigdei5eNjxHNHup7oaBbXyVYSLW2lkPGeKRlZrCkgMdFpCEnOjlHpz94g==``, your new variable assignment will look like this: ``private static readonly string _primaryKey = "NAye14XRGsHFbhpOVUWB7CMG2MOTAigdei5eNjxHNHup7oaBbXyVYSLW2lkPGeKRlZrCkgMdFpCEnOjlHpz94g==";``.
    
1. Locate the **Main** method:

    ```csharp
    public static async Task Main(string[] args)
    { 
    }
    ```

1. Within the **Main** method, add the following lines of code to author a using block that creates and disposes a **DocumentClient** instance:

    ```csharp
    using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
    {
        
    }
    ```

1. Your ``Program`` class definition should now look like this:

    ```csharp
    public class Program
    { 
        private static readonly Uri _endpointUri = new Uri("<your uri>");
        private static readonly string _primaryKey = "<your key>";
        private static readonly string _databaseId = "UniversityDatabase";
        private static readonly string _collectionId = "StudentCollection";

        public static async Task Main(string[] args)
        {    
            using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
            {
            }     
        }
    }
    ```

    > We are now going to implement a sample query to make sure our client connection code works.

1. Locate the using block within the **Main** method:

    ```csharp
    using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
    {
                        
    }
    ```

1. Add the following line of code to create a variable named ``collectionLink`` that references the *self-link* Uri for the collection:

    ```csharp
    Uri collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
    ```

1. Add the following line of code to create a string variable named ``sql`` that contains a sample SQL query:

    ```csharp
    string sql = "SELECT TOP 5 VALUE s.studentAlias FROM coll s WHERE s.enrollmentYear = 2018 ORDER BY s.studentAlias";
    ```

    > This query will get the alias of the top 5 2018-enrollees in the collection sorted by their alias alphabetically

1. Add the following line of code to create a document query:

    ```csharp
    IQueryable<string> query = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql));
    ```

1. Add the following lines of code to enumerate over the results and print the strings to the console:

    ```csharp
    foreach(string alias in query)
    {
        await Console.Out.WriteLineAsync(alias);
    }
    ```

1. Your **Main** method should now look like this:

    ```csharp
    public static async Task Main(string[] args)
    {         
        using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
        {
            Uri collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
            string sql = "SELECT TOP 5 VALUE s.studentAlias FROM coll s WHERE s.enrollmentYear = 2018 ORDER BY s.studentAlias";
            IQueryable<string> query = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql));
            foreach(string alias in query)
            {
                await Console.Out.WriteLineAsync(alias);
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

    > You should see five aliases printed to the console window.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

### Query Intra-document Array

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **New File** menu option.

    ![New File](../media/03-new_file.jpg)

1. Name the new file **Student.cs** . The editor tab will automatically open for the new file.

    ![Student Class File](../media/03-student_class.jpg)

1. Paste in the following code for the ``Student`` class:

    ```csharp
    public class Student
    {
        public string[] Clubs { get; set; }
    }
    ```

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

1. Within the **Program.cs** editor tab, locate the **Main** method.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT TOP 5 VALUE students.studentAlias FROM students WHERE students.enrollmentYear = 2018";
    ```

    Replace that line of code with the following code:

    ```csharp
    string sql = "SELECT s.clubs FROM students s WHERE s.enrollmentYear = 2018";
    ```

    > This new query will select the **clubs** property for each student in the result set. The value of the **clubs** property is a string array.

1. Locate the following line of code: 

    ```csharp
    IQueryable<string> query = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql));
    ```

    Replace that line of code with the following code:

    ```csharp
    IQueryable<Student> query = client.CreateDocumentQuery<Student>(collectionLink, new SqlQuerySpec(sql));
    ```

    > The query was updated to return a collection of student entities instead of string values.

1. Locate the following line of code: 

    ```csharp
    foreach(string alias in query)
    {
        await Console.Out.WriteLineAsync(alias);
    }
    ```

    Replace that line of code with the following code:

    ```csharp
    foreach(Student student in query)
    foreach(string club in student.Clubs)
    {
        await Console.Out.WriteLineAsync(club);
    }
    ```

    > Our new query will need to iterate twice. First, we will iterate the collection of students and then we will iterate the collection of clubs for each student instance.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the console project.

    > You should see multiple club names printed to the console window.

1. Click the **🗙** symbol to close the terminal pane.

1. In the Visual Studio Code window, double-click the **Student.cs** file to open an editor tab for the file.

1. Within the **Student.cs** editor tab, replace all of the existing code with the following code for the ``Student`` class:

    ```csharp
    public class Student
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Clubs { get; set; }
    }
    ```

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

1. Within the **Program.cs** editor tab, locate the **Main** method.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT s.clubs FROM students s WHERE s.enrollmentYear = 2018";
    ```

    Replace that line of code with the following code:

    ```csharp
    string sql = "SELECT s.firstName, s.lastName, s.clubs FROM students s WHERE s.enrollmentYear = 2018";
    ```

    > We are now including the **firstName** and **lastName** fields in our query.

1. Locate the following block of code: 

    ```csharp
    foreach(Student student in query)
    foreach(string club in student.Clubs)
    {
        await Console.Out.WriteLineAsync(club);
    }
    ```

    Replace that block of code with the following code:

    ```csharp
    foreach(Student student in query)
    {
        await Console.Out.WriteLineAsync($"{student.FirstName} {student.LastName}");
        foreach(string club in student.Clubs)
        {
            await Console.Out.WriteLineAsync($"\t{club}");
        }
        await Console.Out.WriteLineAsync();
    }
    ```

    > This modification simply prints out more information to the console.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the console project.

1. Click the **🗙** symbol to close the terminal pane.

    > Since we only really care about the list of clubs, we want to peform a self-join that applies a cross product across the **club** properties of each student in the result set.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **New File** menu option.

1. Name the new file **StudentActivity.cs** . The editor tab will automatically open for the new file.

1. Paste in the following code for the ``StudentActivity`` class:

    ```csharp
    public class StudentActivity
    {
        public string Activity { get; set; }
    }
    ```

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

1. Within the **Program.cs** editor tab, locate the **Main** method.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT s.firstName, s.lastName, s.clubs FROM students s WHERE s.enrollmentYear = 2018";
    ```

    Replace that line of code with the following code:

    ```csharp
    string sql = "SELECT activity FROM students s JOIN activity IN s.clubs WHERE s.enrollmentYear = 2018";
    ```

    > Here we are performing an intra-document JOIN to get a projection of all clubs across all matching students.

1. Locate the following line of code: 

    ```csharp
    IQueryable<Student> query = client.CreateDocumentQuery<Student>(collectionLink, new SqlQuerySpec(sql));
    ```

    Replace that line of code with the following code:

    ```csharp
    IQueryable<StudentActivity> query = client.CreateDocumentQuery<StudentActivity>(collectionLink, new SqlQuerySpec(sql));
    ```

1. Locate the following line of code: 

    ```csharp
    foreach(Student student in query)
    {
        await Console.Out.WriteLineAsync($"{student.FirstName} {student.LastName}");
        foreach(string club in student.Clubs)
        {
            await Console.Out.WriteLineAsync($"\t{club}");
        }
        await Console.Out.WriteLineAsync();
    }
    ```

    Replace that line of code with the following code:

    ```csharp
    foreach(StudentActivity studentActivity in query)
    {
        await Console.Out.WriteLineAsync(studentActivity.Activity);
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

    > You should see multiple club names printed to the console window.

1. Click the **🗙** symbol to close the terminal pane.

    > While we did get very useful information with our JOIN query, it would be more useful to get the raw array values instead of a wrapped value. It would also make our query easier to read if we could simply create an array of strings.

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

1. Within the **Program.cs** editor tab, locate the **Main** method.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT activity FROM students s JOIN activity IN s.clubs WHERE s.enrollmentYear = 2018";  
    ```

    Replace that line of code with the following code:

    ```csharp
    string sql = "SELECT VALUE activity FROM students s JOIN activity IN s.clubs WHERE s.enrollmentYear = 2018";
    ```

    > Here we are using the ``VALUE`` keyword to flatten our query.

1. Locate the following line of code: 

    ```csharp
    IQueryable<StudentActivity> query = client.CreateDocumentQuery<StudentActivity>(collectionLink, new SqlQuerySpec(sql));
    ```

    Replace that line of code with the following code:

    ```csharp
    IQueryable<string> query = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql));
    ```

1. Locate the following line of code: 

    ```csharp
    foreach(StudentActivity studentActivity in query)
    {
        await Console.Out.WriteLineAsync(studentActivity.Activity);
    }
    ```

    Replace that line of code with the following code:

    ```csharp
    foreach(string activity in query)
    {
        await Console.Out.WriteLineAsync(activity);
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

    > You should see multiple club names printed to the console window.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

### Projecting Query Results

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **New File** menu option.

1. Name the new file **StudentProfile.cs** . The editor tab will automatically open for the new file.

1. Paste in the following code for the ``StudentProfile`` and ``StudentProfileEmailInformation`` classes:

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

    > This code uses the special alignment features of C# string formatting so you can see all properties of the ``StudentProfile`` instances.

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

*You will use the **HasMoreResults** boolean property and **ExecuteNextAsync** method of the **ResourceResponse** class to implement paging of your query results. Behind the scenes, these properties use a continuation token. A continuation token enables a client to retrieve the ‘next’ set of data in a follow-up query.*

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

    > First we check if there are more results using the ``HasMoreResults`` property of the ``IDocumentQuery<>`` interface. If this value is set to true, we invoke the ``ExecuteNextAsync`` method to get the next batch of results and enumerate them using a ``foreach`` block.

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


## Implement Cross-Partition Queries

*With an unlimited container, you may wish to perform queries that are filtered to a partition key or perform queries across multiple partition keys. You will now implement both types of queries using the various options available in the **FeedOptions** class.*

### Execute Single-Partition Query

1. In the Visual Studio Code window, double-click the **Student.cs** file to open an editor tab for the file.

1. Replace the existing **Student** class implementation with the following code:

    ```csharp
    public class Student
    {
        public string studentAlias { get; set; }
        public int age { get; set; }
        public int enrollmentYear { get; set; }
        public int projectedGraduationYear { get; set; }
    }
    ```

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

1. Within the **Program.cs** editor tab, locate the **Main** method and delete any existing code:

    ```csharp
    public static async Task Main(string[] args)
    {       
    }
    ```

1. Within the **Main** method, add the following lines of code to author a using block that creates and disposes a **DocumentClient** instance:

    ```csharp
    using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
    {
        
    }
    ```

1. Within the new *using* block, add the following line of code to asynchronously open a connection:

    ```csharp
    await client.OpenAsync();
    ```
    
1. Add the following line of code to create a variable named ``collectionLink`` that is a reference (self-link) to an existing collection:

    ```csharp
    Uri collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
    ```

1. Add the following block of code to create a query that is filtered to a single partition key:

    ```csharp
    IEnumerable<Student> query = client
        .CreateDocumentQuery<Student>(collectionLink, new FeedOptions { PartitionKey = new PartitionKey(2016) })
        .Where(student => student.projectedGraduationYear == 2020);
    ```

    > First we will restrict our query to a single partition key using the ``PartitionKey`` property of the ``FeedOptions`` class. One of our partition key values for the ``/enrollmentYear`` path is ``2016``. We will filter our query to only return documents that uses this partition key. Remember, partition key paths are case sensitive. Since our property is named ``enrollmentYear``, it will match on the partition key path of ``/enrollmentYear``.

1. Add the following block of code to print out the results of your query:

    ```csharp
    foreach(Student student in query)
    {
        Console.Out.WriteLine($"Enrolled: {student.enrollmentYear}\tGraduation: {student.projectedGraduationYear}\t{student.studentAlias}");
    }      
    ```

    > We are using the C# string formatting features to print out two properties of our student instances.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the execution.

    > You should only see records from a single partition.

1. Click the **🗙** symbol to close the terminal pane.

### Execute Cross-Partition Query

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    IEnumerable<Student> query = client
        .CreateDocumentQuery<Student>(collectionLink, new FeedOptions { PartitionKey = new PartitionKey(2016) })
        .Where(student => student.projectedGraduationYear == 2020);
    ```

    Replace that code with the following code:

    ```csharp
    IEnumerable<Student> query = client
        .CreateDocumentQuery<Student>(collectionLink, new FeedOptions { EnableCrossPartitionQuery = true })
        .Where(student => student.projectedGraduationYear == 2020);
    ```

    > We could ignore the partition keys and simply enable cross-partition queries using the ``EnableCrossPartitionQuery`` property of the ``FeedOptions`` class. You must explicitly opt-in using the SDK classes if you wish to perform a cross-partition query from the SDK.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the execution.

    > You will notice that results are coming from more than one partition. You can observe this by looking at the values for ``type`` on the left-hand side of the output.

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

    > A continuation token allows us to resume a paginated query either immediately or later. When creating a query, the results are automatically paged. If there are more results, the returned page of results will also include a continuation token. This token should be passed back in    This implementation creates a **do-while** loop that will continue to get pages of results as long as the continuation token is not null.

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

### Observe How Partitions Are Accessed in a Cross-Partition Query

1. In the Visual Studio Code window, double-click the **Student.cs** file to open an editor tab for the file.

1. Replace the existing **Student** class implementation with the following code:

    ```csharp
    public class Student
    {
        public string studentAlias { get; set; }
        public int age { get; set; }
        public int enrollmentYear { get; set; }
        public int projectedGraduationYear { get; set; }

        public FinancialInfo financialData { get; set; }

        public class FinancialInfo
        {
            public double tuitionBalance { get; set; }
        }
    }
    ```

1. In the Visual Studio Code window, double-click the **Program.cs** file to open an editor tab for the file.

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

                FeedOptions options = new FeedOptions 
                { 
                    EnableCrossPartitionQuery = true
                };

                string sql = "SELECT * FROM students s WHERE s.academicStatus.suspension = true";

                IDocumentQuery<Student> query = client
                    .CreateDocumentQuery<Student>(collectionLink, sql, options)
                    .AsDocumentQuery();

                int pageCount = 0;
                while(query.HasMoreResults)
                {
                    await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
                    foreach(Student result in await query.ExecuteNextAsync())
                    {
                        await Console.Out.WriteLineAsync($"Enrollment: {result.enrollmentYear}\tBalance: {result.financialData.tuitionBalance}\t{result.studentAlias}@consoto.edu");
                    }
                }        
            }
        }
    ```

    > We are creating a cross-partition query here that may (or may not) have results for each partition key. Since this is a server-side fan-out and we are not filtering on a partition key, the search will be forced to check each partition. You can potentially have pages returned that have no results for partition keys that do not have any matching data.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the output of the console application.

    > You should see a list of documents grouped by "pages" of results. Scroll up and look at the results for **every page**. You should also notice that there is at least one page that does not have any results. This page occurs because the server-side fan-out is forced to check every partition since you are not filtering by partition keys. The next few examples will illustrate this even more.

1. Click the **🗙** symbol to close the terminal pane.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.academicStatus.suspension = true";
    ```

    Replace that code with the following code:

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14000";
    ```

    > This new query should return results for most partition keys.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the execution.

    > You will notice in the results that one page exists that does not have any relevant data. This occurs because there's at least one partition that does not have any data that matches the query specified above. Since we are not filtering on partition keys, all partitions much be checked as part of the server-side fan-out.

1. Click the **🗙** symbol to close the terminal pane.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14000";
    ```

    Replace that code with the following code:

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14950";
    ```

    > This new query should return results for most partition keys.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the execution.

    > Now only 54 records will match your query. They are pretty evenly distributed across the partition keys, so you will only see one page without results.

1. Click the **🗙** symbol to close the terminal pane.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14950";
    ```

    Replace that code with the following code:

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14996";
    ```

    > This new query should return results for most partition keys.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the execution.

    > Only 3 records match this query. You should see more empty pages.

1. Click the **🗙** symbol to close the terminal pane.

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14996";
    ```

    Replace that code with the following code:

    ```csharp
    string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14998";
    ```

    > This new query should return results for most partition keys.

1. Save all of your open editor tabs.

1. In the Visual Studio Code window, right-click the **Explorer** pane and select the **Open in Command Prompt** menu option.

1. In the open terminal pane, enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the results of the execution.

    > Only 1 record matches this query. You should see every multiple empty pages.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

1. Close the Visual Studio Code application.

1. Close your browser application.
