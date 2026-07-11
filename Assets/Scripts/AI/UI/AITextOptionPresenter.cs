using System;
using System.Collections.Generic;
using UnityEngine;

// Presents AI dialogue text and player options using the AI interaction UI controller.
// - Delegates all UI work to AIInteractionUIController
// - Emits selected intentTag via OnIntentSelected event
public class AITextOptionPresenter : MonoBehaviour
{
    // Event invoked when the player selects an option. Argument is the selected IntentTag.
    public event Action<string> OnIntentSelected;

    [SerializeField]
    private AIInteractionUIController aiInteractionUIController;

    private void Awake()
    {
        if (aiInteractionUIController == null)
            aiInteractionUIController = FindFirstObjectByType<AIInteractionUIController>();

        if (aiInteractionUIController == null)
            Debug.LogWarning("AITextOptionPresenter: AIInteractionUIController not found in scene.");
    }

    // Present text and options. options can be null or empty to show only text.
    // The provided callback will be invoked with the selected IntentTag when the player selects an option.
    public void Present(string text, List<AIPlayerOptionDto> options, Action<string> callback = null)
    {
        if (aiInteractionUIController == null)
        {
            Debug.LogWarning("AITextOptionPresenter: No AIInteractionUIController available to present options.");
            callback?.Invoke(null);
            return;
        }

        aiInteractionUIController.Show(text, options, (intentTag) =>
        {
            Debug.Log($"[AITextOptionPresenter] Player selected intent: {intentTag}");
            OnIntentSelected?.Invoke(intentTag);
            callback?.Invoke(intentTag);
        });
    }
}
