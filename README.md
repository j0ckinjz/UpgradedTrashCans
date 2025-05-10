
# 📦 UpgradedTrashCans — MelonLoader Mod for *Schedule I*

This mod adds **two purchasable trash can upgrades** and a new **trash grabber pro** to the Hardware Store in *Schedule I*:

- 🗑️ **Trash Bin** – 40 capacity
- 🗑️ **Trash Compactor** – 100 capacity
- 🤖 **Trash Grabber Pro** – 50 capacity

These upgraded trash cans & grabber appear alongside the default model and function identically, but with improved stats and visual tinting.

---

## 🛒 Default Shop Settings

| Variant           | Price  | Capacity | Pickup Radius  | Unlock Rank  | Tier  |
|-------------------|--------|----------|----------------|--------------|-------|
| Default           | $25    | 20       | 4.0 units      | N/A          | N/A   |
| Trash Bin         | $250   | 40       | 5.0 units      | Hoodlum      | 1     |
| Trash Compactor   | $1000  | 100      | 8.0 units      | Hustler      | 5     |
| Trash Grabber     | $750   | 50       | N/A            | Hustler      | 1     |

- 🏗️ Placed using the normal building grid.
- 💾 Persist across save/load cycles.
- 🖼️ Custom shop icons and model tinting for each variant.
- 🧼 Color tint is visible even when stored on a shelf.
- 🔄 All models reuse base game prefabs with enhanced behavior.

---

## ⚙️ Customization (Optional)

Settings can be changed in-game via the **Mod Manager Phone App**, including:

- 🛠️ **Capacity**
- 🎯 **Cleaner Pickup Radius** (for trash cans)
- 🎨 **Color Tint**
- 🎖️ **Rank & Tier Unlock**
- 💸 **Price**
- 🔓 **Unlock Immediately** (bypass rank restrictions)

**To customize:**
1. Open the Mod Manager Phone App
2. Tap into **Upgraded Trash Can**
3. Adjust any value to your liking  
*Changes apply on next scene load.*

---

## 🔄 **Multiplayer Sync**
  
- The **host’s settings** are automatically synced to all clients
- Sync includes:
  - Capacity
  - Trash Can Radius
  - Color
  - Required rank & tier
  - UnlockImmediately flag
- Clients wait for settings to become available using a safe fallback retry loop
- Version compatibility is checked between host and client before applying settings
- Clients will not inject upgraded items if host mod is disabled or missing

---

## 📁 Installation

1. Install **MelonLoader** for *Schedule I*
2. Drop `UpgradedTrashCans.dll` into the `Mods` folder
3. Launch the game and enjoy smarter waste management

---

## 🐞 Bug Reporting

If you encounter a bug, please help by enabling debug logging:

- Open the **Mod Manager Phone App** and locate UpgradedTrashCan config
- Scroll to the bottom and locate the **Enable Debug Logging** toggle
- Turn it on and reproduce the issue
- Logs will be written to the MelonLoader console/logs folder

You can submit bugs with logs attached via NexusMods.

---

## 🧪 Notes
- Settings are applied automatically on scene load
- If joining a multiplayer session, client settings will be overridden by the host
- The mod uses `LobbyData` for robust cross-player sync

---
© 2024 j0ckinjz. Licensed under Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)
