﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSortFix
{
    internal static class TaskSortPatchHelper
    {
        // Type defs
        public static Type QuestStringFieldComparer = AccessTools.TypeByName("EFT.UI.TasksScreen+QuestStringFieldComparer");
        public static Type QuestLocationComparer = AccessTools.TypeByName("EFT.UI.TasksScreen+QuestLocationComparer");
        public static Type QuestStatusComparer = AccessTools.TypeByName("EFT.UI.TasksScreen+QuestStatusComparer");
        public static Type QuestProgressComparer = AccessTools.TypeByName("EFT.UI.TasksScreen+QuestProgressComparer");
    }
}
