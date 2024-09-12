using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using MemCore;

namespace ManaServer
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("\nStarting ManaServer...");

            // Positional Argument
            var gameArg = new Argument<string>(
                "game", 
                description: "The game configuration to use"
            );
            // Options
            var cfgOpt = new Option<DirectoryInfo>(
                aliases: new[] {"--config", "-c"},
                description: "Path to a custom config directory"
            );
            var intOpt = new Option<int>(
                aliases: new[] {"--interval", "-i"},
                getDefaultValue: () => 1000,
                description: "State update interval in ms"
            );
            var portOpt = new Option<int>(
                aliases: new[] {"--port", "-p"},
                getDefaultValue: () => 5556,
                description: "Port to use for zmq messages"
            );
            var verbOpt = new Option<bool>(
                aliases: new[] {"--verbose", "-v"},
                description: "Enable verbose output"
            );

            // Root command
            var rootCommand = new RootCommand("ManaServer Game memory reader app");
            rootCommand.Add(gameArg);
            rootCommand.Add(cfgOpt);
            rootCommand.Add(intOpt);
            rootCommand.Add(portOpt);
            rootCommand.Add(verbOpt);

            // Handler
            rootCommand.SetHandler((game, cfgDir, interval, port, verbose) =>
            {
                if (verbose)
                    Console.WriteLine($"Config Directory: {cfgDir.FullName}");

                if (!cfgDir.Exists)
                    throw new System.Exception($"The provided config directory does not exist!");

                DirectoryInfo gameCfgDir = new DirectoryInfo(Path.Combine(cfgDir.FullName, game));

                Console.WriteLine($"Loading game config for {game}...");
                if (verbose)
                    Console.WriteLine($"Searching for Game Config: {gameCfgDir.FullName}");

                if (!gameCfgDir.Exists)
                    throw new System.Exception($"A config for the game {game} could not be found in the provided config directory!");

                FileInfo gameMemCfgFile = new FileInfo(Path.Combine(gameCfgDir.FullName, "mem.yaml"));

                if (verbose)
                    Console.WriteLine($"Searching for Game Memory Config: {gameMemCfgFile.FullName}");

                if (!gameMemCfgFile.Exists)
                    throw new System.Exception($"The Memory Config Yaml is missing from the game config!");

                // Create MemoryCore object and connect to process
                MemoryCore memoryCore;
                Console.WriteLine("Waiting to connect to game...");
                while (true)
                {
                    try
                    {
                        memoryCore = new MemoryCore(gameMemCfgFile.FullName, false);
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        // Process not available. Keep waiting
                        Thread.Sleep(500);
                        continue;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine($"A system exception occured in MemCore: {ex.Message}");
                        throw;
                    }
                }

                Console.WriteLine("Connected!\n");

                var memQ = new MemQServer($"tcp://*:{port}", memoryCore.OutputState, interval, verbose);

                Console.WriteLine("Starting Message Server...");
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
                    Thread.Sleep(interval);
                }
            },
            gameArg, cfgOpt, intOpt, portOpt, verbOpt);

            return await rootCommand.InvokeAsync(args);
        }
    }
}