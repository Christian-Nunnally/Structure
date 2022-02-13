using Structure.Editors.Obsolete;
using Structure.IO;
using Structure.Modules.Interface;
using System;
using System.Collections.Generic;

namespace Structure.Modules.Obsolete
{
    internal class TreeTask : StructureModule, IObsoleteModule
    {
        public static List<TaskEditorObsolete> OpenEditors = new List<TaskEditorObsolete>();
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
            OpenEditors.Add(editor);
            IO.Run(() => editor.Edit());
            if (OpenEditors.Count > 0) OpenEditors.RemoveAt(OpenEditors.Count - 1);
        }
    }
}