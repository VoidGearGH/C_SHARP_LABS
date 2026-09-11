using System;
using System.Text;

namespace Labs
{
    internal class Program
    {
        struct GeneticData
        {
            public string protein;
            public string organism;
            public string amino_acids;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Input the sequenceses path\n");
            string? sequencesPath = Console.ReadLine();
            string[]? sequencesLines = File.Exists(sequencesPath) ? File.ReadAllLines(sequencesPath) : null;
            
            if(sequencesLines == null || !File.Exists(sequencesPath))
            {
                Console.WriteLine($"File which is by path {sequencesPath} is empty, or the path is wrong!");
                Environment.Exit(0);
            }
            
            Console.WriteLine("Input the commands path\n");
            string? commandsPath = Console.ReadLine();
            string[]? commandsLines = File.Exists(commandsPath) ? File.ReadAllLines(commandsPath) : null;
            
            if(commandsLines == null || !File.Exists(commandsPath))
            {
                Console.WriteLine($"File which is by path {commandsPath} is empty, or the path is wrong!");
                Environment.Exit(0);
            }
        }
        static string UnpackFile(string line)
        {
            StringBuilder newLine = new StringBuilder();

            for (int i = 0; i < line.Length; ++i)
            {
                int ascI = (int)line[i];
                if (i + 1 < line.Length && ascI >= 51 && ascI <= 57 && (int)line[i + 1] >= 65 && (int)line[i + 1] <= 90)
                {
                    int count = ascI - 48;
                    for (int j = 0; j < count; j++)
                    {
                        newLine.Append(line[i + 1]);
                    }
                    i++;
                }
                else
                {
                    newLine.Append(line[i]);
                }
            }

            return newLine.ToString();
        }
    }
}
