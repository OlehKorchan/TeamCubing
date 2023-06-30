# TeamCubing
Link to the current hosted instance - https://team-cubing.azurewebsites.net/

# Branching rules:

1. Workflow is GitFlow
2. Use the following prefixes for your feature branches: ["feature", "bugfix", "test"]
   - "feauture" - for new features or improvements of existing functionality
   - "bugfix" - for bug fixes
   - "test" - for tests ONLY, if the branch contains other changes - use another prefix
3. If your changes don't belong to the existing JIRA ticket, please describe your commits using the rules: https://gist.github.com/robertpainsi/b632364184e70900af4ab688decf6f53
4. If you have a JIRA ticket for your branch, please use its JIRA number in the branch name as well as in the commit message
5. Examples of good branch names:
   - "feature/TCUBING-99-Add-megaminx-support"
   - "bugfix/Fix-timer-issue"
  
# How to run project locally

# Required software

1. Any C# IDE you want, suggestions: JetBrains Rider, VSCode, Visual Studio.
2. .NET 6 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
3. Azure CosmosDB Emulator: https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21
4. NodeJS: https://nodejs.org/en
5. Angular: https://angular.io/guide/setup-local

# Startup

1. To run application just run following commands inside root directory of the project:
2. ```cd src/TeamCubing.API```
3. ```dotnet run```
4. If you want to use client application along with hosted server instance - run an angular app using production mode
   either change the link to the API in environment.ts file.
   Run client application from the "src/TeamCubing.API/ClientApp/src/app" using ```ng serve``` and appropriate params.
   directory
