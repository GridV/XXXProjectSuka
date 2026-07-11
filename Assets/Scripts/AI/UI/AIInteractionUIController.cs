using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Responsible only for displaying AI text and AI player options on the UI.
// This component does not contain gameplay or AI provider logic.
public class AIInteractionUIController : MonoBehaviour
{
    [Header("Text Display")]
    [SerializeField]
    private TextMeshProUGUI aiTextField;

    [Header("Option Buttons")]
    [SerializeField]
    private Button[] optionButtons;

    private Action<string> onIntentSelected;

    private void Awake()
    {
        if (aiTextField == null)
            Debug.LogWarning("AIInteractionUIController: aiTextField reference is not set.");

        if (optionButtons == null || optionButtons.Length == 0)
            Debug.LogWarning("AIInteractionUIController: optionButtons array is empty or not assigned.");
    }

    // Show AI text and player option buttons. Returns the selected IntentTag through the callback.
    public void Show(string text, List<AIPlayerOptionDto> options, Action<string> onIntentSelected)
    {
        this.onIntentSelected = onIntentSelected;
        Debug.Log($"AIInteractionUIController.Show text='{text}', options={options?.Count ?? 0}");
        gameObject.SetActive(true);
        UpdateText(text);
        PrepareOptionButtons(options);
    }

    public void Hide()
    {
        ClearOptionButtons();
        if (aiTextField != null)
        {
            aiTextField.text = string.Empty;
            aiTextField.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    private void UpdateText(string text)
    {
        if (aiTextField == null)
            return;

        if (string.IsNullOrWhiteSpace(text))
        {
            aiTextField.text = string.Empty;
            aiTextField.gameObject.SetActive(false);
        }
        else
        {
            aiTextField.text = text;
            aiTextField.gameObject.SetActive(true);
        }
    }

    private void PrepareOptionButtons(List<AIPlayerOptionDto> options)
    {
        ClearOptionButtons();

        if (optionButtons == null || optionButtons.Length == 0)
            return;

        if (options == null || options.Count == 0)
        {
            SetButtonsActive(false);
            return;
        }

        int maxButtons = optionButtons.Length;
        int optionCount = Math.Min(options.Count, maxButtons);

        if (options.Count > maxButtons)
        {
            Debug.LogWarning($"AIInteractionUIController: Only {maxButtons} option buttons are available, but {options.Count} options were provided.");
        }

        for (int i = 0; i < optionCount; i++)
        {
            var option = options[i];
            var button = optionButtons[i];
            if (button == null)
                continue;

            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();

            var label = GetButtonLabel(button);
            if (label != null)
            {
                label.text = !string.IsNullOrEmpty(option.ButtonText)
                    ? option.ButtonText
                    : (!string.IsNullOrEmpty(option.Label) ? option.Label : string.Empty);
            }

            string intentTag = option?.IntentTag ?? string.Empty;
            button.onClick.AddListener(() => HandleOptionSelected(intentTag));
        }

        for (int i = optionCount; i < maxButtons; i++)
        {
            var button = optionButtons[i];
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    private void ClearOptionButtons()
    {
        if (optionButtons == null)
            return;

        foreach (var button in optionButtons)
        {
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    private void SetButtonsActive(bool isActive)
    {
        if (optionButtons == null)
            return;

        foreach (var button in optionButtons)
        {
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(isActive);
        }
    }

    private TextMeshProUGUI GetButtonLabel(Button button)
    {
        if (button == null)
            return null;

        return button.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void HandleOptionSelected(string intentTag)
    {
        onIntentSelected?.Invoke(intentTag);
        Debug.Log($"[AIInteractionUI] Selected option: {intentTag}");
    }
}
