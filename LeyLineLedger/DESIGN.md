# LeyLineLedger – Design Document

## Overview

LeyLineLedger is a character-based bank mod for ACEmulator. Each character has a bank account holding **Pyreals**, **Luminance**, **Keys**, and **custom items** (optional). Funds can be deposited, withdrawn, and transferred between characters by name. Purchases and vendor sales integrate with the bank (direct debit and auto-deposit). Death does not drop carried pyreals when the mod’s behavior is active.

---

## 1. Bank Contents (Character-Based)

| Currency / Item | Storage                | Notes                                                                 |
|-----------------|------------------------|-----------------------------------------------------------------------|
| Pyreals         | PropertyInt64 (cash)   | No realistic cap; client display capped                              |
| Luminance       | PropertyInt64 (lum)    | Same as current `LuminanceProperty`                                  |
| Keys            | PropertyInt64 or Items | Optional; off by default. Placeholder: e.g. Sturdy Iron Key.          |
| Custom items    | PropertyInt64 or Items | Optional; off by default. Placeholder: e.g. Ancient Falatacot Trinket. |

- **Storage:** Per-character bank state lives on the **player Biota** (same as current `Settings.CashProperty` and `Settings.LuminanceProperty`). No new tables required.
- **Lookup:** Transfers use target **character name** (case-insensitive). Resolve via DB: Character by name → Character.Id (same as main Biota id) → load Biota, update/save PropertyInt64. If character does not exist or is deleted (`IsDeleted`), transfer fails.

- **Keys and custom items (optional):** Not active by default. Servers can enable by adding entries to Settings (e.g. Keys list with placeholder like Sturdy Iron Key; Custom items list with placeholder like Ancient Falatacot Trinket). Storage follows the same pattern as existing BankItem (PropertyInt64 per type or item WCID). When configured, `/bank` can show and optionally deposit/withdraw/transfer them.

---

## 2. Commands

### 2.1 Full Commands

| Command | Description |
|--------|-------------|
| `/bank` | Show bank balances (pyreals, luminance, keys, custom items if enabled). |
| `/bank deposit` | Deposit all loose pyreals, luminance, and trade notes into the bank. |
| `/bank transfer pyreals <amount> <character name>` | Transfer pyreals to another character. |
| `/bank transfer luminance <amount> <character name>` | Transfer luminance to another character. |
| `/bank withdraw pyreals <amount>` | Withdraw pyreals into pack. Prefer MMD / higher-value trade notes when possible. |
| `/bank withdraw all` | Withdraw **all** banked pyreals into pack, converted into the best mix of trade notes. |

(Withdraw is to **self** only; no target character. Transfer is for sending to **another** character.)

### 2.2 Shorthand

| Shorthand | Equivalent |
|----------|------------|
| `/b` | `/bank` |
| `/b d` | `/bank deposit` |
| `/b t p <amount> <character name>` | `/bank transfer pyreals ...` |
| `/b t l <amount> <character name>` | `/bank transfer luminance ...` |
| `/b w p <amount>` | `/bank withdraw pyreals <amount>` |

### 2.3 Parsing and Validation

- **Amount:** Integer; must be &gt; 0. For transfer/withdraw, fail with a clear message if insufficient bank balance.
- **Character name:** One token or multi-word; case-insensitive. Must match a non-deleted character in the shard DB; otherwise transfer fails with “character not found” (do not reveal deletion).
- **Shorthand:** Map `/b` → `bank`, single-letter subcommands (`d`, `t`, `w`) and currency (`p`, `l`) so that the same logic as full commands can be used after normalization.

---

## 3. Withdraw Behavior: Prefer MMD Stacks

- When withdrawing pyreals, convert bank balance into **stacks of the highest denominations first** (e.g. MMD 250k, then lower trade note denominations, then pyreal stacks).
- Use existing `Settings.Currencies` (or a dedicated “withdrawal order” list) sorted by value descending. Create items via existing spawn path (e.g. `TryCreateItems` or equivalent) in that order until the requested amount is satisfied.
- If the exact amount cannot be formed from available denominations, withdraw the closest possible (e.g. round down to a creatable amount) or define a simple rule (e.g. “withdraw in MMDs only when amount ≥ 250k”) and document it.

