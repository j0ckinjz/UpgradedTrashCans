using UnityEngine;
using MelonLoader;
using UnityEngine.Rendering.Universal;
using System.Collections;
#if IL2CPP
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.Trash;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Interaction;
using Il2CppFluffyUnderware.DevTools.Extensions;
using Il2CppScheduleOne.Audio;
#elif MONO
using ScheduleOne.Equipping;
using ScheduleOne.Trash;
using ScheduleOne.UI.Items;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.Audio;
#endif

namespace UpgradedTrashCans
{
    public static class TrashGrabberExtensions
    {
        public static void EjectAllTrashNow(Equippable_TrashGrabber grabber)
        {
            if (grabber?.itemInstance?.ID == null)
                return;

            if (!VariantLookup.GrabberByID.TryGetValue(grabber.itemInstance.ID, out var variant) ||
                variant.ID != "trash_grabber_pro")
                return;

            if (grabber?.trashGrabberInstance?.Content?.Entries == null)
                return;

            var content = grabber.trashGrabberInstance.Content;
            if (content.Entries.Count == 0) return;

            Vector3 basePos =
                grabber.transform.TransformPoint(grabber.TrashDropOffset) +
                grabber.transform.forward * 1f;

            var entries = new List<TrashContent.Entry>();
            foreach (var entry in content.Entries)
                entries.Add(entry);

            content.Clear();
            grabber.RefreshVisuals();
            grabber.TrashDropSound?.Play();

            MelonCoroutines.Start(DelayedUIUpdate(grabber));
            MelonCoroutines.Start(SpawnTrashStack(entries, basePos, grabber));
        }

        private static IEnumerator DelayedUIUpdate(Equippable_TrashGrabber grabber)
        {
            yield return null;

            foreach (var ui in UnityEngine.Object.FindObjectsOfType<TrashGrabberItemUI>())
            {
                if (ui.itemInstance == grabber.itemInstance)
                {
                    ui.UpdateUI();
                    break;
                }
            }
        }

