## EmpyreanEchoes

This mod extends ACE loot, items, creatures, and pets using a configurable mutator pipeline and a set of opt-in features.



### **Overview**

* **Features** are patches that enable additional functionality on players, items, creatures, and pets (for example, growth items, bonus stats, CreatureEx variants, or pet QoL).
  * Some mutations may require a feature for the item they produce to do what it should.
* **Mutators** change loot after its generated.
  * The `Event` is a bitfield of `MutatorEvent` which decides when a mutator mutates.  
    All will require a `WorldObject` but some will have more granularity or other information available.
    * `Loot` catches all loot but misses corpse/location/etc.
    * `Corpse` modifies items in a creature corpse after `Creature.GenerateTreasure`
    * `Generator` runs after `GeneratorProfile.TreasureGenerator`
    * `EnterWorld` runs before `WorldObject.EnterWorld`
    * Could expose things like `RollMundane` / `CreateDinnerware` / etc.
  * Different sets of valid targets are available.  Succeeds if missing.  
    Available targets are:
    * `WeenieTypeTargets` is a set of `WeenieType` the WorldObject must be
    * `TreasureTargets` is a set of `TreasureItemType_Orig` 
  
  * `Odds` is the 0-1 chance of being applied to an item of a tier
    * If missing it will always succeed.
    * Tier 0 is a default if missing tier information
  * There are default sets but you can make your own using names from the relevant enums.
  * You can make multiple Mutators if you want to have different odds for different targets.
  
* Settings for mutators/features live in `Settings.json` and are surfaced via the `Settings` class.

### Monster Settings

* `Settings.Features` must include `Feature.CreatureEx` to enable CreatureEx support.
* `Settings.CreatureFeatures` controls which `CreatureExType` variants are active (used as Harmony patch categories).
* `Settings.CreatureChance` (0–1) is the global chance that a normal creature spawn is replaced by a random `CreatureEx` type.
* A `CreatureExType` FakeInt (ID 10029) on a weenie forces a specific variant, bypassing `CreatureChance`.
* Slayer-related knobs:
  * `Settings.CreatureTypeGroups` names pools of `CreatureType` (for example `CreatureTypeGroup.Popular`).
  * `Settings.Slayers` picks which pool to use when rolling slayers.
  * `Settings.SlayerPower` maps item tier to slayer damage multiplier.



### Groups

* `CreatureType`

* `EquipmentSet` 

* `SpellId`

* `Augment` - attempt to change a WorldObject

  



### Dependencies

Some features or mutators depend on a utility `Feature`:

* ~~`CorpseInfo` adds additional information to a `Corpse`~~
  * ~~`FakeInt.CorpseLivingWCID`~~
  * ~~`FakeDID.CorpseLandblockId`~~
  * ~~`FakeBool.CorpseSpawnedDungeon`~~
* `FakePropertyCache` stores a cached version for a players `FakeProperty` bonus and updates on change.  Not strictly required but gives a significant performance boost.



### Feature List

* `ItemLevelUpGrowth` – growth-style leveling for configured items; augments properties as they level based on type.
* `BonusStats` – applies bonus fake-property-derived stats (attributes, vitals, skills) with caps from `BonusCaps`.
* `CorpseInfo` – tracks extra corpse metadata via fake properties (currently unused / experimental).
* `PetAttackSelected` – pets attack your selected target instead of default behavior.
* `PetMessageDamage` – sends chat messages showing pet damage.
* `PetStow` – adds support for stowing/dismissing pets.
* `ProcOnAttack` – enables on-attack procs on items (see Proc mutators).
* `ProcOnHit` – enables on-hit procs on items (see Proc mutators); often used for cloak-style procs.
* `ProcRateOverride` – overrides default cloak and Aetheria proc rates using `CloakProcRate` and `AetheriaProcRate`.
* `SummonCreatureAsPet` – turns eligible creatures into summonable pets.
* `FakeXpBoost` – applies XP bonuses via fake properties.
* `FakePropertyCache` – caches FakeInt/FakeFloat bonuses for performance-sensitive reads.
* `FakeLeech` – life/mana/stamina leech behavior driven by fake properties.
* `FakePercentDamage` – percent-of-health damage effects driven by fake properties.
* `FakeCombo` – combo-style mechanics based on consecutive hits.
* `FakeCulling` – culling-style effects on low-health targets.
* `FakeItemLoot` – extra item/loot handling driven by fake properties.
* `FakeKillTask` – kill-task style counters and rewards.
* `FakeReflection` – damage reflection using fake properties.
* `FakeSpellReflection` – spell reflection using fake properties.
* `FakeDurability` – durability-style behavior using fake properties.
* `FakeEquipRestriction` – additional equipment restriction rules.
* `AutoLoot` – support tooling for loot profiles (path and username settings).
* `FakeSpellMeta` – meta/spell-tag behavior changes.
* `FakeSpellSplitSplash` – split/splash behavior for spells.
* `FakeSpellChain` – chained spell behavior.
* `MutatorHooks` – enables the core mutator pipeline that calls all configured `Mutator` instances.
* `Hardcore` – hardcore life/timer enforcement using fake properties.
* `Ironman` – Ironman-style restrictions using fake properties.
* `CreatureEx` – enables CreatureEx creature variants and their supporting patches.
* `FakeMissileSplitSplash` – split/splash behavior for missile attacks.
* `CreatureMaxAmmo` – overrides the “3 misses then swap ammo” behavior for creatures.
* `OverrideSpellProjectiles` – breaks out spell projectile logic to allow custom handling.
* `OverrideCreatePlayer` – breaks out player creation to allow custom handling (starter gear, etc.).
* `OverrideCheckUseRequirements` – breaks out use-requirement checks.
* `TimeInGame` – tracks accurate total time in game via fake properties at logout.
* `DamageOverTimeConversion` – converts damage into damage-over-time behavior.
* `LifeMagicElementalMod` – scales Life projectiles by elemental damage modifiers.
* `EquipPostCreation` – attempts to auto-equip starter gear after character creation.
* `PetSummonMultiple` – allows summoning multiple pets.
* `PetEx` – extended pet behavior primitives used by other pet features.
* `UnarmedWeapon` – special handling for unarmed attacks as weapons.
* `PetExShareDamage` – shares damage between pets and their owner.

