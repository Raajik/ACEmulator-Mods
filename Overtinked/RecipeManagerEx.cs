using ACE.Database.Models.World;
using ACE.Server.Entity.Actions;

namespace Overtinked;

// Overtinked Harmony patches for RecipeManager.UseObjectOnTarget (and related flow). Runs under Settings.RecipeManagerCategory.
// Validates recipe, requirements, and success chance then starts the craft animation; allows Overtinked's VerifyRequirements prefix to apply.
[HarmonyPatchCategory(Settings.RecipeManagerCategory)]
public class RecipeManagerEx
{
    // Prefix: handles use-object-on-target for crafting. Checks busy/combat, gets recipe, verifies requirements (via Overtinked), gets chance, and starts animation; returns false to skip original.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RecipeManager), nameof(RecipeManager.UseObjectOnTarget), new Type[] { typeof(Player), typeof(WorldObject), typeof(WorldObject), typeof(bool) })]
    public static bool PreUseObjectOnTarget(Player player, WorldObject source, WorldObject target, bool confirmed, ref RecipeManager __instance)
    {
        var allowCraftInCombat = PropertyManager.GetBool("allow_combat_mode_crafting").Item;

        if (CheckBusy(player, allowCraftInCombat))
            return false;

        if (source == target)
        {
            player.Session.Network.EnqueueSend(new GameMessageSystemChat($"The {source.NameWithMaterial} cannot be combined with itself.", ChatMessageType.Craft));
            player.Session.Network.EnqueueSend(new GameEventCommunicationTransientString(player.Session, $"You can't use the {source.NameWithMaterial} on itself."));
            player.SendUseDoneEvent();
            return false;
        }

        var recipe = RecipeManager.GetRecipe(player, source, target);

        if (recipe == null)
        {
            player.Session.Network.EnqueueSend(new GameMessageSystemChat($"The {source.NameWithMaterial} cannot be used on the {target.NameWithMaterial}.", ChatMessageType.Craft));
            player.SendUseDoneEvent();
            return false;
        }

        if (!RecipeManager.VerifyRequirements(recipe, player, source, target))
        {
            player.SendUseDoneEvent(WeenieError.YouDoNotPassCraftingRequirements);
            return false;
        }

        if (recipe.IsTinkering())
            ModManager.Log($"[OVERTINKED] {player.Name}.UseObjectOnTarget({source.NameWithMaterial}, {target.NameWithMaterial}) | Status: {(confirmed ? "" : "un")}confirmed", ModManager.LogLevel.Debug);

        var percentSuccess = RecipeManager.GetRecipeChance(player, source, target, recipe);

        if (percentSuccess == null)
        {
            player.SendUseDoneEvent();
            return false;
        }

        StartCraftAnimation(player, source, target, confirmed, allowCraftInCombat, recipe, percentSuccess);

        return false;
    }

    // Builds the craft action chain: optional combat drop, clap motion, then either success dialog or HandleRecipe.
    private static void StartCraftAnimation(Player player, WorldObject source, WorldObject target, bool confirmed, bool allowCraftInCombat, Recipe recipe, double? percentSuccess)
    {
        var showDialog = RecipeManager.HasDifficulty(recipe) && player.GetCharacterOption(CharacterOption.UseCraftingChanceOfSuccessDialog);

        if (!confirmed && player.LumAugSkilledCraft > 0)
            player.SendMessage($"Your Aura of the Craftman augmentation increased your skill by {player.LumAugSkilledCraft}!");

        var motionCommand = MotionCommand.ClapHands;

        var actionChain = new ActionChain();
        var nextUseTime = 0.0f;

        player.IsBusy = true;

        if (allowCraftInCombat && player.CombatMode != CombatMode.NonCombat)
        {
            var stanceTime = player.SetCombatMode(CombatMode.NonCombat);
            actionChain.AddDelaySeconds(stanceTime);
            nextUseTime += stanceTime;
        }

        var motion = new Motion(player, motionCommand);
        var currentStance = player.CurrentMotionState.Stance;
        var clapTime = !confirmed ? ACE.Server.Physics.Animation.MotionTable.GetAnimationLength(player.MotionTableId, currentStance, motionCommand) : 0.0f;

        if (!confirmed)
        {
            actionChain.AddAction(player, () => player.SendMotionAsCommands(motionCommand, currentStance));
            actionChain.AddDelaySeconds(clapTime);
            nextUseTime += clapTime;
        }

        if (showDialog && !confirmed)
        {
            actionChain.AddAction(player, () => RecipeManager.ShowDialog(player, source, target, recipe, percentSuccess.Value));
            actionChain.AddAction(player, () => player.IsBusy = false);
        }
        else
        {
            actionChain.AddAction(player, () => RecipeManager.HandleRecipe(player, source, target, recipe, percentSuccess.Value));

            actionChain.AddAction(player, () =>
            {
                if (!showDialog)
                    player.SendUseDoneEvent();

                player.IsBusy = false;
            });
        }

        actionChain.EnqueueChain();

        player.NextUseTime = DateTime.UtcNow.AddSeconds(nextUseTime);
    }

    // Returns true if player is busy or must be in peace mode and isn't (and we send the appropriate error).
    private static bool CheckBusy(Player player, bool allowCraftInCombat)
    {
        if (player.IsBusy)
        {
            player.SendUseDoneEvent(WeenieError.YoureTooBusy);
            return true;
        }

        if (allowCraftInCombat && player.CombatMode != CombatMode.NonCombat)
        {
            player.SendUseDoneEvent(WeenieError.YouMustBeInPeaceModeToTrade);
            return true;
        }

        return false;
    }
}
