using Structure.IO;
using Structure.IO.Persistence;
using System;

namespace Structure.Editors
{
    public class ItemPicker<T> where T : Node
    {
        private Action<T> _pickedAction;
        private readonly bool _exitAfterPick;

        public TreeEditor<T> TreeEditor { get; }

        public ItemPicker(
            StructureIO io,
            string prompt,
            bool allowParents,
            bool exitAfterPick,
            NodeTree<T> tree,
            bool allowInserting,
            Action<T> pickedAction = null)
        {
            TreeEditor = new TreeEditor<T>(io, prompt, tree, allowInserting);
            TreeEditor.EnterPressedOnLeafAction = Pick;
            if (allowParents) TreeEditor.EnterPressedOnParentAction = Pick;

            _exitAfterPick = exitAfterPick;
            _pickedAction = pickedAction;
        }

        public void SetPickAction(Action<T> action)
        {
            _pickedAction = action;
        }

        private void Pick(T item)
        {
            _pickedAction(item);
            if (_exitAfterPick) TreeEditor.ShouldExit = true;
        }

        public void Edit() => TreeEditor.Edit();
    }
}