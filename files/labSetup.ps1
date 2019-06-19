function New-Database ($resourceGroupName, $accountName, $databaseName) {
    $databaseResourceName = $accountName + "/sql/" + $databaseName

    $databaseProperties = @{
        "resource" = @{ "id" = $databaseName };
    } 
    New-AzResource -ResourceType "Microsoft.DocumentDb/databaseAccounts/apis/databases" `
        -ApiVersion "2015-04-08" -ResourceGroupName $resourceGroupName `
        -Name $databaseResourceName -PropertyObject $databaseProperties -Force
}

function New-Container ($resourceGroupName, $accountName, $databaseName, $containerName, $partition, $throughput = 400) {
    $containerResourceName = $accountName + "/sql/" + $databaseName + "/" + $containerName

    $containerProperties = @{
        "resource" = @{
            "id"           = $containerName; 
            "partitionKey" = @{
                "paths" = @($partition); 
                "kind"  = "Hash"
            }; 
            # "indexingPolicy"=@{
            #     "indexingMode"="Consistent"; 
            #     "includedPaths"= @(@{
            #         "path"="/*";
            #         "indexes"= @(@{
            #                 "kind"="Range";
            #                 "dataType"="number";
            #                 "precision"=-1
            #             },
            #             @{
            #                 "kind"="Range";
            #                 "dataType"="string";
            #                 "precision"=-1
            #             }
            #         )
            #     });
            # };
        };
        "options"  = @{ "Throughput" = $throughput }
    } 

    New-AzResource -ResourceType "Microsoft.DocumentDb/databaseAccounts/apis/databases/containers" `
        -ApiVersion "2015-04-08" -ResourceGroupName $resourceGroupName `
        -Name $containerResourceName -PropertyObject $containerProperties -Force
}

