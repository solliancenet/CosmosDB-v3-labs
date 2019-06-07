# Load Data Into Cosmos DB with ADF

In this lab, you will populate an Azure Cosmos DB container from an existing set of data using tools built in to Azure. After importing, you will use the Azure portal to view your imported data.

## Setup

> Before you start this lab, you will need to create an Azure Cosmos DB database and container that you will use throughout the lab. You will also use the **Azure Data Factory (ADF)** to import existing data into your container.

### Create Azure Cosmos DB Database and Container

*You will now create a database and container within your Azure Cosmos DB account.*

1. On the left side of the portal, click the **Resource groups** link.

    ![Resource groups](../media/03-resource_groups.jpg)

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** *Resource Group*.

    ![Lab resource group](../media/03-lab_resource_group.jpg)

1. In the **cosmosgroup-lab** blade, select the **Azure Cosmos DB** account you recently created.

    ![Cosmos resource](../media/03-cosmos_resource.jpg)

1. In the **Azure Cosmos DB** blade, locate and click the **Overview** link on the left side of the blade.

    ![Overview pane](../media/03-overview.jpg)

1. At the top of the **Azure Cosmos DB** blade, click the **Add Container** button.

    ![Add container](../media/03-add_collection.jpg)

1. In the **Add Container** popup, perform the following actions:

    1. In the **Database id** field, select the **Create new** option and enter the value **UniversityDatabase**.

    1. Ensure the **Provision database throughput** option is not selected.

        > Provisioning throughput for a database allows you to share the throughput among all the containers that belong to that database. Within an Azure Cosmos DB database, you can have a set of containers which shares the throughput as well as containers, which have dedicated throughput.

    1. In the **Container Id** field, enter the value **StudentCollection**.

    1. In the **Partition key** field, enter the value ``/enrollmentYear``.

    1. In the **Throughput** field, enter the value ``11000``.

    1. Click the **+ Add Unique Key** link.

    1. In the new **Unique Keys** field, enter the value ``/studentAlias``.

    1. Click the **OK** button.

    ![Add container](../media/04-add_collection_settings.jpg)

1. Wait for the creation of the new **database** and **container** to finish before moving on with this lab.

### Retrieve Account Credentials

*The .NET SDK requires credentials to connect to your Azure Cosmos DB account. You will collect and store these credentials for use throughout the lab.*

1. On the left side of the **Azure Cosmos DB** blade, locate the **Settings** section and click the **Keys** link.

    ![Keys pane](../media/03-keys_pane.jpg)

1. In the **Keys** pane, record the values in the **CONNECTION STRING**, **URI** and **PRIMARY KEY** fields. You will use these values later in this lab.

    ![Credentials](../media/03-keys.jpg)

### Import Lab Data Into Container

You will use **Azure Data Factory (ADF)** to import the JSON array stored in the **students.json** file from Azure Blob Storage.

