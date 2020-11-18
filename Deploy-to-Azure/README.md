# Deployment

## Overview

This sample demonstrates how to deploy a Blazor server application coupled with ASP.NET core web API to **Azure Cloud** using [Azure App Service](https://docs.microsoft.com/azure/app-service/). To do so, we will use the [same code sample from Chapter 3](../WebApp-your-API/MyOrg).

## Deployment to Azure App Services

There are two web projects in this sample. To deploy them to **Azure App Services**, you'll need, for each one, to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the web site instead of the local environment.

## Create Azure App Services and publish the projects using Visual Studio

Follow the link to [Create Azure App Service and Publish Project with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure?view=vs-2019).

## Create Azure App Services and publish the projects using Visual Studio Code

### Steps to deploy Web API (ToDoListService-aspnetcore)

#### Step 1. Create and Publish `ToDoListService-aspnetcore` in an Azure App Services

1. Install the VS Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Sign-in to App Service using Azure AD Account.
1. Open the Service project folder.
1. Choose View > Terminal from the main menu.
1. The terminal opens in the Service folder.
1. Run the following command:

    ```console
    dotnet publish --configuration Release
    ```

1. Publish folder is created under path ``bin/Release/<Enter_Framework_FolderName>``.
1. Right Click on **Publish** folder and select **Deploy to Web App**.
1. Select **Create New Web App**, enter unique name for the app, for example **ToDoListService-aspnetcore**.
1. Select Windows as the OS. Press Enter.

#### Step 2. Update Azure App Services Configuration

1. Go to [Azure portal](https://portal.azure.com).
    - On the Settings tab, select Authentication / Authorization. Make sure `App Service Authentication` is Off. Select **Save**.
1. Browse your website. If you see the default web page of the project, the publication was successful.

#### Step 3. Update the Azure AD app registration for `ToDoListService-aspnetcore`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `ToDoListService-aspnetcore` application.
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://ToDoListService-aspnetcore.azurewebsites.net](https://ToDoListService-aspnetcore.azurewebsites.net). Save the configuration.

### Update the `WebApp-blazor-server` to call the `ToDoListService-aspnetcore`

1. In your IDE, go to the `Client` project.
2. Open `Client\appsettings.json`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published,
   for example, [https://ToDoListService-aspnetcore.azurewebsites.net](https://ToDoListService-aspnetcore.azurewebsites.net).
3. Run the client! If you are trying multiple different client types (for example, .NET, Windows Store, Android, iOS, Electron etc.) you can have them all call this one published web API.

### Steps to deploy Web App (WebApp-blazor-server)

#### Step 1. Create and Publish `WebApp-blazor-server` in an Azure App Services

1. Open the Client project folder.
1. Choose View > Terminal from the main menu.
1. The terminal opens in the Client folder.
1. Run the following command:

    ```console
    dotnet publish --configuration Release
    ```

1. Publish folder is created under path ``bin/Release/<Enter_Framework_FolderName>``.
1. Right Click on **Publish** folder and select **Deploy to Web App**.
1. Select **Create New Web App**, enter unique name for the app, for example **WebApp-blazor-server**.
1. Select Windows as the OS. Press Enter.

#### Step 2. Update Azure App Services Configuration

1. Go to [Azure portal](https://portal.azure.com).
    - On the Settings tab, select Authentication / Authorization. Make sure `App Service Authentication` is Off. Select **Save**.
1. Browse your website. If you see the default web page of the project, the publication was successful.

#### Step 3. Update the Azure AD app registration for `WebApp-blazor-server`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `WebApp-calls-API-blazor-server` application.
1. In the **Authentication** page for your application, update the Logout URL fields with the address of your service, for example [https://WebApp-blazor-server.azurewebsites.net](https://WebApp-blazor-server.azurewebsites.net)
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://WebApp-blazor-server.azurewebsites.net](https://WebApp-blazor-server.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect URIs, make sure that there a new entry using the App service's URI for each redirect URI.

> :warning: If your app is using an *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will emptied. In addition, if you increase the instance count of your web site, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

## More information

- [App Service overview](https://docs.microsoft.com/azure/app-service/overview)
- [Deploy ASP.NET Core apps to Azure App Service](https://docs.microsoft.com/aspnet/core/host-and-deploy/azure-apps)

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory`] [`msal`] [`dotnet`].

If you find a bug in the sample, raise the issue on [GitHub Issues](../../../../issues).

To provide feedback on or suggest features for Azure Active Directory, visit [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.