        private static IEnumerator SpawnTrashStack(List<TrashContent.Entry> entries, Vector3 basePos, Equippable_TrashGrabber grabber)
        {
            foreach (var entry in entries)
            {
                string id = entry.TrashID;
                int quantity = entry.Quantity;

                for (int i = 0; i < quantity; i++)
                {
                    TrashManager.Instance.CreateTrashItem(
                        id,
                        basePos,
                        Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f),
                        Vector3.down * 7f,
                        Guid.NewGuid().ToString()
                    );

                    yield return null;
                }
            }
        }
    }

    public static class TrashGrabberRadiusManager
    {
        private static GameObject? cachedProjector;
        private static Equippable_TrashGrabber? activeGrabber;
        private static AudioSource? clickSource;
        private static AudioSource? whooshSource;
        private static bool soundPlayedThisCycle = false;
        private static float nextPickupTime = 0f;
        private const float pickupCooldown = 0.15f;
        public static MelonPreferences_Entry<float> Grabber_Radius;
        private static readonly int GroundMask = LayerMask.GetMask("Default", "Ground", "Terrain", "Environment");
        public static Vector2 XZ(this Vector3 v) => new Vector2(v.x, v.z);
        private static bool radiusModeEnabled = false;
        private static readonly KeyCode toggleKey = KeyCode.R;
        private static readonly KeyCode pickupKey = KeyCode.E;

        public static void Update()
        {
            if (!ModManager.Grabber_RadiusPickup.Value)
                return;

            // Toggle mode
            if (Input.GetKeyDown(toggleKey) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                radiusModeEnabled = !radiusModeEnabled;
                Log.Msg($"[RadiusManager] Radius mode {(radiusModeEnabled ? "enabled" : "disabled")}.");
            }

            if (activeGrabber != null)
            {
                var projector = activeGrabber.transform.Find("GrabberRadiusProjector");
                if (projector != null)
                {
                    projector.gameObject.SetActive(radiusModeEnabled);

                    if (radiusModeEnabled && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit, 10f, GroundMask))
                    {
                        projector.transform.position = new Vector3(hit.point.x, hit.point.y + 0.03f, hit.point.z);
                        projector.transform.rotation = Quaternion.LookRotation(Vector3.down);
                    }
                }
            }

            if (!radiusModeEnabled || activeGrabber == null)
                return;

            if (Input.GetKeyDown(pickupKey) && Time.time >= nextPickupTime)
            {
                nextPickupTime = Time.time + pickupCooldown;
                PerformRadiusPickup(activeGrabber);
            }
        }

        public static void RegisterActiveGrabber(Equippable_TrashGrabber grabber)
        {
            var sources = grabber.GetComponentsInChildren<AudioSource>(true);
            foreach (var source in sources)
            {
                if (source == null) continue;

                var name = source.gameObject.name;

                if (name == "Clicksound" && clickSource == null)
                {
                    clickSource = source;
                    Log.Debug("[RadiusManager] Cached Clicksound AudioSource");
                }
                else if (name == "Whoosh sound" && whooshSource == null)
                {
                    whooshSource = source;
                    Log.Debug("[RadiusManager] Cached Whoosh AudioSource");
                }
            }

            if (grabber.itemInstance?.ID != "trash_grabber_pro")
                return;

            activeGrabber = grabber;
            EnsureProjectorAttached(grabber);
        }

        private static void EnsureProjectorAttached(Equippable_TrashGrabber grabber)
        {
            if (grabber.transform.Find("GrabberRadiusProjector") != null)
                return;

            if (cachedProjector == null)
            {
                var source = FindProjectorFromAnyTrashBag();
                if (source != null)
                {
                    cachedProjector = UnityEngine.Object.Instantiate(source);
                    cachedProjector.SetActive(false);
                    UnityEngine.Object.DontDestroyOnLoad(cachedProjector);
                    Log.Debug("[RadiusManager] Cached projector from memory.");
                }
                else
                {
                    Log.Warn("[RadiusManager] Could not find projector.");
                    return;
                }
            }

            var clone = UnityEngine.Object.Instantiate(cachedProjector, grabber.transform);
            clone.name = "GrabberRadiusProjector";
            clone.SetActive(radiusModeEnabled);

            clone.layer = grabber.gameObject.layer;

            if (clone.TryGetComponent(out DecalProjector projector))
            {
                float radius = Mathf.Clamp(ModManager.Grabber_Radius.Value, 0.5f, 3f);
                float diameter = radius * 2f;
                projector.size = new Vector3(diameter, diameter, 0.5f);
            }

            Log.Debug("[RadiusManager] Projector attached to grabber.");
        }

        private static GameObject? FindProjectorFromAnyTrashBag()
        {
            var allBags = Resources.FindObjectsOfTypeAll<TrashBag_Equippable>();
            foreach (var bag in allBags)
            {
                if (bag == null) continue;

                if (bag.PickupAreaProjector != null)
                {
                    Log.Debug("[RadiusManager] Found PickupAreaProjector on bag.");
                    return bag.PickupAreaProjector.gameObject;
                }

                var fallback = FindChildRecursive(bag.transform, "CircleProjector");
                if (fallback != null)
                {
                    Log.Debug("[RadiusManager] Found fallback CircleProjector.");
                    return fallback.gameObject;
                }
            }

            return null;
        }

        private static void PerformRadiusPickup(Equippable_TrashGrabber grabber)
        {
            var srcItems = NetworkSingleton<TrashManager>.Instance?.trashItems;
            if (srcItems == null || grabber.trashGrabberInstance == null)
                return;

            var items = new List<TrashItem>(srcItems.Count);
            for (int i = 0; i < srcItems.Count; i++)
            {
                if (srcItems[i] != null)
                    items.Add(srcItems[i]);
            }

            var projector = grabber.transform.Find("GrabberRadiusProjector");
            if (projector == null) return;

            Vector3 centerPos = projector.position;
            Vector2 flatCenter = new(centerPos.x, centerPos.z);

            float radius = Mathf.Clamp(ModManager.Grabber_Radius.Value, 0.5f, 3f);
            float radiusSqr = radius * radius;

            int pickedUp = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                Vector3 closest = item.GetComponent<Collider>()?.ClosestPoint(centerPos) ?? item.transform.position;
                Vector2 flatItem = new Vector2(closest.x, closest.z);

                if ((flatItem - flatCenter).sqrMagnitude > radiusSqr)
                    continue;

                if (ForcePickup(grabber, item))
                    pickedUp++;
                else
                    Log.Debug($"[RadiusManager] ForcePickup failed for {item.name}");

                if (grabber.GetCapacity() <= 0)
                    break;
            }

            if (pickedUp > 0)
                Log.Msg($"[RadiusManager] Picked up {pickedUp} items.");
        }

        private static bool ForcePickup(Equippable_TrashGrabber grabber, TrashItem item)
        {
            soundPlayedThisCycle = false;

            var id = item.ID ?? item.name;
            if (string.IsNullOrEmpty(id))
            {
                Log.Debug($"[ForcePickup] Failed: item ID missing for {item.name}");
                return false;
            }

            var instance = grabber.trashGrabberInstance;
            if (instance == null)
            {
                Log.Debug("[ForcePickup] Failed: grabber instance null");
                return false;
            }

            int before = grabber.GetCapacity();

            instance.AddTrash(id, 1);

            int after = grabber.GetCapacity();
            if (after >= before)
            {
                Log.Debug($"[ForcePickup] Failed to add {item.name} (capacity unchanged)");
                return false;
            }

            if (item.TryGetComponent<InteractableObject>(out var interactable))
                interactable.Destroy();

            UnityEngine.Object.Destroy(item.gameObject);

            if (!soundPlayedThisCycle)
            {
                if (clickSource != null && clickSource.TryGetComponent(out RandomizedAudioSourceController controller))
                {
                    controller.Play();
                }

                if (whooshSource != null && whooshSource.TryGetComponent(out RandomizedAudioSourceController controller2))
                {
                    controller2.Play();
                }
                soundPlayedThisCycle = true;
            }

            return true;
        }

        private static Transform? FindChildRecursive(Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name == name)
                    return child;

                var found = FindChildRecursive(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}