namespace EmpyreanEchoes.Features;

[CommandCategory(nameof(Feature.ItemLevelUpGrowth))]
[HarmonyPatchCategory(nameof(Feature.ItemLevelUpGrowth))]
public static class ItemLevelUpLootGrowth
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.OnItemLevelUp), new Type[] { typeof(WorldObject), typeof(int) })]
    public static void PreOnItemLevelUpLoot(WorldObject item, int prevItemLevel, ref Player __instance)
    {
        if (!S.Settings.EnableLootItemLeveling)
            return;

        if (item == null)
            return;

        // Only handle items explicitly marked as growth items.
        if (item.GetProperty(FakeBool.GrowthItem) != true)
            return;

        int currentLevel = item.ItemLevel ?? 0;
        if (currentLevel <= prevItemLevel)
            return;

        for (int level = prevItemLevel + 1; level <= currentLevel; level++)
        {
            TryGrowLootItem(item, level, __instance);
        }
    }

    private static void TryGrowLootItem(WorldObject item, int level, Player player)
    {
        // Simple element-appropriate rending or minor stat boosts.
        bool applied = false;

        int? damageTypeInt = item.GetProperty(PropertyInt.DamageType);
        DamageType damageType = damageTypeInt.HasValue ? (DamageType)damageTypeInt.Value : DamageType.Undef;

        ImbuedEffectType? chosen = damageType switch
        {
            DamageType.Acid => ImbuedEffectType.AcidRending,
            DamageType.Cold => ImbuedEffectType.ColdRending,
            DamageType.Electric => ImbuedEffectType.ElectricRending,
            DamageType.Fire => ImbuedEffectType.FireRending,
            DamageType.Pierce => ImbuedEffectType.PierceRending,
            DamageType.Slash => ImbuedEffectType.SlashRending,
            _ => null,
        };

        if (chosen.HasValue && (item.ImbuedEffect & chosen.Value) == 0)
        {
            item.ImbuedEffect |= chosen.Value;
            applied = true;
        }
        else if (item.WeenieType == WeenieType.MeleeWeapon || item.WeenieType == WeenieType.MissileLauncher || item.WeenieType == WeenieType.Caster)
        {
            // Small damage boost when no new imbue is available.
            item.Damage = (item.Damage ?? 0) + 1;
            applied = true;
        }
        else if (item.WeenieType == WeenieType.Clothing)
        {
            // Small armor boost for armor pieces.
            item.ArmorLevel = (item.ArmorLevel ?? 0) + 1;
            applied = true;
        }

        if (applied)
            player.SendMessage($"Your {item.Name} grows stronger at level {level}/{item.ItemMaxLevel}.");
    }
}

