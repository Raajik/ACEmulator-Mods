# CHANGEExpansion – TODO

Outstanding work and ideas for the Expansion mod. (Source: Readme.md § Todo, and in-code TODO/FIXME comments.)

## From Readme

- EquipmentCache caps.
- Consider different collections for parallel support (e.g. data structures for parallel programming).
- Mutations: check mutation compatibility; don’t patch various MutatorHooks if they have no Mutator for that type.
- If ACE moves to .NET >7, consider switching to polymorphic serialization.
- Currently doesn’t wipe a set/proc if one exists and a mutation isn’t rolled.
- Possibly inefficient way of checking for self-targeting spells.
- Consider intercepting WorldObject creation instead of mutating after creation.

## By file

### Enums / shared

- **AugmentGroup.cs (line 141)**  
  Decide on exception vs empty array for unknown augment.

- **Affixes and Augments.md (line 14)**  
  Skill / SAC / player-related properties.

### Features – creatures

- **CreatureEx.cs (line 38)**  
  Decide on allowing partial name matches for types.

- **CreatureEx.cs (line 80)**  
  Decide on random from enum values vs. an expected offset.

- **Stomper.cs (line 40)**  
  Fix GetNearby.

- **Berserker.cs (line 48)**  
  Send updates.

- **Stunner.cs (line 52)**  
  Decide what to actually disable.

- **Evader.cs**  
  (No TODO; autoDodgeChance constant is in use.)

### Features – pets

- **PetSummonMultiple.cs (lines 351, 376, 441)**  
  Find a non-terrible way for the commented logic; fix teleport.

- **PetEx.cs (line 110)**  
  Figure out how to handle teleport.

### Features – spells / combat

- **FakeSpellSplitSplash.cs (line 32)**  
  Use something like the commented approach.

- **FakeSpellChain.cs (line 26)**  
  Update splash.

- **FakeSpellMeta.cs (commented blocks)**  
  In-dungeon check; splashing debuff; fix checking attack type vs equipped; spell components; spell version resolver.

- **FakeMissileSplitSplash.cs (lines 23, 76)**  
  Allow without ammo / fix redundancy.

- **FakeCombo.cs (line 11)**  
  Reset on breakers besides time.

- **FakePercentDamage.cs (line 69)**  
  Make fake damage from helper.

- **DamageOverTimeConversion.cs (lines 33, 56)**  
  Rounding; choose between action chains or traditional route.

### Features – items / props

- **CorpseInfo.cs (lines 43, 223)**  
  Clean up; figure out issue with the referenced logic.

- **ItemLevelUpGrowth.cs (line 16)**  
  WO → Original type.

- **FakeDurability.cs (line 78)**  
  Think about caching/behavior.

- **FakePropertyCache.cs (lines 9, 81)**  
  Add cleanup of players (e.g. on logoff) and props.

### Features – mutators / hooks

- **Mutator.cs (lines 7, 90)**  
  Decide on target types; decide on throwing an error on fail.

- **ProcOnHit.cs (lines 34, 39)**  
  Check target behavior; consider custom set.

- **ProcOnAttack.cs (lines 5, 27)**  
  Move region / todo somewhere.

- **MutatorHooks.cs (lines 59, 247, 295, 322)**  
  Prevent duplicate shutdowns; postfix; create separate handlers for entering inventory.

### Features – player / commands

- **OverrideCreatePlayer.cs (line 259)**  
  OlthoiLair: check when olthoi play is allowed in ACE.

- **Commands.cs (line 141)**  
  Remove in release.

### Helpers

- **SpellHelper.cs (line 37, commented)**  
  Clean up and verify.
