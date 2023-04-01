using Aki.Common.Utils;
using Aki.Reflection.Patching;
using EFT;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static System.Collections.Specialized.BitVector32;
using static TaskSortFix.TaskHelper;

namespace TaskSortFix
{
    // Get current session on TasksScreen.Show().
    public class PatchTasksScreenShow : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TasksScreen).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        private static void PatchPrefix(ISession session)
        {
            Session = session;
            Logger.LogMessage($"Received session, has {Session.Traders.Count()} traders");
        }
    }
    public class PatchShowQuests : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TasksScreen).GetMethod("ShowQuests", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        private static bool PatchPrefix(TasksScreen __instance, QuestControllerClass questController, ISession session)
        {
            var questsAvailability = typeof(TasksScreen).GetField("_questsAvailability", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as Dictionary<QuestClass, bool>;
            var ui = typeof(TasksScreen).GetField("UI", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as GClass788;
            var questsAdditionalFilter = typeof(TasksScreen).GetField("_questsAdditionalFilter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as Func<QuestClass, bool>;
            var FilterInGame = typeof(TasksScreen).GetMethod("FilterInGame", BindingFlags.Instance | BindingFlags.NonPublic);
            var noActiveTasksObject = typeof(TasksScreen).GetField("_noActiveTasksObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as GameObject;
            var currentLocationId = typeof(TasksScreen).GetField("_currentLocationId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as string;
            var activeProfile = typeof(TasksScreen).GetField("_activeProfile", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as Profile;
            var notesTaskDescription = typeof(TasksScreen).GetField("_notesTaskDescription", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as GameObject;
            var notesTaskDescriptionTemplate = typeof(TasksScreen).GetField("_notesTaskDescriptionTemplate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as GameObject;
            
            // Original Method Implementation
            ui.Dispose();
            List<QuestClass> list = (from quest in (from x in questController.Quests
                                                    where x.Template != null && (x.QuestStatus == EQuestStatus.Started || x.QuestStatus == EQuestStatus.AvailableForFinish || x.QuestStatus == EQuestStatus.MarkedAsFailed)
                                                    select x).Where(questsAdditionalFilter)
                                     where (bool)FilterInGame.Invoke(__instance, new object[] { questController, quest })
                                     select quest).ToList<QuestClass>();

            if (!list.Any<QuestClass>())
            {
                noActiveTasksObject.SetActive(true);
                return false;
            }
            noActiveTasksObject.SetActive(false);

            //Activator.CreateInstance(QuestStringFieldComparer, new object[] { EQuestsSortType.Trader });
            //var questStringFieldComparerContsructor = 


            switch (__instance.SortType)
            {
                case EQuestsSortType.Trader:
                    list.Sort(Activator.CreateInstance(QuestStringFieldComparer, new object[] { EQuestsSortType.Trader }) as IComparer<QuestClass>);
                    break;
                case EQuestsSortType.Type:
                    list.Sort(Activator.CreateInstance(QuestStringFieldComparer, new object[] { EQuestsSortType.Type }) as IComparer<QuestClass>);
                    break;
                case EQuestsSortType.Task:
                    list.Sort(Activator.CreateInstance(QuestStringFieldComparer, new object[] { EQuestsSortType.Task }) as IComparer<QuestClass>);
                    break;
                case EQuestsSortType.Location:
                    //list.Sort(new TasksScreen.QuestLocationComparer(__instance._currentLocationId));
                    break;
                case EQuestsSortType.Status:
                    //list.Sort(new TasksScreen.QuestStatusComparer());
                    break;
                case EQuestsSortType.Progress:
                    //list.Sort(new TasksScreen.QuestProgressComparer());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            string GetTraderLocalizedName(string traderId)
            {
                return session.Traders.First(trader => trader.Id == traderId).LocalizedName;
            }

            //list.Sort((a, b) => GetTraderLocalizedName(a.Template.TraderId).CompareTo(GetTraderLocalizedName(b.Template.TraderId)));

            if (__instance.SortAscend)
            {
                list.Reverse();
            }

            for (var id = 0; id < list.Count; id++)
            {
                Logger.LogMessage($"{id} QUEST NAME: {list[id].Template.Name}");
            }


            foreach (QuestClass questClass in list)
            {
                bool value = string.IsNullOrEmpty(currentLocationId) || ((questClass.Template.LocationId == "any" || questClass.Template.LocationId == currentLocationId) && questClass.Template.PlayerGroup == activeProfile.Side.ToPlayerGroup());
                if (!questsAvailability.ContainsKey(questClass))
                {
                    questsAvailability.Add(questClass, value);
                }
            }

            // - This line fucks up the order even if you make every single quest in the dictionary true or false.
            //list.Sort((QuestClass questX, QuestClass questY) => questsAvailability[questY].CompareTo(questsAvailability[questX]));
            NotesTaskDescriptionShort description = notesTaskDescription.InstantiatePrefab<NotesTaskDescriptionShort>(notesTaskDescriptionTemplate);

            var notesTaskTemplate = typeof(TasksScreen).GetField("_notesTaskTemplate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as NotesTask;
            var notesTaskContent = typeof(TasksScreen).GetField("_notesTaskContent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as RectTransform;
            var inventoryController = typeof(TasksScreen).GetField("_inventoryController", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as InventoryControllerClass;

            var NotesTaskShow = typeof(NotesTask).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic);
            ui.AddViewList<QuestClass, NotesTask>(list, notesTaskTemplate, notesTaskContent, delegate (QuestClass quest, NotesTask view)
            {
                NotesTaskShow.Invoke(view, new object[] { quest, session, inventoryController, questController, description, questsAvailability[quest] });
            });

            return false;
        }
    }
    public class PatchQuestStringFieldComparer : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return QuestStringFieldComparer.GetMethod("Compare", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(object __instance, QuestClass x, QuestClass y, ref int __result)
        {
            //  These literally return the id of a trader, there's no locale
            //Logger.LogMessage($"TraderId Localized: x {x.Template.TraderId.Localized()} y {y.Template.TraderId.Localized()}");
            //Logger.LogMessage($"TraderId LocalizedName: x {x.Template.TraderId.LocalizedName()} y {y.Template.TraderId.LocalizedName()}");

            //  Doesn't work for Quest Type
            //Logger.LogMessage($"QuestType: x {x.Template.} y {y.Template.QuestType}");
            //Logger.LogMessage($"QuestType Localized: x {x.Template.QuestType.Localized()} y {y.Template.QuestType.Localized()}");

            var xField = __instance.GetType().GetField("_xField", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as string;
            var yField = __instance.GetType().GetField("_yField", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as string;
            var sortType = (EQuestsSortType)QuestStringFieldComparer.GetField("_sortType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);


            // Copy from source code, slightly tweaked to actually work.
            if (x == y)
            {
                __result = 0;
                return false;
            }
            if (y == null)
            {
                __result = 1;
                return false;
            }
            if (x == null)
            {
                __result = -1;
                return false;
            }

            switch (sortType)
            {
                case EQuestsSortType.Trader:
                    // Just replace these with localized name instead of ids
                    xField = Session.Traders.First(trader => trader.Id == x.Template.TraderId).LocalizedName;
                    yField = Session.Traders.First(trader => trader.Id == y.Template.TraderId).LocalizedName;
                    break;
                case EQuestsSortType.Type:
                    xField = x.Template.QuestType.ToStringNoBox<RawQuestClass.EQuestType>();
                    yField = y.Template.QuestType.ToStringNoBox<RawQuestClass.EQuestType>();
                    break;
                case EQuestsSortType.Task:
                    xField = x.Template.Name;
                    yField = y.Template.Name;
                    break;
                case EQuestsSortType.Location:
                case EQuestsSortType.Status:
                case EQuestsSortType.Progress:
                    goto IL_DF;
                default:
                    goto IL_DF;
            }
            if (!string.Equals(xField, yField))
            {
                __result = string.Compare(xField, yField, StringComparison.Ordinal);
                return false;
            }
            __result = x.StartTime.CompareTo(y.StartTime);
            return false;
        IL_DF:
            throw new ArgumentOutOfRangeException();
        }
    }

    public class PatchQuestLocationComparer : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return QuestLocationComparer.GetMethod("Compare", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool PatchPrefix(object __instance, QuestClass x, QuestClass y, ref int __result)
        {
            //  These literally return the id of a trader, there's no locale
            //Logger.LogMessage($"TraderId Localized: x {x.Template.TraderId.Localized()} y {y.Template.TraderId.Localized()}");
            //Logger.LogMessage($"TraderId LocalizedName: x {x.Template.TraderId.LocalizedName()} y {y.Template.TraderId.LocalizedName()}");

            //  Doesn't work for Quest Type
            //Logger.LogMessage($"QuestType: x {x.Template.} y {y.Template.QuestType}");
            //Logger.LogMessage($"QuestType Localized: x {x.Template.QuestType.Localized()} y {y.Template.QuestType.Localized()}");

            var xField = __instance.GetType().GetField("_xField", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as string;
            var yField = __instance.GetType().GetField("_yField", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as string;
            var sortType = (EQuestsSortType)QuestStringFieldComparer.GetField("_sortType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);


            // Copy from source code, slightly tweaked to actually work.
            if (x == y)
            {
                __result = 0;
                return false;
            }
            if (y == null)
            {
                __result = 1;
                return false;
            }
            if (x == null)
            {
                __result = -1;
                return false;
            }

            switch (sortType)
            {
                case EQuestsSortType.Trader:
                    // Just replace these with localized name instead of ids
                    xField = Session.Traders.First(trader => trader.Id == x.Template.TraderId).LocalizedName;
                    yField = Session.Traders.First(trader => trader.Id == y.Template.TraderId).LocalizedName;
                    break;
                case EQuestsSortType.Type:
                    xField = x.Template.QuestType.ToStringNoBox<RawQuestClass.EQuestType>();
                    yField = y.Template.QuestType.ToStringNoBox<RawQuestClass.EQuestType>();
                    break;
                case EQuestsSortType.Task:
                    xField = x.Template.Name;
                    yField = y.Template.Name;
                    break;
                case EQuestsSortType.Location:
                case EQuestsSortType.Status:
                case EQuestsSortType.Progress:
                    goto IL_DF;
                default:
                    goto IL_DF;
            }
            if (!string.Equals(xField, yField))
            {
                __result = string.Compare(xField, yField, StringComparison.Ordinal);
                return false;
            }
            __result = x.StartTime.CompareTo(y.StartTime);
            return false;
        IL_DF:
            throw new ArgumentOutOfRangeException();
        }
    }
}
