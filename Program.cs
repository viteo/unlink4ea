using Microsoft.Win32;
using Steamworks;
using System.Diagnostics;

if (args.Length == 0 || String.IsNullOrEmpty(args[0]))
{
    Console.WriteLine("No arguments");
    return;
}

Uri uri = new Uri(args[0]);
Console.WriteLine($"AbsoluteUri: {uri.AbsoluteUri}");
Console.WriteLine($"Scheme: {uri.Scheme}");
Console.WriteLine($"AbsolutePath: {uri.AbsolutePath}");
Console.WriteLine($"Host: {uri.Host}");
Console.WriteLine($"Query: {uri.Query}");

var queryDict = System.Web.HttpUtility.ParseQueryString(uri.Query);
foreach (var q in queryDict.AllKeys)
    Console.WriteLine(q + ": " + queryDict[q]);

var steamPath = Registry.GetValue(Registry.ClassesRoot.Name + "\\steam\\Shell\\Open\\Command", string.Empty, null)?.ToString();
if (steamPath == null)
{
    Console.WriteLine("Steam installation not found");
    return;
}

var jfoFilepath = Path.Combine(Path.GetDirectoryName(steamPath.Split('"')[1]),
                "steamapps\\common\\Jedi Fallen Order\\SwGame\\Binaries\\Win64\\SwGame-Win64-Shipping.exe");
                //"steamapps\\common\\Jedi Fallen Order\\SwGame\\Binaries\\Win64\\starwarsjedifallenorder.exe");
Console.WriteLine(jfoFilepath);
var process = new Process();
process.StartInfo.FileName = jfoFilepath;
process.StartInfo.WorkingDirectory = Path.GetDirectoryName(jfoFilepath);
process.StartInfo.UseShellExecute = false;

try
{
    if (!SteamAPI.Init())
    {
        Console.WriteLine("SteamAPI.Init() failed!");
        return;
    }
}
catch (DllNotFoundException e)
{ // We check this here as it will be the first instance of it.
    Console.WriteLine(e);
    return;
}

if (!Packsize.Test())
{
    Console.WriteLine("You're using the wrong Steamworks.NET Assembly for this platform!");
    return;
}

if (!DllCheck.Test())
{
    Console.WriteLine("You're using the wrong dlls for this platform!");
    return;
}

//start
process.Start();

Console.WriteLine("Requesting Current Stats - " + SteamUserStats.RequestCurrentStats());

Console.WriteLine("CurrentGameLanguage: " + SteamApps.GetCurrentGameLanguage());
Console.WriteLine("PersonaName: " + SteamFriends.GetPersonaName());

{
    string folder;
    uint length = SteamApps.GetAppInstallDir(SteamUtils.GetAppID(), out folder, 260);
    Console.WriteLine("AppInstallDir: " + length + " " + folder);
}

//process.WaitForExit();
while (!process.HasExited)
{
    // Must be called from the primary thread.
    SteamAPI.RunCallbacks();

    if (Console.KeyAvailable)
    {
        ConsoleKeyInfo info = Console.ReadKey(true);

        if (info.Key == ConsoleKey.Escape)
        {
            break;
        }
        else if (info.Key == ConsoleKey.Spacebar)
        {
            SteamUserStats.RequestCurrentStats();
            Console.WriteLine("Requesting Current Stats");
        }
    }

    Thread.Sleep(50);
}

SteamAPI.Shutdown();

Console.WriteLine("Game finished");
Console.ReadLine();