### Mutations

Below is a quick-reference for each `Mutation` and the primary `Settings` fields that control it.

| Mutation | Purpose | Key settings |
| --- | --- | --- |
| `AutoScale` | Scales creature stats based on tier/context. | `Settings.Mutators` (events/targets). |
| `Enlightened` | Enlightenment-style bonuses on items/players. | `Settings.Mutators`. |
| `GrowthItem` | Growth-style leveling for specific loot items. | `GrowthAugments`, `GrowthFixedLevelAugments`, `GrowthTierLevelRange`, `GrowthXpBase`, `GrowthXpScaleByTier`. |
| `LootGrowthItem` | General loot item leveling and optional pre-imbues. | `EnableLootItemLeveling`, `EnableLootItemPreImbue`, `LootItemXpBase`, `LootItemXpScale`, `LootItemMaxLevelMin`, `LootItemMaxLevelMax`, `LootItemPreImbueChance`, `LootItemLevelingEligibleWeenieTypes`. |
| `IronmanLocked` | Gates loot/behavior behind Ironman state. | `Settings.Mutators`, fake Ironman properties. |
| `LocationLocked` | Gates loot/behavior to certain locations. | `Settings.Mutators`. |
| `ProcOnAttack` | Adds on-attack procs to eligible items. | `Settings.Mutators`, `ProcOnSpells`, `CloakProcRate`. |
| `ProcOnHit` | Adds on-hit and cloak-style procs. | `Settings.Mutators`, `ProcOnSpells`, `CloakProcRate`, `AetheriaProcRate`. |
| `Resize` | Resizes world objects/creatures. | `Settings.Mutators`. |
| `Set` | Rolls equipment sets on armor/clothes/cloaks/jewelry. | `ItemTypeEquipmentSets`, `EquipmentSetGroups`. |
| `ShinyPet` | Upgrades certain pet devices into “shiny” pets. | `Settings.Mutators` (targets/odds for `TreasureItemType_Orig.PetDevice`). |
| `Slayer` | Adds creature-type slayer mods to weapons. | `Slayers`, `SlayerPower`, `CreatureTypeGroups`. |
| `TowerLocked` | Restricts behavior/items to tower contexts. | `Settings.Mutators`. |
| `RandomColor` | Cosmetic recoloring of items/creatures. | `Settings.Mutators`. |
| `SampleMutator` | Example mutator used for testing and demos. | `Settings.Mutators` (usually `Odds = Always`, `Events = Containers | EmoteGive`). |

#### ProcOnHit

* Cloak-style mutations that require the `ProcOnHit` feature to also be enabled.
* If `ProcOnSpells` refers to a valid `SpellGroup` it will be used instead of the default cloak list
*  is true the spells in `CloakSpells` will be used instead of the normal pool.
* ***Still requires you to be wearing a cloak with a proc.***  Probably could change this with a rewrite/Postfix of `SpellProjectile.DamageTarget` and `Player.TakeDamage` which make the checks.



#### Set

* Adds a set based on the `EquipmentSetGroups` corresponding to the `TreasureItemType_Orig` of the item in `ItemTypeEquipmentSets`

* By default: 
  * Armor/clothing roll armor sets  
  * Cloak/jewelry rolls cloak sets
  * Missing/Weapons roll nothing





