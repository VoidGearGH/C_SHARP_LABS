using System;
using System.Text;

namespace Lab3
{
    internal class Program
    {
        abstract class ExpressionSet<T> where T : ExpressionSet<T>
        {
            protected StringBuilder Sb { get; set; }

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
        }

        class Word : ExpressionSet<Word>
        {
            public Word(string value) : base(new StringBuilder(value)) { }
            public override Word Create(string value) => new Word(value);
        }
        class Punctuation : ExpressionSet<Punctuation>
        {
            public Punctuation(string value) : base (new StringBuilder(value)) { }

            public override Punctuation Create(string value) => new Punctuation(value);
        }
        class Sentence : ExpressionSet<Sentence>
        {
            public Sentence(string value) : base(new StringBuilder(value)) { }

            public override Sentence Create(string value) => new Sentence(value);
        }
        class Text : ExpressionSet<Text>
        {
            public Text(string value) : base(new StringBuilder(value)) { }

            public override Text Create(string value) => new Text(value);
        }
        class Parser
        {

        }
        static void Main()
        {
            
        }
    }
}