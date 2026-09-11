using System;

namespace Labs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input the sequenceses path\n");
            string? sequencesPath = Console.ReadLine();
            string[]? sequencesLines = File.Exists(sequencesPath) ? File.ReadAllLines(sequencesPath) : null;
            
            if(sequencesLines == null || !File.Exists(sequencesPath))
            {
                Console.WriteLine($"File which is by path {sequencesPath} is empty, or the path is wrong!");
            }
            
            Console.WriteLine("Input the sequenceses path\n");
            string? commandsPath = Console.ReadLine();
            string[]? commandsLines = File.Exists(commandsPath) ? File.ReadAllLines(commandsPath) : null;
            
            if(commandsLines == null || !File.Exists(commandsPath))
            {
                Console.WriteLine($"File which is by path {commandsPath} is empty, or the path is wrong!");
            }
        }
    }
}
