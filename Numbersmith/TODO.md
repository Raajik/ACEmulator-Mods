# Numbersmith – TODO

Outstanding work and ideas for the Numbersmith mod. (Source: PatchClass.cs, LevelCost.cs.)

## Settings / startup

- **PatchClass.cs (line ~43)**  
  Consider saving settings when a default formula is used (commented `//TODO:` in code).

## LevelCost patch

- **LevelCost.cs (line 18)**  
  Use an array sized to max level instead of (or in addition to) `Dictionary<uint, ulong> totalCosts` for total cost lookups.

- **LevelCost.cs (line 184)**  
  Prefer a closed-form or better approach for `TotalLevelCost` if possible; current implementation works with LevelCost only.

- **LevelCost.cs (line 472)**  
  (Optional) Revisit “not independent variables” comment if formula semantics change.