1. On the left side of the portal, click the **Resource groups** link.

    > To learn more about copying data to Cosmos DB with ADF, please read [ADF's documentation](https://docs.microsoft.com/en-us/azure/data-factory/connector-azure-cosmos-db). 

    ![Resource groups](../media/03-resource_groups.jpg)

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** *Resource Group*.


3. Click **add** to add a new resource

    ![Add adf](../media/03-add_adf.jpg)

4. Search for **Data Factory** and select it

    ![adf-search](../media/03-adf_search.png)

5. Create a new **Data Factory**. You should name this data factory **importstudentdata** with a unique number appended and select the relevant Azure subscription. You should ensure your existing **cosmosdblab-group** resource group is selected as well as a Version **V2**. Select **East US** as the region. Click **create**.

    ![df](../media/03-adf_selections.jpg)

6. After creation, open your newly created Data Factory. Select **Author & Monitor** and you will launch ADF. You should see a screen similar to the screenshot below. Select **Copy Data**. We will be using ADF for a one-time copy of data from a source JSON file on Azure Blob Storage to a database in Cosmos DB’s SQL API. ADF can also be used for more frequent data transfers from Cosmos DB to other data stores.
    ![](../media/03-adf_author&monitor.jpg)
    ![](../media/03-adf_copydata.jpg)

7. Edit basic properties for this data copy. You should name the task **ImportStudents** and select to **Run once now**

   ![adf-properties](../media/03-adf_properties.jpg)

8. **Create a new connection** and select **Azure Blob Storage**. We will import data from a json file on Azure Blob Storage. In addition to Blob Storage, you can use ADF to migrate from a wide variety of sources. We will not cover migration from these sources in this tutorial.

    ![](../media/03-adf_blob.jpg)

9. Name the source **StudentsJson** and select **Use SAS URI** as the Authentication method. Please use the following SAS URI for read-only access to this Blob Storage container: https://cosmoslabs.blob.core.windows.net/?sv=2018-03-28&ss=bfqt&srt=sco&sp=rl&se=2020-04-01T13:14:14Z&st=2018-11-06T06:14:14Z&spr=https&sig=8HltMx1smolMcSmOhfVdC3drxtmTkq2%2BfJ8574uK60A%3D

    ![](../media/03-adf_connecttoblob.jpg)

10. Select the **students** folder

    ![](../media/03-adf_choosestudents.jpg)

11. Do not check **Copy file recursively** and **Binary Copy**. Also ensure that **Compression Type** is "none".

    ![](../media/03-adf_source_next.jpg)

12. ADF should auto-detect the file format to be JSON. You can also select the file format as **JSON format.** You should also make sure you select **Array of Objects**  as the File pattern.

    ![](../media/03-adf_source_dataset_format.jpg)

13. You have now successfully connected the Blob Storage container with the students.json file. You should select **StudentsJson** as the source and click **Next**.

    ![](../media/03-adf_SourceNext.JPG)

14. Add the Cosmos DB target data store by selecting **Create new connection** and selecting **Azure Cosmos DB (SQL API)**.

    ![](../media/03-adf_selecttarget.jpg)

15. Name the linked service **targetcosmosdb** and select your Azure subscription and Cosmos DB account. You should also select the Cosmos DB database that you created earlier.

    ![](../media/03-adf_selecttargetdb.jpg)

16. Select your newly created **targetcosmosdb** connection as the Destination date store.

    ![](../media/03-adf_destconnectionnext.jpg)

17. Select your container from the drop-down menu. You will map your Blob storage file to the correct Cosmos DB container. Select **Skip column mapping for all tables** before continuing.

    ![](../media/03-adf_correcttable.jpg)

18. You should have selected to skip column mappings in a previous step. Click through this screen.

    ![](../media/03-adf_destinationconnectionfinal.jpg)

19. There is no need to change any settings. Click **next**.

    ![](../media/03-adf_settings.jpg)

20. After deployment is complete, select **Monitor**.

    ![](../media/03-adf_deployment.jpg)

21. After a few minutes, refresh the page and the status for the ImportStudents pipeline should be listed as **Succeeded**.

    ![](../media/03-adf-succeeded.jpg)

22. Once the import process has completed, close the ADF. You will now proceed to validate your imported data. 

## Executing Simple Queries

*The Azure Cosmos DB Data Explorer allows you to view documents and run queries directly within the Azure Portal. In this exercise, you will use the Data Explorer to view the data stored in our container.*

### Validate Imported Data

*You will validate that the data was successfully imported into your container using the **Items** view in the **Data Explorer**.*

1. Return to the **Azure Portal** (<http://portal.azure.com>).

1. On the left side of the portal, click the **Resource groups** link.

    ![Resource groups](../media/03-resource_groups.jpg)

1. In the **Resource groups** blade, locate and select the **cosmosgroup-lab** *Resource Group*.

    ![Lab resource group](../media/03-lab_resource_group.jpg)

1. In the **cosmosgroup-lab** blade, select the **Azure Cosmos DB** account you recently created.

    ![Cosmos resource](../media/03-cosmos_resource.jpg)

1. In the **Azure Cosmos DB** blade, locate and click the **Data Explorer** link on the left side of the blade.

    ![Data Explorer pane](../media/03-data_explorer_pane.jpg)

1. In the **Data Explorer** section, expand the **UniversityDatabase** database node and then expand the **StudentCollection** container node. 

    ![Container node](../media/03-collection_node.jpg)

1. Within the **StudentCollection** node, click the **Items** link to view a subset of the various documents in the container. Select a few of the documents and observe the properties and structure of the documents.

    ![Documents](../media/03-documents.jpg)

    ![Example document](../media/03-example_document.jpg)
