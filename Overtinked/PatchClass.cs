using ACE.Database.Models.World;

namespace Overtinked;

// Overtinked Harmony patches: extends RecipeManager.TinkeringDifficulty from settings, overrides VerifyRequirements
// to use Overtinked limits (MaxTries, MaxImbueEffects), and applies imbue by dataId in TryMutate.
[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    // Static reference for use in static patch methods (e.g. PreTryMutate).
    internal static Settings? CurrentSettings;

    public override async Task OnWorldOpen()
    {
        Settings = SettingsContainer.Settings;
        CurrentSettings = Settings;

        SalvageEffectApplier.BuildLookup(Settings);
        ModifyTinkering();

        if (Settings.EnableRecipeManagerPatch)
            ModC.Harmony.PatchCategory(Settings.RecipeManagerCategory);
    }

    // Snapshot of default TinkeringDifficulty; can be used to restore on shutdown.
    static readonly List<float> difficulty = RecipeManager.TinkeringDifficulty.ToList();

    private void RestoreTinkering()
    {
        RecipeManager.TinkeringDifficulty = difficulty.ToList();
    }

    // Appends extra difficulty steps so RecipeManager has one entry per allowed tinker (up to MaxTries), scaled by Settings.Scale.
    private void ModifyTinkering()
    {
        var diffs = RecipeManager.TinkeringDifficulty.Count;
        var last = RecipeManager.TinkeringDifficulty.Last();
        var toAdd = Math.Max(0, Settings.MaxTries - diffs);
        var steps = Enumerable.Range(diffs, toAdd)
            .Select((x, i) => last + (i + 1) * Settings.Scale);

        RecipeManager.TinkeringDifficulty.AddRange(steps);
        ModManager.Log($"Overtinked diffs ({RecipeManager.TinkeringDifficulty.Count}): {string.Join(", ", RecipeManager.TinkeringDifficulty)}");
    }

    // Prefix: replaces RecipeManager.VerifyRequirements. Uses Settings.MaxTries for NumTimesTinkered and Settings.MaxImbueEffects for ImbuedEffect; delegates other requirement types to RecipeManager.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RecipeManager), nameof(RecipeManager.VerifyRequirements), new Type[] { typeof(Recipe), typeof(Player), typeof(WorldObject), typeof(RequirementType) })]
    public static bool PreVerifyRequirements(Recipe recipe, Player player, WorldObject obj, RequirementType reqType, ref RecipeManager __instance, ref bool __result)
    {
        #region Setup
        __result = true;

        var boolReqs = recipe.RecipeRequirementsBool.Where(i => i.Index == (int)reqType).ToList();
        var intReqs = recipe.RecipeRequirementsInt.Where(i => i.Index == (int)reqType).ToList();
        var floatReqs = recipe.RecipeRequirementsFloat.Where(i => i.Index == (int)reqType).ToList();
        var strReqs = recipe.RecipeRequirementsString.Where(i => i.Index == (int)reqType).ToList();
        var iidReqs = recipe.RecipeRequirementsIID.Where(i => i.Index == (int)reqType).ToList();
        var didReqs = recipe.RecipeRequirementsDID.Where(i => i.Index == (int)reqType).ToList();

        var totalReqs = boolReqs.Count + intReqs.Count + floatReqs.Count + strReqs.Count + iidReqs.Count + didReqs.Count;

        if (RecipeManager.Debug && totalReqs > 0)
            Console.WriteLine($"{reqType} Requirements: {totalReqs}");
        #endregion

        foreach (var requirement in intReqs)
        {
            int? value = obj.GetProperty((PropertyInt)requirement.Stat);
            double? normalized = value != null ? Convert.ToDouble(value.Value) : null;

            var propInt = (PropertyInt)requirement.Stat;
            var comp = (CompareType)requirement.Enum;

            if (propInt == PropertyInt.ImbuedEffect)
            {
                value = obj.GetProperty((PropertyInt)requirement.Stat) ?? 0;
            }

            __result = propInt switch
            {
                PropertyInt.NumTimesTinkered => comp.Compare(Settings.MaxTries - 1, value ?? 0, player, requirement.Message) ? __result : false,
                PropertyInt.ImbuedEffect => comp.Compare(Settings.MaxImbueEffects - 1, BitOperations.PopCount((uint)(value ?? 0)), player, requirement.Message) ? __result : false,
                _ => RecipeManager.VerifyRequirement(player, (CompareType)requirement.Enum, normalized, Convert.ToDouble(requirement.Value), requirement.Message),
            };
            continue;
        }

        #region Unmodified Requirement Checks
        foreach (var requirement in boolReqs)
        {
            bool? value = obj.GetProperty((PropertyBool)requirement.Stat);
            double? normalized = value != null ? Convert.ToDouble(value.Value) : null;

            if (RecipeManager.Debug)
                Console.WriteLine($"PropertyBool.{(PropertyBool)requirement.Stat} {(CompareType)requirement.Enum} {requirement.Value}, current: {value}");

            if (!RecipeManager.VerifyRequirement(player, (CompareType)requirement.Enum, normalized, Convert.ToDouble(requirement.Value), requirement.Message))
                __result = false;
        }

        foreach (var requirement in floatReqs)
        {
            double? value = obj.GetProperty((PropertyFloat)requirement.Stat);

            if (RecipeManager.Debug)
                Console.WriteLine($"PropertyFloat.{(PropertyFloat)requirement.Stat} {(CompareType)requirement.Enum} {requirement.Value}, current: {value}");

            if (!RecipeManager.VerifyRequirement(player, (CompareType)requirement.Enum, value, requirement.Value, requirement.Message))
                __result = false;
        }

        foreach (var requirement in strReqs)
        {
            string value = obj.GetProperty((PropertyString)requirement.Stat);

            if (RecipeManager.Debug)
                Console.WriteLine($"PropertyString.{(PropertyString)requirement.Stat} {(CompareType)requirement.Enum} {requirement.Value}, current: {value}");

            if (!RecipeManager.VerifyRequirement(player, (CompareType)requirement.Enum, value, requirement.Value, requirement.Message))
                __result = false;
        }

        foreach (var requirement in iidReqs)
        {
            var value = obj.GetProperty((PropertyInstanceId)requirement.Stat);
            double? normalized = value != null ? Convert.ToDouble(value.Value) : null;

            if (RecipeManager.Debug)
                Console.WriteLine($"PropertyInstanceId.{(PropertyInstanceId)requirement.Stat} {(CompareType)requirement.Enum} {requirement.Value}, current: {value}");

            if (!RecipeManager.VerifyRequirement(player, (CompareType)requirement.Enum, normalized, Convert.ToDouble(requirement.Value), requirement.Message))
                __result = false;
        }

        foreach (var requirement in didReqs)
        {
            uint? value = obj.GetProperty((PropertyDataId)requirement.Stat);
            double? normalized = value != null ? Convert.ToDouble(value.Value) : null;

            if (RecipeManager.Debug)
                Console.WriteLine($"PropertyDataId.{(PropertyDataId)requirement.Stat} {(CompareType)requirement.Enum} {requirement.Value}, current: {value}");

            if (!RecipeManager.VerifyRequirement(player, (CompareType)requirement.Enum, normalized, Convert.ToDouble(requirement.Value), requirement.Message))
                __result = false;
        }

        if (RecipeManager.Debug && totalReqs > 0)
            Console.WriteLine($"-----");
        #endregion

        return false;
    }

    // Prefix: custom salvage rules, new imbues (Bleed/Cleaving/Nether), buffed jewelry, or standard imbue by dataId; otherwise run original.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RecipeManager), nameof(RecipeManager.TryMutate), new Type[] { typeof(Player), typeof(WorldObject), typeof(WorldObject), typeof(Recipe), typeof(uint), typeof(HashSet<uint>) })]
    public static bool PreTryMutate(Player player, WorldObject source, WorldObject target, Recipe recipe, uint dataId, HashSet<uint> modified, ref RecipeManager __instance, ref bool __result)
    {
        Settings? s = CurrentSettings;
        if (s == null)
            return true;

        uint wcid = source.WeenieClassId;

        // 1) New imbues: Bleed (Serpentine), Cleaving (Tiger Eye), Nether Rending (Onyx).
        if (TryApplyNewImbue(s, wcid, target))
        {
            RecipeManager.HandleTinkerLog(source, target);
            if (s.ShowPlayerSalvageMessage)
                player.SendMessage($"You apply the imbue to your {target.NameWithMaterial}.");
            __result = true;
            return false;
        }

        // 2) Custom salvage rule (random or fixed numeric effect).
        SalvageTinkerRule? rule = SalvageEffectApplier.GetRule(s, wcid);
        if (rule != null)
        {
            int value = SalvageEffectApplier.RollValue(rule);
            if (SalvageEffectApplier.ApplyEffect(target, rule, value, isFailure: false))
            {
                RecipeManager.HandleTinkerLog(source, target);
                if (s.ShowPlayerSalvageMessage)
                    player.SendMessage($"Your {target.NameWithMaterial}: {rule.EffectKind} {(value >= 0 ? "+" : "")}{value}.");
                ModManager.Log($"[Overtinked] {player?.Name} applied {rule.Name ?? wcid.ToString()} -> {rule.EffectKind} {value} on {target.Guid}", ModManager.LogLevel.Debug);
                __result = true;
                return false;
            }
        }

        // 3) Buffed jewelry imbue (Hematite HP, Malachite Stam, etc.).
        if (TryApplyBuffedImbue(s, wcid, target, player))
        {
            RecipeManager.HandleTinkerLog(source, target);
            __result = true;
            return false;
        }

        // 4) Standard imbue by dataId.
        if (!imbueDataIDs.TryGetValue(dataId, out var imbueEffect))
            return true;

        target.ImbuedEffect |= imbueEffect;

        if (RecipeManager.incItemTinkered.Contains(dataId))
            RecipeManager.HandleTinkerLog(source, target);

        __result = true;
        return false;
    }

    private static bool TryApplyNewImbue(Settings s, uint wcid, WorldObject target)
    {
        if (s.BleedImbue?.Enabled == true && s.BleedImbue.SalvageWcids != null && s.BleedImbue.SalvageWcids.Contains(wcid))
        {
            OvertinkedImbueStore.Add(target.Guid.Full, OvertinkedImbueFlags.Bleed);
            return true;
        }
        if (s.CleavingImbue?.Enabled == true && s.CleavingImbue.SalvageWcids != null && s.CleavingImbue.SalvageWcids.Contains(wcid))
        {
            OvertinkedImbueStore.Add(target.Guid.Full, OvertinkedImbueFlags.Cleaving);
            return true;
        }
        if (s.NetherRendingImbue?.Enabled == true && s.NetherRendingImbue.SalvageWcids != null && s.NetherRendingImbue.SalvageWcids.Contains(wcid))
        {
            OvertinkedImbueStore.Add(target.Guid.Full, OvertinkedImbueFlags.NetherRending);
            return true;
        }
        return false;
    }

    const int ImbueFailureWorkmanshipCap = 10;

    // Prefix: when EnableFailureRedesign and our roll fails, apply opposite effect (numeric) or +1 Workmanship (imbue), or let original run.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RecipeManager), nameof(RecipeManager.HandleRecipe), new Type[] { typeof(Player), typeof(WorldObject), typeof(WorldObject), typeof(Recipe), typeof(double) })]
    public static bool PreHandleRecipe(Player player, WorldObject source, WorldObject target, Recipe recipe, double percentSuccess, ref RecipeManager __instance)
    {
        Settings? s = CurrentSettings;
        if (s == null || (!s.EnableFailureRedesign && !s.EnableDefaultImbueFailureWorkmanship))
            return true;

        bool rolledSuccess = Random.Shared.NextDouble() * 100.0 < percentSuccess;
        if (rolledSuccess)
            return true;

        // Ensure a failed attempt never increases tinker count; we'll restore this after applying failure effects.
        int numTimesTinkered = target.GetProperty(PropertyInt.NumTimesTinkered) ?? 0;

        uint wcid = source.WeenieClassId;
        HashSet<uint> imbueWcids = ImbueSalvageWcids.Build(s);

        if (imbueWcids.Contains(wcid) && s.EnableDefaultImbueFailureWorkmanship)
        {
            player.TryConsumeFromInventoryWithNetworking(wcid, 1);
            int currentInt = GetWorkmanship(target);
            if (currentInt < ImbueFailureWorkmanshipCap)
            {
                SetWorkmanship(target, currentInt + 1);
                player.Session?.Network?.EnqueueSend(new GameMessageSystemChat($"Your imbue attempt failed, but the {target.NameWithMaterial} gains a point of workmanship.", ChatMessageType.Craft));
                ModManager.Log($"[Overtinked] Imbue failure Workmanship: {player.Name} +1 Workmanship on {target.Guid} (now {currentInt + 1}).");
            }
            else
            {
                player.Session?.Network?.EnqueueSend(new GameMessageSystemChat($"Your imbue attempt failed. The {target.NameWithMaterial} is already at maximum workmanship.", ChatMessageType.Craft));
            }
            target.SetProperty(PropertyInt.NumTimesTinkered, numTimesTinkered);
            return false;
        }

        SalvageTinkerRule? rule = SalvageEffectApplier.GetRule(s, wcid);
        if (rule == null || !s.EnableFailureRedesign)
            return true;

        player.TryConsumeFromInventoryWithNetworking(wcid, 1);
        int magnitude = rule.FixedValue ?? (rule.MinValue + rule.MaxValue) / 2;
        if (SalvageEffectApplier.ApplyEffect(target, rule, magnitude, isFailure: true))
        {
            player.Session?.Network?.EnqueueSend(new GameMessageSystemChat($"Your tinkering failed! The {target.NameWithMaterial} is now slightly worse.", ChatMessageType.Craft));
            ModManager.Log($"[Overtinked] Failure redesign: {player.Name} applied opposite {rule.EffectKind} -{magnitude} on {target.Guid}");
        }
        target.SetProperty(PropertyInt.NumTimesTinkered, numTimesTinkered);

        return false;
    }

    private static bool TryApplyBuffedImbue(Settings s, uint wcid, WorldObject target, Player? player)
    {
        if (s.BuffedImbueRules == null)
            return false;
        BuffedImbueRule? buffed = s.BuffedImbueRules.FirstOrDefault(r => r.Enabled && r.Wcids != null && r.Wcids.Contains(wcid));
        if (buffed == null)
            return false;
        int primaryMin = buffed.PrimaryMin;
        int primaryMax = buffed.PrimaryMax;
        if (primaryMin > primaryMax)
            (primaryMin, primaryMax) = (primaryMax, primaryMin);
        int rolled = primaryMin == primaryMax ? primaryMin : Random.Shared.Next(primaryMin, primaryMax + 1);
        string stat = buffed.PrimaryStat ?? "";
        if (string.IsNullOrEmpty(stat))
            return true;
        // Map friendly names to ACE PropertyInt (exact name depends on server; adjust if needed).
        string propName = stat switch { "MaxHealth" => "HealthCapacity", "MaxStamina" => "StaminaCapacity", "MaxMana" => "ManaCapacity", _ => stat };
        if (Enum.TryParse<PropertyInt>(propName, ignoreCase: true, out var prop))
        {
            int current = target.GetProperty(prop) ?? 0;
            target.SetProperty(prop, current + rolled);
        }
        if (!string.IsNullOrEmpty(buffed.ImbuedEffectTypeName) && Enum.TryParse<ImbuedEffectType>(buffed.ImbuedEffectTypeName, ignoreCase: true, out var effectType))
            target.ImbuedEffect |= effectType;
        if (!string.IsNullOrEmpty(buffed.SecondaryStat) && buffed.SecondaryValue > 0)
            BuffedJewelrySecondaryStore.Add(target.Guid.Full, buffed.SecondaryStat, buffed.SecondaryValue);
        if (s.ShowPlayerSalvageMessage && player != null)
            player.SendMessage($"Your {target.NameWithMaterial}: {buffed.PrimaryStat} +{rolled}.");
        return true;
    }

    // Postfix: add Damage Rating from buffed jewelry secondary (e.g. 5% of MaxStamina or MaxMana per equipped item).
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.GetDamageRating), new Type[] { typeof(int) })]
    public static void PostGetDamageRating(Player __instance, int damageRating, ref float __result)
    {
        if (__instance?.EquippedObjects == null)
            return;
        double bonusRating = 0;
        foreach (WorldObject item in __instance.EquippedObjects.Values)
        {
            var entry = BuffedJewelrySecondaryStore.Get(item.Guid.Full);
            if (entry == null)
                continue;
            string stat = entry.Value.Stat;
            double pct = entry.Value.Percent / 100.0;
            if (string.Equals(stat, "DamageRatingFromStamina", StringComparison.OrdinalIgnoreCase))
                bonusRating += pct * __instance.Stamina.MaxValue;
            else if (string.Equals(stat, "DamageRatingFromMana", StringComparison.OrdinalIgnoreCase))
                bonusRating += pct * __instance.Mana.MaxValue;
        }
        if (bonusRating <= 0)
            return;
        __result *= (float)(1.0 + bonusRating / 100.0);
    }

    private static int GetWorkmanship(WorldObject wo)
    {
        var t = Traverse.Create(wo).Property("Workmanship");
        if (!t.PropertyExists())
            return 0;
        object? v = t.GetValue();
        if (v is int i)
            return i;
        if (v is double d)
            return (int)d;
        if (v is float f)
            return (int)f;
        return 0;
    }

    private static void SetWorkmanship(WorldObject wo, int value)
    {
        var t = Traverse.Create(wo).Property("Workmanship");
        if (t.PropertyExists())
            t.SetValue(value);
    }

    // Maps recipe mutation dataIds to ImbuedEffectType flags so we can set target.ImbuedEffect when the recipe applies an imbue.
    private static readonly Dictionary<uint, ImbuedEffectType> imbueDataIDs = new()
    {
        [0x38000038] = ImbuedEffectType.MeleeDefense,
        [0x38000039] = ImbuedEffectType.MissileDefense,
        [0x38000037] = ImbuedEffectType.MagicDefense,
        [0x38000025] = ImbuedEffectType.ArmorRending,
        [0x38000024] = ImbuedEffectType.CripplingBlow,
        [0x38000023] = ImbuedEffectType.CriticalStrike,
        [0x3800003A] = ImbuedEffectType.AcidRending,
        [0x3800003B] = ImbuedEffectType.BludgeonRending,
        [0x3800003C] = ImbuedEffectType.ColdRending,
        [0x3800003D] = ImbuedEffectType.ElectricRending,
        [0x3800003E] = ImbuedEffectType.FireRending,
        [0x3800003F] = ImbuedEffectType.PierceRending,
        [0x38000040] = ImbuedEffectType.SlashRending,
        [0x38000041] = ImbuedEffectType.Spellbook,
    };
}
