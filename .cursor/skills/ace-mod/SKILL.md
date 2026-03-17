---
name: ace-mod
description: Expert ACE mod development using ACE.BaseMod and Harmony. Use when building or modifying ACE mods in this repo or when the user invokes /ace-mod.
---

# ACE Mod Development Skill

You are an expert in ACEmulator (ACE) mod development using ACE.BaseMod and Harmony. The user is a beginner — always explain clearly, define jargon, and provide complete working code rather than fragments.

---

## Architecture Overview

ACE mods are **separate C# class library projects** (DLLs) that hot-reload into a running server without restarting. The stack:
- **ACE.BaseMod** (by aquafir) — the template/framework that provides `BasicMod`, `BasicPatch<T>`, `FakeProperty`, and a rich set of extension helpers
- **Harmony** — intercepts (patches) ACE's methods at runtime without editing ACE source
- **ModManager** — ACE's built-in loader; discovers and loads mods from the `Mods/` directory

Mods do NOT modify ACE source. They run alongside it and hook into it.

---

## Canonical File Structure

```
C:\ACE\Mods\YourModName\          ← OUTPUT folder (build target, not source)
├── YourModName.dll
├── ACE.Shared.dll                 ← MUST be present — each mod bundles its own copy
├── Microsoft.EntityFrameworkCore.dll  ← bundled automatically
├── Meta.json
└── Settings.json                  ← pre-shipped, documented settings template (still auto-created by BaseMod if missing)

C:\Users\you\source\repos\YourModName\   ← SOURCE folder (keep separate from output)
├── YourModName.csproj
├── Mod.cs
├── PatchClass.cs
├── Settings.cs
├── GlobalUsings.cs
└── Meta.json
```

**Important:** Keep source and output in separate directories. Set `OutputPath` in the `.csproj` to point at `C:\ACE\Mods\$(AssemblyName)`. Never place source `.cs` files in the output folder.

---

## Required Source Files

### 1. Mod.cs — Entry Point (rarely changed)
```csharp
namespace YourModName;

public class Mod : BasicMod
{
    public Mod() : base() => Setup(nameof(YourModName), new PatchClass(this));
}
```

### 2. PatchClass.cs — All Logic Goes Here

> **Critical:** Do NOT use C# primary constructor syntax for PatchClass. `OnWorldOpen` does not
> fire reliably before players issue their first command, and **if the mod is hot-reloaded after
> the world is already up, `OnWorldOpen()` never runs**. So `PatchClass.Settings` can stay null
> unless you also load settings in the constructor or in `Start()`. Use an explicit constructor
> body and/or override `Start()` so settings are always available.

**Option A — Explicit constructor (recommended):**
```csharp
namespace YourModName;

[HarmonyPatch]
public class PatchClass : BasicPatch<Settings>
{
    // Explicit constructor — runs immediately when ACE loads the mod.
    // The null-coalescing fallback uses all defaults defined in Settings.cs,
    // so the mod still works even if Settings.json doesn't exist yet.
    public PatchClass(BasicMod mod, string settingsName = "Settings.json") : base(mod, settingsName)
    {
        try { Settings ??= SettingsContainer.Settings; }
        catch { Settings ??= new Settings(); }
    }

    // IMPORTANT: Also assign here so hot-reload picks up edits to Settings.json
    public override async Task OnWorldOpen()
    {
        try { Settings = SettingsContainer.Settings; }
        catch { Settings ??= new Settings(); }
    }

    // ── HARMONY PATCHES go here ──────────────────────────────────────────
    // ── COMMANDS go here ─────────────────────────────────────────────────
}
```

**Option B — Primary constructor + Start() (hot-reload safe):**  
If you keep primary constructor syntax, you **must** override `Start()` and set Settings there. `Start()` runs on **every** mod load (cold boot and hot-reload); `OnWorldOpen()` is a **one-shot** ACE event at server startup only.
```csharp
[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    public override void Start()
    {
        base.Start();
        Settings = SettingsContainer.Settings ?? new Settings();
    }

    public override async Task OnWorldOpen()
    {
        Settings = SettingsContainer.Settings ?? new Settings();
        // ... other startup (e.g. load JSON, patch categories)
    }
}
```

### 3. Settings.cs — Configurable Values
```csharp
namespace YourModName;

public class Settings
{
    public bool EnableFeature { get; set; } = true;
    public float Multiplier { get; set; } = 1.5f;
    public Dictionary<string, float> PerItemOverrides { get; set; } = new();
    public List<int> Thresholds { get; set; } = new() { 10, 50, 100 };
}
```
Access anywhere with: `PatchClass.Settings.Multiplier`

---

## Settings & documentation workflow (standard)

From now on, use this workflow for every ACE mod in this repo:

- **Always ship a `Settings.json` template with the mod.**  
  - Place it next to the DLL in the mod output folder.  
  - Keep it small and focused on likely-tuned knobs; rely on C# defaults for everything else.  
  - BaseMod will still auto-create `Settings.json` from `Settings.cs` on first run if the file is missing, but in this repo we prefer to pre-create it so admins can read/tweak settings before first boot.

