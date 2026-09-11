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
                Environment.Exit(0);
            }
            
            Console.WriteLine("Input the sequenceses path\n");
            string? commandsPath = Console.ReadLine();
            string[]? commandsLines = File.Exists(commandsPath) ? File.ReadAllLines(commandsPath) : null;
            
            if(commandsLines == null || !File.Exists(commandsPath))
            {
                Console.WriteLine($"File which is by path {commandsPath} is empty, or the path is wrong!");
                Environment.Exit(0);
            }

            string[] unpackedSequencesLines = UnpackFile(sequencesLines);
            string[] unpackedCommandsLines = UnpackFile(commandsLines);
        }
        static string[] UnpackFile(string[] lines)
        {
            string[] newLines = new string[lines.Length];

            for(int k = 0; k < lines.Length; ++k)
            {
                string line = lines[k];

                string newLine = "";

                for(int i = 0; i < line.Length; ++i)
                {
                    int ascI = (int)line[i];
                    if (i + 1 < line.Length && ascI >= 3 && ascI <= 9 && (int)line[i + 1] >= 65 && (int)line[i + 1] <= 90)
                    {
                        while(ascI > 0)
                        {
                            newLine += line[i + 1];
                            --ascI;
                        }
                    }
                    else newLine += line[i];
                }

                newLines[k] = newLine;
            }

            return newLines;
        }
    }
}
