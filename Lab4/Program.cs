using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Serialization;

namespace Lab3
{
    public interface IToken
    {
        string Value { get; set; }
    }

    [Serializable]
    public abstract class ExpressionSet<T> : IToken where T : ExpressionSet<T>
    {
        [XmlIgnore]
        public string Value { get; set; }

        [XmlIgnore]
        public StringBuilder Sb { get; set; }

        protected ExpressionSet(StringBuilder sb)
        {
            Sb = sb;
        }

        public static T operator +(ExpressionSet<T> expr, char c)
        {
            return expr.Create(expr.Sb.Append(c).ToString());
        }

        public static T operator +(ExpressionSet<T> expr, string s)
        {
            return expr.Create(expr.Sb.Append(s).ToString());
        }

        public abstract T Create(string value);

        public override string ToString() => Sb.ToString();
    }

    public class Word : ExpressionSet<Word>
    {
        public Word() : base(new StringBuilder("")) { }
        public Word(string value) : base(new StringBuilder(value)) { Value = value; }
        public override Word Create(string value) => new Word(value);
    }

    public class Punctuation : ExpressionSet<Punctuation>
    {
        public Punctuation() : base(new StringBuilder("")) { }
        public Punctuation(string value) : base(new StringBuilder(value)) { Value = value; }
        public override Punctuation Create(string value) => new Punctuation(value);
    }

    [XmlInclude(typeof(Word))]
    [XmlInclude(typeof(Punctuation))]
    public class Sentence : ExpressionSet<Sentence>
    {
        public Sentence() : base(new StringBuilder("")) { }

        [XmlIgnore]
        public List<IToken> Tokens { get; set; } = new();

        public Sentence(string value) : base(new StringBuilder(value)) { }
        public override Sentence Create(string value) => new Sentence(value);

        [XmlIgnore]
        public int WordCount => Tokens.OfType<Word>().Count();

        [XmlIgnore]
        public int Length => Sb.Length;

        [XmlIgnore]
        public bool IsInterrogative => Tokens.OfType<Punctuation>().Any(p => p.Value.Contains("?"));
    }

    public class Text : ExpressionSet<Text>
    {
        [XmlIgnore]
        public List<Sentence> Sentences { get; set; } = new List<Sentence>();

        public Text() : base(new StringBuilder("")) { }
        public Text(string value) : base(new StringBuilder(value)) { }
        public override Text Create(string value) => new Text(value);

        public void PrintSortedByWordCount()
        {
            var sorted = Sentences.OrderBy(s => s.WordCount).ToList();
            foreach (var s in sorted)
            {
                Console.WriteLine($"{s.WordCount} words: {s.ToString().Trim()}");
            }
        }

        public void PrintSortedByLength()
        {
            var sorted = Sentences.OrderBy(s => s.Length).ToList();
            foreach (var s in sorted)
            {
                Console.WriteLine($"{s.Length} chars: {s.ToString().Trim()}");
            }
        }

        public void PrintUniqueWordsOfLengthInInterrogative(int length)
        {
            var words = Sentences.Where(s => s.IsInterrogative)
                                 .SelectMany(s => s.Tokens.OfType<Word>().Where(w => w.Value.Length == length).Select(w => w.Value))
                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                 .ToList();
            Console.WriteLine($"Words of length {length} in interrogative sentences: {string.Join(", ", words)}");
        }

        public void RemoveWordsOfLengthStartingWithConsonant(int length)
        {
            const string consonants = "бвгджзйклмнпрстфхцчшщBCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz";
            foreach (var sentence in Sentences)
            {
                var toRemove = sentence.Tokens.OfType<Word>()
                    .Where(w => w.Value.Length == length && w.Value.Length > 0 && consonants.Contains(w.Value[0]))
                    .ToList();
                foreach (var word in toRemove)
                {
                    sentence.Tokens.Remove(word);
                }
                RebuildSentenceString(sentence);
            }
        }

