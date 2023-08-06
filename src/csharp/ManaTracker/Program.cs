// See https://aka.ms/new-console-template for more information
using MemCore;

Console.WriteLine("Hello, World!");

string gameName = "re1";
var memoryCore = new MemoryCore(gameName);

var state = memoryCore.GetState();
Console.WriteLine(state.ToString());