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

    enum GameState
    {
        Start,
        End
    }

    class Game
    {
        public int Size { get; init; }
        public GameState State { get; private set; }

        private Player _cat;
        private Player _mouse;
        public static string? InputPath { get; set; }
        public static string? OutputPath { get; set; }
        public Game(int size)
        {
            Size = size;
            _cat = new Player("Cat");
            _mouse = new Player("Mouse");
            State = GameState.Start;
        }
        public void Run()
        {
            while(State != GameState.End)
            {

            }
        }
        private void DoMoveCommand(char command, int steps)
        {
            switch (command)
            {
                case 'M': _mouse.Move(steps); break;
                case 'C': _cat.Move(steps); break;
            }
        }

        private void DoPrintCommand()
        {
        }

        private int GetDistance()
        {
            return Math.Abs(_cat.Location - _mouse.Location);
        }
    }

    internal class Program
    {
        static void Main()
        {
            Game.InputPath = "1.ChaseData.txt";
            Game.OutputPath = "1.PursuitLog.txt";

            Game game = new Game(16);
            game.Run();
        }
    }
}