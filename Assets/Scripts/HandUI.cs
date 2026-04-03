using UnityEngine;

public class HandUI : MonoBehaviour
{
    public HandController handController;

    public void SortByType()
    {
        if (handController == null)
        {
            Debug.LogWarning("HandUI: handController не назначен");
            return;
        }

        handController.SortCardsByTypeRightToLeft();
    }
}