namespace Swiftmend;

[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    internal static Settings? CurrentSettings;

    static readonly ConcurrentDictionary<uint, SwiftmendHotState> ActiveHots = new();
    static readonly ConcurrentDictionary<uint, bool> TickRunning = new();

    public override void Start()
    {
        base.Start();
        Settings = SettingsContainer.Settings ?? new Settings();
        CurrentSettings = Settings;
    }

    public override async Task OnWorldOpen()
    {
        Settings = SettingsContainer.Settings ?? new Settings();
        CurrentSettings = Settings;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldObject), nameof(WorldObject.HandleActionUseOnTarget),
        new Type[] { typeof(Player), typeof(WorldObject) })]
    public static bool PreHandleActionUseOnTarget(WorldObject __instance, Player player, ref WorldObject target)
    {
        Settings? s = CurrentSettings;
        if (s == null || !s.Enabled)
            return true;

        if (player == null)
            return true;

        if (__instance.WeenieType != WeenieType.Healer)
            return true;

        ApplySwiftmendHot(player, __instance, s);

        target = player;

        RecipeManager.UseObjectOnTarget(player, __instance, player);

        return false;
    }

    static void ApplySwiftmendHot(Player player, WorldObject kit, Settings s)
    {
        try
        {
            if (player.IsDead)
                return;

            var healingSkill = player.GetCreatureSkill(Skill.Healing);
            int skillValue = (int)healingSkill.Current;

            if (skillValue <= 0)
                return;

            double multiplier = s.BaseSkillPercentPerTick;

            if (healingSkill.AdvancementClass == SkillAdvancementClass.Specialized)
                multiplier *= s.SpecializedMultiplier;

            double perTick = skillValue * multiplier;
            if (perTick <= 0.0)
                return;

            float healthPerTick = 0;
            float staminaPerTick = 0;
            float manaPerTick = 0;

            string name = kit.Name?.ToLowerInvariant() ?? string.Empty;

            if (s.EnableHealthKits && (name.Contains("healing") || name.Contains("health") || (!name.Contains("stamina") && !name.Contains("mana"))))
                healthPerTick = (float)perTick;

            if (s.EnableStaminaKits && (name.Contains("stamina") || name.Contains("stam")))
                staminaPerTick = (float)perTick;

            if (s.EnableManaKits && name.Contains("mana"))
                manaPerTick = (float)perTick;

            if (healthPerTick <= 0 && staminaPerTick <= 0 && manaPerTick <= 0)
                return;

            int totalTicks = Math.Max(1, (int)Math.Round(s.HotDurationSeconds / s.HotTickSeconds));

            uint id = player.Guid.Full;

            ActiveHots.AddOrUpdate(id,
                _ => new SwiftmendHotState
                {
                    HealthPerTick = healthPerTick,
                    StaminaPerTick = staminaPerTick,
                    ManaPerTick = manaPerTick,
                    RemainingTicks = totalTicks
                },
                (_, state) =>
                {
                    state.HealthPerTick = healthPerTick;
                    state.StaminaPerTick = staminaPerTick;
                    state.ManaPerTick = manaPerTick;
                    state.RemainingTicks = totalTicks;
                    return state;
                });

            if (!TickRunning.ContainsKey(id))
                StartTickChain(player, s);
        }
        catch (Exception ex)
        {
            ModManager.Log($"Swiftmend ApplySwiftmendHot error: {ex}", ModManager.LogLevel.Error);
        }
    }

    static void StartTickChain(Player player, Settings s)
    {
        if (player == null)
            return;

        uint id = player.Guid.Full;

        if (!ActiveHots.ContainsKey(id))
            return;

        TickRunning[id] = true;

        var chain = new ActionChain();
        chain.AddDelaySeconds((float)s.HotTickSeconds);
        chain.AddAction(player, () => TickOnce(player, s));
        chain.EnqueueChain();
    }

    static void TickOnce(Player player, Settings s)
    {
        if (player == null)
            return;

        uint id = player.Guid.Full;

        if (!ActiveHots.TryGetValue(id, out SwiftmendHotState state))
        {
            TickRunning.TryRemove(id, out _);
            return;
        }

        if (player.IsDead)
        {
            ActiveHots.TryRemove(id, out _);
            TickRunning.TryRemove(id, out _);
            return;
        }

        if (state.HealthPerTick > 0)
            player.UpdateVitalDelta(player.Health, (int)Math.Round(state.HealthPerTick));

        if (state.StaminaPerTick > 0)
            player.UpdateVitalDelta(player.Stamina, (int)Math.Round(state.StaminaPerTick));

        if (state.ManaPerTick > 0)
            player.UpdateVitalDelta(player.Mana, (int)Math.Round(state.ManaPerTick));

        state.RemainingTicks--;

        if (state.RemainingTicks <= 0)
        {
            ActiveHots.TryRemove(id, out _);
            TickRunning.TryRemove(id, out _);
            return;
        }

        var chain = new ActionChain();
        chain.AddDelaySeconds((float)s.HotTickSeconds);
        chain.AddAction(player, () => TickOnce(player, s));
        chain.EnqueueChain();
    }
}


