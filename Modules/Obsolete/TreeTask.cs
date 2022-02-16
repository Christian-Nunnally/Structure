using Structure.Editors.Obsolete;
using Structure.IO;
using Structure.Modules.Interface;
using System;

namespace Structure.Modules.Obsolete
{
    public class TreeTask : StructureModule, IObsoleteModule
    {
        private UserAction _doTasks;

        public IModule UpgradeModule()
        {
            return new TreeTaskV2();
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.T, _doTasks);
        }

        protected override void OnEnable()
        {
            _doTasks = new UserAction("Do tasks", Start);
            Hotkey.Add(ConsoleKey.T, _doTasks);
        }

        private void Start()
        {
            var editor = new TaskEditorObsolete(IO, Data);
            Data.OpenEditors.Add(editor);
            IO.Run(editor.Edit);
            if (Data.OpenEditors.Count > 0) Data.OpenEditors.RemoveAt(Data.OpenEditors.Count - 1);
        }
    }
}