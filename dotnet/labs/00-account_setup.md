# Account Setup

## Log-in to the Azure Portal

1. In a new window, sign in to the **Azure Portal** (<http://portal.azure.com>).

1. Once you have logged in, you may be prompted to start a tour of the Azure portal. You can safely skip this step.

## Setup

> Before you start this lab, you will need to create an Azure Cosmos DB database and collection that you will use throughout the lab. The .NET SDK requires credentials to connect to your Azure Cosmos DB account. You will collect and store these credentials for use throughout the lab.

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

