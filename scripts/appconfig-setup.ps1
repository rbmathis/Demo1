param(
    [Parameter(Mandatory = $true)]
    [string] $StoreName,

    [Parameter(Mandatory = $false)]
    [string] $Label = "Demo1"
)

# Ensure the appconfig extension is installed
if (-not (az extension list | ConvertFrom-Json | Where-Object { $_.name -eq 'appconfig' })) {
    az extension add --name appconfig
}

Write-Host "Creating Feature1 flag (enabled) with label '$Label' in store $StoreName..."
az appconfig feature set --name $StoreName --feature Feature1 --label $Label --state enabled --description "Demo1 Feature1 flag"

Write-Host "Setting sentinel key to trigger refresh"
az appconfig kv set --name $StoreName --key "FeatureManagement:Sentinel" --value "1" --label $Label

Write-Host "Done. Verify in Azure Portal or with 'az appconfig feature show'."