using System;

namespace Lab2
{
    enum State
    {
        Winner,
        Loser,
        Playing,
        NotInGame
    }

    class IntValidator
    {
        private static bool IsDigitsOnly(string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }
            return true;
        }
        public static bool Validate(string? input)
        {
            return IsDigitsOnly(input);
        }
    }

    class Player
    {
        public string Name { get; private set; }
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
        static void Main(string[] args)
        {

        }
    }
}