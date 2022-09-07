using UnityEngine;

using PositionLimits = PlayerShipMovement.PlayerLimits;

public class PlayerMovementInputLimitAdjuster
{
     static public void AdjustInputValuesBasedOnPositionLimits(
        Vector3 currentPlayerPosition,
        ref float xInput,
        ref float yInput,
        PositionLimits xScreenLimits,
        PositionLimits yScreenLimits)
    {
        AdjustXinput(currentPlayerPosition.x, ref xInput, xScreenLimits);

        AdjustYinput(currentPlayerPosition.y, ref yInput, yScreenLimits);
    }

    static private void AdjustXinput(
        float currentPlayerPosition_X,
        ref float xInput,
        PositionLimits xScreenLimits)
    {
        if (currentPlayerPosition_X <= xScreenLimits.minLimit)
        {
            // Check if the inputs goes on that direction -> horizontal min is negative
            if (Mathf.Approximately(Mathf.Sign(xInput), -1f))
            {
                xInput = 0f;
            }
        }
        else if (currentPlayerPosition_X >= xScreenLimits.maxLimit)
        {
            // Check if the inputs goes on that direction -> horizontal max is positive
            if (Mathf.Approximately(Mathf.Sign(xInput), 1f))
            {
                xInput = 0f;
            }
        }
    }

    static private void AdjustYinput(
        float currentPlayerPosition_Y,
        ref float yInput,
        PositionLimits yScreenLimits)
    {
        if (currentPlayerPosition_Y <= yScreenLimits.minLimit)
        {
            // Check if the inputs goes on that direction -> vertical min is negative
            if (Mathf.Approximately(Mathf.Sign(yInput), -1f))
            {
                yInput = 0f;
            }
        }
        else if (currentPlayerPosition_Y >= yScreenLimits.maxLimit)
        {
            // Check if the inputs goes on that direction -> vertical max is positive
            if (Mathf.Approximately(Mathf.Sign(yInput), 1f))
            {
                yInput = 0f;
            }
        }
    }
}
