using System;
using MemCore;

namespace ManaTracker
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Starting ManaTracker...");

            string gameName = "re1";
            bool dryRun = false;
            var memoryCore = new MemoryCore(gameName, dryRun);

            if (dryRun)
            {
                memoryCore.OutputConfig();
                return;
            }

            var memQ = new MemQServer("tcp://*:5556", memoryCore.OutputState);

            Console.WriteLine("Starting ManaTracker Memory Server...");
            memQ.Start();

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Stopping ManaTracker...");
                memQ.Stop();
                e.Cancel = true;
            };

            Console.WriteLine("Press Ctrl-C to stop ManaTracker.");
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}