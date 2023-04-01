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
            new PatchShowQuests().Enable(); // Reflection of original method with 1 line commented out.
            new PatchQuestStringFieldComparer().Enable(); // Reflecion of original, but properly using locale for comparison
            new PatchQuestLocationComparer().Enable(); // Reflection of original, but also using locale
        }
    }
}
