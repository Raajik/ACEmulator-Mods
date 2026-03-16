namespace Swarmed;

[HarmonyPatch]
public class PatchClass(BasicMod mod, string settingsName = "Settings.json") : BasicPatch<Settings>(mod, settingsName)
{
    internal static Settings? CurrentSettings;

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
}
