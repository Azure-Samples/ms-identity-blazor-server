## Using the sample

<details>
 <summary>Expand to see how to use the sample</summary>

 Running from **VS Code**:

 ```powershell
  dotnet run
 ```

 If you're running from Visual Studio, press **F5** or **Ctrl+F5** (for no debug run)

 On the main page you will be offered to Log In or to go to a "Fetch data" page
 If you choose to go to "Fetch data" page without logging-in, you will be asked to login with a standard UI.
 When the application will be logged in, it will try to connect to Azure SQL Database with an [Access Token](https://aka.ms/access-tokens) it acquired for the currently logged-in user.
 Successful connection will be indicated when the page will state that the user is logged-in into the database and a table with mock forecast data is displayed.

 ![fetch_data_page](./Client/ReadmeFiles/fetch-data-page.png)

 The page displays a message with user and host names that are values of @user and @host on SQL Database.

Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

[Consider taking a moment to share your experience with us.](https://forms.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR73pcsbpbxNJuZCMKN0lURpUN0FTWEJKSlBBWEZPV1JQMVBMMzBLNjFHRyQlQCN0PWcu)

</details>
