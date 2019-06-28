# Account Setup

## Log-in to the Azure Portal

1. In a new window, sign in to the **Azure Portal** (<http://portal.azure.com>).

1. Once you have logged in, you may be prompted to start a tour of the Azure portal. You can safely skip this step.

## Setup

> Before you start this lab, you will need to create an Azure Cosmos DB database and collection that you will use throughout the lab. The .NET SDK requires credentials to connect to your Azure Cosmos DB account. You will collect and store these credentials for use throughout the lab.

1. To begin setup, Git clone or download the repo containing these instructions from [Github](https://github.com/CosmosDB/labs).

    > To automatically set up the resources needed for each lab, you will need to run Powershell scripts for Azure resource setup and to create local code files. Azure setup uses the Azure Powershell module. If you do not already have it installed, go to https://docs.microsoft.com/en-us/powershell/azure/install-az-ps for setup instructions before continuing.

1. Open a Powershell session and navigate to the folder containing your downloaded copy of the repo. Inside the repo, navigate to the **dotnet\setup** folder:

    ``` powershell
    cd .\dotnet\setup\
    ```

1. To enable the setup scripts to run in your open Powershell session, enter the following:

    ``` powershell
    Set-ExecutionPolicy Unrestricted -Scope Process
    ```

1. To automatically copy the starter code for the labs into a **CosmosLabs** folder in your **Documents** folder run the labCodeSetup.ps1 script:

    ``` powershell
    .\labCodeSetup.ps1
    ```

    > The starter code for each lab is initially located in the **templates** folder. To use a different folder for lab code the files can be copied manually instead.

1. Connect to your Azure account to begin Azure resource setup:

    ``` powershell
    Connect-AzAccount
    ```
    or
    ``` powershell
    Connect-AzAccount -subscription <subscription id>
    ```

1. To create the Azure resources for the labs run the labSetup.ps1 script:

    ``` powershell
    .\labSetup.ps1
    ```

    > This script creates resources in the *West US* region by default. To use another region add **-location 'region name'** to the above command.

1. Some Azure resources can take 10 minutes or more to complete so expect the script to run for a while before completing. 

### Retrieve Account Credentials

1. On the left side of the portal, click the **Resource groups** link.

    ![Resource groups](../media/02-resource_groups.jpg)

1. In the **Resource groups** blade, locate and select the **cosmoslabs** *Resource Group*.

    ![Lab resource group](../media/02-lab_resource_group.jpg)

1. In the **cosmoslabs** blade, select the **Azure Cosmos DB** account you recently created.

    ![Cosmos resource](../media/02-cosmos_resource.jpg)

1. In the **Azure Cosmos DB** blade, locate the **Settings** section and click the **Keys** link.

    ![Keys pane](../media/02-keys_pane.jpg)

1. In the **Keys** pane, record the values in the **CONNECTION STRING**, **URI** and **PRIMARY KEY** fields. You will use these values later in this lab.

    ![Credentials](../media/02-keys.jpg)

