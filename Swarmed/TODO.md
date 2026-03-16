# Swarmed – TODO

Outstanding work and ideas for the Swarmed mod.

## Implemented

### 1. Call-for-help chance: configurable, separate for landscape and dungeon

- The "call for help" chance is **configurable** and **separate** for landscape vs dungeon (`LandscapeChance`, `DungeonChance`).
- **Defaults:** both set to **15%** (0.15) in Settings and Settings.json.

### 2. Reinforcement health: 30–70% of original max HP

- Reinforcements get health in a **random range** per spawn using `ReinforcementHealthMin` (default 0.3) and `ReinforcementHealthMax` (default 0.7).
- Example: 100/100 HP original → each reinforcement gets 30/30–70/70 HP (rolled per spawn).

### 3. Variable reinforcement size: 10–80% of original

- Reinforcement **size** uses `ObjScale`: each spawn gets a random scale in `ReinforcementScaleMin`–`ReinforcementScaleMax` (defaults 0.1–0.8) times the original creature’s scale.

## Ideas (future)
