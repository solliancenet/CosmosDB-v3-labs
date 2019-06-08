# Optimistic Concurrency Control

## Viewing the ETag Property of a Requested Resource

*The SQL API supports optimistic concurrency control (OCC) through HTTP entity tags, or ETags. Every SQL API resource has an ETag, and the ETag is set on the server every time a document is updated. In this exercise, we will view the ETag property of a resource that is requested using the SDK.*

### Observe the ETag Property 

1. Locate the *using* block within the **Main** method:

    ```csharp
    using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
    {
    }
    ```

1. Within the **Main** method, locate the following line of code: 

    ```csharp
    await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
    ```

    Replace that line of code with the following code:

    ```csharp
    await Console.Out.WriteLineAsync($"ETag: {response.Resource.ETag}");    
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

    > You should see an ETag for the document.

1. Enter and execute the following command:

    ```sh
    dotnet run
    ```

    > This command will build and execute the console project.

1. Observe the output of the console application.

    > The ETag should remain unchanged since the document has not been changed.

1. Click the **🗙** symbol to close the terminal pane.

1. Locate the *using* block within the **Main** method:

    ```csharp
    using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
    {
    }
    ```
    
1. Within the **Main** method, locate the following line of code: 

    ```csharp
    await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
    ```

    Replace that line of code with the following code:

    ```csharp
    await Console.Out.WriteLineAsync($"Existing ETag:\t{response.Resource.ETag}");    
    ```

1. Within the **using** block, add a new line of code to create an **AccessCondition** instance that will use the **ETag** from the document and specify an **If-Match** header:

    ```csharp
    AccessCondition cond = new AccessCondition { Condition = response.Resource.ETag, Type = AccessConditionType.IfMatch };
    ```

1. Add a new line of code to update a property of the document using the **SetPropertyValue** method:

    ```csharp
    response.Resource.SetPropertyValue("FirstName", "Demo");
    ```

    > This line of code will modify a property of the document. Here we are modifying the **FirstName** property and changing it's value from **Example** to **Demo**.

1. Add a new line of code to create an instance of the **RequestOptions** class using the **AccessCondition** created earlier:

    ```csharp
    RequestOptions options = new RequestOptions { AccessCondition = cond };
    ```

1. Add a new line of code to invoke the **ReplaceDocumentAsync** method passing in both the document and the options:

    ```csharp
    response = await client.ReplaceDocumentAsync(response.Resource, options);
    ```

1. Add a new line of code to print out the **ETag** of the newly updated document:

    ```csharp
    await Console.Out.WriteLineAsync($"New ETag:\t{response.Resource.ETag}"); 
    ```

1. Your **Main** method should now look like this:

    ```csharp
    public static async Task Main(string[] args)
    {    
        using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
        {
            await client.OpenAsync();
            Uri documentLink = UriFactory.CreateDocumentUri(_databaseId, _collectionId, "example.document");            
            ResourceResponse<Document> response = await client.ReadDocumentAsync(documentLink);
            await Console.Out.WriteLineAsync($"Existing ETag:\t{response.Resource.ETag}"); 

            AccessCondition cond = new AccessCondition { Condition = response.Resource.ETag, Type = AccessConditionType.IfMatch };
            response.Resource.SetPropertyValue("FirstName", "Demo");
            RequestOptions options = new RequestOptions { AccessCondition = cond };
            response = await client.ReplaceDocumentAsync(response.Resource, options);
            await Console.Out.WriteLineAsync($"New ETag:\t{response.Resource.ETag}"); 
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

    > You should see that the value of the ETag property has changed. The **AccessCondition** class helped us implement optimistic concurrency by specifying that we wanted the SDK to use the If-Match header to allow the server to decide whether a resource should be updated. The If-Match value is the ETag value to be checked against. If the ETag value matches the server ETag value, the resource is updated. If the ETag is no longer current, the server rejects the operation with an "HTTP 412 Precondition failure" response code. The client then re-fetches the resource to acquire the current ETag value for the resource.

1. Click the **🗙** symbol to close the terminal pane.

1. Close all open editor tabs.

1. Close the Visual Studio Code application.

1. Close your browser application.
