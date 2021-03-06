using System;
using System.Linq;

namespace Structure
{
    public class RoutineEditor : TreeEditor<TaskItem>
    {
        public RoutineEditor(PersistedTree<TaskItem> routineTree) : base("Edit routines", routineTree)
        {
            CustomActions.Add(("i", (Action)(() => IO.Run(PromptToAddRoutineItem))));
            NoChildrenAction = PromptToAddRoutineItem;
            EnableReparenting = false;
        }

        private void PromptToAddRoutineItem()
        {
            var routineCount = Tree.Count();
            if (CurrentParent == null)
            {
                var allRoutines = Tree.Where(x => x.Value.ParentID == null);
                var neededPoints = Utility.ExperienceForLevel(allRoutines.Count(), 0, 2, 40);
                if (Routiner.RoutinePoints >= neededPoints)
                {
                    IO.PromptYesNo($"Spend {neededPoints} points to create a new routine?", () => AddRoutineItem(neededPoints, null));
                }
                else
                {
                    IO.News($"{Routiner.RoutinePoints}/{neededPoints} routine points needed.");
                    return;
                }
            }
            else
            {
                var routineID = CurrentParent.ID;
                var routineItems = Tree.Where(x => x.Value.ParentID == routineID);
                var neededPoints = Utility.ExperienceForLevel(routineItems.Count(), 0, 2, 40);
                if (Routiner.RoutinePoints >= neededPoints)
                {
                    IO.PromptYesNo($"Spend {neededPoints} points to create a new routine?", () => AddRoutineItem(neededPoints, routineID));
                }
                else
                {
                    IO.News($"{Routiner.RoutinePoints}/{neededPoints} routine points needed.");
                    return;
                }
            }
            if (Tree.Count() == routineCount)
            {
                ViewParent();
            }
        }

        private void AddRoutineItem(int cost, string parentId)
        {
            if (Routiner.RoutinePoints < cost)
            {
                return;
            }
            IO.Write("Enter routine item: ");
            IO.Read(x => AddRoutineItem(parentId, x));
            Routiner.RoutinePoints -= cost;
        }

        private void AddRoutineItem(string routineParent, string routineName)
        {
            if (string.IsNullOrEmpty(routineName)) return;
            var routineItem = new TaskItem { Task = routineName, ParentID = routineParent };
            Tree.Set(routineItem);
        }
    }
}