        public void ReplaceWordsOfLengthInSentence(int sentenceIndex, int length, string replacement)
        {
            if (sentenceIndex >= 0 && sentenceIndex < Sentences.Count)
            {
                var sentence = Sentences[sentenceIndex];
                for (int i = 0; i < sentence.Tokens.Count; i++)
                {
                    if (sentence.Tokens[i] is Word w && w.Value.Length == length)
                    {
                        sentence.Tokens[i] = new Word(replacement);
                    }
                }
                RebuildSentenceString(sentence);
            }
        }

        public void RemoveStopWords(HashSet<string> stopWords)
        {
            foreach (var sentence in Sentences)
            {
                var toRemove = sentence.Tokens.OfType<Word>()
                    .Where(w => stopWords.Contains(w.Value.ToLower()))
                    .ToList();
                foreach (var word in toRemove)
                {
                    sentence.Tokens.Remove(word);
                }
                RebuildSentenceString(sentence);
            }
        }

        private void RebuildSentenceString(Sentence sentence)
        {
            sentence.Sb.Clear();
            foreach (var token in sentence.Tokens)
            {
                if (token is Word)
                {
                    sentence.Sb.Append(token.Value).Append(' ');
                }
                else
                {
                    sentence.Sb.Append(token.Value);
                }
            }
        }

        public void ExportToXml(string filePath)
        {
            var xmlText = new XmlText
            {
                Sentences = Sentences.Select(s => new XmlSentence
                {
                    Text = s.ToString(),
                    WordCount = s.WordCount,
                    Length = s.Length,
                    IsInterrogative = s.IsInterrogative,
                    Words = s.Tokens.OfType<Word>().Select(w => w.Value).ToList()
                }).ToList()
            };

            var serializer = new XmlSerializer(typeof(XmlText));
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            serializer.Serialize(writer, xmlText);
        }
    }

    [Serializable]
    [XmlRoot("Text")]
    public class XmlText
    {
        [XmlArray("Sentences")]
        [XmlArrayItem("Sentence")]
        public List<XmlSentence> Sentences { get; set; } = new();
    }

    [Serializable]
    public class XmlSentence
    {
        [XmlAttribute("Text")]
        public string Text { get; set; }

        [XmlAttribute("WordCount")]
        public int WordCount { get; set; }

        [XmlAttribute("Length")]
        public int Length { get; set; }

        [XmlAttribute("IsInterrogative")]
        public bool IsInterrogative { get; set; }

        [XmlArray("Words")]
        [XmlArrayItem("Word")]
        public List<string> Words { get; set; } = new();
    }

    public class ExpressionsCollector
    {
        public List<Word> words = new();
        public List<Punctuation> punctuations = new();
        public List<Sentence> sentences = new();
        public List<Text> text = new();
    }

    public class Parser
    {
        private string[]? lines;
        public ExpressionsCollector collector;

        public Parser() { }

        public Text Parse(string? path)
        {
            collector = new ExpressionsCollector();
            Text resultText = new Text("");

            if (path != null && File.Exists(path))
            {
                lines = File.ReadAllLines(path, Encoding.UTF8);
                Sentence currentSentence = new Sentence("");

                foreach (string line in lines)
                {
                    var matches = Regex.Matches(line, @"\p{L}+|[^\p{L}\s]+");

                    foreach (Match match in matches)
                    {
                        string value = match.Value;
                        if (Regex.IsMatch(value, @"^\p{L}+$"))
                        {
                            Word word = new Word(value);
                            currentSentence.Tokens.Add(word);
                            currentSentence.Sb.Append(value).Append(' ');
                            collector.words.Add(word);
                        }
                        else
                        {
                            Punctuation punct = new Punctuation(value);
                            currentSentence.Tokens.Add(punct);
                            currentSentence.Sb.Append(value);
                            collector.punctuations.Add(punct);

                            if (value.Contains(".") || value.Contains("!") || value.Contains("?"))
                            {
                                resultText.Sentences.Add(currentSentence);
                                collector.sentences.Add(currentSentence);
                                currentSentence = new Sentence("");
                            }
                        }
                    }
                }

                if (currentSentence.Tokens.Count > 0)
                {
                    resultText.Sentences.Add(currentSentence);
                    collector.sentences.Add(currentSentence);
                }
            }
            return resultText;
        }
    }

