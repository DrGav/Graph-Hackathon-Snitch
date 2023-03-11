$tenantId = 'TENNENT_ID_GOES_HERE'
$ClientID = 'CLIENT_ID_GOES_HERE'
$ClientSecret = 'CLIENT_SECRET_GOES_HERE'


$users = "EMAIL_TO_UPDATE_STATUS_ON_GOES_HERE"

#Number of hours the presence should be valid.
$Hours = "8"

function Get-MSGraphAppToken{
    <#  .SYNOPSIS
        Get an app based authentication token required for interacting with Microsoft Graph API
    .PARAMETER TenantID
        A tenant ID should be provided.
 
    .PARAMETER ClientID
        Application ID for an Azure AD application. Uses by default the Microsoft Intune PowerShell application ID.
 
    .PARAMETER ClientSecret
        Web application client secret.
        
    .EXAMPLE
        # Manually specify username and password to acquire an authentication token:
        Get-MSGraphAppToken -TenantID $TenantID -ClientID $ClientID -ClientSecert = $ClientSecret 
    .NOTES
        Author: Jan Ketil Skanke
        Contact: @JankeSkanke
        Created: 2020-15-03
        Updated: 2020-15-03
 
        Version history:
        1.0.0 – (2020-03-15) Function created      
    #>
[CmdletBinding()]
    param (
        [parameter(Mandatory = $true, HelpMessage = "Your Azure AD Directory ID should be provided")]
        [ValidateNotNullOrEmpty()]
        [string]$TenantID,
        [parameter(Mandatory = $true, HelpMessage = "Application ID for an Azure AD application")]
        [ValidateNotNullOrEmpty()]
        [string]$ClientID,
        [parameter(Mandatory = $true, HelpMessage = "Azure AD Application Client Secret.")]
        [ValidateNotNullOrEmpty()]
        [string]$ClientSecret
        )
Process {
    $ErrorActionPreference = "Stop"
       
    # Construct URI
    $uri = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token"
    # Construct Body
    $body = @{
        client_id     = $clientId
        scope         = "https://graph.microsoft.com/.default"
        client_secret = $clientSecret
        grant_type    = "client_credentials"
        }
    
    try {
        $MyTokenRequest = Invoke-WebRequest –Method Post –Uri $uri –ContentType "application/x-www-form-urlencoded" –Body $body –UseBasicParsing
        $MyToken =($MyTokenRequest.Content | ConvertFrom-Json).access_token
            If(!$MyToken){
                Write-Warning "Failed to get Graph API access token!"
                Exit 1
            }
        $MyHeader = @{"Authorization" = "Bearer $MyToken" }
       }
    catch [System.Exception] {
        Write-Warning "Failed to get Access Token, Error message: $($_.Exception.Message)"; break
    }
    return $MyHeader
    }
}


$global:Header = Get-MSGraphAppToken –TenantID $tenantId –ClientID $ClientID –ClientSecret $ClientSecret 

foreach($user in $users){

    $uri = "https://graph.microsoft.com/v1.0/users/$user"

    $UserID = (Invoke-RestMethod –Uri $uri –Method GET –Headers $global:Header -ContentType "application/json").id

    #$UserID = "b0fdb0c0-8821-4932-b62e-d9675c1b2b6f"

$body = @"
{
    "availability": "DoNotDisturb",
    "activity": "DoNotDisturb",
    "expirationDuration": "PT$($Hours)H"
  }
"@

$uri = "https://graph.microsoft.com/beta/users/$userid/presence/setUserPreferredPresence"

Invoke-RestMethod –Uri $uri –Method Post –Body $body –Headers $global:Header -ContentType "application/json"

}