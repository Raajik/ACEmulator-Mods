# ACEmulator-Mods

A bundle of ACE (Asheron's Call Emulator) mods. These are customized/updated versions meant to be built and used with [ACE.BaseMod](https://github.com/aquafir/ACE.BaseMod) or a compatible ACE server setup.

## Mods in this repo

- **CHANGEBank** — Banking features such as coin drops, direct deposit, and account management.
- **CHANGEBalance (Numbersmith)** — Formula-driven combat and progression balance patches for damage, crits, healing, and experience.
- **CHANGERaise** — Alternate raising/leveling options and custom level cost progression.
- **CHANGECustomSpells** — Spreadsheet-driven custom spell definitions and spell extension hooks.
- **CHANGEExpansion** — Expansion feature layer: creatures, enums, mutators, and gameplay systems.
- **CHANGEEasyEnlightenment** — Easier enlightenment requirements and wield checks for endgame.
- **LeyLineLedger** — Banking and ledger mod for items, currency, and luminance.
- **Overtinked** — Extended tinkering limits (max tinkers, imbues). Based on the Tinkering sample by aquafir.

## Building

These projects reference `ACE.Shared` and the ACE server assemblies. To build:

1. Clone or open the parent solution that contains **ACE.Shared** (e.g. [ACE.BaseMod](https://github.com/aquafir/ACE.BaseMod)).
2. Place this repo’s contents at `ACEmulator-Mods` in that solution (i.e. so the path is `ACE.BaseMod/ACEmulator-Mods/`).
3. Open the main solution and build the ACEmulator-Mods projects, or build them from the command line with the working directory set to the parent repo.

Most mods use `ProjectReference Include="..\..\ACE.Shared\ACE.Shared.csproj"`, so the folder must live two levels below the repo root that contains `ACE.Shared`. `LeyLineLedger` can also be built against the official `ACEmulator.ACE.Shared` / `ACRealms.ACE.Shared` NuGet packages without the local `ACE.Shared` project.

## License and credits

Per-mod credits are in each mod’s Readme and Meta.json. Overtinked is based on the Tinkering sample by aquafir (ACE.BaseMod).
