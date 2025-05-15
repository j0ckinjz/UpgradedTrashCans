# 📦 Upgraded Trash Cans — Changelog

## v1.5.4
- 🔔 **Updates Checker Support**: Added metadata compatibility with `Updates Checker`
  → Mod version can now be tracked automatically.
- 🧹 **Code Cleanup & Null Safety**: Improved internal stability with better null checks and safer initialization handling.
- ⚙️ **Early Settings Initialization**: Moved ModManager Preferences to initialize earlier.

## v1.5.3
- ✅ **Bugfix**: `UnlockImmediately` flag now syncs correctly for Trash Can variants in multiplayer  
  → Clients will no longer see bin/compactor as unlocked if the host hasn’t allowed it
- 🏷️ **Version Split**: Main branch renamed to `UpgradedTrashCans_Il2Cpp.dll` to separate from Mono version

## v1.5.2
- ✅ **Client Safety Check**: Clients can no longer inject upgraded trash cans if the host does not have the mod installed  
  → Prevents ghost items and null reference behavior

## v1.5.1
- 🛠️ **Bugfix**: Restored trash can radius visuals broken by the latest game update

## v1.5
- 🔄 **Multiplayer Settings Sync**: All clients now inherit the host's settings  
  → Capacity, price, unlock state, etc., now sync from host to clients

## v1.4
- 🔁 **Multiplayer Refactor**: Reworked injection logic  
  → Modified base game objects directly for better multiplayer stability

## v1.3
- ⚙️ **ModManager Support**: Added in-game configuration via the Mod Manager Phone App

## v1.2
- 🤖 **New Variant**: Added the **Trash Grabber Pro**  
  → Increased capacity and customizable tint color

## v1.1
- 🗑️ **Multiple Variants**: Added two upgradeable trash cans via a variant list system

## v1.0
- 🧪 **Prototype**: Injected a single new trash can as a proof-of-concept
