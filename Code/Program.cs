using System;
using System.Collections.Generic;
using System.Threading;
using static Structure.Data;
using static Structure.IO;

namespace Structure
{
    internal class Program
    {
        public static Queue<Action> QueudActions = new Queue<Action>();

        public bool _exiting;
        public int _maxLevel;
        private DateTime _lastCharacterBonusTime = DateTime.Now;

        public Program(int maxLevel)
        {
            _maxLevel = maxLevel;
            InitializeHotkey(ConsoleKey.I, PrintInfo);
            InitializeHotkey(ConsoleKey.T, StartNewTask);
            UpdateCharacterBonus();
            IncrementToxins();

            new List<string>(TaskList).ForEach(x => QueudActions.Enqueue(() => DoTask(x)));

            while (!_exiting)
            {
                if (QueudActions.Count != 0)
                {
                    ExecuteSubroutine(QueudActions.Dequeue());
                }
                else
                {
                    PromptPrestiegeOptions();
                    ReadAny();
                }

                TryToLevelUp();
            }
        }

        private int PointsForNextLevel => Utility.ExperienceForLevel(Level + 1, 10, 75, 25);

        private static void Main(string[] args) => new Program(int.Parse(args[0]));

        private void PrintInfo()
        {
            Write($"Lifetime Prestiege = {LifetimePrestiege}");
            Write($"Prestiege Points = {Prestiege}");
            Write($"Current level: {Level}/{_maxLevel}");
            Write($"Points: {Points}/{PointsForNextLevel}");
            Write($"Character Bonus = {CharacterBonus}");
            Write($"You have {Grass} blades of grass.");
            Write($"Toxins = {Toxins}");
            Write($"Length of the code = {LastCodeLength}");
            ReadAny();
        }

        private void StartNewTask()
        {
            Write("Enter task: ");
            ReadLine(DoTask);
        }

        private void DoTask(string task)
        {
            TaskList.Add(task);
            Clear();
            Write(task);
            PromptOptions("Task complete?",
                ('y', "yes", () => { TaskList.Remove(task); GrantPoints(1); }
            ),
                ('d', "delete", () => TaskList.Remove(task)),
                ('p', "postpone", () => QueudActions.Enqueue(() => DoTask(task))));
        }

        private void UpdateCharacterBonus()
        {
            var lengthChange = Utility.GetCodeLength() - LastCodeLength;
            LastCodeLength = Utility.GetCodeLength();
            CharacterBonus -= lengthChange;
            Write($"Code length increased by {lengthChange} characters.");
            Write($"Character bonus {-lengthChange}");
        }

        private void GrantCharacterBonus()
        {
            if ((DateTime.Now - _lastCharacterBonusTime).TotalMinutes > 5)
            {
                Write("Character bonus +5");
                CharacterBonus += 5;
                _lastCharacterBonusTime = DateTime.Now;
            }
        }

        private void PromptPrestiegeOptions()
        {
            if (Prestiege > 0)
            {
                PromptOptions($"Would you like to spend 1/{Prestiege} on 10 blades of grass?", ('y', "yes", Buy10Grass), ('n', "n", () => { }));
            }
        }

        private void Buy10Grass()
        {
            Grass += 10;
            Prestiege -= 1;
            Write($"You now have {Grass} blades of grass.");
            Write($"Each blade of grass reduces added toxins by 1 per day.");
        }

        private void IncrementPrestiege()
        {
            Level = 0;
            Points = 0;
            Prestiege = 1;
            LifetimePrestiege = 1;
            _exiting = true;
        }

        private void TryToLevelUp()
        {
            if (Points >= PointsForNextLevel)
            {
                PromptOptions("You have enough points to level up, want to?", ('y', "yes", LevelUp), ('n', "n", () => { }));
            }
        }

        private void LevelUp()
        {
            Points -= PointsForNextLevel;
            if (Level >= _maxLevel || CharacterBonus < 0)
            {
                PromptOptions("You have reached your max level. Restart and gain 1 prestiege point?", ('y', "yes", IncrementPrestiege), ('n', "n", () => { }));
            }
            else
            {
                Level++;
                _exiting = true;
            }
        }

        private void IncrementToxins()
        {
            var prestiegeBonus = (LifetimePrestiege / 1000f) + 1;
            var grassAbsorption = (int)(Grass * prestiegeBonus);
            var addedToxins = Math.Max(0, Utility.GetCodeLength() - grassAbsorption);
            Write($"Added toxins per day = {addedToxins} ({grassAbsorption} toxins being absorbed by grass blades)");
            Write($"Toxins = {Toxins}");
            if (GetLastWriteTime("Toxins").Date != DateTime.Today.Date)
            {
                Toxins += addedToxins;
            }
        }

        private void GrantPoints(int points)
        {
            var prestiegeBonus = (LifetimePrestiege / 1000f) + 1;
            var newPoints = points * prestiegeBonus;
            Points += (int)Math.Floor(newPoints);
            Write($"Points +{points} ({prestiegeBonus}% from prestiege)");
            GrantCharacterBonus();
            Thread.Sleep(700);
            Clear();
        }
    }
}