- **Document settings in two places:**
  1. **Inline JSON comments** in `Settings.json`  
     - Use `//` comments **on the line above** each key.  
     - Each comment should state in plain language: what the setting changes, the expected type/range, and any sentinel values (e.g. `0` disables, `-1` = unlimited).  
     - Example:
       ```jsonc
       // Chance for reinforcements to spawn when a creature dies (0.0–1.0). 0 disables spawns.
       "LandscapeChance": 0.15,
       ```
  2. A **`### Settings` section in the mod's `Readme.md`**  
     - Group settings by feature area (e.g. "Quest XP", "Loot Growth", "Tinkering", "QoL").  
     - For each key, list: name, type, default, and a one–two sentence, player-facing description.  
     - Use this when inline comments would become too long or when behavior depends on multiple keys.

- **Let Settings.cs remain the source of truth.**  
  - All defaults live in `Settings.cs`.  
  - `Settings.json` is a documented override layer; values you omit fall back to the C# defaults.  
  - When you add a new property in `Settings.cs`, also update:
    - The shipped `Settings.json` template with a commented example/default.  
    - The mod's `Readme.md` `Settings` table/section.

- **For nested/advanced settings:**
  - Keep JSON comments high-level and move detailed behavior to the Readme.  
  - For large collections (e.g. loot tables, mutator lists), document the shape and a small example in Readme, and ship a sane, minimally noisy default in JSON.

Following this pattern ensures every mod is self-documenting: admins can open `Settings.json` and immediately understand the knobs, and Readmes provide the deeper context and examples.

### 4. Meta.json
```json
{
  "Name": "YourModName",
  "Author": "YourName",
  "Description": "What this mod does.",
  "Version": "1.0",
  "Enabled": true,
  "HotReload": true,
  "RegisterCommands": true,
  "ACEVersion": "0.0"
}
```

### 5. GlobalUsings.cs
Copy from `references/globalusings.md` if present. Always include `global using ACE.Database;` if you use `DatabaseManager` or `ShardDbContext`.

---

## Correct .csproj Setup

This is the battle-tested configuration. Read all the comments — several of these choices were discovered through hard debugging.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <NoWarn>0436;1073;8509</NoWarn>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <OutputPath>C:\ACE\Mods\$(AssemblyName)</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <PlatformTarget>x64</PlatformTarget>
    <SatelliteResourceLanguages>en</SatelliteResourceLanguages>
  </PropertyGroup>

  <ItemGroup>
    <!--
      CRITICAL: Do NOT add ExcludeAssets="runtime" here.
      ACE does NOT load ACE.Shared centrally — each mod must bundle its own copy.
      If ACE.Shared.dll is missing from the output folder you get:
        WARN: Missing IHarmonyMod Type YourMod.Mod from YourMod, ...

      Also do NOT use a <ProjectReference> to ACE.Shared — use this NuGet only.
    -->
    <PackageReference Include="ACEmulator.ACE.Shared" Version="1.*" />

    <!--
      Harmony IS loaded centrally by ACE, so ExcludeAssets="runtime" IS correct here.
      Do not bundle Harmony — it will conflict with the server's copy.
    -->
    <PackageReference Include="Lib.Harmony" Version="2.3.3" ExcludeAssets="runtime" />
  </ItemGroup>

  <ItemGroup>
    <!-- Private=False = don't copy to output; server already has these -->
    <Reference Include="ACE.Adapter">
      <HintPath>C:\ACE\Server\ACE.Adapter.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="ACE.Common">
      <HintPath>C:\ACE\Server\ACE.Common.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="ACE.Database">
      <HintPath>C:\ACE\Server\ACE.Database.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="ACE.DatLoader">
      <HintPath>C:\ACE\Server\ACE.DatLoader.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="ACE.Entity">
      <HintPath>C:\ACE\Server\ACE.Entity.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="ACE.Server">
      <HintPath>C:\ACE\Server\ACE.Server.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="Microsoft.EntityFrameworkCore">
      <HintPath>C:\ACE\Server\Microsoft.EntityFrameworkCore.dll</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>

  <ItemGroup>
    <None Update="Meta.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <!--
    CRITICAL: Run on ALL builds (not just Release).
    deps.json in the output folder causes "Missing IHarmonyMod Type" at runtime
    even when the DLL itself is perfectly valid.
  -->
  <Target Name="CleanupFiles" AfterTargets="PostBuildEvent">
    <ItemGroup>
      <FilesToDelete Include="$(OutDir)*.deps.json" />
      <FilesToDelete Include="$(OutDir)*runtimeconfig.json" />
      <FilesToDelete Include="$(OutDir)*.pdb" />
      <FilesToDelete Include="$(OutDir)runtimes\**\*.*" />
      <FoldersToDelete Include="$(OutDir)runtimes" />
    </ItemGroup>
    <Delete Files="@(FilesToDelete)" />
    <RemoveDir Directories="@(FoldersToDelete)" />
  </Target>

  <Target Name="ZipOutputPath" AfterTargets="PostBuildEvent" Condition="$(ConfigurationName) == Release">
    <ZipDirectory SourceDirectory="$(OutputPath)" DestinationFile="$(OutputPath)\..\$(ProjectName).zip" Overwrite="true" />
  </Target>
