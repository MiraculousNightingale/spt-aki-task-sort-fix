using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskSortFix
{
    internal static class TaskHelper
    {
        public static ISession Session { get; set; }
        // Type defs
        public static Type QuestStringFieldComparer = AccessTools.TypeByName("EFT.UI.TasksScreen+QuestStringFieldComparer");
    }
}
