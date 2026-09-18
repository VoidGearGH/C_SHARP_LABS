using System;
using System.IO;
using System.Collections.Generic;

namespace Lab2
{
    enum PlayerState
    {
        Winner,
        Loser,
        Playing,
        NotInGame
    }

    class IntValidator
    {
        private static bool IsDigitsOnly(string str)
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

        public static bool Validate(string input)
        {
            return IsDigitsOnly(input);
        }
    }

    class Player
    {
        public string Name { get; private set; }
        public int Location { get; private set; } = -1;
        public PlayerState State { get; private set; } = PlayerState.NotInGame;
        public int DistanceTravelled { get; private set; } = 0;

        public Player(string name)
        {
            Name = name;
        }

        public void Move(int steps, int boardSize)
        {
            if (State == PlayerState.NotInGame)
            {
                Location = steps;
                State = PlayerState.Playing;
            }
            else if (State == PlayerState.Playing)
            {
                DistanceTravelled += Math.Abs(steps);
                Location = ((Location - 1 + steps) % boardSize + boardSize) % boardSize + 1;
            }
        }

        public void SetState(PlayerState state)
        {
            State = state;
        }
    }

    enum GameState
    {
        Start,
        End
    }

    class Game
    {
        public int Size { get; set; }
        public GameState State { get; private set; }

        private Player _cat;
        private Player _mouse;
        public static string InputPath { get; set; } = string.Empty;
        public static string OutputPath { get; set; } = string.Empty;

        public Game(int size)
        {
            Size = size;
            _cat = new Player("Cat");
            _mouse = new Player("Mouse");
            State = GameState.Start;
        }

        public void Run()
        {
            string[] lines = File.ReadAllLines(InputPath);
            Size = int.Parse(lines[0].Trim());

            List<string> output = new List<string>();
            output.Add("Cat and Mouse");
            output.Add("");
            output.Add("Cat Mouse Distance");
            output.Add("-----------------");

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                char command = parts[0][0];

                if (command == 'M' || command == 'C')
                {
                    int steps = int.Parse(parts[1]);
                    DoMoveCommand(command, steps);

                    if (_cat.State == PlayerState.Playing && _mouse.State == PlayerState.Playing)
                    {
                        if (_cat.Location == _mouse.Location)
                        {
                            _cat.SetState(PlayerState.Winner);
                            _mouse.SetState(PlayerState.Loser);
                            State = GameState.End;
                            break;
                        }
                    }
                }
                else if (command == 'P')
                {
                    DoPrintCommand(output);
                }
            }

            if (State != GameState.End)
            {
                _cat.SetState(PlayerState.Loser);
                _mouse.SetState(PlayerState.Winner);
            }

            output.Add("-----------------");
            output.Add("");
            output.Add("");
            output.Add($"Cat distance traveled: {_cat.DistanceTravelled}");
            output.Add($"Mouse distance traveled: {_mouse.DistanceTravelled}");
            output.Add("");

            if (_mouse.State == PlayerState.Loser)
            {
                output.Add($"Mouse caught at: {_mouse.Location}");
            }
            else
            {
                output.Add("Mouse evaded Cat");
            }

            File.WriteAllLines(OutputPath, output);
        }

        private void DoMoveCommand(char command, int steps)
        {
            switch (command)
            {
                case 'M': _mouse.Move(steps, Size); break;
                case 'C': _cat.Move(steps, Size); break;
            }
        }

        private void DoPrintCommand(List<string> output)
        {
            string catStr = _cat.State == PlayerState.NotInGame ? "??" : _cat.Location.ToString();
            string mouseStr = _mouse.State == PlayerState.NotInGame ? "??" : _mouse.Location.ToString();

            if (_cat.State == PlayerState.NotInGame || _mouse.State == PlayerState.NotInGame)
            {
                output.Add($"{catStr} {mouseStr}");
            }
            else
            {
                string distStr = GetDistance().ToString();
                output.Add($"{catStr} {mouseStr} {distStr}");
            }
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
            Game.InputPath = @"C:\Users\User\Documents\C_SHARP_LABS\Lab2\1.ChaseData.txt";
            Game.OutputPath = @"1.PursuitLog.txt";

            Game game = new Game(16);
            game.Run();
        }
    }
}