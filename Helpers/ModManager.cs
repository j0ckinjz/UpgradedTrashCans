using MelonLoader;
#if IL2CPP
using Il2CppScheduleOne.Levelling;
#elif MONO
using ScheduleOne.Levelling;
#endif

namespace UpgradedTrashCans
{
    public enum ColorOption
    {
        Green, Blue, Red, Yellow, Cyan, Magenta, Black, White
    }
    public enum ExtendedColorOption
    {
        Purple, Orange, Pink, Brown, Lime, Teal, Indigo, Violet
    }
    public enum ColorSource
    {
        Standard, Extended, CustomRGB
    }
    public enum RankOption
    {
        Street_Rat, Hoodlum, Peddler, Hustler, Bagman,
        Enforcer, Shot_Caller, Block_Boss, Underlord, Baron, Kingpin
    }

    public static class ModManager
    {
        public static ModSettingsGroup TrashBinSettings;
        public static ModSettingsGroup TrashCompactorSettings;
        public static MelonPreferences_Category DebugCategory;
        public static MelonPreferences_Category GrabberCategory;
        public static MelonPreferences_Entry<ColorSource> Grabber_Source;
        public static MelonPreferences_Entry<ColorOption> Grabber_Color;
        public static MelonPreferences_Entry<ExtendedColorOption> Grabber_ExtendedColor;
        public static MelonPreferences_Entry<int> Grabber_Capacity;
        public static MelonPreferences_Entry<RankOption> Grabber_Rank;
        public static MelonPreferences_Entry<int> Grabber_Tier;
        public static MelonPreferences_Entry<bool> Grabber_UnlockImmediately;
        public static MelonPreferences_Entry<float> Grabber_Price;
        public static MelonPreferences_Entry<string> Grabber_CustomRGB;
        public static MelonPreferences_Entry<bool> Grabber_BulkEject;
        public static MelonPreferences_Entry<bool> Grabber_RadiusPickup;
        public static MelonPreferences_Entry<float> Grabber_Radius;
        public static MelonPreferences_Entry<bool> DebugLogs;

        public static void InitializeSettings()
        {
            TrashBinSettings = CreateGroup("01_TrashBin", "Upgraded Trash Bin", ColorOption.Green, ExtendedColorOption.Lime, 40, 4.5f, RankOption.Hoodlum, 1, false, 250f);
            TrashCompactorSettings = CreateGroup("02_TrashCompactor", "Upgraded Trash Compactor", ColorOption.Blue, ExtendedColorOption.Indigo, 100, 7.5f, RankOption.Hustler, 5, false, 1000f);

            GrabberCategory = MelonPreferences.CreateCategory("UpgradedTrashCans_03_UpgradedTrashGrabber", "Trash Grabber Pro");
            Grabber_Price = GrabberCategory.CreateEntry("01_Price", 750f, "Price", "Set the shop price for the Trash Grabber Pro.");
            Grabber_Capacity = GrabberCategory.CreateEntry("02_Capacity", 50, "Capacity", "Set the capacity of the Trash Grabber Pro.");
            Grabber_Rank = GrabberCategory.CreateEntry("03_Rank", RankOption.Hustler, "Rank Unlock", "Required rank to unlock the Trash Grabber Pro.");
            Grabber_Tier = GrabberCategory.CreateEntry("04_Tier", 1, "Tier Unlock", "Required tier at the rank to unlock the Trash Grabber Pro.");
            Grabber_UnlockImmediately = GrabberCategory.CreateEntry("05_UnlockImmediately", false, "Unlock Immediately", "Skip rank requirement for Trash Grabber Pro.");
            Grabber_Source = GrabberCategory.CreateEntry("06_ColorSource", ColorSource.Standard, "Color Source", "Choose which color mode to use (Base, Extended, Custom RGB).");
            Grabber_Color = GrabberCategory.CreateEntry("07_Color", ColorOption.Cyan, "Standard Colors", "Select the color of the Trash Grabber Pro.");
            Grabber_ExtendedColor = GrabberCategory.CreateEntry("08_ExtendedColor", ExtendedColorOption.Teal, "Extended Colors", "Custom expanded tints.");
            Grabber_CustomRGB = GrabberCategory.CreateEntry("09_CustomRGB", "0.5f, 0.5f, 0.5f", "Custom RGB Values", "Format: R,G,B (0–1 range)");
            Grabber_BulkEject = GrabberCategory.CreateEntry("10_Enable Bulk Eject", true, "Enable Shift+Click Eject All");
            Grabber_RadiusPickup = GrabberCategory.CreateEntry("11_EnableRadiusPickup", true, "Enable Radius Toggle (Shift+R)", "Adds a pickup radius.");
            Grabber_Radius = GrabberCategory.CreateEntry("12_GrabberRadius", 1f, "Grabber Radius (0.5 - 3)", "Adjusts the size of the pickup radius.");

            DebugCategory = MelonPreferences.CreateCategory("UpgradedTrashCans_04_DebugLogs", "Enable Debug Logging");
            DebugLogs = DebugCategory.CreateEntry("01_Debug", false, "Enable Debug Logs");

            Log.Msg("ModManager Settings initialized.");
        }

