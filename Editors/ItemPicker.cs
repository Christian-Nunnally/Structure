using Structure.IO;
using Structure.IO.Persistence;
using Structure.TaskItems;
using System;

namespace Structure.Editors
{
    public class ItemPicker<T> : TreeEditor<T> where T : Node
    {
        private Action<T> _pickedAction;
        private readonly bool _exitAfterPick;

        public ItemPicker(
            StructureIO io,
            string prompt,
            bool allowLeafs,
            bool allowParents,
            bool exitAfterPick,
            NodeTreeCollection<T> tree,
            Action<T> pickedAction = null)
            : base(io, prompt, tree)
        {
            if (allowLeafs) EnterPressedOnLeafAction = Pick;
            if (allowParents) EnterPressedOnParentAction = Pick;

            _exitAfterPick = exitAfterPick;
            _pickedAction = pickedAction;
        }

        protected void SetPickAction(Action<T> action)
        {
            _pickedAction = action;
        }

        private void Pick(T item)
        {
            _pickedAction(item);
            if (_exitAfterPick) ShouldExit = true;
        }
    }
}