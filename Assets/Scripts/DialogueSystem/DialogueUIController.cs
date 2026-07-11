using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIController : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private bool enableFade = true;
    [SerializeField] private float fadeOutSeconds = 0.5f;
    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private CanvasGroup canvasGroup;

    public bool IsTransitioning { get; private set; }
    private Coroutine fadeRoutine;


    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private TextMeshProUGUI npcText;

    [Header("Left (Positive)")]
    [SerializeField] private Button[] leftButtons;

    [Header("Right (Negative)")]
    [SerializeField] private Button[] rightButtons;

    [SerializeField] private StateController stateController;

    private DialogueEntry currentEntry;

    [Header("Main Text Container")]
    [SerializeField] private GameObject mainTextContainer;

    private void SetMainText(string text)
    {
        if (npcText == null) return;
        npcText.text = text ?? string.Empty;
    }

    public void ShowDialogueForTask(string taskId)
    {
        var data = dialogueManager.LoadDialogue(taskId);
        if (data == null) return;

        currentEntry = dialogueManager.GetRandomEntry();
        npcText.text = currentEntry.npcVariants[Random.Range(0, currentEntry.npcVariants.Count)];

        // Отдельные списки для типов
        var positives = new List<PlayerOption>();
        var negatives = new List<PlayerOption>();

        foreach (var option in currentEntry.playerOptions)
        {
            if (option.type.ToLower().Contains("pos")) positives.Add(option);
            else negatives.Add(option);
        }

        SetupButtons(leftButtons, positives);
        SetupButtons(rightButtons, negatives);
    }
    public void HideAllOptions()
    {
        // Скрываем все кнопки слева
        if (leftButtons != null)
            foreach (var btn in leftButtons)
                if (btn != null)
                    btn.gameObject.SetActive(false);

        // Скрываем все кнопки справа
        if (rightButtons != null)
            foreach (var btn in rightButtons)
                if (btn != null)
                    btn.gameObject.SetActive(false);

        if (npcText != null)
             mainTextContainer.gameObject.SetActive(false);
    }

    private void SetupButtons(Button[] buttons, List<PlayerOption> options)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < options.Count)
            {
                var option = options[i];
                string text = option.variants[Random.Range(0, option.variants.Count)];

                buttons[i].gameObject.SetActive(true);
                var tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                tmp.text = text;
                tmp.fontSize = 36; // увеличенный шрифт
                tmp.enableAutoSizing = false;
                Debug.Log($"[SetupButtons] Binding click for {buttons[i].name} -> {text}");
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() =>
                {
                    OnAnswerSelected(option.id, option.action);
                });
            }
            else buttons[i].gameObject.SetActive(false);
        }
    }
    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        HideAllOptions();
        if (stateController == null)
            stateController = FindFirstObjectByType<StateController>();
        Debug.Log("[DialogueUIController] Awake: UI reset (dialogue and options hidden).");
    }
    public void ShowDialogue(DialogueData dialogue, string dialogueIdToShow)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("[DialogueUIController] No dialogue data to show.");
            return;
        }
        DialogueEntry entry = null;
        for (int i = 0; i < dialogue.dialogues.Count; i++)
        {
            if (dialogue.dialogues[i].id == dialogueIdToShow)
                entry = dialogue.dialogues[i];
        }
        if( entry == null)
        {
            Debug.LogWarning($"[DialogueUIController] Dialogue entry with ID {dialogueIdToShow} not found.");
            return;
        }

        ShowDialogueEntry(entry);
    }
    public void ShowDialogueEntry(DialogueEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("[DialogueUIController] Null entry passed!");
            return;
        }

        RunFadeTransition(() => RenderDialogueEntry(entry));
    }
    private void RenderDialogueEntry(DialogueEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("[DialogueUIController] Null entry passed!");
            return;
        }

        // Показываем реплику NPC
        string npcLine = entry.npcVariants.Count > 0
            ? entry.npcVariants[Random.Range(0, entry.npcVariants.Count)]
            : "(no NPC line)";
        npcText.text = npcLine;
        mainTextContainer.gameObject.SetActive(true);
        npcText.gameObject.SetActive(true);

        // Разделяем опции по типам (Positive/Negative)
        var positives = new List<PlayerOption>();
        var negatives = new List<PlayerOption>();

        foreach (var opt in entry.playerOptions)
        {
            if (opt.type.ToLower().Contains("pos"))
                positives.Add(opt);
            else
                negatives.Add(opt);
        }

        // Настраиваем кнопки
        SetupButtons(leftButtons, positives);
        SetupButtons(rightButtons, negatives);
    }
    public void ShowInTaskContext(InTaskContext context)
    {
        RunFadeTransition(() => RenderInTaskContext(context));
    }
    private void RenderInTaskContext(InTaskContext context)
    {
        HideAllOptions();

        if (context == null)
        {
            SetMainText(string.Empty);
            return;
        }

        // Main description / NPC line
        SetMainText(context.text);

        if (context.options == null || context.options.Length == 0)
            return;

        // Convert InTaskOption -> PlayerOption-like flow
        var positives = new List<InTaskOption>();
        var negatives = new List<InTaskOption>();

        foreach (var opt in context.options)
        {
            if (opt == null) continue;

            if (opt.side == InTaskOptionSide.Positive)
                positives.Add(opt);
            else
                negatives.Add(opt);
        }

        SetupInTaskButtons(leftButtons, positives);
        SetupInTaskButtons(rightButtons, negatives);
    }
    private void SetupInTaskButtons(Button[] buttons, List<InTaskOption> options)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < options.Count)
            {
                var option = options[i];

                buttons[i].gameObject.SetActive(true);

                var tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    tmp.text = option.label ?? string.Empty;
                    tmp.fontSize = 36;
                    tmp.enableAutoSizing = false;
                }

                buttons[i].onClick.RemoveAllListeners();

                string id = option.id;
                string action = option.action;

                buttons[i].onClick.AddListener(() =>
                {
                    var ctx = GameContext.Instance.TaskContext;
                    var controller = ctx.controller;

                    if (controller != null)
                        controller.OnOptionSelected(id, action);
                    else
                        Debug.LogWarning("[DialogueUIController] No controller in GameContext!");
                });
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }
    private void RunFadeTransition(System.Action renderAction)
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

    private System.Collections.IEnumerator FadeRoutine(System.Action renderAction)
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

    private void OnAnswerSelected(string answerId, string action)
    {
        stateController.OnDialogueAction(answerId, action);
    }
}
