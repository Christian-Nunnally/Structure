using System;

namespace Structure
{
    public class TaskPicker : TreeEditor<TaskItem>
    {
        protected Action<TaskItem> PickedAction;
        private readonly string _pickPrompt;
        private readonly bool _exitAfterPick;

        public TaskPicker(
            string prompt,
            string pickPrompt,
            bool allowLeafs,
            bool allowParents,
            bool exitAfterPick,
            PersistedTree<TaskItem> tree,
            Action<TaskItem> pickedAction = null)
            : base(prompt, tree)
        {
            if (allowLeafs) EnterPressedOnLeafAction = ConfirmPick;
            if (allowParents) EnterPressedOnParentAction = ConfirmPick;
            _pickPrompt = pickPrompt;
            _exitAfterPick = exitAfterPick;
            PickedAction = pickedAction;
        }

        private void ConfirmPick(TaskItem task) => IO.PromptOptions($"{_pickPrompt} {task}?", true,
            new UserAction("no", () => { }),
            new UserAction("yes (Enter)", () =>
            {
                PickedAction(task);
                if (_exitAfterPick) ShouldExit = true;
            }));
    }
}