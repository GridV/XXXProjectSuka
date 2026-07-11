using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TaskRepeatCountController : BaseTaskController
{
    private int targetRepeats;
 

    private float minTime;
    private float maxTime;
    private float elapsed;

    private bool isActive;
    private bool isPaused;

    #region DancePartParams

    [SerializeField] 
    private CinemachineDanceSwitcher danceCamera;
    private const int BaseLayer = 0;
    private const string DanceState = "RumbaDance";
    private const string IdleState = "idle_basic_2";
    private bool isDancing;
    private Coroutine retryRoutine;

    private const string ClothingTag = "Clothing";
    private readonly List<GameObject> hiddenClothing = new();

    #endregion

    public override void StartTask()
    {
        base.StartTask();
        if (danceCamera == null)
            danceCamera = ctx != null ? ctx.danceCameraService : null;

        GenerateTargets();
        
        elapsed = 0f;

        isActive = true;
        isPaused = false;

        taskDialogues = ctx.dialogueManager.LoadDialogue("TaskRepeat");

        DialogueEntry entry = GetDialogueEntryById("InitialDialogue");
        if (entry == null)
        {
            Debug.LogWarning("[TaskRepeatCount] Initial dialogue not found.");
            return;
        }

        // Make a safe copy (do not mutate the original DialogueData)
        var formatted = FormatEntry(entry);

        ctx.dialogueUI.ShowDialogueEntry(formatted);
        currentEntryId = entry.id;

        Debug.Log($"[TaskRepeatCount] Target={targetRepeats} Min={minTime:F1}s Max={maxTime:F1}s");
    }
    private void OnDisable()
    {
        danceCamera?.StopDance();
    }
    public DialogueEntry FormatEntry(DialogueEntry entry)
    {
        var formatted = new DialogueEntry
        {
            id = entry.id,
            npcVariants = new System.Collections.Generic.List<string>(),
            playerOptions = entry.playerOptions
        };

        for (int i = 0; i < entry.npcVariants.Count; i++)
            formatted.npcVariants.Add(string.Format(entry.npcVariants[i], targetRepeats));
        return formatted;
    }
    private void Update()
    {
        if (IsUiTransitioning())
            return;

        TickLimits(Time.deltaTime);

        if (!isActive) return;
        if (isPaused) return;

        elapsed += Time.deltaTime;
    }
    /*
    public void PauseTask(string answerId)
    {
        var option = GetInTaskOptionById(answerId);
        currentInTaskContextId = option.nextOption;
        currentInTaskContext = GetInTaskOptions(currentInTaskContextId);
        PauseLimits();
        ctx.rhythmManager.PausePattern();
    }

    public void ResumeTask(string answerId)
    {
        var option = GetInTaskOptionById(answerId);
        currentInTaskContextId = option.nextOption;
        currentInTaskContext = GetInTaskOptions(currentInTaskContextId);
        ResumeLimits();
        ctx.rhythmManager?.ResumePattern();
    }
 
    */
    private void GenerateTargets()
    {
        int minR = currentTask.randomMinRepeats;
        int maxR = currentTask.randomMaxRepeats;

       /*TODO: working copy targetRepeats = Random.Range(minR, maxR + 1);
        targetRepeats = 3; // fixed for testing*/
        targetRepeats = Random.Range(minR, maxR + 1);

        minTime = Mathf.CeilToInt(targetRepeats / 3f);  // 3 per second
        maxTime = targetRepeats + 10f; // extra 10 seconds buffer
    }

    private string Evaluate()
    {
        
        if (elapsed < minTime)
        {
            Debug.Log("[TaskRepeatCount] Too fast. Resetting.");
            elapsed = 0f;
            currentInTaskContext = GetInTaskOptions("reset");
            if (retryRoutine != null)
                StopCoroutine(retryRoutine);

            retryRoutine = StartCoroutine(RetryAfterTooFast());
            return "reset";
        }

        float score = (elapsed <= maxTime) ? 1f : Mathf.Clamp01(maxTime / Mathf.Max(0.01f, elapsed));
        Debug.Log($"[TaskRepeatCount] Completed. Time={elapsed:F2}s Score={score:F2}");

        isActive = false;
        ExitToIdle();
        SetEngagementReward(currentTask != null ? currentTask.engagementReward : 0);
        FinishTask();
        return null;
    }
    private void FinishTask()
    {
        ResetAnimatorToIdle();

        danceCamera?.StopDance();
        ctx.rhythmManager?.StopPattern();
        CleanupLimits();

        DialogueEntry entry = GetDialogueEntryById("FinalDialogue");
        if (entry != null)
        {   
            RequestState(StateRequest.EnterDialogue);
            ctx.dialogueUI.ShowDialogue(taskDialogues, "FinalDialogue");
            currentEntryId = "FinalDialogue";
            
        }
        else
        {
            RequestState(StateRequest.CompleteTask);
        }
    }
    public override bool OnOptionSelected(string answerId, string action)
    {
        base.OnOptionSelected(answerId, action);
        var option = GetPlayerOptionById(answerId);
        switch (action)
        {
            case "StartTask":
                currentInTaskContext = base.GetInTaskOptions(currentInTaskContextId);
                //currentEntryId = option.nextDialogueId;
                EnterDance();
                return true;
            case "NextDialogue":
                if (option == null || string.IsNullOrEmpty(option.nextDialogueId))
                {
                    Debug.LogWarning("[InitialTaskController] Next dialogue ID is missing!");
                    return false;
                }
                DialogueEntry entry = GetDialogueEntryById(option.nextDialogueId);
                var formatted = FormatEntry(entry);
                ctx.dialogueUI.ShowDialogueEntry(formatted);
                currentEntryId = option.nextDialogueId;
                return true;
            case "TryFinish": //TODO: calculate chanse try to start finish task
                 
                return true;

        }
        return false;
    }
    public override string OnInTaskOptionSelected(string answerId, string action)
    {
        base.OnInTaskOptionSelected(answerId, action);

        switch (action)
        {
            case "Finish":
                return Evaluate();
 
            default:
                return null;
        }
    }

    private void ResetAnimatorToIdle()
    {
        isDancing = false;
        ShowClothing();
        danceCamera?.StopDance();

        if (characterAnimator == null)
            return;

        characterAnimator.CrossFadeInFixedTime(IdleState, 0.15f, BaseLayer, 0f);
        Debug.Log("[TaskRepeatCount] Animator reset to idle.");
    }

    #region DancePartMethods
    private System.Collections.IEnumerator RetryAfterTooFast()
    {
        yield return new WaitForSeconds(2f);
    }
    private void ExitToIdle()
    {
        if (characterAnimator == null) return;
        if (!isDancing) return;

        characterAnimator.CrossFade(IdleState, 0.30f, BaseLayer, 0f);
        StopDance();
    }
    private void EnterDance()
    {
        if (characterAnimator == null) return;
        if (isDancing) return;

        isDancing = true;
        characterAnimator.CrossFadeInFixedTime(DanceState, 0.45f, BaseLayer, 0f);

        danceCamera?.StartDance();
        HideClothing();
    }

    private void StopDance()
    {
        if (!isDancing) return;

        isDancing = false;
        danceCamera?.StopDance();
        ShowClothing();
    }
 
    private void ShowClothing()
    {
        foreach (var go in hiddenClothing)
        {
            if (go != null)
                go.SetActive(true);
        }

        hiddenClothing.Clear();
    }

    private void HideClothing()
    {
        hiddenClothing.Clear();

        var clothes = GameObject.FindGameObjectsWithTag(ClothingTag);
        foreach (var go in clothes)
        {
            if (go.activeSelf)
            {
                hiddenClothing.Add(go);
                go.SetActive(false);
            }
        }
    }

    #endregion
}
