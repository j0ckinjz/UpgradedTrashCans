using System.Collections;
#if IL2CPP
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Levelling;
using Il2CppScheduleOne.UI.Shop;
using Il2CppScheduleOne.ObjectScripts.WateringCan;
using Il2CppScheduleOne.Networking;
using Il2CppScheduleOne;
#elif MONO
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.UI.Shop;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.Networking;
using ScheduleOne;
#endif

namespace UpgradedTrashCans
{
    public static class TrashInjector
    {
        public static IEnumerator InitializeShopListings()
        {
            if (Lobby.Instance?.IsHost == false && !VariantSyncManager.HasReceivedHostVariants)
            {
                Log.Msg("[TrashInjector] Skipping injection — host mod is disabled or missing.");
                yield break;
            }

            var hardwareStores = new List<ShopInterface>();
            try
            {
                foreach (var shop in ShopInterface.AllShops)
                {
                    if (IsValidHardwareStore(shop))
                        hardwareStores.Add(shop);
                }
            }
            catch
            {
                Log.Warn("[TrashInjector] Failed to access ShopInterface.AllShops — using fallback");
                foreach (var shop in UnityEngine.Object.FindObjectsOfType<ShopInterface>())
                {
                    if (IsValidHardwareStore(shop))
                        hardwareStores.Add(shop);
                }
            }

            if (hardwareStores.Count == 0)
            {
                Log.Warn("[TrashInjector] No hardware store interfaces found.");
                yield break;
            }

            BuildableItemDefinition baseTrashCan = null;
            TrashGrabberDefinition baseGrabber = null;

            foreach (var entry in Registry.Instance.ItemRegistry)
            {
                if (entry == null || entry.Definition == null)
                    continue;

                if (entry.Definition.name == "TrashCan")
                {
#if IL2CPP
                    baseTrashCan = entry.Definition.TryCast<BuildableItemDefinition>();
#elif MONO
                    baseTrashCan = entry.Definition as BuildableItemDefinition;
#endif
                }
                else if (entry.Definition.name == "TrashGrabber")
                {
#if IL2CPP
                    baseGrabber = entry.Definition.TryCast<TrashGrabberDefinition>();
#elif MONO
                    baseGrabber = entry.Definition as TrashGrabberDefinition;
#endif
                }

                if (baseTrashCan != null && baseGrabber != null)
                    break;
            }

            if (baseTrashCan != null)
            {
                foreach (var variant in TrashCanVariants.All)
                {
                    InjectCustomTrashCan(baseTrashCan, variant);
                }
            }

            if (baseGrabber != null)
            {
                foreach (var variant in TrashGrabberVariants.All)
                {
                    InjectCustomTrashGrabber(baseGrabber, variant);
                }
            }

            foreach (var shop in hardwareStores)
            {
                foreach (var variant in TrashCanVariants.All)
                {
                    if (variant?.Definition == null)
                    {
                        Log.Warn($"Skipped injecting trash can variant {variant?.Name} due to missing definition.");
                        continue;
                    }

                    var listing = new ShopListing
                    {
                        Item = variant.Definition,
                        name = variant.Name,
                        OverridePrice = true,
                        OverriddenPrice = variant.Price,
                        CurrentStock = 999
                    };

                    shop.Listings.Add(listing);
                    shop.CreateListingUI(listing);
                    Log.Debug($"Injected {variant.Name} into shop.");
                }

                foreach (var variant in TrashGrabberVariants.All)
                {
                    if (variant?.Definition == null)
                    {
                        Log.Warn($"Skipped injecting trash grabber variant {variant?.Name} due to missing definition.");
                        continue;
                    }

                    var listing = new ShopListing
                    {
                        Item = variant.Definition,
                        name = variant.Name,
                        OverridePrice = true,
                        OverriddenPrice = variant.Price,
                        CurrentStock = 999
                    };

                    shop.Listings.Add(listing);
                    shop.CreateListingUI(listing);
                    Log.Debug($"Injected {variant.Name} into shop.");
                }
            }

            Log.Msg("Upgraded items injected successfully into all Hardware Stores!");
        }

        private static bool IsValidHardwareStore(ShopInterface shop)
        {
            if (shop == null || shop.Listings == null || shop.Listings.Count == 0)
                return false;

            try
            {
                return shop.name == "HardwareStoreInterface" || shop.name == "HardwareStoreInterface (North Store)";
            }
            catch
            {
                return false;
            }
        }

        private static void InjectCustomTrashCan(BuildableItemDefinition baseDef, Variants variant)
        {
            if (baseDef == null || baseDef.BuiltItem == null)
            {
                Log.Debug("Invalid base listing for trash can injection.");
                return;
            }

            var def = UnityEngine.Object.Instantiate(baseDef);
            def.Name = variant.Name;
            def.ID = variant.ID;
            def.Description = variant.Description;
            def.BasePurchasePrice = variant.Price;
            def.name = variant.Name.Replace(" ", "");

            def.RequiresLevelToPurchase = !variant.UnlockImmediately;
            if (!variant.UnlockImmediately)
                def.RequiredRank = new FullRank(variant.RequiredRank, variant.Tier);

            if (baseDef.Icon != null)
            {
                def.Icon = SpriteLoader.TintSprite(baseDef.Icon, variant.Color, $"{variant.Name}_Icon");
            }
            else
            {
                Log.Warn($"{baseDef.Name} has no icon.");
            }

            def.BuiltItem = baseDef.BuiltItem;

            DefinitionTracker.TrackDefinition(def);
            variant.Definition = def;
        }

        private static void InjectCustomTrashGrabber(TrashGrabberDefinition baseDef, TrashGrabberVariant variant)
        {
            if (baseDef == null || baseDef.Equippable == null)
            {
                Log.Debug("Invalid base listing for trash grabber injection.");
                return;
            }

            var def = UnityEngine.Object.Instantiate(baseDef);
            def.ID = variant.ID;
            def.Name = variant.Name;
            def.name = variant.Name.Replace(" ", "");
            def.Description = variant.Description;
            def.BasePurchasePrice = variant.Price;

            def.RequiresLevelToPurchase = !variant.UnlockImmediately;
            if (!variant.UnlockImmediately)
                def.RequiredRank = new FullRank(variant.RequiredRank, variant.Tier);

            if (baseDef.Icon != null)
                def.Icon = SpriteLoader.TintSprite(baseDef.Icon, variant.Color, $"{variant.Name}_Icon");
            else
                Log.Debug($"{baseDef.Name} icon not found.");

            def.Equippable = baseDef.Equippable;

            DefinitionTracker.TrackDefinition(def);
            variant.Definition = def;
        }
    }
}