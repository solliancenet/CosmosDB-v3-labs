param($codePath = "$Home\Documents")

New-Item -Name CosmosLabs -Path $codePath -ItemType Directory -Force

Copy-Item -Path .\templates\* -Filter "*.*" -Recurse -Destination "$codePath\CosmosLabs" -Container -Force

Write-Output "" "Copied lab code files to: $codePath\CosmosLabs"