        private static ModSettingsGroup CreateGroup(string keyPrefix, string displayName, ColorOption defaultColor, ExtendedColorOption defaultExtendedColor, int defaultCapacity, float defaultRadius, RankOption defaultRank, int defaultTier, bool unlockImmediately, float defaultPrice)
        {
            var cat = MelonPreferences.CreateCategory($"UpgradedTrashCans_{keyPrefix}", displayName);
            return new ModSettingsGroup
            {
                Category = cat,
                Price = cat.CreateEntry("01_Price", defaultPrice, "Price", $"Shop purchase price for the {displayName}."),
                Capacity = cat.CreateEntry("02_Capacity", defaultCapacity, "Capacity", $"Number of trash items the {displayName} can hold."),
                Radius = cat.CreateEntry("03_Radius", defaultRadius, "Cleaner Pickup Radius", $"Cleaner pickup radius for the {displayName} (units)."),
                Rank = cat.CreateEntry("04_Rank", defaultRank, "Rank Unlock", $"Rank required to unlock the {displayName} in shop."),
                Tier = cat.CreateEntry("05_Tier", defaultTier, "Tier Unlock", $"Tier at required rank for the {displayName} to unlock."),
                UnlockImmediately = cat.CreateEntry("06_UnlockImmediately", unlockImmediately, "Unlock Immediately", $"If true, the {displayName} is available immediately."),
                Source = cat.CreateEntry("07_ColorSource", ColorSource.Standard, "Color Source", "Choose which color mode to use (Base, Extended, Custom RGB)."),
                Color = cat.CreateEntry("08_Color", defaultColor, "Standard Colors", $"Color tint for the {displayName} model."),
                ExtendedColor = cat.CreateEntry("09_ExtendedColor", defaultExtendedColor, "Extended Colors", $"Expanded color tints for the {displayName} model."),
                CustomRGB = cat.CreateEntry("10_CustomRGB", "0.5f, 0.5f, 0.5f", "Custom RGB Values", "Format: R,G,B (0–1 range)")
            };
        }
        
        public class ModSettingsGroup
        {
            public MelonPreferences_Category Category;
            public MelonPreferences_Entry<ColorOption> Color;
            public MelonPreferences_Entry<ExtendedColorOption> ExtendedColor;
            public MelonPreferences_Entry<int> Capacity;
            public MelonPreferences_Entry<float> Radius;
            public MelonPreferences_Entry<RankOption> Rank;
            public MelonPreferences_Entry<int> Tier;
            public MelonPreferences_Entry<bool> UnlockImmediately;
            public MelonPreferences_Entry<float> Price;
            public MelonPreferences_Entry<string> CustomRGB;
            public MelonPreferences_Entry<ColorSource> Source;

            public void ApplyTo(Variants variant)
            {
                variant.Price = Price.Value;
                variant.Color = Source.Value switch
                {
                    ColorSource.CustomRGB =>
                        ColorExtensions.TryParseRGB(CustomRGB.Value, out var rgb) ? rgb : UnityEngine.Color.white,
                    ColorSource.Extended => ExtendedColor.Value.ToUnityColor(),
                    _ => Color.Value.ToUnityColor()
                };
                variant.Capacity = Capacity.Value;
                variant.Radius = Radius.Value;
                variant.RequiredRank = (ERank)Rank.Value;
                variant.Tier = Tier.Value;
                variant.UnlockImmediately = UnlockImmediately.Value;
            }
        }
    }
}