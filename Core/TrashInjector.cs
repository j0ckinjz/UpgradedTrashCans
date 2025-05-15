using System.Collections;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Levelling;
using Il2CppScheduleOne.UI.Shop;
using Il2CppScheduleOne.ObjectScripts.WateringCan;
using Il2CppScheduleOne.Networking;

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

            // Find shop interfaces in the scene by name
            var hardwareStores = new List<ShopInterface>();

            try
            {
                foreach (var shop in ShopInterface.AllShops)
                {
                    if (IsValidHardwareStore(shop))
                        hardwareStores.Add(shop);
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn($"[TrashInjector] Failed to access ShopInterface.AllShops — using fallback");

                // Fallback: search entire scene
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

            // Use first store as base to clone item definitions
            var baseStore = hardwareStores[0];
            var baseListings = baseStore?.Listings?.ToArray() ?? Array.Empty<ShopListing>();

            var baseTrashCan = baseListings.FirstOrDefault(l =>
            {
                try
                {
                    return !IsUnityNull(l.Item) && l.Item.name == "TrashCan";
                }
                catch
                {
                    return false;
                }
            });

            var baseGrabber = baseListings.FirstOrDefault(l =>
            {
                try
                {
                    return !IsUnityNull(l.Item) && l.Item.name == "TrashGrabber";
                }
                catch
                {
                    return false;
                }
            });

            // Inject Trash Can variants
            if (baseTrashCan != null)
            {
                foreach (var variant in TrashCanVariants.All)
                {
                    InjectCustomTrashCan(baseStore, baseTrashCan, variant);
                }
            }

            // Inject Trash Grabber variants
            if (baseGrabber != null)
            {
                foreach (var variant in TrashGrabberVariants.All)
                {
                    InjectCustomTrashGrabber(baseStore, baseGrabber, variant);
                }
            }

            // Add injected variants to all stores
            foreach (var shop in hardwareStores)
            {
                foreach (var variant in TrashCanVariants.All)
                {
                    if (variant?.Definition == null)
                    {
                        Log.Warn($"Skipped injecting trash can variant {variant?.Name} due to missing definition.");
                        continue;
                    }

                    try
                    {
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
                        Log.Debug($"Injected {variant.Name} with ID {variant.ID} into shop.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to inject trash can variant {variant.Name}: {ex.Message}");
                    }
                }

                foreach (var variant in TrashGrabberVariants.All)
                {
                    if (variant?.Definition == null)
                    {
                        Log.Warn($"Skipped injecting trash grabber variant {variant?.Name} due to missing definition.");
                        continue;
                    }

                    try
                    {
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
                        Log.Debug($"Injected {variant.Name} with ID {variant.ID} into shop.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to inject trash grabber variant {variant.Name}: {ex.Message}");
                    }
                }
            }

            Log.Msg("Upgraded items injected successfully into all Hardware Stores!");
            yield break;
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

        private static bool IsUnityNull(UnityEngine.Object obj)
        {
            return obj == null || obj.Equals(null);
        }


        private static void InjectCustomTrashCan(ShopInterface shop, ShopListing baseListing, Variants variant)
        {
            var baseDef = baseListing.Item.TryCast<BuildableItemDefinition>();
            if (baseDef == null || baseDef.BuiltItem == null)
            {
                Log.Debug("Invalid base listing for trash can injection.");
                return;
            }

            // Clone the definition only — reuse the prefab
            var def = UnityEngine.Object.Instantiate(baseDef);
            def.Name = variant.Name;
            def.ID = variant.ID;
            def.Description = variant.Description;
            def.BasePurchasePrice = variant.Price;
            def.name = variant.Name.Replace(" ", "");

            def.RequiresLevelToPurchase = !variant.UnlockImmediately;
            if (!variant.UnlockImmediately)
                def.RequiredRank = new FullRank(variant.RequiredRank, variant.Tier);

            // Tint the icon
            if (baseDef.Icon != null)
            {
                def.Icon = SpriteLoader.TintSprite(baseDef.Icon, variant.Color, $"{variant.Name}_Icon");
            }
            else
            {
                Log.Warn($"{baseDef.Name} has no icon.");
            }
            // Use the network-safe built prefab (no cloning)
            def.BuiltItem = baseDef.BuiltItem;

            // Track definition for runtime customization
            DefinitionTracker.TrackDefinition(def);
            variant.Definition = def;
        }

        private static void InjectCustomTrashGrabber(ShopInterface shop, ShopListing baseListing, TrashGrabberVariant variant)
        {
            var baseDef = baseListing.Item.TryCast<TrashGrabberDefinition>();
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

            // Reuse the base prefab
            def.Equippable = baseDef.Equippable;

            DefinitionTracker.TrackDefinition(def);
            variant.Definition = def;
        }
    }
}
