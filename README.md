## 📦 Upgraded Trash Cans — Smarter Waste Management for *Schedule I*

This mod enhances trash-related gameplay by adding two new trash can upgrades and a pro-level grabber to the Hardware Store — all customizable and fully multiplayer compatible.

---

### 🗑️ New Hardware Store Variants with Rank Requirements

- **Trash Bin** — 40 capacity, medium cleaner radius
- **Trash Compactor** — 100 capacity, large cleaner radius
- **Trash Grabber Pro** — 50 capacity

Each variant:
- Includes custom name, price, and color tint
- Shows up as a separate item in the shop
- Works like the original but with upgraded stats
- Is visible in build mode and storage with a distinct color
- Has its own unlock rank. Level up to unlock these upgraded items.

---

### ⚙️ Customization (Optional)

Use the Mod Manager Phone App (if installed) to tweak:
- Price
- Capacity
- Cleaner pickup radius (trash cans)
- Color tint (standard, extended, or custom RGB)
- Required rank / tier
- Unlock Immediately toggle

*Changes take effect on the next scene load.*  
*If the dropdown isn't working, you may need to update Mod Manager.*

---

### 🎨 Using Custom RGB Values in Mod Manager

You can manually enter RGB color values for your trash cans or grabber using the **Custom RGB** field in the Mod Manager Phone App.

- Format: `R,G,B`
- Example: `0.2f, 0.8f, 0.2f` will give you a bright lime green
- Only the first 3 values are needed — Alpha (transparency) is always set to `1f` automatically

Need inspiration? Here's the official list of Unity color values:  
https://docs.unity3d.com/ScriptReference/Color.html

*Tip: You may enter full decimal precision, but I usually round to the second place. Each value represents the intensity of Red, Green, or Blue.*

---

### 🔄 Multiplayer Support

- Host’s settings are automatically synced to all clients
- Clients apply synced values after joining using a safe retry system
- Includes mod version compatibility checks to prevent desync
- Clients will not inject upgraded items if host mod is disabled or missing

#### Multiplayer Compatibility Matrix

✅ **Host Has Mod** | ✅ **Client Has Mod**  
*Full Sync* — Host settings override client settings. Variants appear and behave as expected.

✅ **Host Has Mod** | ❌ **Client No Mod**  
*Partial Support* — Host can buy/place variants. Client sees base visuals. Trash cans function but can't be picked up by clients.

❌ **Host No Mod** | ✅ **Client Has Mod**  
*No Injection* — Mod skips shop injection.

---

### ⚠️ Known Issues / Limitations

- **Saving the game while the mod is disabled** will remove all upgraded trash items from your save.  
  You will need to re-purchase them after re-enabling the mod.

- In **multiplayer**, when a client places an upgraded trash can, other players may briefly see a normal gray trash can.  
  This happens because the game must first sync the object over the network before the mod can apply its changes.  
  The mod will retry up to 10 times, with a 2-frame delay between each attempt.  
  (In testing, it typically synced correctly within 3 tries.)

---

### 🔀 Which Version Should I Use?

- **Il2Cpp** — For the **Main Branch** (Recommended if you're not sure)
- **Mono** — For the **Alternative Branch** (You would’ve set this manually, so you’ll know if you’re using it)

> Make sure to copy the version that matches your game branch. Mismatched versions won’t work!

---

### 💾 Installation

1. Install MelonLoader for *Schedule I*
2. Drop `UpgradedTrashCans.dll` into the `Mods` folder
3. Launch the game — upgrades will appear in the Hardware Store

---

### 🐞 Bug Reporting

1. Open the Mod Manager Phone App → UpgradedTrashCan settings  
2. Scroll to the bottom and enable **Debug Logging**  
3. Reproduce the issue  
4. Logs are saved to the MelonLoader `Logs/` folder  
5. Submit reports via [GitHub](https://github.com/j0ckinjz/UpgradedTrashCans) or NexusMods

---

### 🧪 Notes

- Items persist across save/load
- Custom tints apply even when shelved
- Uses original game prefabs — no extra assets required

---

### 📁 Source Code & Releases

- 💻 GitHub: [j0ckinjz/UpgradedTrashCans](https://github.com/j0ckinjz/UpgradedTrashCans)
- 📦 Thunderstore: [j0ckinjz on Thunderstore](https://thunderstore.io/c/schedule-i/p/j0ckinjz/)
- 📥 NexusMods: [Upgraded Trash Cans on NexusMods](https://www.nexusmods.com/schedule1/mods/928)

📜 Licensed under [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/)
🛠 Mod by **j0ckinjz**

---

### 📝 Author’s Note

The original vision of this mod was to offer more **progression-based content** to the trash management system of the game.  
That’s why each upgrade includes reasonable capacity, radius, prices, and rank requirements.

As development progressed, I added optional support for the **ModManager Phone App**. This may allow players to set values far beyond intended balance.  
*Please note: this mod was not designed to be a cheat — it’s meant to give users the freedom to fine-tune values to their preferred playstyle.*