</Project>
```

**Optional — System.Drawing.Common:** If your mod (or a dependency) pulls in `System.Drawing.Common` and you see version conflicts with ACE binaries, pin the package: `<PackageReference Include="System.Drawing.Common" Version="8.0.0" />` so NuGet resolves a compatible patch (e.g. 8.0.8).

---

## Diagnosing "Missing IHarmonyMod Type" at Runtime

This server log warning means ACE found the DLL but couldn't instantiate the mod. Work through these in order:

| Cause | Check | Fix |
|---|---|---|
| `deps.json` in output folder | Look for `YourMod.deps.json` in `C:\ACE\Mods\YourMod\` | Ensure the `CleanupFiles` target runs on all builds, not just Release. Delete manually and restart. |
| `ACE.Shared.dll` missing from output | Check output folder for `ACE.Shared.dll` | Remove `ExcludeAssets="runtime"` from the ACE.Shared PackageReference |
| Conflicting mod loaded | Another mod registers the same command names | Remove or disable the other mod while testing |
| `ProjectReference` to ACE.Shared | Check csproj for `<ProjectReference ... ACE.Shared ...>` | Delete it; use only the NuGet PackageReference |

**The output folder of a correctly built mod should look like a known-working mod (e.g. QuestBonus).** Compare the two side-by-side — the file list should be nearly identical.

---

## Mod folder path

- **ModManager.ModPath** returns the **parent** folder `C:\ACE\Mods`, **not** `C:\ACE\Mods\YourMod`. To get your mod's folder, use `Path.Combine(ModManager.ModPath, "YourModName", ...)`.
- Alternatively, **current mod directory** (where the DLL lives) is `Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)` — e.g. `C:\ACE\Mods\Loremaster`. Use this when loading sidecar files (JSON, data) next to the DLL.

---

## Per-Player State (Thread-Safe)

When your mod needs to store settings or state for each player (e.g. "is this feature on for player X"), use `ConcurrentDictionary`. This is safe because multiple players can kill creatures simultaneously on different threads.

```csharp
// In your AutoLoot-style class (not PatchClass):
static readonly ConcurrentDictionary<Player, bool> featureEnabled = new();
static readonly ConcurrentDictionary<Player, int> numericSetting  = new();

// Read (with default if not set yet):
bool isOn  = featureEnabled.GetOrAdd(player, false);
int  value = numericSetting.GetOrAdd(player, 50);

// Write:
featureEnabled[player] = true;
numericSetting[player] = 30;
```

Use `Player` as the key within a session. For persistence across restarts, use `player.Guid.Full` (a `uint`) as the key so the check survives reconnects.

---

## Persisting Player Preferences Across Restarts

Store per-character settings in JSON files. Use a lazy-load pattern so the file is only read once per session, even if the player never types your command.

### Pattern

```csharp
// Tracks which characters have been loaded this session (keyed by GUID, not Player)
static readonly ConcurrentDictionary<uint, bool> loadedPlayers = new();

static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

// Returns the path to a player's prefs file, creating the directory if needed.
// Example: C:\ACE\Mods\YourMod\Data\PlayerData\12345678.json
static string GetPlayerDataPath(Player player)
{
    var dir = Path.Combine(PatchClass.Settings.DataPath, "PlayerData");
    Directory.CreateDirectory(dir);
    return Path.Combine(dir, $"{player.Guid.Full}.json");
}

// Call this at the top of any command or patch that reads player state.
// It is idempotent — safe to call multiple times.
static void EnsureLoaded(Player player)
{
    if (!loadedPlayers.TryAdd(player.Guid.Full, true))
        return;  // already loaded this session

    var path = GetPlayerDataPath(player);
    if (!File.Exists(path))
        return;  // new player — defaults apply

    try
    {
        var prefs = JsonSerializer.Deserialize<MyPlayerPrefs>(File.ReadAllText(path));
        if (prefs == null) return;

        featureEnabled[player] = prefs.FeatureEnabled;
        numericSetting[player] = prefs.NumericSetting;
    }
    catch (Exception ex)
    {
        ModManager.Log($"YourMod: failed to load prefs for {player.Name}: {ex.Message}", ModManager.LogLevel.Warn);
    }
}

// Call this immediately after every toggle so settings survive restarts.
static void SavePrefs(Player player)
{
    try
    {
        var prefs = new MyPlayerPrefs
        {
            FeatureEnabled = featureEnabled.GetOrAdd(player, false),
            NumericSetting = numericSetting.GetOrAdd(player, 50),
        };
        File.WriteAllText(GetPlayerDataPath(player), JsonSerializer.Serialize(prefs, JsonOptions));
    }
    catch (Exception ex)
    {
        ModManager.Log($"YourMod: failed to save prefs for {player.Name}: {ex.Message}", ModManager.LogLevel.Warn);
    }
}
```

### The prefs DTO (a plain C# class — no logic, just data)

```csharp
public class MyPlayerPrefs
{
    public bool FeatureEnabled { get; set; } = false;
    public int  NumericSetting  { get; set; } = 50;
    // Store filenames (not full paths) so saves survive if the server moves files
    public List<string> ActiveProfiles { get; set; } = new();
}
```

### Important notes
- Store file **names**, not full paths, in saved prefs — full paths break if the server moves files.
- Use `player.Guid.Full` (a `uint`) as the key in `loadedPlayers` — stable across reconnects within the same server session.

---

## Moving Items from a Corpse to a Player (Ghost Item Fix)

When auto-looting, you must **remove the item from the corpse** before adding it to the player. Calling only `TryCreateInInventoryWithNetworking` adds it to the player's visual inventory but leaves it in the corpse's internal dictionary — producing a ghost item that appears on the body indefinitely.

**Wrong (causes ghost items):**
```csharp
player.TryCreateInInventoryWithNetworking(item);
```

**Correct:**
```csharp
// Remove from corpse first — this is what makes the ghost disappear
if (!corpse.TryRemoveFromInventory(item.Guid, out var removed))
    continue;  // couldn't remove — skip this item

