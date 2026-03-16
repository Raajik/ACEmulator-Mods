namespace AethericWeaver;

// Settings loaded from Settings.json used by AethericWeaver
public class Settings
{
    // When true, the spell spreadsheet is applied automatically on world open
    public bool AutoloadSpreadsheet { get; set; } = true;
    // Path to the Excel file containing spell customizations
    public string Spreadsheet { get; set; } = Path.Combine(Mod.Instance.ModPath, "Spells.xlsx");

    // Custom spells use a template, apply modifications, then use Id as the final spell key
    public List<SpellCustomization> CustomSpells { get; set; } = new()
    {
        new(SpellId.StrengthSelf1, SpellId.StrengthSelf1, Name: "Lesser Strength I", StatModVal: 5, TargetEffect: PlayScript.LevelUp),
        new(SpellId.StrengthSelf1, (SpellId)9999, Name: "Boosted Strength I", StatModVal: 111, Category: (SpellCategory)987),
        new(SpellId.AcidVolley1, SpellId.AcidVolley1, EType: DamageType.Bludgeon, BaseIntensity: 50, Variance: 0),
    };

    // Equipment set overrides; each EquipmentSet key maps to a list of tiers
    public Dictionary<EquipmentSet, List<SetTier>> Sets { get; set; } = new()
    {
        [EquipmentSet.Adepts] = new()
        {
            new(3, new()
            {
                SpellId.HarmSelf3,
                SpellId.AcidProtectionSelf3,
            }),
            new(4, new()
            {
                SpellId.HarmSelf5,
                SpellId.AcidProtectionSelf5,
            }),
        },
        [(EquipmentSet)999] = new()
        {
            new(1, new()
            {
                SpellId.HarmSelf6,
                SpellId.FireProtectionSelf6,
            }),
            new(2, new()
            {
                SpellId.HarmSelf7,
                SpellId.FireProtectionSelf7,
            }),
        },
    };
}

public record struct SetTier(uint NumEquipped, List<SpellId> Spells);