---

## 4. Direct Debit (Vendors Use Bank)

- **Pyreals and trade notes:** Purchasing from vendors uses **bank balance first**, then physical pyreals/trade notes in pack. Already implemented in `Debit.cs` (VendorsUseBank, SpendCurrency prefix, BuyItems_ValidateTransaction, GameEventApproachVendor).
- **Display cap:** Client can only display pyreals up to **int32**. Vendor UI must show a **capped value** (e.g. `Math.Min(banked + carried, int.MaxValue)` or `int.MaxValue` when at cap) so the client does not overflow. Already partially handled in `Debit.PreUpdateCoinValue` (coins capped to int.MaxValue).

---

## 5. Auto-Deposit on Sale

- Selling to vendors deposits proceeds **directly into the bank** (no physical pyreal stacks). Already implemented in `DirectDeposit.cs` (HandleActionSellItem prefix, IncCash on payout).
- Optional per-player toggle (e.g. `/ddt`) remains; when off, use default server behavior (physical coins).

---

## 5.1 Possible future feature – AutoLoot integration

- When an AutoLoot-style mod is present and LeyLineLedger is enabled, **looted pyreals** could be deposited directly into the bank instead of as physical coin stacks in the player’s inventory.
- Detection: check for the presence/enabled state of the AutoLoot mod (e.g. via Mod metadata or a shared interface) and gate this behavior behind a LeyLineLedger setting (e.g. `AutoLootDepositsToBank`).
- Behavior: on loot resolution, before adding pyreals/trade notes to inventory, call `IncCash(amount)` on the owning player and suppress creation of physical pyreal stacks when the setting is active. Non-pyreal loot remains unchanged and continues to use AutoLoot’s existing rules.

---

## 6. Death Penalty Adjustment (no coin explosion)

- **Default:** No pyreals drop on death. Patch `Player.GetNumCoinsDropped` so the default behavior is `__result = 0` (no physical coins dropped). `DeathPenalty` only enforces the `MaxCoinsDropped` cap (default 0 = no physical drops).
- **Possible future feature – optional bank penalty (not currently implemented):** A setting (e.g. `DeathBankPyrealPercent`) could remove a **percentage of banked pyreals** on death. When `DeathBankPyrealPercent` > 0 (e.g. 10 = 10%): (1) no physical coins would be dropped (`GetNumCoinsDropped` remains 0 or capped), (2) the server could deduct `(banked pyreals * percent / 100)` from the character's bank (PropertyInt64 cash). Example: 0 = no bank penalty, 10 = 10% of banked pyreals lost on death. Implementation details are left as a future extension to the death penalty patch (e.g. compute the amount from `GetCash() * percent / 100` and call `IncCash(-amount)` on the player when integrating with the actual death flow).

---

## 7. Planned: Luminance withdrawal gems

- **Planned command:** `/bank withdraw luminance <amount>` (and shorthand like `/b w l <amount>`) will convert a portion of a character's **banked luminance** into a **luminance gem** item (or similar gem-like token).
- **Usage:** The gem can be traded to another player. When that player **uses** the gem, the mod will consume the item and credit the gem's luminance value directly into the **recipient's banked luminance** (same PropertyInt64 as normal luminance bank), not as spendable "available luminance".
- **Limits:** Exact WCID, stack behavior, and amount caps are intentionally left configurable via Settings (e.g. a `LuminanceGem` entry and max-per-gem value). By default the feature is **off**; servers can enable it by configuring the gem WCID and enabling the command.
- **Integration:** Implementation will mirror pyreal withdraw behavior (bank → item), but write to `Settings.LuminanceProperty` instead of `Settings.CashProperty` when the gem is consumed. Vendor and direct-deposit flows are unaffected.

## 8. Technical Notes

- **Character lookup for transfer:** Use **ACE.Database** (ShardDbContext or equivalent). Query `Character` by name (case-insensitive, `IsDeleted == false`). If not found or deleted, fail transfer. Get `Character.Id` (same as player Biota id). Then either:
  - **If target is online:** Get `Player` by name (e.g. `PlayerManager.FindByName`); if non-null, use `player.IncBanked(...)` (and optionally notify them).
  - **If target is offline:** Load Biota by id, update the relevant PropertyInt64 (cash/luminance), save Biota. On next login, the character’s bank reflects the transfer.
