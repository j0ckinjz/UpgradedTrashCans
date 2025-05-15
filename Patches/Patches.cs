using HarmonyLib;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ObjectScripts;
using UnityEngine;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Trash;
using MelonLoader;

namespace UpgradedTrashCans
{
    public static class VariantLookup
    {
        public static HashSet<string> KnownGrabberIDs { get; private set; } = new();
        public static Dictionary<string, TrashGrabberVariant> GrabberByID { get; private set; } = new();

        public static void Refresh()
        {
            KnownGrabberIDs = TrashGrabberVariants.All.Select(v => v.ID).ToHashSet();
            GrabberByID = TrashGrabberVariants.All.ToDictionary(v => v.ID);
        }
    }

    [HarmonyPatch(typeof(Equippable_TrashGrabber), nameof(Equippable_TrashGrabber.GetCapacity))]
    public static class Patch_TrashGrabber_GetCapacity
    {
        public static void Postfix(Equippable_TrashGrabber __instance, ref int __result)
        {
            var def = __instance?.itemInstance?.Definition;
            if (def?.ID == null || !VariantLookup.KnownGrabberIDs.Contains(def.ID))
                return;

            if (!VariantLookup.GrabberByID.TryGetValue(def.ID, out var variant))
                return;

            int used = __instance.trashGrabberInstance?.GetTotalSize() ?? 0;
            __result = Mathf.Max(0, variant.Capacity - used);
        }
    }

    [HarmonyPatch(typeof(Equippable_TrashGrabber), nameof(Equippable_TrashGrabber.RefreshVisuals))]
    public static class Patch_TrashGrabber_RefreshVisuals
    {
        public static void Postfix(Equippable_TrashGrabber __instance)
        {
            if (__instance?.TrashContent == null ||
                __instance.TrashContent_Min == null ||
                __instance.TrashContent_Max == null)
                return;

            var def = __instance.itemInstance?.Definition;
            if (def?.ID == null || !VariantLookup.KnownGrabberIDs.Contains(def.ID))
                return;

            if (!VariantLookup.GrabberByID.TryGetValue(def.ID, out var variant))
                return;

            int used = __instance.trashGrabberInstance?.GetTotalSize() ?? 0;
            float percent = Mathf.Clamp01((float)used / variant.Capacity);

            Vector3 range = __instance.TrashContent_Max.localPosition - __instance.TrashContent_Min.localPosition;
            Vector3 basePosition = __instance.TrashContent_Min.localPosition;
            __instance.TrashContent.localPosition = basePosition + (range * percent);

            VisualHelper.TintRenderers(__instance.transform, variant.Color, "Body", "Trigger");
        }
    }

    [HarmonyPatch(typeof(Il2CppScheduleOne.UI.Items.TrashGrabberItemUI), nameof(Il2CppScheduleOne.UI.Items.TrashGrabberItemUI.UpdateUI))]
    public static class Patch_TrashGrabberItemUI_UpdateUI
    {
        public static void Postfix(Il2CppScheduleOne.UI.Items.TrashGrabberItemUI __instance)
        {
            if (__instance == null) return;

            var label = __instance.ValueLabel;
            var item = __instance.itemInstance;
            var def = item?.Definition;

            if (label == null || def?.ID == null || !VariantLookup.KnownGrabberIDs.Contains(def.ID))
                return;

            if (!VariantLookup.GrabberByID.TryGetValue(def.ID, out var variant))
                return;

            var grabberInst = __instance.trashGrabberInstance;
            if (grabberInst == null) return;

            float percent = Mathf.Clamp01((float)grabberInst.GetTotalSize() / variant.Capacity);
            label.text = $"{Mathf.RoundToInt(percent * 100)}%";
        }
    }

    [HarmonyPatch(typeof(StorageVisualizer), nameof(StorageVisualizer.RefreshVisuals))]
    public static class Patch_StorageVisualizer_RefreshVisuals
    {
        public static void Postfix(StorageVisualizer __instance)
        {
            var storedItems = __instance.GetComponentsInChildren<StoredItem>(true);
            foreach (var stored in storedItems)
            {
                var item = stored.item;
                if (item?.Definition?.ID == null) continue;
                if (!VariantLookup.GrabberByID.TryGetValue(item.Definition.ID, out var variant))
                    continue;
                VisualHelper.TintRenderers(stored.transform, variant.Color, "Body", "Trigger");
            }
        }
    }

    [HarmonyPatch(typeof(TrashContainerItem), nameof(TrashContainerItem.Start))]
    public static class Patch_TrashContainerItem_Start
    {
        public static void Postfix(TrashContainerItem __instance)
        {
            // Start the coroutine from a MonoBehaviour context
            if (__instance != null)
                MelonCoroutines.Start(WaitForValidName(__instance));
        }

        private static System.Collections.IEnumerator WaitForValidName(TrashContainerItem instance)
        {
            const int retryDelayFrames = 2;
            const int maxRetries = 10;

            int attempts = 0;

            while (attempts < maxRetries)
            {
                string name = null;

                try
                {
                    name = instance?.Name;
                }
                catch (Exception ex)
                {
                    Log.Debug($"[TrashContainerItem Start] Exception accessing Name on attempt {attempts + 1}: {ex.Message}");
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    ApplyVariantSettings(instance, name);
                    yield break;
                }

                attempts++;
                Log.Debug($"[TrashContainerItem Start] Name was null or errored on attempt {attempts}/{maxRetries}. Retrying after {retryDelayFrames} frames...");

                for (int i = 0; i < retryDelayFrames; i++)
                    yield return null;
            }

            Log.Debug("[TrashContainerItem Start] Name remained null after all retry attempts.");
        }

        private static void ApplyVariantSettings(TrashContainerItem instance, string name)
        {
            foreach (var variant in TrashCanVariants.All)
            {
                if (!string.Equals(variant.Name, name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (instance.TryGetComponent(out TrashContainer container))
                {
                    container.TrashCapacity = variant.Capacity;
                }

                instance.PickupRadius = variant.Radius;

                if (instance.PickupAreaProjector != null)
                {
                    float diameter = variant.Radius * 2f;
                    instance.PickupAreaProjector.size = new Vector3(diameter, diameter, instance.PickupAreaProjector.size.z);

                    Log.Debug($"[TrashContainer] Set projector size to ({diameter:F2}, {diameter:F2}, {instance.PickupAreaProjector.size.z:F2}) for radius {variant.Radius:F2}");
                }

                VisualHelper.TintRenderers(instance.transform, variant.Color, "Body");

                return;
            }
        }
    }
}
