using System;

namespace Structure
{
    [Serializable]
    public class ActionTaskItem : TaskItem
    {
        private Action<TaskItem> _action;

        public ActionTaskItem() : this(NoOp)
        {
        }

        public ActionTaskItem(Action<TaskItem> action)
        {
            _action = action;
        }

        public virtual void DoAction(PersistedTree<TaskItem> tree)
        {
            _action(this);
        }

        private static void NoOp(TaskItem _)
        {
        }
    }
}