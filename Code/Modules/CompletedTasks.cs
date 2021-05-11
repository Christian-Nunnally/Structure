using System;

namespace Structure
{
    public class CompletedTasks : Module
    {
        private UserAction _doTasks;

        public CompletedTasks()
        {
        }

        public override void Disable()
        {
            Hotkey.Remove(ConsoleKey.S, _doTasks);
        }

        public override void Enable()
        {
            _doTasks = Hotkey.Add(ConsoleKey.S, new UserAction("Search tasks", Search));
        }

        private void Search()
        {
            IO.WriteNoLine("Search completed tasks: ");
            IO.Read(Search);
            IO.ReadAny();
        }

        private void Search(string searchTerm)
        {
            var count = 0;
            foreach (var task in Data.ActiveTaskTree)
            {
                if (task.Value.Task.ToLower().Contains(searchTerm.ToLower()))
                {
                    IO.Write($"{task.Value.CompletedDate} {task.Value.Task}");
                    count++;
                }
            }
            IO.Write("Count: " + count.ToString());
        }
    }
}