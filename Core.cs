using MelonLoader;

[assembly: MelonInfo(typeof(UpgradedTrashCans.Core), "UpgradedTrashCans", "1.5.2", "j0ckinjz")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace UpgradedTrashCans
{
    public class Core : MelonMod
    {
        public override void OnLateInitializeMelon()
        {            
            ModManager.InitializeSettings();
            Log.Msg($"Mod Initialized. Version {Info.Version}");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main")
            {
                VariantSyncManager.SyncSettingsToVariants();
                DefinitionTracker.ClearAll();
                MelonCoroutines.Start(TrashInjector.InitializeShopListings());
            }
        }
    }
    internal static class Log
    {
        public static void Msg(string msg) => Melon<Core>.Logger.Msg(msg);
        public static void Warn(string msg) => Melon<Core>.Logger.Warning(msg);
        public static void Error(string msg) => Melon<Core>.Logger.Error(msg);
        public static void Debug(string message)
        {
            if (ModManager.DebugLogs?.Value == true)
                Msg(message);
        }
    }

}

