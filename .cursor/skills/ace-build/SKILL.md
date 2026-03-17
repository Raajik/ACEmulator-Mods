---
name: ace-build
description: Run dotnet build for all mod projects inside the ACEmulator-Mods repository. Use when the user types /ace-build or asks to rebuild all mods for this repo.
---

# ACE Mod Build Helper

## When to use this skill

Use this skill in this repository when:

- The user types `/ace-build`.
- The user asks to "build all mods" or "rebuild all current mods" within the `ACEmulator-Mods` repo.

## Core behavior

- Treat the workspace root as the `ACEmulator-Mods` repository root.
- Build every mod project that lives **inside this repo** (including `LeyLineLedger` and `Numbersmith`, which now live under `ACEmulator-Mods`).
- Use `dotnet build` so that normal restore and compilation behavior runs.

## Step-by-step instructions

1. **Identify mod projects**
   - Assume each mod lives in its own subdirectory under this repo (for example `AutoLoot`, `AethericWeaver`, `LeyLineLedger`, `Numbersmith`, etc).
   - Look for `.csproj` files whose paths are **within** `ACEmulator-Mods` (no `../` segments). Include `AutoLoot/AutoLoot.csproj` when the AutoLoot folder exists.
   - **Skip** any project whose directory name starts with `CHANGE` (see below). Do not run `dotnet build` for those.

2. **Excluded projects (placeholder / work-in-progress mods)**
   - Skip any mod in a folder **prefixed with `CHANGE`** (e.g. `CHANGEExpansion`, `CHANGERaise`, `CHANGEEasyEnlightenment`). These are placeholder mods; do not build them.

3. **Build each project**
   - For each `.csproj` file you identify:
     - Run the `Shell` tool with:
       - `working_directory`: the directory that contains the `.csproj`.
       - `command`: `dotnet build`.
       - A generous `block_until_ms` (for example, 600000 or higher) so the build can complete.
   - You may build multiple projects in parallel with the `parallel` tool wrapper when appropriate.

4. **Current known project layout**
   - Always include **AutoLoot** when present. Mod projects inside this repo include (build these):
     - `AutoLoot/AutoLoot.csproj`
     - `AethericWeaver/AethericWeaver.csproj`
     - `LeyLineLedger/LeyLineLedger.csproj`
     - `Loremaster/Loremaster.csproj`
     - `Numbersmith/Numbersmith.csproj`
     - `Overtinked/Overtinked.csproj`
     - `QOL/QOL.csproj`
     - `Swarmed/Swarmed.csproj`
   - Do **not** build any project in a directory whose name starts with `CHANGE` (placeholder mods).

5. **Reporting results**
   - After running the builds, summarize succinctly:
     - Which mod projects built successfully.
     - Which failed, along with the first relevant error message for each failure.
   - Keep the summary high level; avoid pasting large build logs unless the user explicitly asks.

## Examples

- If the user types:

  - `/ace-build`

  Then:

  - Use this skill to:
    - Detect `AethericWeaver/AethericWeaver.csproj` as a mod inside this repo.
    - Run `dotnet build` in the `AethericWeaver` directory.
    - Report the success or failure back to the user.

