# Readme

## How to configure to run locally

### 1. Prerequisites

You should configure Azure Storage account prior running the sample. For local development use Azure Storage Emulator with below artifacts:

#### Blob containers

`pnp-drone` - contains `templates.xml` - PnP Provisioning XML file and folder `assets` with required files. Templated used is ["Contoso Drone Landing"](https://github.com/SharePoint/sp-dev-provisioning-templates/blob/master/tenant/ContosoDroneLanding/README.md)  
The structure:  

![image](img/storage-1.png)

#### Queue

Create a queue with name `pnp-provision`.

#### Table

Create an Azure Table with name `PnPDroneProvisioning`.

#### Azure Logic app

Azure logic app receives web url from custom SharePoint Site Design. Similar to the [Calling Microsoft Flow from a site script](https://docs.microsoft.com/en-us/sharepoint/dev/declarative-customization/site-design-trigger-flow-tutorial), but Logic App instead of a Flow, because HTTP connector is premium.

### 2. Azure AD app registration

1. Create a new Azure AD app registration, enable explicit flow, add `user_impersonation` scope via Expose an API
2. Take a note on your App display name, App (Client) ID

### 3. SharePoint

Create a new app in SharePoint using AppRegnew.aspx. Add tenant permissions via AppInv.aspx page:

```xml
<AppPermissionRequests AllowAppOnlyPolicy="true">
  <AppPermissionRequest Scope="http://sharepoint/content/tenant" Right="FullControl" />
</AppPermissionRequests>
```

Take a note on ClientId and ClientSecret.

### 4.a Visual Studio: `SignalRHub`

1. Right click on a project -> Manage user secrets. Add below user secrets:

    ``` json
    {
        "SignalR:ConnectionString": "Endpoint=https://localhost:44341;AccessKey=vC9iluK0NakvY1H1OhWDFLXpZqg4KPlE+8TCQ=;Version=1.0;",
        "AzureAd:Instance": "https://login.microsoftonline.com/",
        "AzureAd:TenantId": "<your tenant id>",
        "AzureAd:ClientId": "<client id from step #2>",
        "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
        "SharePointOrigin": "https://<your org>.sharepoint.com"
    }
    ```

    `SignalR:ConnectionString`:  
    `Endpoint` is your host url of SignalRHub  
    `AccessKey` is any string, will be used to communicate to SignalR hub with JWT tokens

### 4.b Visual Studio: `ProvisioningJob`

Azure WebJob relies on some configuration settings, which should be provided via Environment variables. Edit your system environmental variables and add three variables:

- `SignalR:ConnectionString` : the same as from step `#4`
- `AppId` - your client id from step `#3`
- `AppSecret` - your client id from step `#3`

![img](/img/env.png)

### 5. SPFx

1. Open `pnp-notifier\config\package-solution.json` and under `webApiPermissionRequests` change resource name to match the display name from step `#2`
2. Open `pnp-notifier\src\extensions\consts.ts` and update `cientId` with one from step `#2`, `cloudHubUrl` should point to your Azure Web app url (when deployed).
3. Package solution, upload to App Catalog, approve Permission Request like described in the [Manage permission requests](https://docs.microsoft.com/en-us/sharepoint/dev/spfx/use-aadhttpclient#manage-permission-requests)

![img](img/api.png)

### 6. Add Site Script and Site Design

Under `PS\siteScript sample.ps1` change `url` to point to your Logic app trigger url.

### 7. Are you still here? Ok, run it!

Run `gulp serve` in SPFx folder (you need to change `serveConfigurations` under `pnp-notifier\config\serve.json` to match your tenant).

Run WebJob locally, run SignalR web application, add a new message into the `pnp-provision` queue in format:

```json
{
  "WebUrl": "https://<your org>.sharepoint.com/sites/drone-12"
}
```

Queue will trigger your web job, the job will send notification to your SharePoint web site through the SignalR hub.

## How to publish

1. Create a new Azure Web app. 
2. Deploy SignalR hub into the Azure Web app.
3. Deploy WebJob to the same Azure Web app (AlwaysOn should be `true`).
4. On the Web app, add below App settings:
  
   - `AppId` - SharePoint Client Id from step `#3`
   - `AppSecret` - SharePoint Client Secret from step `#3`
   - `AzureAd:ClientId` - Client Id of app registration on step `#2`
   - `AzureAd:Instance` - `https://login.microsoftonline.com/`
   - `AzureAd:TenantId` - your tenant id
   - `AzureWebJobsDashboard` - Azure storage connection string
   - `AzureWebJobsEnv` - `Production`
   - `SharePointOrigin` - `https://<your-org>.sharepoint.com`
   - `SignalR:ConnectionString` - `Endpoint=https://<your azure web pp>.azurewebsites.net;AccessKey=vC9iluK0NakvY1H1ORwhWDFLXpZqg4KPlE+8TCQ=;Version=1.0;`
