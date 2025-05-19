using UnityEngine;
#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
#elif MONO
using ScheduleOne;
using ScheduleOne.ItemFramework;
#endif

namespace UpgradedTrashCans
{
    public static class ColorExtensions
    {
        public static Color ToUnityColor(this ColorOption option)
        {
            return option switch
            {
                ColorOption.Green => Color.green,
                ColorOption.Blue => Color.blue,
                ColorOption.Red => Color.red,
                ColorOption.Yellow => Color.yellow,
                ColorOption.Cyan => Color.cyan,
                ColorOption.Magenta => Color.magenta,
                ColorOption.Black => Color.black,
                _ => Color.white,
            };
        }

        public static Color ToUnityColor(this ExtendedColorOption option)
        {
            return option switch
            {
                ExtendedColorOption.Purple => new Color(0.63f, 0.13f, 0.94f),
                ExtendedColorOption.Orange => new Color(1f, 0.65f, 0f),
                ExtendedColorOption.Pink => new Color(1f, 0.75f, 0.8f),
                ExtendedColorOption.Brown => new Color(0.6f, 0.3f, 0.15f),
                ExtendedColorOption.Lime => new Color(0.2f, 0.8f, 0.2f),
                ExtendedColorOption.Teal => new Color(0f, 0.5f, 0.5f),
                ExtendedColorOption.Indigo => new Color(0.29f, 0f, 0.51f),
                ExtendedColorOption.Violet => new Color(0.93f, 0.51f, 0.93f),
                _ => Color.white
            };
        }

        public static bool TryParseRGB(string input, out Color color)
        {
            color = Color.white;

            if (string.IsNullOrWhiteSpace(input))
            {
                Log.Warn($"RGB string is null or empty: \"{input}\"");
                return false;
            }

            var parts = input.Split(',');
            if (parts.Length != 3)
            {
                Log.Warn($"RGB string does not contain 3 components: \"{input}\"");
                return false;
            }

            if (float.TryParse(parts[0].TrimEnd('f'), out var r) &&
                float.TryParse(parts[1].TrimEnd('f'), out var g) &&
                float.TryParse(parts[2].TrimEnd('f'), out var b))
            {
                color = new Color(r, g, b, 1f);
                return true;
            }

            Log.Warn($"Failed to parse float values from RGB string: \"{input}\"");
            return false;
        }
    }

    public static class SpriteLoader
    {
        public static Sprite TintSprite(Sprite original, Color tint, string name = null)
        {
            Texture2D originalTexture = MakeReadableCopy(original.texture);
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

            Color[] pixels = originalTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color baseColor = pixels[i];

                Color darkened = Color.Lerp(baseColor, Color.black, 0.2f);
                float luminance = 0.3f * baseColor.r + 0.59f * baseColor.g + 0.11f * baseColor.b;
                Color tinted = Color.Lerp(darkened, tint * luminance, 0.7f);

                pixels[i] = new Color(tinted.r, tinted.g, tinted.b, baseColor.a); // preserve alpha
            }

            newTexture.SetPixels(pixels);
            newTexture.Apply();

            var sprite = Sprite.Create(
                newTexture,
                new Rect(0, 0, newTexture.width, newTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            if (!string.IsNullOrEmpty(name))
                sprite.name = name;

            return sprite;
        }

        private static Texture2D MakeReadableCopy(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(
                source.width, source.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(source, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readable.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readable;
        }
    }

    internal static class VisualHelper
    {       
        public static void TintRenderers(Transform root, Color color, params string[] targetNames)
        {
            var targets = root.GetComponentsInChildren<Transform>(true)
                .Where(t => targetNames.Contains(t.name))
                .Select(t => t.GetComponent<MeshRenderer>())
                .Where(r => r != null);

            foreach (var renderer in targets)
                renderer.material.color = color;
        }
    }

    public static class DefinitionTracker
    {
        private static readonly List<ItemDefinition> TrackedDefinitions = new();

        public static void TrackDefinition(ItemDefinition def)
        {
            if (def == null) return;

            Registry.Instance.AddToRegistry(def);
            Registry.Instance.AddToItemDictionary(new Registry.ItemRegister { ID = def.ID, Definition = def });

            Log.Debug($"Tracking definition: {def.ID}");
            TrackedDefinitions.Add(def);
        }

        public static void ClearAll()
        {
            foreach (var def in TrackedDefinitions)
            {
                Registry.Instance.RemoveFromRegistry(def);
                Registry.Instance.RemoveItemFromDictionary(new Registry.ItemRegister
                {
                    ID = def.ID,
                    Definition = def
                });
            }

            TrackedDefinitions.Clear();
            Log.Msg("Cleared all tracked definitions.");
        }
    }
}