- **Threading:** DB access for “get character by name” and “save target Biota” may be async/callback-based; keep command handler responsive (e.g. enqueue work or use async and send result message when done).
- **Keys / Custom items:** Optional; not active by default. If stored as PropertyInt64 (counts), same as cash/luminance (add properties and commands). If stored as item WCIDs (like current BankItem), reuse existing give/take pattern; add to `/bank` display and optionally add transfer subcommands. Use placeholders (e.g. Sturdy Iron Key, Ancient Falatacot Trinket) in Settings when configuring.

---

## 9. Files and Responsibilities (Current + Planned)

| File | Purpose |
|-----|--------|
| `Mod.cs` | Mod entry; register patch class. |
| `PatchClass.cs` | Settings load; **single** `/bank` (and `/b`) command handler; subcommands: list, deposit, transfer, withdraw; optional `/lum`/`/cash` if kept for compatibility. |
| `Settings.cs` | DirectDeposit, VendorsUseBank, MaxCoinsDropped, DeathBankPyrealPercent, CashProperty, LuminanceProperty, Currencies (withdrawal order), optional Keys and CustomItems (disabled by default; placeholders). |
| `Debit.cs` | Vendor buy: use bank first; vendor UI coin display capped to int32. |
| `DirectDeposit.cs` | Vendor sell: deposit to bank; `/ddt` toggle. |
| `DeathPenalty.cs` | Death: default no physical drop; optional DeathBankPyrealPercent (deduct % of banked pyreals). |
| **New (optional):** `BankTransfer.cs` or logic in PatchClass | Resolve character by name; debit source; credit target (online or offline); message both. |

---

## 10. Summary of Intended Behavior

- **One** main command: `/bank` (and `/b`) with subcommands: show balances, deposit all, transfer (pyreals/luminance) to character by name, withdraw pyreals (prefer MMD stacks).
- Character-based bank; transfer by name; fail if character does not exist (or is deleted).
- No realistic storage limits; vendor display capped at int32.
- Direct debit and auto-deposit on sale already in place; death drops no physical pyreals by default, with optional percentage penalty on banked pyreals.

---

## 11. Implementation Plan

### Phase 1: Unify commands and clarify behavior (no new features)

1. **Single `/bank` entrypoint**
   - Keep one `[CommandHandler("bank", ...)]` and add `[CommandHandler("b", ...)]` that forwards to the same handler with the same `parameters`.
   - Parse first token as subcommand: (none) | `deposit` | `transfer` | `withdraw`. Map shorthand: `d` → deposit, `t` → transfer, `w` → withdraw. For transfer, parse second token as currency: `pyreals` | `luminance` (or `p` | `l`).

2. **Subcommand behavior**
   - **No args or “list”:** Show balances (pyreals, luminance; keys/custom items if enabled). Reuse existing `GetCash()` / `GetBanked(LuminanceProperty)`.
   - **deposit:** Same as current “cash give” + “lum give”: deposit all loose pyreals/trade notes and all luminance. One message summarizing what was deposited and new balances.

3. **Settings**
   - Default: no physical pyreal drop on death (`GetNumCoinsDropped` -> 0 in death penalty patch). Optional `DeathBankPyrealPercent` (0 = off; e.g. 10 = 10% of banked pyreals lost on death). When `DeathBankPyrealPercent` > 0, in the death penalty patch also deduct from the player's bank: `IncCash(-(GetCash() * percent / 100))`.

4. **Vendor display cap**
   - Ensure vendor/approach messages send coin value capped at `int.MaxValue` (already in `Debit.PreUpdateCoinValue`). Confirm `GameEventApproachVendor` and any other vendor coin display paths use the same cap.

**Deliverable:** `/bank`, `/b`, `/b d` work; death drop configurable; vendor display safe. No transfer/withdraw yet.

---

### Phase 2: Withdraw pyreals (prefer MMD stacks)

1. **Withdraw parser**
   - For `/bank withdraw pyreals <amount>` (and `/b w p <amount>`), parse positive integer amount. Validate `amount <= GetCash()`; else send “Insufficient funds.”