// Now give the removed copy to the player
player.TryCreateInInventoryWithNetworking(removed);
```

Always use `removed` (the value returned by `TryRemoveFromInventory`), not the original `item`.

---

## Detecting Rare Items

```csharp
var rareId = item.GetProperty(PropertyInt.RareId);
bool isRare = rareId != null && rareId > 0;
```

Tier detection (1–6) via WCID ranges is unreliable without researching `.dat` files. If you need tier-aware behavior, require the player to opt in rather than trying to auto-detect tier.

---

## Detecting Learnable Scrolls

```csharp
// Is this item a scroll with a spell attached that the player hasn't learned?
if (item.WeenieType == WeenieType.Scroll)
{
    var spellId = item.GetProperty(PropertyDataId.Spell);
    if (spellId != null && spellId != 0 && !player.SpellIsKnown((uint)spellId))
    {
        // This scroll is learnable by the player
    }
}
```

---

## VTClassic .utl Profile Limitations

The VTClassic loot rule format can compare one property to a **fixed constant**. It cannot express:
- Ratio comparisons (`value >= N × burden`) — implement these as C# filters instead
- Runtime player state checks (e.g. "does the player know this spell?") — also C#-only

When a loot rule genuinely requires runtime logic, skip the `.utl` file entirely and add a C# pass in your `PostGenerateTreasure` patch:

```csharp
// VendorTrash: loot items worth >= (ratio × burden)
var value   = item.Value ?? 0;
var burden  = item.EncumbranceVal ?? 1;
if (burden <= 0) burden = 1;

if (value >= threshold * burden)
{
    // loot it
}
```

---

## Getting the Player from a GenerateTreasure Postfix

`GenerateTreasure` is called on a `Creature`, not a `Player`. Use `TryGetPetOwnerOrAttacker()` to resolve the killer to a player (handles pet kills transparently):

```csharp
[HarmonyPostfix]
[HarmonyPatch(typeof(Creature), nameof(Creature.GenerateTreasure),
    new Type[] { typeof(DamageHistoryInfo), typeof(Corpse) })]
public static void PostGenerateTreasure(DamageHistoryInfo killer, Corpse corpse,
    ref Creature __instance, ref List<WorldObject> __result)
{
    if (killer.TryGetPetOwnerOrAttacker() is not Player player)
        return;  // killed by a non-player (monster, NPC, etc.) — skip

    // player is now the Player who gets the loot
}
```

---

## Querying Offline Character Quest Data

Quest data for offline characters lives in `CharacterPropertiesQuestRegistry` — NOT on `Biota`. Several seemingly obvious APIs don't exist or return the wrong type.

**What does NOT work:**
- `GetBiota()` — returns an entity model with no quest navigation properties
- `context.BiotaPropertiesQuestRegistry` — this table/property does not exist
- `DatabaseManager.Shard.BaseDatabase.GetCharacterQuests()` — method does not exist

**What DOES work:**
```csharp
// In GlobalUsings.cs:
global using ACE.Database;
global using ACE.Database.Models.Shard;

// Query offline characters' quests:
var offlineCharacterIds = PlayerManager.GetAllOffline()
    .Where(p => p.Account?.AccountId == accountId)
    .Select(p => p.Guid.Full)
    .ToHashSet();

if (offlineCharacterIds.Count > 0)
{
    using var context = new ShardDbContext();
    var offlineQuests = context.CharacterPropertiesQuestRegistry
        .Where(q => offlineCharacterIds.Contains(q.CharacterId) && q.NumTimesCompleted > 0)
        .Select(q => q.QuestName)
        .ToList();
    foreach (var name in offlineQuests)
        solvedQuestNames.Add(name);
}
```

**Key schema facts:**
- Table: `CharacterPropertiesQuestRegistry`
- Columns: `CharacterId` (uint), `QuestName` (string), `NumTimesCompleted` (int)
- Online players: use `player.QuestManager.Quests` directly — no DB query needed
- Account-wide quest union: query all character IDs for that `AccountId`, merge quest name sets

---

## Harmony Patch Patterns

### Key Rules
- The patch class must have `[HarmonyPatch]` on it
- Each patch method must be `public static`
- `__instance` = the object the method was called on
- `ref` lets you modify values in Prefix; `__result` is the return value in Postfix
- `__state` passes data from a Prefix to its matching Postfix

### Postfix — Run code AFTER (most common)
```csharp
[HarmonyPostfix]
[HarmonyPatch(typeof(Player), nameof(Player.PlayerEnterWorld))]
public static void PostPlayerEnterWorld(ref Player __instance)
{
    __instance.SendMessage($"Welcome back, {__instance.Name}!");
}
```

### Prefix — Run code BEFORE, optionally skip original
```csharp
// return true  → run original after this
// return false → skip original (set __result yourself)
[HarmonyPrefix]
[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.GetWeaponCriticalChance),
    new Type[] { typeof(WorldObject), typeof(Creature), typeof(CreatureSkill), typeof(Creature) })]
