using Console_Auth_App;
using System.CommandLine;

var root = new RootCommand();

var authCommand = new Command("auth", "Authenntication with local server");
authCommand.SetHandler(Auth.Handler);

var callCommand = new Command("call", "Perform authenicated call");
callCommand.SetHandler(Call.Handler);

root.AddCommand(authCommand);
root.AddCommand(callCommand);