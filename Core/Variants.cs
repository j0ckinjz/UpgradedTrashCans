using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Levelling;
using Il2CppScheduleOne.ObjectScripts.WateringCan;
using UnityEngine;

namespace UpgradedTrashCans
{
    public enum TrashCanType
    {
        Bin,
        Compactor
    }

    public class Variants
    {
        public string Name;
        public string ID;
        public float Price;
        public string Description;
        public int Capacity;
        public Color Color;
        public float Radius;
        public ERank RequiredRank;
        public int Tier;
        public BuildableItemDefinition Definition;
        public bool UnlockImmediately;
        public TrashCanType Type;
    }

    public static class TrashCanVariants
    {
        public static List<Variants> All = new()
        {
            new Variants
            {
                Name = "Trash Bin",
                ID = "trash_bin",
                Description = "A slightly larger trash bin.",
                Price = 250f,
                Capacity = 40,
                Color = Color.green,
                Radius = 5f,
                RequiredRank = ERank.Hoodlum,
                Tier = 1,
                Type = TrashCanType.Bin
            },
            new Variants
            {
                Name = "Trash Compactor",
                ID = "trash_compactor",
                Description = "A high-capacity trash compactor.",
                Price = 1000f,
                Capacity = 100,
                Color = Color.blue,
                Radius = 8f,
                RequiredRank = ERank.Hustler,
                Tier = 5,
                Type = TrashCanType.Compactor
            }
        };
    }

    public class TrashGrabberVariant
    {
        public string Name;
        public string ID;
        public string Description;
        public float Price;
        public Color Color;
        public ERank RequiredRank;
        public int Tier;
        public bool UnlockImmediately;
        public int Capacity;
        public TrashGrabberDefinition Definition;
    }

    public static class TrashGrabberVariants
    {
        public static List<TrashGrabberVariant> All = new()
        {
            new TrashGrabberVariant
            {
                Name = "Trash Grabber Pro",
                ID = "trash_grabber_pro",
                Description = "An advanced trash grabber with increased capacity.",
                Price = 750f,
                Color = Color.green,
                RequiredRank = ERank.Hustler,
                Tier = 5,
                UnlockImmediately = false,
                Capacity = 50
            }
        };
    }
}