public static bool PreGetWeaponCritChance(ref float __result, Creature target)
{
    if (target is Player) return true;
    __result = PatchClass.Settings.CritChance;
    return false;
}
```

### Prefix + Postfix sharing state
```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(QuestManager), nameof(QuestManager.Update), new Type[] { typeof(string) })]
public static void PreUpdate(string questFormat, QuestManager __instance, ref int __state)
{
    __state = __instance.GetCurrentSolves(questFormat);
}

[HarmonyPostfix]
[HarmonyPatch(typeof(QuestManager), nameof(QuestManager.Update), new Type[] { typeof(string) })]
public static void PostUpdate(string questFormat, QuestManager __instance, int __state)
{
    var newSolves = __instance.GetCurrentSolves(questFormat);
    if (__state == 0 && newSolves > 0)
        ModManager.Log($"First solve: {questFormat}");
}
```

### Targeting overloaded methods
```csharp
[HarmonyPatch(typeof(Player), nameof(Player.GrantXP),
    new Type[] { typeof(long), typeof(XpType), typeof(ShareType) })]
```

### Modifying a parameter value
```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(Player), nameof(Player.GrantXP),
    new Type[] { typeof(long), typeof(XpType), typeof(ShareType) })]
public static void PreGrantXP(ref long amount, Player __instance)
{
    amount = (long)(amount * __instance.QuestBonus());
}
```

---

## Command Pattern

```csharp
[CommandHandler("mycommand", AccessLevel.Player, CommandHandlerFlag.RequiresWorld,
    "Description shown in /help", "Usage: /mycommand [args]")]
public static void HandleMyCommand(Session session, params string[] parameters)
{
    var player = session.Player;
    player.SendMessage("Hello!");
    if (parameters.Length > 0 && int.TryParse(parameters[0], out int n))
        player.SendMessage($"Got: {n}");
}
```

**AccessLevel**: `Player` → `Advocate` → `Sentinel` → `Envoy` → `Developer` → `Admin`  
**CommandHandlerFlag**: `None` (works from server console too), `RequiresWorld` (must be in-game)

**Warning:** Command names are global across all loaded mods. Two mods registering `/qb` will conflict — remove one or rename your commands.

**Safety:** Prefer null-safe player resolution: `if (session?.Player is not Player p) return;` then use `p`. This guards against race-y server state when commands run during load/unload or from console.

---

## FakeProperty System

```csharp
player.SetProperty(FakeFloat.QuestBonus, 1.25);
double? bonus = player.GetProperty(FakeFloat.QuestBonus);
player.RemoveProperty(FakeFloat.QuestBonus);

// Custom properties for your own mod — use IDs above 40000:
var myProp = (PropertyFloat)40100;
player.SetProperty(myProp, 99.5);
```

Pre-defined values in ACE.Shared (don't redefine):
- `FakeBool`: `Ironman`, `Hardcore`, `Quarantined`, `Permanent`, `GrowthItem`
- `FakeInt`: `HardcoreLives`, `GrowthTier`, `ComboKill`
- `FakeFloat`: `QuestBonus`, `TimestampLastKill`, `ItemXpBoost`
- `FakeString`: `IronmanPlan`

See `references/fakeprops.md` for the full list.

---

## Particle Effects and Visual Auras on NPCs

### Critical: DefaultScriptId does NOT render on animated humanoid creatures

`WorldObject.DefaultScriptId` (maps to `PropertyDataId.PhysicsScript` / DID type 10) controls a looping particle script baked into the `CreateObject` physics packet. It works well on **static objects** (portals, lifestones, forges) but **does not visually render on animated humanoid NPC/creature models**.

**Why:** The AC client resolves script particle emitter nodes against the object's `SetupId`. Humanoid NPC setups lack the emitter node types that portal/restriction scripts target, so the script plays at the invisible physics capsule rather than the visible model. Database confirms: `DefaultScriptId` is only set in weenie data for `WeenieType.Portal` (31) and `WeenieType.LifeStone` (22) — never on `WeenieType.Creature` (10).

**Also does not work on humanoid NPCs:**
- `GameMessageScript` broadcast (one-shot) with portal/restriction scripts — `PlayScript.RestrictionEffectGold` (154), `.RestrictionEffectBlue` (152), `.RestrictionEffectGreen` (153), `PlayScript.ShieldUpGreen` (49), `PlayScript.SpecialStateGreen` (133)
- Setting `DefaultScriptId` on equipped items in `TrinketOne` slot — no physics attachment point
- Setting `DefaultScriptId` on equipped items in `NeckWear` slot — `TryEquipObject` returns True but script doesn't render

**What DOES work:** `PlayScript.RestrictionEffect*` values loop correctly as `DefaultScriptId` on **static WorldObjects** (portals, lifestones). Scripts are baked into the `CreateObject` packet via `PhysicsDescriptionFlag.DefaultScript` (0x2000).

### Persistent Aura on a Quest-Giver NPC: Beacon Pattern

Since you can't render a looping script directly on a humanoid NPC, spawn an ephemeral ethereal static object (a "beacon") at the NPC's location. Use **WCID 36577** (`ace36577-aura`) — a purpose-built invisible, ethereal WorldObject with an aura setup file. Static objects DO render `DefaultScriptId`.

The beacon must be spawned via `ActionChain` with a delay because `Creature.GenerateWieldList()` (where you detect quest givers) fires before `CurrentLandblock` is set on the creature.

```csharp
// Detect quest givers in GenerateWieldList postfix:
const uint AuraBeaconWcid = 36577; // ace36577-aura — invisible, ethereal, aura setup

