namespace Structure
{
    public static class Data
    {
        public static PersistedTree<TaskItem> ActiveTaskTree = new PersistedTree<TaskItem>("ActiveTaskTree");
        public static PersistedList<string> EnabledModules = new PersistedList<string>("EnabledModules");
        public static PersistedList<string> FinishedRoutines = new PersistedList<string>("FinishedRoutines");
        private static readonly PersistedInt _xp = new PersistedInt("XP");
        private static readonly PersistedInt _points = new PersistedInt("Points");
        private static readonly PersistedInt _level = new PersistedInt("Level");

        public static int XP { get => _xp.Get(); set => _xp.Set(value); }
        public static int Points { get => _points.Get(); set => _points.Set(value); }
        public static int Level { get => _level.Get(); set => _level.Set(value); }
    }
}