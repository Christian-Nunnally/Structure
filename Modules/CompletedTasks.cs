using System;

namespace Structure
{
    public class CompletedTasks : Module
    {
        private UserAction _doTasks;

        public CompletedTasks()
        {
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.S, _doTasks);
        }

        protected override void OnEnable()
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
            foreach (var task in CommonData.ActiveTaskTree)
            {
                if (task.Value.Name.ToLower().Contains(searchTerm.ToLower()))
                {
                    IO.Write($"{task.Value.CompletedDate} {task.Value.Name}");
                    count++;
                }
            }
            IO.Write("Count: " + count.ToString());
        }
    }
}