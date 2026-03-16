namespace Overtinked;

// Resolves salvage rules by WCID (including dual-WCID pairs) and applies numeric effects to the target.
public static class SalvageEffectApplier
{
    // WCID -> rule lookup built from Settings.SalvageRules (each rule's Wcids all point to that rule).
    private static Dictionary<uint, SalvageTinkerRule>? _ruleByWcid;

    public static void BuildLookup(Settings settings)
    {
        var dict = new Dictionary<uint, SalvageTinkerRule>();
        if (settings.SalvageRules == null)
            return;
        foreach (SalvageTinkerRule rule in settings.SalvageRules)
        {
            if (!rule.Enabled || rule.Wcids == null)
                continue;
            foreach (uint w in rule.Wcids)
                dict[w] = rule;
        }
        _ruleByWcid = dict;
    }

    public static SalvageTinkerRule? GetRule(Settings? settings, uint sourceWcid)
    {
        if (settings == null)
            return null;
        if (_ruleByWcid == null)
            BuildLookup(settings);
        return _ruleByWcid != null && _ruleByWcid.TryGetValue(sourceWcid, out var r) ? r : null;
    }

    public static int RollValue(SalvageTinkerRule rule)
    {
        if (rule.FixedValue.HasValue)
            return rule.FixedValue.Value;
        int min = rule.MinValue;
        int max = rule.MaxValue;
        if (min > max)
            (min, max) = (max, min);
        if (min == max)
            return min;
        return Random.Shared.Next(min, max + 1);
    }

    public static bool ApplyEffect(WorldObject target, SalvageTinkerRule rule, int value, bool isFailure)
    {
        int v = isFailure ? -value : value;
        string kind = rule.EffectKind ?? "";
        switch (kind)
        {
            case "Damage":
                target.Damage = (target.Damage ?? 0) + v;
                return true;
            case "DamageVariance":
                // Success: reduce variance (e.g. 20 -> *0.8). Failure: increase (e.g. *1.2).
                float factor = 1f - (v / 100f);
                if (factor <= 0f)
                    factor = 0.01f;
                target.DamageVariance = (target.DamageVariance ?? 1f) * factor;
                return true;
            case "WeaponSpeed":
                // Positive value = faster (lower WeaponTime). Failure: negative = slower.
                int current = (int)(target.WeaponTime ?? 0);
                target.WeaponTime = Math.Max(0, current - v);
                return true;
            case "ArmorLevel":
                target.ArmorLevel = (target.ArmorLevel ?? 0) + v;
                return true;
            case "DamageMod":
                target.DamageMod = (target.DamageMod ?? 1f) + (v / 100f);
                return true;
            case "WeaponDefense":
                target.WeaponDefense = (target.WeaponDefense ?? 0f) + (v / 100f);
                return true;
            case "WeaponOffense":
                target.WeaponOffense = (target.WeaponOffense ?? 0f) + (v / 100f);
                return true;
            case "ArmorModVsAcid":
                target.ArmorModVsAcid = (target.ArmorModVsAcid ?? 1f) + (v / 100f);
                return true;
            case "ArmorModVsSlash":
                target.ArmorModVsSlash = (target.ArmorModVsSlash ?? 1f) + (v / 100f);
                return true;
            case "ArmorModVsBludgeon":
                target.ArmorModVsBludgeon = (target.ArmorModVsBludgeon ?? 1f) + (v / 100f);
                return true;
            case "ArmorModVsFire":
                target.ArmorModVsFire = (target.ArmorModVsFire ?? 1f) + (v / 100f);
                return true;
            case "ArmorModVsCold":
                target.ArmorModVsCold = (target.ArmorModVsCold ?? 1f) + (v / 100f);
                return true;
            case "ArmorModVsElectric":
                target.ArmorModVsElectric = (target.ArmorModVsElectric ?? 1f) + (v / 100f);
                return true;
            default:
                return false;
        }
    }
}
