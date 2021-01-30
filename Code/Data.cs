namespace Structure
{
    public static class Data
    {
        public static PersistedTree<TaskItem> ActiveTaskTree = new PersistedTree<TaskItem>("ActiveTaskTree");
        public static PersistedTree<TaskItem> CompletedTaskTree = new PersistedTree<TaskItem>("CompletedTaskTree");
        public static PersistedList<string> CompletedTasks = new PersistedList<string>("CompletedTasks");
        public static PersistedList<string> EnabledModules = new PersistedList<string>("EnabledModules");
        public static PersistedList<string> FinishedRoutines = new PersistedList<string>("FinishedRoutines");
        private static PersistedInt _xp = new PersistedInt("XP");
        private static PersistedInt _points = new PersistedInt("Points");
        private static PersistedInt _toxins = new PersistedInt("Toxins");
        private static PersistedInt _level = new PersistedInt("Level");
        private static PersistedInt _characterBonus = new PersistedInt("CharacterBonus");
        private static PersistedInt _lastCodeLength = new PersistedInt("LastCodeLength");
        private static PersistedInt _prestiege = new PersistedInt("Prestiege");
        private static PersistedInt _characterBonusPerFile = new PersistedInt("CharacterBonusPerFile");

        public static int XP { get => _xp.Get(); set => _xp.Set(value); }
        public static int Points { get => _points.Get(); set => _points.Set(value); }
        public static int Toxins { get => _toxins.Get(); set => _toxins.Set(value); }
        public static int Level { get => _level.Get(); set => _level.Set(value); }
        public static int CharacterBonus { get => _characterBonus.Get(); set => _characterBonus.Set(value); }
        public static int LastCodeLength { get => _lastCodeLength.Get(); set => _lastCodeLength.Set(value); }
        public static int Prestiege { get => _prestiege.Get(); set => _prestiege.Set(value); }
        public static int CharacterBonusPerFile { get => _characterBonusPerFile.Get(); set => _characterBonusPerFile.Set(value); }
    }
}