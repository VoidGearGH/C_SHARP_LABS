using System;
using System.Text;

namespace Lab2
{
    enum State
    {
        Winner,
        Looser,
        Playing,
        NotInGame
    }
    class IntValidator
    {
        private static void CheckAsc(string? str, ref bool validateFlag)
        {
            if (str == null)
            {
                Console.WriteLine("String without container!");
                Environment.Exit(0);
            }

            foreach (char c in str)
            {
                int ascNum = (int)c;
                if (!(ascNum >= 49 && ascNum <= 58))
                {
                    validateFlag = true;
                    break;
                }
            }
        }
        public static void Validate(int value, string name)
        {
            string valueString = Convert.ToString(value);
            bool validateFlag = false;

            CheckAsc(valueString, ref validateFlag);

            if(validateFlag)
            {
                validateFlag = false;
                do
                {
                    string? newValueString = Console.ReadLine();
                    CheckAsc(newValueString, ref validateFlag);
                }
                while (validateFlag);
            }
        }
    }
    class Player
    {
        public string Name {  get; private set; }
        public int Location { get; private set; } = -1;
        public State State { get; private set; } = State.NotInGame;

        public int DistanceTravelled { get; private set; } = 0;
        public Player(string name) 
        {
            Name = name;
        }
        public void Move(int steps)
        {

        }
    }
    internal class Program
    {
                
    }
}