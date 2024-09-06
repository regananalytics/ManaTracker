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
            string gameName = "re1";

            // 1. Set up the default config directory relative to the app root
            var defaultConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            // Build configuration using cascading configuration sources
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile(defaultConfigPath, optional: false, reloadOnChange: true);

            var appConf = configurationBuilder.Build();
            var pathConf = appConf.GetSection("Path");

            string configPath = "";
            foreach (var setting in pathConf.GetChildren())
            {
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
            var memoryCore = new MemoryCore(gameConfFile, false);

            var memQ = new MemQServer("tcp://*:5556", memoryCore.OutputState);

            Console.WriteLine("Starting ZMQ Server...");
            memQ.Start();

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Stopping ManaServer...");
                memQ.Stop();
                e.Cancel = true;
            };

            Console.WriteLine("Press Ctrl-C to stop ManaServer.");
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}