function Add-DataSet ($resourceGroupName, $dataFactoryName, $location, $cosmosAccount, $cosmosDatabase, $cosmosContainer) {
    $cosmosLocation = "https://$cosmosAccount.documents.azure.com:443/"

    $storageAccountLocation = "https://cosmoslabsv3update.blob.core.windows.net"
    $storageAccountSas = "sv=2018-03-28&ss=bfqt&srt=sco&sp=rl&st=2019-06-11T13%3A43%3A56Z&se=2020-06-12T13%3A43%3A00Z&sig=KJRYFY4%2Fm1pu6rklgvx8T%2BEl5JzF7LUt%2FErvKt1NBhw%3D"
    $sourceBlobFolder = "nutrition"
    $sourceBlobFile = "DataSample.json" # small sample file for script testing, full file is NutritionData.json
    $pipelineName = "ImportLabNutritionData"

    $cosmosKey = Invoke-AzResourceAction -Action listKeys `
        -ResourceType "Microsoft.DocumentDb/databaseAccounts" -ApiVersion "2015-04-08" -Force `
        -ResourceGroupName $resourceGroupName -Name $cosmosAccount | Select-Object -First 1 -ExpandProperty primaryMasterKey

    # Create a data factory
    $df = Set-AzDataFactoryV2 -ResourceGroupName $resourceGroupName -Location $location -Name $dataFactoryName -Force

    # Create an Az.Storage linked service in the data factory

    ## JSON definition of the linked service. 
    $storageLinkedServiceDefinition = @"
{
    "name": "AzureStorageLinkedService",
    "properties": {
        "type": "AzureStorage",
        "typeProperties": {
            "connectionString": {
                "value": "BlobEndpoint=$storageAccountLocation;SharedAccessSignature=$storageAccountSas",
                "type": "SecureString"
            }
        }
    }
}
"@

    $cosmosLinkedServiceDefinition = @"
    {
        "name": "CosmosDbSQLAPILinkedService",
        "properties": {
            "type": "CosmosDb",
            "typeProperties": {
                "connectionString": {
                    "type": "SecureString",
                    "value": "AccountEndpoint=$cosmosLocation;AccountKey=$cosmosKey;Database=$cosmosDatabase"
                }
            }
        }
    }
"@

    ## IMPORTANT: stores the JSON definition in a file that will be used by the Set-AzDataFactoryLinkedService command. 
    $storageLinkedServiceDefinition | Out-File StorageLinkedService.json
    $cosmosLinkedServiceDefinition | Out-File CosmosLinkedService.json
    
    ## Creates a linked service in the data factory
    Set-AzDataFactoryV2LinkedService -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "AzureStorageLinkedService" -File StorageLinkedService.json -Force
    Set-AzDataFactoryV2LinkedService -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "CosmosLinkedService" -File CosmosLinkedService.json -Force

    # Create an Azure Blob dataset in the data factory

    ## JSON definition of the dataset
    $datasetDefinition = @"
{
    "name": "BlobDataset",
    "properties": {
        "type": "AzureBlob",
        "typeProperties": {
            "format": {
                "type": "JsonFormat",
                "filePattern": "arrayOfObjects"
            },
            "fileName": "$sourceBlobFile",
            "folderPath": "$sourceBlobFolder"
        },
        "linkedServiceName": {
            "referenceName": "AzureStorageLinkedService",
            "type": "LinkedServiceReference"
        },
        "parameters": {
        }
    }
}
"@
    $cosmosDatasetDefinition = @"
{
    "name": "CosmosDbSQLAPIDataset",
    "properties": {
        "type": "DocumentDbCollection",
        "linkedServiceName":{
            "referenceName": "CosmosLinkedService",
            "type": "LinkedServiceReference"
        },
        "typeProperties": {
            "collectionName": "$cosmosContainer"
        }
    }
}
"@

    ## IMPORTANT: store the JSON definition in a file that will be used by the Set-AzDataFactoryDataset command. 
    $datasetDefinition | Out-File BlobDataset.json
    $cosmosDatasetDefinition | Out-File CosmosDataset.json

    ## Create a dataset in the data factory
    Set-AzDataFactoryV2Dataset -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "BlobDataset" -File "BlobDataset.json" -Force
    Set-AzDataFactoryV2Dataset -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "CosmosDataset" -File "CosmosDataset.json" -Force

    # Create a pipeline in the data factory

    ## JSON definition of the pipeline
    $pipelineDefinition = @"
{
    "name": "$pipelineName",
    "properties": {
        "activities": [
            {
                "name": "ImportLabData",
                "type": "Copy",
                "inputs": [
                    {
                        "referenceName": "BlobDataset",
                        "type": "DatasetReference"
                    }
                ],
                "outputs": [
                    {
                        "referenceName": "CosmosDataset",
                        "type": "DatasetReference"
                    }
                ],
                "typeProperties": {
                    "source": {
                        "type": "BlobSource"
                    },
                    "sink": {
                        "type": "DocumentDbCollectionSink",
                        "nestingSeparator": ".",
                        "writeBehavior": "insert"
            }
                }
            }
        ],
        "parameters": {
        }
    }
}
"@

    ## IMPORTANT: store the JSON definition in a file that will be used by the Set-AzDataFactoryPipeline command. 
    $pipelineDefinition | Out-File CopyPipeline.json

    ## Create a pipeline in the data factory
    Set-AzDataFactoryV2Pipeline -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name $pipelineName -File "CopyPipeline.json" -Force

    # Create a pipeline run 

    # Create a pipeline run by using parameters
    $runId = Invoke-AzDataFactoryV2Pipeline -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -PipelineName $pipelineName

    # Check the pipeline run status until it finishes the copy operation
    while ($True) {
        $result = Get-AzDataFactoryV2ActivityRun -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -PipelineRunId $runId -RunStartedAfter (Get-Date).AddMinutes(-30) -RunStartedBefore (Get-Date).AddMinutes(30)

        if (($result | Where-Object { $_.Status -eq "InProgress" } | Measure-Object).count -ne 0) {
            Write-Host "Pipeline run status: In Progress" -foregroundcolor "Yellow"
            Start-Sleep -Seconds 30
        }
        else {
            Write-Host "Pipeline '$pipelineName' run finished. Result:" -foregroundcolor "Yellow"
            $result
            break
        }
    }

    # Get the activity run details 
    $result = Get-AzDataFactoryV2ActivityRun -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName `
        -PipelineRunId $runId `
        -RunStartedAfter (Get-Date).AddMinutes(-10) `
        -RunStartedBefore (Get-Date).AddMinutes(10) `
        -ErrorAction Stop

    $result

    if ($result.Status -eq "Succeeded") {
`
            $result.Output -join "`r`n"`
    
    }`
        else {
`
            $result.Error -join "`r`n"`
    
    }
}

function Add-EventHub($resourceGroupName, $location, $eventHubNS, $eventHubName, $retentionDays){
    New-AzEventHubNamespace -ResourceGroupName $resourceGroupName -NamespaceName $eventHubNS -Location $location
    New-AzEventHub -ResourceGroupName $resourceGroupName -NamespaceName $eventHubNS -EventHubName $eventHubName -MessageRetentionInDays $retentionDays
}

function Add-StreamProcessor($resourceGroupName, $location, $eventHubNS, $jobName){
    # NOTE - Creating a Power BI Output is not supported via scripting

    $jobDefinition = @"
    {
        "location": "$location",
        "properties": {
            "sku":{
                "name": "standard"
            },
            "eventsOutOfOrderPolicy": "adjust",
            "eventsOutOfOrderMaxDelayInSeconds": 10,
            "compatibilityLevel": 1.1
        }
    }
"@  

    $jobDefinition | Out-File JobDefinition.json

    New-AzStreamAnalyticsJob -ResourceGroupName $resourceGroupName -File "JobDefinition.json" -Name $jobName

    $inputDefintion = @"
    {
        "properties": {
            "type": "Stream",
            "datasource": {
                "type": "Microsoft.ServiceBus/EventHub",
                "properties": {
                    "serviceBusNamespace": "$EventHubNS",
                    "sharedAccessPolicyName": "RootManageSharedAccessKey",
                    "sharedAccessPolicyKey": "RootManageSharedAccessKey",
                    "eventHubName": "carteventhub"
                }
            },
            "serialization": {
                "type": "Json",
                "properties": {
                    "encoding": "UTF8"
                }
            }            
        },
        "name": "cartInput",
        "type": "Microsoft.StreamAnalytics/streamingjobs/inputs"
        }
"@

    $inputDefintion | Out-File InputDefinition.json

    New-AzStreamAnalyticsInput -ResourceGroupName $resourceGroupName -JobName $jobName -File "InputDefinition.json"  

    $transformationDefintiion = @"
    {
        "properties": {
            "streamingUnits": 1,
            "query": "/*TOP 5*/\r\n WITH Counter AS\r\n (\r\n SELECT Item, Price, Action, COUNT(*) AS
                    countEvents\r\n FROM cartinput\r\n WHERE Action = 'Purchased'\r\n GROUP BY Item, Price, Action,
                    TumblingWindow(second,300)\r\n ), \r\n top5 AS\r\n (\r\n SELECT DISTINCT\r\n CollectTop(5)  OVER
                    (ORDER BY countEvents) AS topEvent\r\n FROM Counter\r\n GROUP BY TumblingWindow(second,300)\r\n ),
                    \r\n arrayselect AS \r\n (\r\n SELECT arrayElement.ArrayValue\r\n FROM top5\r\n CROSS APPLY
                    GetArrayElements(top5.topevent) AS arrayElement\r\n ) \r\n SELECT arrayvalue.value.item,
                    arrayvalue.value.price, arrayvalue.value.countEvents\r\n INTO top5Output\r\n FROM
                    arrayselect\r\n\r\n /*REVENUE*/\r\n SELECT System.TimeStamp AS Time, SUM(Price)\r\n INTO
                    incomingRevenueOutput\r\n FROM cartinput\r\n WHERE Action = 'Purchased'\r\n GROUP BY
                    TumblingWindow(minute, 5)\r\n\r\n /*UNIQUE VISITORS*/\r\n SELECT System.TimeStamp AS Time,
                    COUNT(DISTINCT CartID) as uniqueVisitors\r\n INTO uniqueVisitorCountOutput\r\n FROM cartinput\r\n
                    GROUP BY TumblingWindow(second, 30)\r\n\r\n  /*AVERAGE PRICE*/      \r\n SELECT System.TimeStamp
                    AS Time, Action, AVG(Price)  \r\n INTO averagePriceOutput  \r\n FROM cartinput  \r\n GROUP BY
                    Action, TumblingWindow(second,30) "             
        },                                    
        "name": "Transformation",
        "type": "Microsoft.StreamAnalytics/streamingjobs/transformations"        
    }
"@

    $transformationDefintiion | Out-File TransformationDefinition.json

    New-AzStreamAnalyticsTransformation -ResourceGroupName $resourceGroupName -JobName $jobName -File "TransformationDefinition.json"    
}

# Settings to apply to new deployment
$location = "West US"
$resourceGroupName = "cosmoslabs"
$randomNum = Get-Random # some resources need unique names - this could be user entered instead
$accountName = "cosmoslab$($randomNum)"
$eventHubNS = "shoppingHub$($randomNum)"

New-AzResourceGroup -Name $resourceGroupName -Location $location

$locations = @(
    @{ "locationName" = $location; "failoverPriority" = 0 }
)

$CosmosDBProperties = @{
    "databaseAccountOfferType" = "Standard";
    "locations"                = $locations;
}

 New-AzResource -ResourceType "Microsoft.DocumentDb/databaseAccounts" `
     -ApiVersion "2015-04-08" -ResourceGroupName $resourceGroupName -Location $location `
     -Name $accountName -PropertyObject $CosmosDBProperties -Force

New-Database $resourceGroupName $accountName "FinancialDatabase"
New-Container $resourceGroupName $accountName "FinancialDatabase" "PeopleCollection" "/accountHolder/LastName"
New-Container $resourceGroupName $accountName "FinancialDatabase" "TransactionCollection" "/costCenter"

New-Database $resourceGroupName $accountName "NutritionDatabase"
New-Container $resourceGroupName $accountName "NutritionDatabase" "FoodCollection" "/foodGroup"

#Lab08
 New-Database $resourceGroupName $accountName "StoreDatabase"
New-Container $resourceGroupName $accountName "StoreDatabase" "CartContainer" "/Item"
New-Container $resourceGroupName $accountName "StoreDatabase" "CartContainerByState" "/BuyerState"
New-Container $resourceGroupName $accountName "StoreDatabase" "StateSales" "/State"

Add-EventHub $resourceGroupName $location $eventHubNS "CartEventHub" 1
Add-StreamProcessor $resourceGroupName $location $eventHubNS "CartStreamProcessor"
#END Lab08

Add-DataSet $resourceGroupName "importNutritionData$randomNum" $location $accountName "NutritionDatabase" "FoodCollection"

# To remove the whole resource group
# Remove-AzResourceGroup  -Name $resourceGroupName
