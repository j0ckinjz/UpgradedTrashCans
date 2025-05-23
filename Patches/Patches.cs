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
using Il2CppScheduleOne.Property;
#elif MONO
using ScheduleOne.Equipping;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Storage;
using ScheduleOne.Building;
using ScheduleOne.ItemFramework;
using ScheduleOne.Trash;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.UI.Items;
using ScheduleOne.Property;
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
                    Log.Debug($"Exception accessing Name on attempt {attempts + 1}: {ex.Message}");
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    ApplyVariantSettings(instance, name);
                    yield break;
                }

                attempts++;
                Log.Debug($"Name was null or errored on attempt {attempts}/{maxRetries}. Retrying after {retryDelayFrames} frames...");

                for (int i = 0; i < retryDelayFrames; i++)
                    yield return null;
            }

            Log.Debug("Name remained null after all retry attempts.");
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
                var type = instance.GetType();

                if (VersionHelper.IsBetaOrNewer)
                {
                    var squareProp = type.GetProperty("PickupSquareWidth");
                    var calcProp = type.GetProperty("calculatedPickupRadius");

                    if (squareProp != null && squareProp.CanWrite)
                        squareProp.SetValue(instance, variant.Radius);

                    if (calcProp != null && calcProp.CanWrite)
                        calcProp.SetValue(instance, variant.Radius * Mathf.Sqrt(2f));

                    Log.Debug($"Set PickupSquareWidth + calculatedPickupRadius via reflection");
                }
                else
                {
                    var radiusProp = type.GetProperty("PickupRadius");
                    if (radiusProp != null && radiusProp.CanWrite)
                        radiusProp.SetValue(instance, variant.Radius);

                    Log.Debug($"Set PickupRadius via reflection");
                }


                if (instance.PickupAreaProjector != null)
                {
                    float diameter = radius * 2f;
                    instance.PickupAreaProjector.size = new Vector3(diameter, diameter, instance.PickupAreaProjector.size.z);

                    Log.Debug($"Set projector size to ({diameter:F2}, {diameter:F2}, {instance.PickupAreaProjector.size.z:F2}) using PickupSquareWidth");
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
        public static bool Prepare() => VersionHelper.IsBetaOrNewer;
        public static void Postfix(ItemInstance itemInstance)
        {
            PreviewHelper.TrackPreviewVariant(itemInstance);
        }
    }

    [HarmonyPatch(typeof(DecalProjector), "OnEnable")]
    public static class Patch_DecalProjector_OnEnable
    {
        public static bool Prepare() => VersionHelper.IsBetaOrNewer;
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
        public static bool Prepare() => VersionHelper.IsBetaOrNewer;
        public static void Postfix(DecalProjector __instance)
        {
            var root = __instance.transform?.root;
            var container = root?.GetComponentInChildren<TrashContainer>();
            if (container == null)
                return;

            PreviewHelper.State.Current = null;
            Log.Debug("Cleared active preview variant after ghost disable.");
        }
    }
    [HarmonyPatch(typeof(Property), nameof(Property.DoBoundsContainPoint))]
    public static class Patch_Manor_DoBoundsContainPoint
    {
        public static bool Prepare() => !VersionHelper.IsBetaOrNewer;
        private static readonly Vector2[] skipBinPolygon = new Vector2[]
        {
        new Vector2(174f, -63.5f),
        new Vector2(176f, -63.5f),
        new Vector2(176f, -60f),
        new Vector2(174f, -60f)
        };

        public static void Postfix(Property __instance, Vector3 point, ref bool __result)
        {
            if (__instance.PropertyName == "Manor" && __result)
            {
                Vector2 testPoint = new Vector2(point.x, point.z);
                if (PointInPolygon(testPoint, skipBinPolygon))
                {
                    __result = false;
                }
            }
        }

        // Ray casting algorithm for point-in-polygon
        private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            int j = polygon.Length - 1;
            bool inside = false;

            for (int i = 0; i < polygon.Length; j = i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                               (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