#### Slayer

* `SlayerPower` determines the power of the corresponding tier of item.
* If `Slayers` refers to a valid `CreatureTypeGroup` it will be used instead of the full list
  * Invalid|Unknown|Wall are removed from the pool




### Features


#### EnableOnAttackForNonAetheria

* `EnableOnAttackForNonAetheria` is needed to patch ACE to check non-Aetheria for OnAttack triggers
  * TitaniumWeenie's [UniqueWeenies](https://github.com/titaniumweiner/ACEUniqueWeenies) contains compatible weenies








### Video

<details>
 <summary>Sets</summary>

https://github.com/aquafir/ACE.BaseMod/assets/83029060/1300de91-fa7f-442c-a2f1-527bc4a282f0
</details>

<details>
 <summary>OnAttack Proc</summary>
https://github.com/aquafir/ACE.BaseMod/assets/83029060/81e635c1-115a-453e-b1e3-c2efbf67d781
</details>




### Enums

* [EquipmentSet](https://github.com/ACEmulator/ACE/blob/fdfdec9f0a16bbcbb89a9120ce4f889520a51708/Source/ACE.Entity/Enum/EquipmentSet.cs#L4)
* [TreasureItemType_Orig enum](https://github.com/ACEmulator/ACE/blob/fdfdec9f0a16bbcbb89a9120ce4f889520a51708/Source/ACE.Server/Factories/Enum/TreasureItemType_Orig.cs#L4)

* [SpellId](https://github.com/ACEmulator/ACE/blob/fdfdec9f0a16bbcbb89a9120ce4f889520a51708/Source/ACE.Entity/Enum/SpellId.cs#L4)

* Sigil
* Surge





### Todo

* EquipmentCache caps
* Need to use [different collections](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/data-structures-for-parallel-programming) for parallel support?
* Mutations
  * Check mutations compatibility
  * Don't patch various MutatorHooks if they have no Mutator with that type

* 
* ~~ArrayOf for pools for constant time sampling?~~
* If ACE ever goes .NET >7 should switch to polymorphic serialization 
* Currently doesn't wipe a set/proc if one exists and a mutation isn't rolled.
* Possibly inefficient way of checking for self-targeting spells
* Possible increase in efficiency by intercepting WO creation instead of mutating after creation







## Scratch

* Convert damage to periodic damage
* Saving grace - chance of damage > hp being set to hp - 1
* Sweetspot angle, support with UB?
* Weal/woe
* Autocombat
* Flat vs. multi
* Clamp in cache or return?
* Bonus based on height / size
* Attributes / Vitals
  * CreatureVital.Base factors in Enlightenmentt
* Vendor AlternateCurrency
  * Award based on scaled difficulty
* skill trained/under
* Spells
  * Custom thoughts
    * Meteors that trigger on destroy?
    * Outdoor only / line of sight?
  * SpellProjectiles are WorldObjects so I can use props
    * WO.CreateSpellProjectiles
      * SpellProjectile.GetProjectileSpellType splits into types
      * WO.CalculateProjectileOrigins uses the type to return a Vector3 list of locations
      * WO.CalculateProjectileVelocity then calculates velocity *using the first Vector3 ??*
      * `Trajectory.solve_ballistic_arc_lateral` or `Trajectory2.CalculateTrajectory` get used to calculate path depending on gravity
    * Then launched by WO.LaunchSpellProjectiles
    * Has
      * OnCollideEnvironment
      * ProjectileImpact
      * `SpellType`
      * SpawnPos `Position`
      * `SpellProjectileInfo` ?
        * Caster/Target `Position`
        * Velocity
      * IsWeaponSpell - true when from caster
      * FromProc - to prevent retries
      * 
  * Position
    * InFrontOf
  * Spell
    * IconId
    * Category - used for family of buffs
    * SpellFlags - diff type flags
  * SpellBase
    * Lots of unused ones
    * Name
    * Desc
    * Icon
    * BaseMana
    * BaseRange
    * BaseRangeConstant
    * Power - Used to determine which spell in the catgory is the strongest.
    * MetaSpellType - subtype
    * ManaMod - additional cost per target
    * Duration
    * DisplayOrder?
* cleave
* kdtree
  * aura
  * transfer falling damage if close enough
* craft all / craft queue
* steal->alt curr
* spell ref
* refl non-spell
* rage
  * Low health / Low stam / low mana
* combo over x pow/acc/lvl
* combo same target
* shield flat/per
* landblock specific wield
* durability
* Sets.  Finds spells in `DatManager.PortalDat.SpellTable.SpellSet`
  * * Combined item levels in a sorted dictionary for `SpellSetTiers` list of spells







https://i.gyazo.com/9d12388f6e416f76c281ea3436ef57e8.mp4
