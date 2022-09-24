using Microsoft.VisualStudio.TestTools.UnitTesting;
using Structur.Editors;
using Structur.Modules;
using Structur.TaskItems;
using StructureTests.Utilities;

namespace StructureTests.Modules
{
    [TestClass]
    public class TaskTreeTests
    {
        public StructureTester Tester { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Tester = new StructureTester();
        }
        
        [TestMethod]
        public void TaskTreeDisabled_TaskTreeHotkeyPressed_TaskTreeDoesNotOpen()
        {
            Tester.Run(HotkeyConstants.StartTaskTreeModuleHotkey);

            Tester.NotContains(TaskTree.Title);
        }

        [TestMethod]
        public void TaskTreeDisabled_EnableTaskTreeAndRefreshScreen_OpenTaskTreeHotkeyPromptVisible()
        {
            Tester.Run(
                ModuleManager.OpenModuleManagerHotkey,
                ModuleManager.EnableModuleHotkey,
                ModuleManager.EnableTaskTreeHotkey,
                TreeEditor<TaskItem>.SubmitHotkey
                );

            Tester.Contains(TaskTree.RunTaskTreePrompt);
        }

        [TestMethod]
        public void TaskTreeEmpty_TaskTreeOpened_InsertTaskPromptVisible()
        {
            EnableTaskTreeModule();
            OpenTaskTreeModule();

            Tester.Run();

            Tester.Contains(TreeEditor<TaskItem>.InsertItemPrompt);
        }

        [TestMethod]
        public void TaskTreePromptOpened_TaskNameTyped_TaskNameVisible()
        {
            EnableTaskTreeModule();
            OpenTaskTreeModule();

            Tester.Run(
                HotkeyConstants.AHotkey,
                HotkeyConstants.AHotkey,
                HotkeyConstants.AHotkey,
                HotkeyConstants.SubmitHotkey
                );

            var character = HotkeyConstants.AHotkey.KeyChar;
            Tester.Contains($"{character}{character}{character}");
        }

        private void EnableTaskTreeModule()
        {
            Tester.Queue(
                ModuleManager.OpenModuleManagerHotkey,
                ModuleManager.EnableModuleHotkey,
                ModuleManager.EnableTaskTreeHotkey
                );
        }

        private void OpenTaskTreeModule() => Tester.Queue(TaskTree.OpenTaskTreeHotkey);
    }
}
