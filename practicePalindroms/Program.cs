using System;
using System.Text;

namespace practicePalindroms
{
    internal class Program
    {
        static bool isPalindrom(string? s)
        {
            if (s == null) return true;

            StringBuilder sb = new StringBuilder();

            foreach (char c in s)
            {
                if(Char.IsLetterOrDigit(c)) sb.Append(c);
            }

            for (int i = 0, j = sb.Length - 1; i < j; ++i, --j)
            {
                if (sb[i] != sb[j]) return false;
            }

            return true;
        }
        static void Main()
        {
            string? s = Console.ReadLine();
            Console.WriteLine(isPalindrom(s));
        }
    }
}
