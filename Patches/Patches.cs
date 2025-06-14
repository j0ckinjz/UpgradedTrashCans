using HarmonyLib;
using UnityEngine;
using MelonLoader;
using UnityEngine.Rendering.Universal;
using System.Collections;
#if IL2CPP
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Building;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Trash;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.UI.Items;
#elif MONO
using ScheduleOne.Equipping;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Storage;
using ScheduleOne.Building;
using ScheduleOne.ItemFramework;
using ScheduleOne.Trash;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.UI.Items;
#endif

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

    [HarmonyPatch(typeof(Equippable_TrashGrabber), nameof(Equippable_TrashGrabber.Update))]
    public static class Patch_TrashGrabber_Update
    {
        public static bool Prefix(Equippable_TrashGrabber __instance)
        {
            if (!Equippable_TrashGrabber.IsEquipped)
                return true;

            if (!ModManager.Grabber_BulkEject.Value)
                return true;

            if (__instance.itemInstance?.ID == null ||
                !VariantLookup.GrabberByID.TryGetValue(__instance.itemInstance.ID, out var variant) ||
                variant.ID != "trash_grabber_pro")
                return true;

            if (Input.GetMouseButtonDown(0) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                TrashGrabberExtensions.EjectAllTrashNow(__instance);
                return false;
            }

            return true;
        }

        public static void Postfix(Equippable_TrashGrabber __instance)
        {
            TrashGrabberRadiusManager.RegisterActiveGrabber(__instance);
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

    [HarmonyPatch(typeof(TrashGrabberItemUI), nameof(TrashGrabberItemUI.UpdateUI))]
    public static class Patch_TrashGrabberItemUI_UpdateUI
    {
        public static void Postfix(TrashGrabberItemUI __instance)
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
            if (__instance == null)
                return;

            var container = __instance.GetComponent<TrashContainer>();
            if (container == null)
                return;

            MelonCoroutines.Start(WaitForValidName(__instance));
        }

        private static IEnumerator WaitForValidName(TrashContainerItem instance)
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

                float radius = variant.Radius;
                instance.PickupSquareWidth = radius;
                instance.calculatedPickupRadius = radius * Mathf.Sqrt(2f);

                if (instance.PickupAreaProjector != null)
                {
                    float diameter = radius * 2f;
                    instance.PickupAreaProjector.size = new Vector3(diameter, diameter, instance.PickupAreaProjector.size.z);

                    Log.Debug($"[TrashContainer] Set projector size to ({diameter:F2}, {diameter:F2}, {instance.PickupAreaProjector.size.z:F2}) using PickupSquareWidth");
                }

                VisualHelper.TintRenderers(instance.transform, variant.Color, "Body");

                return;
            }
        }
    }

    [HarmonyPatch(typeof(BagTrashCanBehaviour), nameof(BagTrashCanBehaviour.AreActionConditionsMet))]
    public static class Patch_BagTrashCanBehaviour_Conditions
    {
        public static void Postfix(BagTrashCanBehaviour __instance, ref bool __result)
        {
            if (!__result) return;

            var item = __instance?.TargetTrashCan;
            var container = item?.Container;
            if (item == null || container == null) return;

            string name = item.Name;
            if (string.IsNullOrEmpty(name)) return;

            bool isUpgraded = TrashCanVariants.All.Any(v =>
                string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

            if (!isUpgraded) return;

            if (container.TrashLevel < container.TrashCapacity)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(BuildStart_Grid), nameof(BuildStart_Grid.StartBuilding))]
    public static class Patch_BuildStartGrid_StartBuilding
    {
        public static void Postfix(ItemInstance itemInstance)
        {
            PreviewHelper.TrackPreviewVariant(itemInstance);
        }
    }

    [HarmonyPatch(typeof(DecalProjector), "OnEnable")]
    public static class Patch_DecalProjector_OnEnable
    {
        public static void Postfix(DecalProjector __instance)
        {
            MelonCoroutines.Start(DelayedRadiusCheck(__instance));
        }

        private static IEnumerator DelayedRadiusCheck(DecalProjector projector)
        {
            yield return null;

            PreviewHelper.ApplyRadiusIfValid(projector);
        }
    }

    [HarmonyPatch(typeof(DecalProjector), "OnDisable")]
    public static class Patch_DecalProjector_OnDisable
    {
        public static void Postfix(DecalProjector __instance)
        {
            var root = __instance.transform?.root;
            var container = root?.GetComponentInChildren<TrashContainer>();
            if (container == null)
                return;

            PreviewHelper.State.Current = null;
            Log.Debug("[Preview] Cleared active preview variant after ghost disable.");
        }
    }
}