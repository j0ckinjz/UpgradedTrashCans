using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Levelling;
using Il2CppScheduleOne.UI.Shop;
using UnityEngine;
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

            foreach (var shop in ShopInterface.AllShops)
            {
                if (shop.name == "HardwareStoreInterface" || shop.name == "HardwareStoreInterface (North Store)")
                    hardwareStores.Add(shop);
            }

            if (hardwareStores.Count == 0)
                yield break;

            // Use first store as base to clone item definitions
            var baseStore = hardwareStores[0];
            var baseListings = baseStore.Listings.ToArray(); // Make LINQ-friendly copy

            var baseTrashCan = baseListings.FirstOrDefault(l => l.Item?.name == "TrashCan");
            var baseGrabber = baseListings.FirstOrDefault(l => l.Item?.name == "TrashGrabber");

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
                    shop.Listings.Add(new ShopListing
                    {
                        Item = variant.Definition,
                        name = variant.Name,
                        OverridePrice = true,
                        OverriddenPrice = variant.Price,
                        CurrentStock = 999
                    });
                    shop.CreateListingUI(shop.Listings[^1]);
                    Log.Debug($"Injected {variant.Name} with ID {variant.ID} into shop.");
                }

                foreach (var variant in TrashGrabberVariants.All)
                {
                    shop.Listings.Add(new ShopListing
                    {
                        Item = variant.Definition,
                        name = variant.Name,
                        OverridePrice = true,
                        OverriddenPrice = variant.Price,
                        CurrentStock = 999
                    });
                    shop.CreateListingUI(shop.Listings[^1]);
                    Log.Debug($"Injected {variant.Name} with ID {variant.ID} into shop.");
                }
            }

            Log.Msg("Upgraded items injected successfully into all Hardware Stores!");
            yield break;
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
            var def = Object.Instantiate(baseDef);
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

            var def = Object.Instantiate(baseDef);
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
