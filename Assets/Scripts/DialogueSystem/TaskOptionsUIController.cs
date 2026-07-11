using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TaskOptionsUIController : MonoBehaviour
{
    private readonly Dictionary<string, TextMeshProUGUI> _activeLabelByOptionId
    = new Dictionary<string, TextMeshProUGUI>();
    [Header("Fade Transition")]
    [SerializeField] private bool enableFade = true;
    [SerializeField] private float fadeOutSeconds = 0.5f;
    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private CanvasGroup canvasGroup;

    public bool IsTransitioning { get; private set; }

    private Coroutine fadeRoutine;

    [Header("Positive Buttons (pre-placed in Canvas)")]
    [SerializeField] private Button[] positiveButtons;

    [Header("Negative Buttons (pre-placed in Canvas)")]
    [SerializeField] private Button[] negativeButtons;

    [Header("Main Text")]
    [SerializeField] private TextMeshProUGUI mainText;

    [Header("Main Text Container")]
    [SerializeField] private GameObject mainTextContainer;
    public void HideAll()
    {
        _activeLabelByOptionId.Clear();
        HideButtonArray(positiveButtons);
        HideButtonArray(negativeButtons);
        HideMainText(mainTextContainer);
    }
    private void Awake()
    {
        HideAll();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    private void HideButtonArray(Button[] buttons)
    {
        if (buttons == null) return;

        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            btn.onClick.RemoveAllListeners();
            btn.gameObject.SetActive(false);
        }
    }
    private void HideMainText(GameObject main)
    {
        if (main == null) return;
        main.gameObject.SetActive(false);
    }
    public void ShowOptions(InTaskContext context, Action<string, string> onSelected)
    {
        RunFadeTransition(() => RenderOptions(context, onSelected));
    }
    private void RenderOptions(InTaskContext context, Action<string, string> onSelected)
    {
        HideAll();
        if (context == null)
            return;

        if (mainText != null)
        {
            bool hasText = !string.IsNullOrEmpty(context.text);
            mainTextContainer.gameObject.SetActive(hasText);
            mainText.text = hasText ? context.text : string.Empty;
        }

        var options = context.options;
        if (options == null || options.Length == 0)
            return;

        int posIndex = 0;
        int negIndex = 0;

        foreach (var opt in options)
        {
            if (opt == null) continue;

            Button targetButton = null;

            if (opt.side == InTaskOptionSide.Positive)
            {
                if (positiveButtons != null && posIndex < positiveButtons.Length)
                    targetButton = positiveButtons[posIndex++];
            }
            else
            {
                if (negativeButtons != null && negIndex < negativeButtons.Length)
                    targetButton = negativeButtons[negIndex++];
            }

            if (targetButton == null) continue;

            targetButton.gameObject.SetActive(true);

            string optionId = opt.id;
            string action = opt.action;

            var label = targetButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                label.text = opt.label ?? string.Empty;

            if (!string.IsNullOrEmpty(optionId) && label != null)
                _activeLabelByOptionId[optionId] = label;

            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(() => onSelected?.Invoke(optionId, action));
        }
    }
    private void RunFadeTransition(Action renderAction)
    {
        if (!enableFade || canvasGroup == null)
        {
            renderAction?.Invoke();
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(renderAction));
    }
    public bool UpdateOptionLabel(string optionId, string newLabel)
    {
        if (_activeLabelByOptionId.TryGetValue(optionId, out var label) && label != null)
        {
            label.text = newLabel ?? string.Empty;
            return true;
        }
        return false;
    }

    private System.Collections.IEnumerator FadeRoutine(Action renderAction)
    {
        IsTransitioning = true;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        yield return Fade(1f, 0f, fadeOutSeconds);

        renderAction?.Invoke();

        yield return Fade(0f, 1f, fadeInSeconds);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        IsTransitioning = false;
        fadeRoutine = null;
    }

    private System.Collections.IEnumerator Fade(float from, float to, float seconds)
    {
        if (seconds <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        canvasGroup.alpha = from;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            canvasGroup.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

}
