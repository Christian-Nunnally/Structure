namespace Structure
{
    public class RoutineExecutor : TaskExecutor
    {
        public RoutineExecutor(TaskItem routineItem)
            : base($"Doing {routineItem?.Task ?? "routines"}", Routiner.ActiveRoutines, Routiner.FinishedRoutines)
        {
            TaskCompletedAction = TaskCompleted;
            SetParent(routineItem);
        }

        private void TaskCompleted(TaskItem routineItem)
        {
            if (routineItem.ParentID == null)
            {
                FinishRoutine(routineItem);
            }
        }

        private void FinishRoutine(TaskItem routineItem)
        {
            IO.News($"Finished {routineItem}.");
            Data.XP += 5;
            Data.CharacterBonus += 5;
            Routiner.RoutinePoints++;
        }
    }
}