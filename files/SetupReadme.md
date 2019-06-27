# Deploying Lab Environment

## Tools Version

Tested with Azure Powershell 2.2.0 on Powershell 5.1.2

``` powershell
Install-Module -Name Az -RequiredVersion 2.2.0
```

Assumes Azure Powershell login to appropriate subscription
``` powershell
Connect-AzAccount
```
or
``` powershell
Connect-AzAccount -subscription <subscription id>
```

## Setup

Run **labSetup.ps1** with optional parameters

- **location**: Azure region name for lab resources. *Default = 'West US'*
- **session**: Any unique characters to append to Azure resource names that are required to be globally unique. *Default = random 1-5 digit number*
- **codePath**: Folder path to populate with starter code for all labs. *Default = '$Home\Documents'*

``` powershell
.\labSetup.ps1 -session 37haydf -location 'East US' -codePath 'c:\labs'
```

## Teardown

Run **labSetup.ps1** with **teardown** switch

``` powershell
.\labSetup.ps1 -teardown
```