    public class Menu
    {
        private Text text;
        private Parser parser;

        public void Start()
        {
            parser = new Parser();
            Console.Write("Enter input file path: ");
            string path = Console.ReadLine();
            text = parser.Parse(path);

            if (text == null || text.Sentences.Count == 0)
            {
                Console.WriteLine("File not found or contains no sentences.");
                return;
            }

            bool isRunning = true;
            while (isRunning)
            {
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1. Print sentences sorted by word count");
                Console.WriteLine("2. Print sentences sorted by length");
                Console.WriteLine("3. Find unique words of given length in interrogative sentences");
                Console.WriteLine("4. Remove words of given length starting with a consonant");
                Console.WriteLine("5. Replace words of given length in a specific sentence");
                Console.WriteLine("6. Remove stop words (RU and EN)");
                Console.WriteLine("7. Export to XML");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        text.PrintSortedByWordCount();
                        break;
                    case "2":
                        text.PrintSortedByLength();
                        break;
                    case "3":
                        int len3 = GetValidInteger("Enter word length: ");
                        text.PrintUniqueWordsOfLengthInInterrogative(len3);
                        break;
                    case "4":
                        int len4 = GetValidInteger("Enter word length: ");
                        text.RemoveWordsOfLengthStartingWithConsonant(len4);
                        Console.WriteLine("Words removed successfully.");
                        break;
                    case "5":
                        int idx = GetValidInteger("Enter sentence index (0-based): ");
                        int len5 = GetValidInteger("Enter word length: ");
                        Console.Write("Enter replacement string: ");
                        string replacement = Console.ReadLine();
                        text.ReplaceWordsOfLengthInSentence(idx, len5, replacement);
                        Console.WriteLine("Words replaced successfully.");
                        break;
                    case "6":
                        Console.Write("Enter Russian stop words file path (or press Enter to skip): ");
                        string ruPath = Console.ReadLine();
                        Console.Write("Enter English stop words file path (or press Enter to skip): ");
                        string enPath = Console.ReadLine();

                        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        if (!string.IsNullOrWhiteSpace(ruPath))
                        {
                            stopWords.UnionWith(Program.LoadStopWordsFromFile(ruPath));
                        }
                        if (!string.IsNullOrWhiteSpace(enPath))
                        {
                            stopWords.UnionWith(Program.LoadStopWordsFromFile(enPath));
                        }

                        if (stopWords.Count == 0)
                        {
                            Console.WriteLine("No stop words loaded. Operation skipped.");
                        }
                        else
                        {
                            text.RemoveStopWords(stopWords);
                            Console.WriteLine($"Stop words removed successfully. Total loaded: {stopWords.Count}");
                        }
                        break;
                    case "7":
                        Console.Write("Enter output XML file path: ");
                        string xmlPath = Console.ReadLine();
                        text.ExportToXml(xmlPath);
                        Console.WriteLine("Exported to XML successfully.");
                        break;
                    case "0":
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private int GetValidInteger(string prompt)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out result))
                {
                    return result;
                }
                Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
        }
    }

    internal class Program
    {
        static void Main()
        {
            new Menu().Start();
        }

        public static HashSet<string> LoadStopWordsFromFile(string filePath)
        {
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var words = line.Split(new[] { ' ', ',', ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        stopWords.Add(word.Trim().ToLower());
                    }
                }
            }
            else
            {
                Console.WriteLine($"Warning: File not found at: {filePath}");
            }
            return stopWords;
        }
    }
}