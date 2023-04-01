using BepInEx;

namespace TaskSortFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            new PatchTasksScreenShow().Enable(); // Receive Session from Show() metod, has to be first.
            new PatchShowQuests().Enable();
            new PatchQuestStringFieldComparer().Enable();
        }
    }
}
