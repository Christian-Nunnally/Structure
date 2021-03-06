using Structure.Editors.Obsolete;
using Structure.IO;
using Structure.Modules.Interface;
using System;
using System.Collections.Generic;

namespace Structure.Modules.Obsolete
{
    public class TreeTaskObsolete : StructureModule, IObsoleteModule
    {
        private UserAction _doTasks;
        public List<TaskEditorObsolete> OpenEditors { get; } = new List<TaskEditorObsolete>();

        public IModule UpgradeModule()
        {
            return new TreeTask();
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
            editor.CustomActions.Add(new UserAction("n", () => GoToNextActiveTask(editor), ConsoleKey.N));
            OpenEditors.Add(editor);
            IO.Run(editor.Edit);
            if (OpenEditors.Count > 0) OpenEditors.RemoveAt(OpenEditors.Count - 1);
        }

        private void GoToNextActiveTask(TaskEditorObsolete editor)
        {
            if (OpenEditors.Count > 1)
            {
                var thisEditorsIndex = OpenEditors.IndexOf(editor);
                if (thisEditorsIndex >= 0)
                {
                    var nextEditorIndex = (thisEditorsIndex + 1) % OpenEditors.Count;
                    var nextEditor = OpenEditors[nextEditorIndex];
                    nextEditor.ShouldExit = false;
                    IO.Run(nextEditor.Edit);
                }
            }
        }
    }
}