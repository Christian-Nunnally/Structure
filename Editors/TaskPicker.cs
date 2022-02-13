using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;
using System;

namespace Structure.Editors
{
    public class TaskPicker : TreeEditor<TaskItem>
    {
        private Action<TaskItem> _pickedAction;
        private readonly bool _exitAfterPick;

        public TaskPicker(
            StructureIO io,
            string prompt,
            bool allowLeafs,
            bool allowParents,
            bool exitAfterPick,
            NodeTreeCollection<TaskItem> tree,
            Action<TaskItem> pickedAction = null)
            : base(io, prompt, tree)
        {
            if (allowLeafs) EnterPressedOnLeafAction = Pick;
            if (allowParents) EnterPressedOnParentAction = Pick;

            _exitAfterPick = exitAfterPick;
            _pickedAction = pickedAction;
        }

        protected void SetPickAction(Action<TaskItem> action)
        {
            _pickedAction = action;
        }

        private void Pick(TaskItem task)
        {
            _pickedAction(task);
            if (_exitAfterPick) ShouldExit = true;
        }
    }
}