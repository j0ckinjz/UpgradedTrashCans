using System.Linq;
using System;
using Il2CppScheduleOne.Levelling;
using MelonLoader;
using UnityEngine;
using Il2CppScheduleOne.Networking;
using Il2CppSteamworks;
using System.Text;
using static UpgradedTrashCans.ModManager;

namespace UpgradedTrashCans
{
    public static class VariantSyncManager
    {
        private const string LobbyTagPrefix = "UpgradedTrashCans_Settings:";
        private static readonly string CurrentSyncVersion = typeof(Core).Assembly.GetName().Version.ToString();
        public static bool HasReceivedHostVariants { get; private set; } = false;


        public static void SyncSettingsToVariants()
        {
            bool isHost = Lobby.Instance?.IsHost == true;
            bool isClient = Lobby.Instance?.IsHost == false && Lobby.Instance?.IsInLobby == true;

            if (isHost)
            {
                foreach (var variant in TrashCanVariants.All)
                {
                    if (variant.Type == TrashCanType.Compactor)
                        TrashCompactorSettings.ApplyTo(variant);
                    else
                        TrashBinSettings.ApplyTo(variant);
                    Log.Debug($"Applied settings to {variant.Name} — Capacity: {variant.Capacity}, Color: {variant.Color}");
                }

                foreach (var variant in TrashGrabberVariants.All)
                {
                    variant.Price = Grabber_Price.Value;
                    variant.Color = Grabber_Source.Value switch
                    {
                        ColorSource.CustomRGB =>
                            ColorExtensions.TryParseRGB(Grabber_CustomRGB.Value, out var rgb) ? rgb : Color.white,
                        ColorSource.Extended => Grabber_ExtendedColor.Value.ToUnityColor(),
                        _ => Grabber_Color.Value.ToUnityColor()
                    };
                    variant.Capacity = Grabber_Capacity.Value;
                    variant.RequiredRank = (ERank)Grabber_Rank.Value;
                    variant.Tier = Grabber_Tier.Value;
                    variant.UnlockImmediately = Grabber_UnlockImmediately.Value;
                    Log.Debug($"Applied settings to {variant.Name} — Capacity: {variant.Capacity}, Color: {variant.Color}, Unlock: {variant.UnlockImmediately}");
                }

                var payload = new StringBuilder();
                payload.Append(LobbyTagPrefix + CurrentSyncVersion + "|");
                foreach (var variant in TrashCanVariants.All)
                {
                    payload.Append($"{variant.ID}:{variant.Price},{variant.Capacity},{variant.Radius},{variant.Color.r:F2},{variant.Color.g:F2},{variant.Color.b:F2},{(int)variant.RequiredRank},{variant.Tier},1;");
                }
                foreach (var variant in TrashGrabberVariants.All)
                {
                    int unlock = variant.UnlockImmediately ? 1 : 0;
                    payload.Append($"{variant.ID}:{variant.Price},{variant.Capacity},0.0,{variant.Color.r:F2},{variant.Color.g:F2},{variant.Color.b:F2},{(int)variant.RequiredRank},{variant.Tier},{unlock};");
                }

                Lobby.Instance.SetLobbyData("UpgradedTrashSync", payload.ToString());
            }
            else if (isClient)
            {
                MelonCoroutines.Start(WaitForLobbyPayload());
            }
            else
            {
                foreach (var variant in TrashCanVariants.All)
                {
                    if (variant.Type == TrashCanType.Compactor)
                        TrashCompactorSettings.ApplyTo(variant);
                    else
                        TrashBinSettings.ApplyTo(variant);
                }

                foreach (var variant in TrashGrabberVariants.All)
                {
                    variant.Price = Grabber_Price.Value;
                    variant.Color = Grabber_Source.Value switch
                    {
                        ColorSource.CustomRGB =>
                            ColorExtensions.TryParseRGB(Grabber_CustomRGB.Value, out var rgb) ? rgb : Color.white,
                        ColorSource.Extended => Grabber_ExtendedColor.Value.ToUnityColor(),
                        _ => Grabber_Color.Value.ToUnityColor()
                    };
                    variant.Capacity = Grabber_Capacity.Value;
                    variant.RequiredRank = (ERank)Grabber_Rank.Value;
                    variant.Tier = Grabber_Tier.Value;
                    variant.UnlockImmediately = Grabber_UnlockImmediately.Value;
                }
            }

            VariantLookup.Refresh();
            Log.Msg("Trash can and grabber settings synced.");
        }

