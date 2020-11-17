# Deployment

## Deployment to Azure App Services

There are two web projects in this sample. To deploy them to **Azure App Services**, you'll need, for each one, to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the web site instead of the local environment.

### Create Azure App Services and Publish the Projects using Visual Studio

Follow the link to [Create Azure App Service and Publish Project with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure?view=vs-2019).

## Create Azure App Services and Publish the Projects using Visual Studio Code

### Create `ToDoListService-aspnetcore` in an Azure App Services

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Select `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `ToDoListService-aspnetcore-contoso.azurewebsites.net`.
1. Next, select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Select `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then select `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it it in the **Dashboard** and select it to open **App Services** **Overview** screen.

#### Publish `ToDoListService-aspnetcore`

1. Install the VS Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Sign-in to App Service using Azure AD Account.
1. Open the ToDoListService-aspnetcore project folder.
1. Choose View > Terminal from the main menu.
1. The terminal opens in the ToDoListService-aspnetcore folder.
1. Run the following command:

    ```console
    dotnet publish --configuration Release
    ```

1. Publish folder is created under path ``bin/Release/<Enter_Framework_FolderName>``.
1. Right Click on **Publish** folder and select **Deploy to Web App**.
1. Select **Create New Web App**, enter unique name for the app.
1. Select Windows as the OS. Press Enter.

#### Update Azure App Services Configuration

1. Go to [Azure portal](https://portal.azure.com).
    - On the Settings tab, select Authentication / Authorization. Make sure `App Service Authentication` is Off. Select **Save**.
1. Browse your website. If you see the default web page of the project, the publication was successful.

#### Update the Azure AD app registration for `ToDoListService-aspnetcore`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `ToDoListService-aspnetcore` application.
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://ToDoListService-aspnetcore-contoso.azurewebsites.net](https://ToDoListService-aspnetcore-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect URIs, make sure that there a new entry using the App service's URI for each redirect URI.

#### Update the `WebApp-blazor-server` to call the `ToDoListService-aspnetcore` Running in Azure App Services

1. In your IDE, go to the `WebApp-blazor-server` project.
2. Open `Client\appsettings.json`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published,
   for example, [https://ToDoListService-aspnetcore-contoso.azurewebsites.net](https://ToDoListService-aspnetcore-contoso.azurewebsites.net).
3. Run the client! If you are trying multiple different client types (for example, .NET, Windows Store, Android, iOS, Electron etc.) you can have them all call this one published web API.

### Create `WebApp-blazor-server` in an Azure App Services

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Select `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `WebApp-blazor-server-contoso.azurewebsites.net`.
1. Next, select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Select `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then select `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it it in the **Dashboard** and select it to open **App Services** **Overview** screen.

#### Publish `WebApp-blazor-server` project

1. Open the WebApp-blazor-server project folder.
1. Choose View > Terminal from the main menu.
1. The terminal opens in the WebApp-blazor-server folder.
1. Run the following command:

    ```console
    dotnet publish --configuration Release
    ```

1. Publish folder is created under path ``bin/Release/<Enter_Framework_FolderName>``.
1. Right Click on **Publish** folder and select **Deploy to Web App**.
1. Select **Create New Web App**, enter unique name for the app.
1. Select Windows as the OS. Press Enter.

#### Update Azure App Services Configuration

1. Go to [Azure portal](https://portal.azure.com). 
    - On the Settings tab, select Authentication / Authorization. Make sure `App Service Authentication` is Off. Select **Save**.
1. Browse your website. If you see the default web page of the project, the publication was successful.

#### Update the Azure AD app registration for `WebApp-blazor-server`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `WebApp-blazor-server` application.
1. In the **Authentication** page for your application, update the Logout URL fields with the address of your service, for example [https://WebApp-blazor-server.azurewebsites.net](https://WebApp-blazor-server.azurewebsites.net)
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://WebApp-blazor-server.azurewebsites.net](https://WebApp-blazor-server.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect URIs, make sure that there a new entry using the App service's URI for each redirect URI.

> :warning: If your app is using an *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will emptied. In addition, if you increase the instance count of your web site, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.