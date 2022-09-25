using Microsoft.VisualStudio.TestTools.UnitTesting;
using Structur.Editors;
using Structur.Modules;
using Structur.TaskItems;
using StructureTests.Integration;
using StructureTests.Utilities;

namespace StructureTests.Modules
{
    [TestClass]
    public class TaskTreeTests : IntegrationTestBase
    {
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
            TypeTaskName();
            SubmitTask();

            Tester.Run();

            var character = HotkeyConstants.AHotkey.KeyChar;
            Tester.Contains($"{character}{character}{character}");
        }

        [TestMethod]
        public void InsertingATaskNamedAAA_BackspacePressed_TaskNameIsAA()
        {
            EnableTaskTreeModule();
            OpenTaskTreeModule();
            TypeTaskName();

            Tester.Run(HotkeyConstants.BackspaceHotkey);

            var a = HotkeyConstants.AHotkey.KeyChar;
            Tester.Contains($"{a}{a}");
            Tester.NotContains($"{a}{a}{a}");
        }

        [TestMethod]
        public void InsertingATaskNamedAAA_LeftArrowPressedAndBTyped_TaskNameIsAABA()
        {
            EnableTaskTreeModule();
            OpenTaskTreeModule();
            TypeTaskName();
            MoveSelectionLeft();
            TypeB();

            Tester.Run();

            var a = HotkeyConstants.AHotkey.KeyChar;
            var b = HotkeyConstants.BHotkey.KeyChar;
            Tester.Contains($"{a}{a}{b}{a}");
        }

        [TestMethod]
        public void InsertingATaskNamedAAA_LeftThenRightArrowPressedAndBTyped_TaskNameIsAAAB()
        {
            EnableTaskTreeModule();
            OpenTaskTreeModule();
            TypeTaskName();
            MoveSelectionLeft();
            MoveSelectionRight();
            TypeB();

            Tester.Run();

            var a = HotkeyConstants.AHotkey.KeyChar;
            var b = HotkeyConstants.BHotkey.KeyChar;
            Tester.Contains($"{a}{a}{b}{a}");
        }

        [TestMethod]
        public void TaskAdded_AddedTaskNotSelected()
        {
            EnableTaskTreeModule();
            OpenTaskTreeModule();
            TypeTaskName();
            SubmitTask();

            Tester.Run();

            var a = HotkeyConstants.AHotkey.KeyChar;
            Tester.NotContains($"> {a}{a}{a}");
        }

        [TestMethod]
        public void TaskAdded_UpPressed_AddedTaskSelected()
        {
            EnableTaskTreeModule();
            OpenTaskTreeModule();
            TypeTaskName();
            SubmitTask();
            MoveSelectionUp();

            Tester.Run(TreeEditor<TaskItem>.MoveSelectionUpHotkey);

            var character = HotkeyConstants.AHotkey.KeyChar;
            Tester.Contains($"{TreeEditor<TaskItem>.SelectorString} {character}{character}{character}");
        }

        //[TestMethod]
        //public void TaskAddedAndSelected_RPressed_TaskRenamePromptOpened()
        //{
        //    EnableTaskTreeModule();
        //    OpenTaskTreeModule();
        //    TypeTaskNameAndInsert();
        //    MoveSelectionUp();

        //    Tester.Run(TreeEditor<TaskItem>.RenameHotkey);

        //    var character = HotkeyConstants.AHotkey.KeyChar;
        //    Tester.Contains($"{TreeEditor<TaskItem>.SelectorString} {character}{character}{character}");
        //}

        private void EnableTaskTreeModule()
        {
            Tester.Queue(
                ModuleManager.OpenModuleManagerHotkey,
                ModuleManager.EnableModuleHotkey,
                ModuleManager.EnableTaskTreeHotkey);
        }

        private void OpenTaskTreeModule() => Tester.Queue(TaskTree.OpenTaskTreeHotkey);

        private void TypeTaskName()
        {
            TypeA();
            TypeA();
            TypeA();
        }

        private void TypeA() => Tester.Queue(HotkeyConstants.AHotkey);

        private void TypeB() => Tester.Queue(HotkeyConstants.BHotkey);

        private void SubmitTask() => Tester.Queue(HotkeyConstants.SubmitHotkey);
        
        private void MoveSelectionUp() => Tester.Queue(TreeEditor<TaskItem>.MoveSelectionUpHotkey);
        
        private void MoveSelectionLeft() => Tester.Queue(HotkeyConstants.LeftArrowHotkey);

        private void MoveSelectionRight() => Tester.Queue(HotkeyConstants.RightArrowHotkey);
    }
}