        private static System.Collections.IEnumerator WaitForLobbyPayload()
        {
            const int maxAttempts = 10;
            const float delaySeconds = 1f;

            for (int i = 0; i < maxAttempts; i++)
            {
                string payload = SteamMatchmaking.GetLobbyData(Lobby.Instance.LobbySteamID, "UpgradedTrashSync");
                if (!string.IsNullOrEmpty(payload))
                {
                    Log.Msg("[MultiplayerSync] Host sync payload found. Applying...");
                    OnLobbyMessageReceived(payload);
                    yield break;
                }

                Log.Debug($"[MultiplayerSync] Waiting for host sync... ({i + 1}/{maxAttempts})");
                yield return new WaitForSeconds(delaySeconds);
            }

            Log.Warn("[MultiplayerSync] Timed out waiting for host variant sync.");
        }

        public static void OnLobbyMessageReceived(string rawMessage)
        {
            if (!rawMessage.StartsWith(LobbyTagPrefix)) return;

            string stripped = rawMessage.Substring(LobbyTagPrefix.Length);
            string[] versionSplit = stripped.Split('|');
            if (versionSplit.Length != 2)
            {
                Log.Error("[MultiplayerSync] Invalid payload format.");
                return;
            }

            string version = versionSplit[0];
            string data = versionSplit[1];

            if (version != CurrentSyncVersion)
            {
                Log.Warn($"[MultiplayerSync] Host mod version ({version}) does not match client mod version ({CurrentSyncVersion}).");
                return;
            }

            try
            {
                var records = data.Split(';');
                foreach (var entry in records)
                {
                    if (string.IsNullOrWhiteSpace(entry)) continue;
                    var parts = entry.Split(':');
                    if (parts.Length != 2) continue;

                    string id = parts[0];
                    var fields = parts[1].Split(',');
                    if (fields.Length != 9) continue;

                    if (!float.TryParse(fields[0], out var price)) continue;
                    if (!int.TryParse(fields[1], out var capacity)) continue;
                    if (!float.TryParse(fields[2], out var radius)) continue;
                    if (!float.TryParse(fields[3], out var r)) continue;
                    if (!float.TryParse(fields[4], out var g)) continue;
                    if (!float.TryParse(fields[5], out var b)) continue;
                    if (!int.TryParse(fields[6], out var rank)) continue;
                    if (!int.TryParse(fields[7], out var tier)) continue;
                    if (!int.TryParse(fields[8], out var unlockFlag)) continue;

                    var variant = TrashCanVariants.All.FirstOrDefault(v => v.ID == id);
                    if (variant != null)
                    {
                        variant.Price = price;
                        variant.Capacity = capacity;
                        variant.Radius = radius;
                        variant.Color = new Color(r, g, b);
                        variant.RequiredRank = (ERank)rank;
                        variant.Tier = tier;
                        variant.UnlockImmediately = unlockFlag == 1;
                        continue;
                    }

                    var grabber = TrashGrabberVariants.All.FirstOrDefault(v => v.ID == id);
                    if (grabber != null)
                    {
                        grabber.Price = price;
                        grabber.Capacity = capacity;
                        grabber.Color = new Color(r, g, b);
                        grabber.RequiredRank = (ERank)rank;
                        grabber.Tier = tier;
                        grabber.UnlockImmediately = unlockFlag == 1;
                    }
                }
                VariantLookup.Refresh();
                HasReceivedHostVariants = true;
                Log.Msg("[MultiplayerSync] Synced variant settings received from host.");
            }
            catch (Exception ex)
            {
                Log.Error("[MultiplayerSync] Failed to parse variant sync message: " + ex.Message);
            }
        }
    }
}