[HarmonyPostfix]
[HarmonyPatch(typeof(Creature), nameof(Creature.GenerateWieldList))]
public static void PostGenerateWieldList(Creature __instance)
{
    if (__instance.WeenieType != WeenieType.Creature) return;
    if (!IsQuestGiver(__instance)) return;

    // CurrentLandblock is null here — defer until creature is on the landblock
    var chain = new ActionChain();
    chain.AddDelaySeconds(1.0);
    chain.AddAction(__instance, () =>
    {
        if (__instance.CurrentLandblock == null) return;

        var beacon = WorldObjectFactory.CreateNewWorldObject(AuraBeaconWcid);
        if (beacon == null) return;

        beacon.DefaultScriptId        = Cfg.ScriptId;   // e.g. 154 = RestrictionEffectGold
        beacon.DefaultScriptIntensity = Cfg.ScriptIntensity;
        beacon.Ethereal               = true;            // no collision, no interaction
        beacon.IgnoreCollisions       = true;
        beacon.Location               = new Position(__instance.Location);

        LandblockManager.AddObject(beacon);
    });
    chain.EnqueueChain();
}

// IsQuestGiver: use weenie cache, NOT creature.Biota (empty for dynamic spawns)
static bool IsQuestGiver(Creature creature)
{
    var weenie = DatabaseManager.World.GetCachedWeenie(creature.WeenieClassId);
    if (weenie?.PropertiesEmote == null) return false;
    return weenie.PropertiesEmote.Any(e =>
        e.PropertiesEmoteAction?.Any(a =>
            a.Type == (uint)EmoteType.StampQuest ||
            a.Type == (uint)EmoteType.InqQuest) == true);
}
```

**Key notes:**
- WCID 36577 has `Ethereal=true` and `Stuck=true` pre-set in its weenie — players cannot interact with or move it
- The beacon is ephemeral (not saved to DB) — it disappears on server restart and re-spawns when the NPC re-spawns via `GenerateWieldList`
- Quest givers are typically stationary and immortal, so the beacon not following movement is acceptable
- `EmoteType.StampQuest = 22` (NPC gives a quest), `EmoteType.InqQuest = 21` (NPC checks quest status) — check the weenie cache, not `creature.Biota.PropertiesEmote` (which is empty for dynamically spawned creatures)

---

## Common ACE Operations

### Grant XP
```csharp
player.GrantXP(1_000_000, XpType.Quest, ShareType.None);
```

### Create and give an item by WCID
```csharp
var item = WorldObjectFactory.CreateNewWorldObject(273); // 273 = Pyreal
if (item != null)
    player.TryAddToInventory(item);
```

### Server broadcast
```csharp
foreach (var p in PlayerManager.GetAllOnline())
    p.SendMessage("[Server] Announcement!", ChatMessageType.Broadcast);
```

### Delayed action
```csharp
var chain = new ActionChain();
chain.AddDelaySeconds(2.0);
chain.AddAction(player, () => player.SendMessage("2 seconds later!"));
chain.EnqueueChain();
```

### Get/Set/Remove properties
```csharp
int? armor = wo.GetProperty(PropertyInt.ArmorLevel);
wo.SetProperty(PropertyInt.ArmorLevel, 500);
wo.RemoveProperty(PropertyBool.Attackable);
```

### Logging
```csharp
ModManager.Log("Info");
ModManager.Log("Warning!", ModManager.LogLevel.Warn);
ModManager.Log("Error!", ModManager.LogLevel.Error);
```

---

## ACE.Shared Helper Extensions

### Player
```csharp
player.SendMessage("Text");
player.TeleportThreadSafe(position);
player.QuarantinePlayer("0xA9B40019 [coords]");
player.PermaDeath();
player.TryAdvanceSkill(Skill.WarMagic);
player.PlaySound(Sound.ReceiveItem);
```

### Creature
```csharp
creature.PercentHealth();
creature.TryDamageDirect(50f, out uint taken, DamageType.Fire);
creature.ScaleProperty(PropertyInt.ArmorLevel, 1.5f);
creature.SetBonus(Skill.WarMagic, 50);
creature.SetBonus(PropertyAttribute.Strength, 10);
```

### WorldObject / Landblock
```csharp
wo.PlayAnimation(PlayScript.AttackSlash1);
wo.MoveInFrontOf(player);
landblock.GetPlayers();
landblock.GetCreatures();
landblock.RespawnCreatures();
```

---

## Category Patches (Opt-in Features)

```csharp
[HarmonyPatchCategory("XpBoost")]
[HarmonyPatch]
public class XpBoostPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.GrantXP),
        new Type[] { typeof(long), typeof(XpType), typeof(ShareType) })]
    public static void PreGrantXP(ref long amount) { ... }
}

