namespace Aptitude;

using System.Collections.Concurrent;
using System.Text.Json;

[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    static readonly ConcurrentDictionary<uint, bool> EnabledByGuid = new();
    static readonly ConcurrentDictionary<uint, bool> PermanentlyOptedOutByGuid = new();
    static readonly ConcurrentDictionary<uint, bool> LoadedPlayers = new();
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public override void Start()
    {
        base.Start();
        Settings = SettingsContainer.Settings ?? new Settings();
    }

    public override async Task OnWorldOpen()
    {
        Settings = SettingsContainer.Settings ?? new Settings();
        ModC.Harmony.PatchCategory("Aptitude");
    }

    static string GetPlayerDataPath(Player player)
    {
        var modDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
        var dir = Path.Combine(modDir, "Data", "PlayerData");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"{player.Guid.Full}.json");
    }

    static void EnsureLoaded(Player player)
    {
        if (player?.Guid == null) return;
        if (!LoadedPlayers.TryAdd(player.Guid.Full, true))
            return;

        var path = GetPlayerDataPath(player);
        if (!File.Exists(path))
            return;

        try
        {
            var json = File.ReadAllText(path);
            var prefs = JsonSerializer.Deserialize<AptitudePlayerPrefs>(json);
            if (prefs == null) return;

            EnabledByGuid[player.Guid.Full] = prefs.Enabled;
            PermanentlyOptedOutByGuid[player.Guid.Full] = prefs.PermanentlyOptedOut;
        }
        catch (Exception ex)
        {
            ModManager.Log($"Aptitude: failed to load prefs for {player.Name}: {ex.Message}", ModManager.LogLevel.Warn);
        }
    }

    static void SavePrefs(Player player)
    {
        if (player?.Guid == null) return;
        try
        {
            var prefs = new AptitudePlayerPrefs
            {
                Enabled = EnabledByGuid.GetOrAdd(player.Guid.Full, false),
                PermanentlyOptedOut = PermanentlyOptedOutByGuid.GetOrAdd(player.Guid.Full, false),
            };
            var path = GetPlayerDataPath(player);
            File.WriteAllText(path, JsonSerializer.Serialize(prefs, JsonOptions));
        }
        catch (Exception ex)
        {
            ModManager.Log($"Aptitude: failed to save prefs for {player.Name}: {ex.Message}", ModManager.LogLevel.Warn);
        }
    }

    // Cross-mod: other mods (e.g. AutoLoot) can call this to gate Aptitude-only features.
    public static bool IsAptitudeEnabled(Player? player)
    {
        if (player?.Guid == null) return false;
        EnsureLoaded(player);
        return EnabledByGuid.GetOrAdd(player.Guid.Full, false);
    }

    [CommandHandler("aptitude", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, -1,
        "Toggle Aptitude mode (skills only via usage; XP for attributes/vitals only). Activate at level 1 only; disabling is permanent.",
        "Usage: /aptitude [on | off]")]
    public static void HandleAptitude(Session session, params string[] parameters)
    {
        if (session?.Player is not Player player)
            return;

        EnsureLoaded(player);
        var guid = player.Guid.Full;
        var enabled = EnabledByGuid.GetOrAdd(guid, false);
        var permanentlyOptedOut = PermanentlyOptedOutByGuid.GetOrAdd(guid, false);

        if (parameters.Length == 0)
        {
            if (permanentlyOptedOut)
                player.SendMessage("Aptitude is permanently disabled on this character (you previously turned it off).");
            else if (enabled)
                player.SendMessage("Aptitude is ON. Skills can only be raised through usage; spend XP on attributes and vitals only. Use /aptitude off to disable (permanent).");
            else
                player.SendMessage("Aptitude is OFF. Use /aptitude on to enable (only at level 1; once off, you cannot turn it back on).");
            return;
        }

        var arg = parameters[0].Trim().ToLowerInvariant();

        if (arg == "off")
        {
            if (!enabled)
            {
                player.SendMessage("Aptitude is already off.");
                return;
            }
            EnabledByGuid[guid] = false;
            PermanentlyOptedOutByGuid[guid] = true;
            SavePrefs(player);
            player.SendMessage("Aptitude has been disabled. You cannot reactivate it on this character.");
            return;
        }

        if (arg == "on")
        {
            if (permanentlyOptedOut)
            {
                player.SendMessage("You have previously disabled Aptitude on this character and cannot reactivate it.");
                return;
            }
            if (player.Level != 1)
            {
                player.SendMessage("Aptitude can only be activated on a new character (level 1).");
                return;
            }
            EnabledByGuid[guid] = true;
            SavePrefs(player);
            player.SendMessage("Aptitude is now ON. Skills can only be raised through usage; use your XP on attributes and vitals.");
            return;
        }

        player.SendMessage("Usage: /aptitude [on | off]");
    }
}

[HarmonyPatchCategory("Aptitude")]
public static class AptitudePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.HandleActionRaiseSkill), new Type[] { typeof(Skill), typeof(uint) })]
    public static bool PreHandleActionRaiseSkill(Skill skill, uint amount, Player __instance, ref bool __result)
    {
        if (!PatchClass.IsAptitudeEnabled(__instance))
            return true;

        __result = false;
        __instance.SendMessage("In Aptitude mode, skills can only be raised through usage. Use your skills in combat, crafting, and exploration.", ChatMessageType.Broadcast);
        return false;
    }
}
