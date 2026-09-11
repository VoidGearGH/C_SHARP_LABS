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

        static void ExecuteSearch(StreamWriter writer, List<GeneticData> proteins, string sequence)
        {
            bool found = false;
            foreach (var p in proteins)
            {
                if (p.amino_acids.Contains(sequence))
                {
                    writer.WriteLine(p.organism);
                    writer.WriteLine(p.protein);
                    found = true;
                }
            }
            if (!found)
            {
                writer.WriteLine("NOT FOUND");
            }
        }

        static void ExecuteDiff(StreamWriter writer, List<GeneticData> proteins, string protein1, string protein2)
        {
            GeneticData? p1 = proteins.FirstOrDefault(p => p.protein == protein1);
            GeneticData? p2 = proteins.FirstOrDefault(p => p.protein == protein2);

            if (p1 == null && p2 == null)
            {
                writer.WriteLine($"MISSING: {protein1}, {protein2}");
                return;
            }
            if (p1 == null) { writer.WriteLine($"MISSING: {protein1}"); return; }
            if (p2 == null) { writer.WriteLine($"MISSING: {protein2}"); return; }

            int diffCount = 0;
            int minLength = Math.Min(p1.Value.amino_acids.Length, p2.Value.amino_acids.Length);
            int maxLength = Math.Max(p1.Value.amino_acids.Length, p2.Value.amino_acids.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (p1.Value.amino_acids[i] != p2.Value.amino_acids[i])
                {
                    diffCount++;
                }
            }
            diffCount += (maxLength - minLength);

            writer.WriteLine($"amino-acids difference: {diffCount}");
        }

        static void ExecuteMode(StreamWriter writer, List<GeneticData> proteins, string proteinName)
        {
            GeneticData? p = proteins.FirstOrDefault(p => p.protein == proteinName);
            if (p == null)
            {
                writer.WriteLine($"MISSING: {proteinName}");
                return;
            }

            Dictionary<char, int> counts = new Dictionary<char, int>();
            foreach (char c in p.Value.amino_acids)
            {
                if (counts.ContainsKey(c)) counts[c]++;
                else counts[c] = 1;
            }

            int maxCount = 0;
            char bestChar = (char)('Z' + 1);

            foreach (var kvp in counts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    bestChar = kvp.Key;
                }
                else if (kvp.Value == maxCount && kvp.Key < bestChar)
                {
                    bestChar = kvp.Key;
                }
            }

            writer.WriteLine($"amino-acid occurs: {bestChar} {maxCount}");
        }

        static string RLEncoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids)) return amino_acids;
            StringBuilder sb = new StringBuilder();
            int count = 1;
            char current = amino_acids[0];

            for (int i = 1; i < amino_acids.Length; i++)
            {
                if (amino_acids[i] == current)
                {
                    count++;
                }
                else
                {
                    if (count >= 3) sb.Append(count);
                    sb.Append(current);
                    current = amino_acids[i];
                    count = 1;
                }
            }
            if (count >= 3) sb.Append(count);
            sb.Append(current);

            return sb.ToString();
        }
    }
}