public override async Task OnWorldOpen()
{
    Settings = SettingsContainer.Settings;
    if (Settings.EnableXpBoost)
        Mod.Harmony.PatchCategory("XpBoost");
}
```

---

## Workflow Checklist

1. **Create source folder separately** from `C:\ACE\Mods\` — never mix source and output.
2. **Start from the correct `.csproj`** — copy the template above verbatim, then adjust paths and assembly name.
3. **Verify output folder** matches a working mod (like QuestBonus) — same DLLs should be present.
4. **Find the ACE method to hook** — search ACE GitHub. `Player.cs` and `Creature.cs` are most common.
5. **Choose Prefix or Postfix** — Postfix to add behavior, Prefix to intercept/modify inputs.
6. **Put configurable values in Settings** — anything a server admin might tune.
7. **Test with hot-reload** — `/mod f [modname]` in-game after rebuild.
8. **If you get `Missing IHarmonyMod Type`** — check for `deps.json`, check `ACE.Shared.dll` is present, check for command name conflicts with other loaded mods.

---

## Common Mistakes

| Problem | Fix |
|---|---|
| `Missing IHarmonyMod Type` | Delete `deps.json` from output; ensure `ACE.Shared.dll` is bundled; remove conflicting mods |
| Patch not firing | Verify `[HarmonyPatch]` is on the class AND the method name/types exactly match |
| `BiotaPropertiesQuestRegistry` not found | Wrong table — quest data is in `CharacterPropertiesQuestRegistry` |
| `GetCharacterQuests()` not found | This method doesn't exist — use `ShardDbContext` directly |
| `DatabaseManager` not resolving | Add `global using ACE.Database;` to `GlobalUsings.cs` |
| Can't access private ACE member | `AccessTools.Field(typeof(ClassName), "fieldName").GetValue(instance)` |
| Settings not updating on hot-reload | Assign `Settings = SettingsContainer.Settings` in `OnWorldOpen()` **and** in constructor or `Start()` |
| Wrong overload patched | Add `new Type[] { typeof(X), typeof(Y) }` as third arg to `[HarmonyPatch]` |
| Two mods, same command name | Commands are global — remove or disable the conflicting mod |
| `ACE.Shared.dll` version conflict | Don't use `ExcludeAssets="runtime"` on ACE.Shared; don't use ProjectReference |
| `PatchClass.Settings` is null on first command | `OnWorldOpen` doesn't run on hot-reload. Use explicit ctor with `Settings ??= SettingsContainer.Settings` or override `Start()` and set Settings there — do NOT rely on OnWorldOpen only |
| Ghost items left on corpse after auto-loot | Call `corpse.TryRemoveFromInventory(item.Guid, out var removed)` first, then `player.TryCreateInInventoryWithNetworking(removed)`. Never pass the original `item` reference |
| `ModManager.ModPath` points to wrong folder | `ModManager.ModPath` returns `C:\ACE\Mods` (the parent), not your mod's folder. Append your mod name: `Path.Combine(ModManager.ModPath, "YourModName", ...)` or use `Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)` |
| Loot rule needs ratio/runtime check | VTClassic `.utl` can only compare one property to a fixed constant — implement ratio or player-state checks as C# filters in your patch, not in a `.utl` file |
| Can't get Player from GenerateTreasure | Use `killer.TryGetPetOwnerOrAttacker() is not Player player` — handles both direct kills and pet kills |

---

## Reference Files

- `references/mod-examples.md` — real annotated source from sample mods
- `references/globalusings.md` — GlobalUsings.cs to copy into new mods
- `references/fakeprops.md` — all FakeProperty values from ACE.Shared
- `references/ace-namespaces.md` — ACE class/method quick reference

## Key Links
- ACE.BaseMod: https://github.com/aquafir/ACE.BaseMod/wiki/Getting-Started
- Harmony docs: https://harmony.pardeike.net/articles/intro.html
- ACE wiki: https://github.com/ACEmulator/ACE/wiki

---

## Patterns from ACEmulator-Mods repo

When working in the **ACEmulator-Mods** repository:

- **Build list:** Discover all `.csproj` files under the repo (e.g. AethericWeaver, LeyLineLedger, Numbersmith, Loremaster, Overtinked, Swarmed, QOL, CHANGERaise, CHANGEExpansion, CHANGEEasyEnlightenment). Use `/ace-build` or build each mod in its own directory with `dotnet build`.
- **Settings + hot-reload:** Prefer loading settings in both **Start()** and **OnWorldOpen()** so that after a hot-reload (when OnWorldOpen never runs), Settings is still set. Use `Settings = SettingsContainer.Settings ?? new Settings();` in both.
- **Nullable and safety:** Use nullable reference types and guard command handlers with `if (session?.Player is not Player p) return;` so code is safe during load/unload or console-invoked commands.
- **System.Drawing.Common:** If a mod or dependency needs it, pin `System.Drawing.Common` (e.g. `Version="8.0.0"`) to avoid conflicts with ACE server binaries.

### Quest item tinkering & leveling (Overtinked)

- **Quest item detection:** Instead of hard-coding WCIDs, centralize detection in a helper like `IsQuestItem(WorldObject item)` that checks:
  - `PropertyBool.Quest == true` when available, and
  - Fallback heuristics such as `Attuned == true && Bonded == true && Value == 0` for reward-style items.
- **Hook point:** Use a **Postfix** on `WorldObjectFactory.CreateNewWorldObject(uint weenieClassId)` to run on all new items, then immediately filter to quest items and tag them so initialization only happens once.
- **Workmanship seeding:** Use `Traverse.Create(wo).Property("Workmanship")` helpers (e.g. `GetWorkmanship` / `SetWorkmanship`) to:
  - Leave any existing workmanship unchanged, and
  - Otherwise roll a random value in a configurable `[QuestItemWorkmanshipMin, QuestItemWorkmanshipMax]` band so quest rewards are tinkering-eligible.
- **Initial effects:** On creation, apply **one** initial tinkering-style effect for quest items:
  - Prefer element-appropriate rending imbues (e.g. `ImbuedEffectType.FireRending` for fire weapons),
  - Otherwise roll a slayer (`PropertyInt.SlayerCreatureType` + `PropertyFloat.SlayerDamageBonus`) or a small numeric boost via a salvage-style helper (e.g. `Damage`, `ArmorLevel`, `ArmorModVsX`).
  - Guard with a lightweight custom property (IDs > 40000) so the same quest item instance is never re-rolled.
- **Quest item leveling:** Reuse ACE’s built-in item XP:
  - On creation, set `ItemXpStyle`, `ItemBaseXp`, `ItemMaxLevel`, `ItemTotalXp = 0` and tag with a custom `FakeBool` (e.g. `QuestGrowthItem`) plus an optional category `FakeInt`.
  - Add a Prefix on `Player.OnItemLevelUp(WorldObject item, int prevItemLevel)` that:
    - Returns unless the quest-growth tag is present and `FakeBool.GrowthItem` (from CHANGEExpansion) is **not** set, and
    - For every level between `prevItemLevel + 1` and `item.ItemLevel`, rolls and applies one additional effect (imbue, slayer, or salvage-like), always skipping effects that would have no impact (e.g. pierce rending on a pure fire weapon).

### Loot-generated item leveling & pre-imbues (CHANGEExpansion)

- **Mutator-based hook:** For loot-generated items, prefer CHANGEExpansion’s `Mutator` pipeline over patching ACE core directly:
  - Implement `LootGrowthItem : Mutator` and wire it via `Mutation.LootGrowthItem` in `Enums/Mutation.cs` and `Settings.Mutators`.
  - Run it on `MutationEvent.Containers` so it sees loot from both creature corpses and generators.
- **Eligibility rules:** In `LootGrowthItem.TryMutateLoot`:
  - Skip items that already have levels (`item.HasItemLevel`) or are tagged `FakeBool.GrowthItem` (standard Growth items),
  - Restrict to a configurable set of equippable `WeenieType`s (weapons, armor, jewelry, cloaks),
  - Optionally require zero existing imbues when rolling pre-imbues.
- **Loot item leveling:** When enabled:
  - Compute XP cost with a tier-scaled curve (e.g. `LootItemXpBase * Math.Pow(LootItemXpScale, profile.Tier - 1)`),
  - Set `ItemXpStyle`, `ItemTotalXp`, `ItemMaxLevel` in a `[LootItemMaxLevelMin, LootItemMaxLevelMax]` range, and `ItemBaseXp`,
  - Tag items with new `FakeBool` / `FakeInt` values (IDs > 40000) such as `LootGrowthItem`, `LootGrowthTier`, and `LootOriginalItemType`.
- **OnItemLevelUp for loot:** Add a dedicated Prefix (e.g. `ItemLevelUpLootGrowth`) on `Player.OnItemLevelUp` that:
  - Returns unless `FakeBool.LootGrowthItem == true` and `FakeBool.GrowthItem != true`,
  - For each new level, applies a small but meaningful gain:
    - Prefer adding an element-appropriate rending imbue if not already present,
    - Otherwise apply a small `Damage` bump to weapons/casters or `ArmorLevel` bump to armor/clothing,
    - Optionally also roll slayer mods or salvage-style armor modifiers if you need more variety.
- **Low-chance pre-imbues:** Inside `LootGrowthItem` (or a separate mutator), implement a configurable `LootItemPreImbueChance`:
  - Roll once per eligible item with `Random.Shared.NextDouble() <= LootItemPreImbueChance`,
  - For clear elemental weapons, set `item.ImbuedEffect |= ImbuedEffectType.*Rending` matching `DamageType`,
  - Ensure you never apply more than one pre-imbue per item by checking `item.ImbuedEffect` (and any custom-imbue tags) first.
- **Safety and balance:** Always:
  - Respect existing Growth items and rares by checking `HasItemLevel` / `FakeBool.GrowthItem` before assigning new systems,
  - Keep default loot XP and per-level gains **below or comparable** to rares so these systems feel rewarding but not overpowering,
  - Expose all major knobs (feature toggles, XP curve, level ranges, pre-imbue chance) in `Settings` so server admins can tune behavior without code changes.