2. **Withdrawal algorithm**
   - Sort `Settings.Currencies` by value descending (or use a dedicated “withdrawal order” list). From the bank balance, compute how to form `amount` using largest denominations first (e.g. MMD then lower notes then pyreals). Call existing item creation (e.g. `TryCreateItems` or per-currency loop) to create stacks, then `IncCash(-totalCreated)`. If exact amount cannot be formed, either round down to creatable amount or define a simple rule and document.

3. **Messages**
   - Confirm “Withdrew X pyreals (e.g. 2 MMD, 50k in notes). Balance: Y.”

**Deliverable:** `/bank withdraw pyreals <amount>` and `/b w p <amount>` work; MMD stacks when possible.

---

### Phase 3: Transfer by character name

1. **Character resolution**
   - Add helper (e.g. in `PatchClass` or `BankTransfer.cs`): resolve character name (case-insensitive) to a non-deleted `Character` in the shard DB. Use `ACE.Database` (ShardDbContext). If not found or `IsDeleted`, return null/false and have command send “Character not found.”

2. **Transfer pyreals**
   - Parse `/bank transfer pyreals <amount> <character name>` (and `/b t p <amount> <name>`). Name may be multi-word; amount is last numeric token or define a clear rule (e.g. amount before name). Debit from sender: `IncCash(-amount)`; fail if insufficient. Credit receiver: if online, get `Player` by name and `IncCash(amount)` (+ optional notify); if offline, load Biota by Character.Id, update cash PropertyInt64, save Biota. Send sender “Transferred X pyreals to &lt;name>.” Optionally notify receiver if online.

3. **Transfer luminance**
   - Same flow for luminance: debit sender `IncBanked(LuminanceProperty, -amount)`, credit receiver (online or offline). Validate against sender’s banked luminance and receiver’s max if needed (e.g. cap at receiver’s available capacity).

4. **Concurrency**
   - Ensure only one of “online path” or “offline path” runs per transfer; use DB save callback or async so the command handler does not block the main thread if DB is used.

**Deliverable:** `/bank transfer pyreals/luminance <amount> <name>` and shorthand work; transfer fails cleanly if character does not exist.

---

### Phase 4: Keys and custom items (optional)

1. **Storage**
   - Keys and custom items are not active by default. Servers can enable by adding entries to Settings. Use placeholders in default config (e.g. Sturdy Iron Key for keys, Ancient Falatacot Trinket for custom items). Define PropertyInt64 (or reuse BankItem pattern) per type. Add to `/bank` display when configured.

2. **Deposit/withdraw**
   - If using PropertyInt64: “deposit” can accept loose items and add to bank counts; “withdraw” creates items. If using BankItem-style, reuse existing give/take logic and wire to `/bank deposit` and withdraw subcommands for keys and custom items.

3. **Transfer (optional)**
   - Add transfer subcommands (e.g. `/bank transfer keys <amount> <name>`, and for custom items if desired) using the same character-resolution and online/offline credit logic.

**Deliverable:** Bank can show and move keys and custom items when configured; optional transfer.

---

### Phase 5: Cleanup and docs

1. **Command help**
   - When `/bank` or `/b` is invoked with no args or invalid args, print a short usage line (e.g. “Usage: /bank [deposit | transfer &lt;pyreals|luminance> &lt;amount> &lt;name> | withdraw pyreals &lt;amount>]” and “Shorthand: /b [d | t p|l &lt;amount> &lt;name> | w p &lt;amount>]”).

2. **Readme and Settings**
   - Update `Readme.md` to match DESIGN.md (commands, shorthand, behavior). Document Settings (DirectDeposit, VendorsUseBank, MaxCoinsDropped, DeathBankPyrealPercent, withdrawal order, optional Keys/CustomItems, etc.).

3. **Legacy commands**
   - Decide whether to keep `/cash` and `/lum` as aliases that forward to `/bank deposit` and `/bank` display, or remove them and point users to `/bank`. Prefer one entrypoint `/bank` plus shorthand `/b`.

**Deliverable:** Clear in-game help, Readme and settings documented, legacy commands either aliased or removed.
