# LeyLineLedger – TODO

Outstanding work and planned features for the LeyLineLedger mod. (Source: DESIGN.md, PatchClass.cs.)

## Core feature

- **Bank transfer**  
  Implement `/bank transfer` (currently a stub; PatchClass.cs line 149). Resolve character by name; debit source; credit target (online or offline); message both.

## Future / planned

- **AutoLoot integration**  
  Optional deposit of looted pyreals to bank when an AutoLoot-style mod is present (e.g. setting `AutoLootDepositsToBank`).

- **Death bank penalty**  
  Optional `DeathBankPyrealPercent` to deduct a percentage of banked pyreals on death (no physical coin drop).

- **Luminance withdrawal gems**  
  `/bank withdraw luminance <amount>` to create a luminance gem; consuming the gem credits the recipient’s banked luminance. Configurable WCID and caps; feature off by default.
