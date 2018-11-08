# SHUsersManagement
Users management practice on .Net Core, using WebAPI and ASP.NET MVC Core

## Tools required to run the ASP.NET Core 2.1 Tutorial Example Locally
To develop and run ASP.NET Core applications locally, download and install the following:

- [.NET Core SDK](https://www.microsoft.com/net/download/core) - includes the .NET Core runtime and command line tools
- [Visual Studio Code](https://code.visualstudio.com/) - code editor that runs on Windows, Mac and Linux
- [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) for Visual Studio Code - adds support to VS Code for developing .NET Core applications

## Running the solution
- Download or clone the tutorial project code from https://github.com/fennnan/SHUsersManagement.git

### Running the Users REST API server Locally
1. Open a shell command windows in the Project root folder
2. Change folder to the **Rest Api Server** project root folder (where the RestServer.csproj file is located)
    - `cd RestServer`
3. Start the api by running `dotnet run` from the command line. You should see the message `Now listening on: http://localhost:5000`. You can test the api directly using an application such as Postman or you can test it with one of the single page applications below.

## Running the ASP.NET Core Mvc UI Locally
1. Open a shell command windows in the Project root folder
2. Change folder to the **Mvc UI application** project root folder (where the MvcUI.csproj file is located)
    - `cd MvcUI`
3. Start the api by running `dotnet run` from the command line in the project root folder (where the WebApi.csproj file is located), you should see the message `Now listening on: http://localhost:6000`
4. You can test the app by opening writting http://localhost:6000 on the direction tool bar.

## Testing the solution
### Unit test

I have used XUnit for writting the Unit Test and Mock as a mocking framework. Because this project is intended as a proof of concept there is only Unit Test for the Rest API Server project.

1. Open a shell command windows in the Project root folder
2. Change folder to the **Rest Api Server Unit Test** project root folder (where the RestServer.Test.csproj file is located)
    - `cd RestServer.Test`
3. Start the api by running `dotnet test` from the command line. After few second it will show the results of the tests

### Integration test

1. Have up and running both projects, Rest API Server and Mvc UI app
2. Log in in the Mvc UI with one of these users:
    - **Admin**/_admin1_ - Rol **ADMIN**
    - **User1**/_user1_ - Rol **PAGE_1**
    - **User2**/_user2_ - Rol **PAGE_2**
    - **User3**/_user3_ - Rol **PAGE_3**
3. Navigate through the differents pages

## To do tasks list

I didn't want to extend the project for a long time, so I decide to not implement some litle (or not so litle), details of the project. 

- Add HTTPS support. The Basic Authentication is sended as a plain text and it shouldn't be in a not encripted web
- Add Unit Test for the Mvc UI app
- Add more tests for the Rest API Server
