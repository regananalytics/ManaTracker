using System;
using MemCore;

namespace ManaServer
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Starting ManaServer...");

            // Default configuration path relative to the DLL location
            string configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "../cfg"
            );

            string gameName = "re1";
            string gameConfFile = Path.Combine(configPath, gameName, "mem.yaml");
            bool dryRun = false;
            var memoryCore = new MemoryCore(gameConfFile, dryRun);

            if (dryRun)
            {
                memoryCore.OutputConfig();
                return;
            }

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