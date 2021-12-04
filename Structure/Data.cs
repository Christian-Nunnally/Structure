﻿using System.Collections.Generic;

namespace Structure
{
    public static class Data
    {
        public static PersistedTree<TaskItem> ActiveTaskTree = new PersistedTree<TaskItem>("ActiveTaskTree");
        public static List<IModule> EnabledModules = new List<IModule>();
        public static PersistedTree<TaskItem> Routines = new PersistedTree<TaskItem>("Routines");
        public static PersistedTree<TaskItem> Metrics = new PersistedTree<TaskItem>("Metrics");
        private static readonly PersistedInt _xp = new PersistedInt("XP");
        private static readonly PersistedInt _points = new PersistedInt("Points");
        private static readonly PersistedInt _level = new PersistedInt("Level");

        public static int XP { get => _xp.Get(); set => _xp.Set(value); }
        public static int Points { get => _points.Get(); set => _points.Set(value); }
        public static int Level { get => _level.Get(); set => _level.Set(value); }
    }
}