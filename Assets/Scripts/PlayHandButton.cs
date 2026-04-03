using UnityEngine;

public class PlayHandButton : MonoBehaviour
{
    public BattleTurnController battleTurnController;

    public void PlayHand()
    {
        if (battleTurnController == null)
        {
            Debug.LogWarning("PlayHandButton: battleTurnController не назначен");
            return;
        }

        battleTurnController.PlayHand();
    }
}