using System;
using MemCore;
using Microsoft.Extensions.Configuration;

namespace ManaServer
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("\nStarting ManaServer...");

            // Build configuration using cascading configuration sources
            var defaultConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile(defaultConfigPath, optional: false, reloadOnChange: true);

            var appConf = configurationBuilder.Build();

            // Top-level settings
            string gameName = appConf["Game"] ?? "re1";
            bool verbose = appConf["Output:verbose"] == "true";
            int interval = int.Parse(appConf["Output:interval"] ?? "1000");

            // Path Settings
            var pathConf = appConf.GetSection("Path");
            string configPath = "";
            foreach (var setting in pathConf.GetChildren())
            {
                if (setting.Value == null)
                    continue;
                configPath = Path.GetFullPath(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, setting.Value)
                );
                if (Path.Exists(configPath))
                    break;
            }
            
            Console.WriteLine($"    Using Game Config Path: {configPath}");
            Console.WriteLine("    Loading Game Config: " + gameName);
            Console.WriteLine("\n");

            string gameConfFile = Path.Combine(configPath, gameName, "mem.yaml");

            MemoryCore memoryCore;
            // Start MemCore
            Console.WriteLine("Waiting to connect to game...");
            while (true)
            {
                try
                {
                    memoryCore = new MemoryCore(gameConfFile, false);
                    break;
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"An exception occured in MemCore: {ex.Message}");
                    throw;
                }
            }
            Console.WriteLine("Connected!\n");
            
            var memQ = new MemQServer("tcp://*:5556", memoryCore.OutputState, interval, verbose);

            Console.WriteLine("Starting ZMQ Server...");
            memQ.Start();

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Stopping ManaServer...");
                memQ.Stop();
                e.Cancel = true;
                Environment.Exit(0);
            };

            Console.WriteLine("Press Ctrl-C to stop ManaServer.");
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}