using ACE.Server.Network.GameMessages.Messages;

namespace Overtinked;

// Extension methods for recipe requirement comparison. Used by PatchClass to evaluate NumTimesTinkered / ImbuedEffect
// against Overtinked limits and to send failure messages to the player.
public static class ComparisonHelpers
{
    // Compares a property value to a threshold using the recipe's CompareType; on failure, sends failMsg to the player.
    public static bool Compare(this CompareType compareType, double? prop, double val, Player player, string failMsg = "")
    {
        var success = compareType switch
        {
            CompareType.GreaterThan => ((prop ?? 0) > val),
            CompareType.LessThanEqual => ((prop ?? 0) <= val),
            CompareType.LessThan => ((prop ?? 0) < val),
            CompareType.GreaterThanEqual => ((prop ?? 0) >= val),
            CompareType.NotEqual => ((prop ?? 0) != val),
            CompareType.NotEqualNotExist => (prop == null || prop.Value != val),
            CompareType.Equal => ((prop ?? 0) == val),
            CompareType.NotExist => (prop == null),
            CompareType.Exist => (prop != null),
            CompareType.NotHasBits => (((int)(prop ?? 0) & (int)val) == 0),
            CompareType.HasBits => (((int)(prop ?? 0) & (int)val) == (int)val),
            _ => true,
        };

        if (!success)
            player.Session.Network.EnqueueSend(new GameMessageSystemChat(failMsg, ChatMessageType.Craft));

        return success;
    }
}
