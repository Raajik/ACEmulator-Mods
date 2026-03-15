# ACEmulator-Mods

A bundle of ACE (Asheron's Call Emulator) mods. These are customized/updated versions meant to be built and used with [ACE.BaseMod](https://github.com/aquafir/ACE.BaseMod) or a compatible ACE server setup.

## Mods in this repo

- **Overtinked** — Extended tinkering limits (max tinkers, imbues). Based on the Tinkering sample by aquafir.
- **CHANGEBalance**, **CHANGERaise**, **CHANGEExpansion**, **CHANGECustomSpells**, **CHANGEBank**, **CHANGEEasyEnlightenment** — Other Raajik-updated mods.

## Building

These projects reference `ACE.Shared` and the ACE server assemblies. To build:

1. Clone or open the parent solution that contains **ACE.Shared** (e.g. [ACE.BaseMod](https://github.com/aquafir/ACE.BaseMod)).
2. Place this repo’s contents at `ACEmulator-Mods` in that solution (i.e. so the path is `ACE.BaseMod/ACEmulator-Mods/`).
3. Open the main solution and build the ACEmulator-Mods projects, or build them from the command line with the working directory set to the parent repo.

Each mod’s `.csproj` uses `ProjectReference Include="..\..\ACE.Shared\ACE.Shared.csproj"`, so the folder must live two levels below the repo root that contains `ACE.Shared`.

## License and credits

Per-mod credits are in each mod’s Readme and Meta.json. Overtinked is based on the Tinkering sample by aquafir (ACE.BaseMod).
