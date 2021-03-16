using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace RustServerUpdater
{
    class Program
    {
        static readonly string configFileName = GetPathToExe() + "RSU_cfg.txt";
        static void Main(string[] args)
        {
            Console.WindowWidth = 130;
            WriteLine("__________                __   _________                                ____ ___            .___       __                \r\n\\______   \\__ __  _______/  |_/   _____/ ______________  __ ___________|    |   \\______   __| _/____ _/  |_  ___________ \r\n |       _/  |  \\/  ___/\\   __\\_____  \\_/ __ \\_  __ \\  \\/ // __ \\_  __ \\    |   /\\____ \\ / __ |\\__  \\\\   __\\/ __ \\_  __ \\\r\n |    |   \\  |  /\\___ \\  |  | /        \\  ___/|  | \\/\\   /\\  ___/|  | \\/    |  / |  |_> > /_/ | / __ \\|  | \\  ___/|  | \\/\r\n |____|_  /____//____  > |__|/_______  /\\___  >__|    \\_/  \\___  >__|  |______/  |   __/\\____ |(____  /__|  \\___  >__|   \r\n        \\/           \\/              \\/     \\/                 \\/                |__|        \\/     \\/          \\/       ", ConsoleColor.Cyan);
            WriteLine(Environment.NewLine);
            WriteLine("Choose an option:");
            WriteLine("1. Update Rust server");
            WriteLine("2. Update Oxide for your server");

            switch (Console.ReadLine())
            {
                case "1":
                    UpdateServer();
                    break;
                case "2":
                    UpdateOxide().Wait();
                    break;
                default:
                    WriteLine("There are only 2 options available", ConsoleColor.Red);
                    break;
            }
        }

        static void UpdateServer()
        {
            var cfg = ReadCfg();
            if (cfg == null || cfg.Length < 2 || !File.Exists(cfg[0]))
            {
                cfg = AskCfg();
                WriteCfg(cfg);
            }

            try
            {
                var process = Process.Start(cfg[0], "+login anonymous +app_update 258550 validate +quit");
                WriteLine("Started update successfully", ConsoleColor.Cyan);
            }
            catch (Exception e) { WriteLine($"Failed to start update: {e.Message}"); }
        }

        static async Task UpdateOxide()
        {
            var cfg = ReadCfg();
            if (cfg == null || cfg.Length < 2 || !Directory.Exists(cfg[1]))
            {
                cfg = AskCfg();
                WriteCfg(cfg);
            }

            WriteLine(Environment.NewLine);
            var updatePercentage = 0;

            var webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                if (e.ProgressPercentage >= updatePercentage + 5)
                {
                    WriteLine($"Downloading Oxide: {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive} bytes)");
                    updatePercentage += 5;
                }
            };

            WriteLine("Oxide download started", ConsoleColor.Cyan);
            await webClient.DownloadFileTaskAsync(new Uri("https://umod.org/games/rust/download/public"), $"{cfg[1]}\\oxide.zip");

            ZipFile.ExtractToDirectory($"{cfg[1]}\\oxide.zip", cfg[1], true);
            File.Delete($"{cfg[1]}\\oxide.zip");

            WriteLine("Oxide updated successfully", ConsoleColor.Cyan);
        }

        static string[] AskCfg()
        {
            var cfg = new string[2];
            WriteLine("Please, enter full path to SteamCMD executable (e.g. C:\\Program Files (x86)\\Steam\\steamcmd.exe)", ConsoleColor.Yellow);
            while (true)
            {
                var path = Console.ReadLine();
                if (path != string.Empty && File.Exists(path))
                {
                    cfg[0] = path;
                    break;
                }
                WriteLine("Failed to find SteamCMD at this path, try to enter another one", ConsoleColor.Red);
            }

            WriteLine("Please, enter path to Rust server folder", ConsoleColor.Yellow);
            while (true)
            {
                var path = Console.ReadLine();
                if (Directory.Exists(path))
                {
                    cfg[1] = path;
                    break;
                }
                WriteLine("Failed to find folder at this path, try to enter another one", ConsoleColor.Red);
            }

            return cfg;
        }

        static string[] ReadCfg()
        {
            if (!File.Exists(configFileName)) { return null; }
            return File.ReadAllLines(configFileName);
        }

        static void WriteCfg(string[] cfg)
        {
            File.WriteAllLines(configFileName, cfg);
        }

        static string GetPathToExe()
        {
            var filename = Process.GetCurrentProcess().MainModule.FileName;
            return filename.Remove(filename.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
        }

        static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}