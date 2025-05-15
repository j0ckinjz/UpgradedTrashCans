# 📦 Upgraded Trash Cans — Smarter Waste Management for *Schedule I*

> **Main Il2Cpp Branch** & **Mono/Alternative Branch** releases availible!

This mod enhances trash-related gameplay by adding **two new trash can upgrades** and a **pro-level grabber** to the Hardware Store — all customizable and fully multiplayer compatible.

---

## 🗑️ Hardware Store Upgrades

- **Trash Bin** — 40 capacity, medium cleaner radius  
- **Trash Compactor** — 100 capacity, large cleaner radius  
- **Trash Grabber Pro** — 50 capacity

Each variant:
- Has custom price, name, and color tint
- Uses original game models with enhanced stats
- Unlocks via rank or instantly (configurable)
- Works in multiplayer with full sync

---

## ⚙️ Customization via Mod Manager (Optional)

Use the in-game Mod Manager Phone App to adjust:
- Price, capacity, radius (for cans)
- Color mode: standard, extended, or custom RGB
- Unlock rank, tier, or toggle unlock immediately  
*Changes apply on the next scene load.*

> 💡 If dropdowns aren’t working, update Mod Manager.

---

## 🎨 Using Custom RGB Values

You can manually enter RGB color values in the Mod Manager:

- **Format**: `R,G,B`  
- **Example**: `0.2f, 0.8f, 0.2f` → bright lime green  
- Alpha is always set to `1f` (fully opaque)

Need inspiration? [Unity Color Reference](https://docs.unity3d.com/ScriptReference/Color.html)

> Tip: You can use full decimal precision or round to 2 decimal places.

---

## 🔄 Multiplayer Sync

- Host settings override all clients
- Variants sync on join with a retry system
- Mod disables injection if host does not have the mod

---

### 🧑‍🤝‍🧑 Compatibility Matrix

| Host | Client | Result |
|------|--------|--------|
| ✅ Has Mod | ✅ Has Mod | Full sync — correct variants and stats |
| ✅ Has Mod | ❌ No Mod  | Partial — client sees base visuals, can't pick up |
| ❌ No Mod  | ✅ Has Mod | No injection — mod disables itself |

---

## ⚠️ Known Issues

- Saving without the mod will remove upgraded items from your save  
- In multiplayer, remote players may briefly see base models before sync  
  The mod retries up to 10 times with short delays

---

## 📦 Changelog

### v1.5.4
- 🔔 **Updates Checker Support**: Added metadata compatibility with `Updates Checker`
  → Mod version can now be tracked automatically.
- 🧹 **Code Cleanup & Null Safety**: Improved internal stability with better null checks and safer initialization handling.
- ⚙️ **Early Settings Initialization**: Moved ModManager Preferences to initialize earlier.

### v1.5.3 to v1.0
- Available on [GitHub](https://github.com/j0ckinjz/UpgradedTrashCans/blob/main/CHANGELOG.md)

---

## 💾 Installation

1. Install MelonLoader for *Schedule I*  
2. Drop `UpgradedTrashCans.dll` into the `Mods/` folder  
3. Launch the game — upgrades will appear in the Hardware Store

---

## 🐞 Bug Reporting

1. Open the Mod Manager Phone App → UpgradedTrashCan settings  
2. Scroll to the bottom and enable **Debug Logging**  
3. Reproduce the issue  
4. Logs are saved to the MelonLoader `Logs/` folder  
5. Submit reports via [GitHub](https://github.com/j0ckinjz/UpgradedTrashCans) or NexusMods

---

## 🧪 Notes

- Items persist across save/load  
- Custom tints apply even when shelved  
- Uses original prefabs — no additional assets required

---

## 📁 Source Code & Releases

- 💻 GitHub: [j0ckinjz/UpgradedTrashCans](https://github.com/j0ckinjz/UpgradedTrashCans)
- 📦 Thunderstore: [j0ckinjz on Thunderstore](https://thunderstore.io/c/schedule-i/p/j0ckinjz/)
- 📥 NexusMods: [Upgraded Trash Cans on NexusMods](https://www.nexusmods.com/schedule1/mods/928)

📜 Licensed under [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/)
🛠 Mod by **j0ckinjz**

---

## 📝 Author’s Note

The goal of this mod was to add meaningful, progression-based upgrades to trash handling in *Schedule I*. Each variant has balanced stats with optional customization via Mod Manager.

> This mod is not a cheat — it’s designed to give players more flexibility while preserving game balance.
