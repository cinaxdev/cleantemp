using System;
using System.IO;
using System.Security.Principal;
using System.Diagnostics;
using System.Threading;

class CleanTemp
{
    static void Main(string[] args)
    {
        if (!IsAdministrator())
        {
            RestartAsAdmin(args);
            return;
        }

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("cleantemp > ");
            Console.ResetColor();
            string input = args.Length > 0 ? args[0] : Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteColored("Blank input", ConsoleColor.Yellow);
                args = new string[0];
                continue;
            }

            string command = input.ToLower();
            string userTemp = Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath();
            string windowsTemp = @"C:\Windows\Temp";

            bool prefetchSuccess = true;

            switch (command)
            {
                case "fullclean":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    CleanFolder(@"C:\Windows\Prefetch", "Prefetch");
                    EmptyRecycleBin();
                    break;

                case "cleantemp":
                    CleanFolder(userTemp, "User Temp");
                    CleanFolder(windowsTemp, "Windows Temp");
                    break;

                case "cleanprefetch":
                    CleanFolder(@"C:\Windows\Prefetch", "Prefetch");
                    break;

                case "credits":
                    WriteColored("by cinax - https://github.com/cinaxdev/cleantemp", ConsoleColor.Blue);
                    break;

                case "exit":
                    return;

                default:
                    WriteColored("Invalid command! Available commands are cleantemp, fullclean and cleanprefetch", ConsoleColor.Red);
                    prefetchSuccess = false;
                    break;
            }

            if (prefetchSuccess || command == "cleantemp")
            {
                WriteColored("Done, thanks for using!", ConsoleColor.Green);
            }

            args = new string[0];
        }
    }

    static void CleanFolder(string path, string displayName)
    {
        if (!Directory.Exists(path))
        {
            WriteColored($"{displayName} folder doesnt exist: {path}", ConsoleColor.Yellow);
            return;
        }

        bool success = true;

        try
        {
            foreach (string file in Directory.GetFiles(path))
            {
                try { File.Delete(file); } catch { success = false; }
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                try { Directory.Delete(dir, true); } catch { success = false; }
            }

            if (success)
                WriteColored($"{displayName} cleaned: {path}", ConsoleColor.Green);
            else
                WriteColored($"{displayName} could not be completely cleaned (Cannot delete some files): {path}", ConsoleColor.DarkYellow);
        }
        catch
        {
            WriteColored($"{displayName} could not be cleaned: {path}", ConsoleColor.Red);
        }
    }

    static void EmptyRecycleBin()
    {
        try
        {
            var shell = Type.GetTypeFromProgID("Shell.Application");
            dynamic recycleBin = Activator.CreateInstance(shell);
            recycleBin.NameSpace(10).Items().InvokeVerb("delete");
            WriteColored("Recycle Bin emptied", ConsoleColor.Green);
        }
        catch
        {
            WriteColored("Cannot empty recycle bin", ConsoleColor.Red);
        }
    }

    static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static void RestartAsAdmin(string[] args)
    {
        var exeName = Process.GetCurrentProcess().MainModule.FileName;
        var startInfo = new ProcessStartInfo
        {
            FileName = exeName,
            Verb = "runas",
            Arguments = string.Join(" ", args),
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch { }

        Environment.Exit(0);
    }

    static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}