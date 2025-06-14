using MelonLoader;

[assembly: MelonInfo(typeof(UpgradedTrashCans.Core), "UpgradedTrashCans", "1.6.1", "j0ckinjz")]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: System.Reflection.AssemblyMetadata("NexusModID", "928")]

namespace UpgradedTrashCans
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
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
        public override void OnUpdate()
        {
            TrashGrabberRadiusManager.Update();
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