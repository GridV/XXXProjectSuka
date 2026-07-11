using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// Базовый контроллер для любого задания.
/// Все наследники (InitialTaskController, SpecialTaskController и т.д.)
/// будут переопределять его методы при необходимости.
/// </summary>
public class BaseTaskController : MonoBehaviour
{
    public enum StateRequest
    {
        None = 0,
        EnterDialogue,
        EnterExecutingTask,
        EnterPaused,
        CompleteTask
    }
    protected TaskData currentTask;
    public string currentEntryId;
    protected DialogueData taskDialogues;
    protected string dialogueEntryId = "InitialDialogue";
    protected TaskContext ctx;
    protected InTaskContext currentInTaskContext;
    protected string currentInTaskContextId = "default";
    protected TaskManager TaskManager => TaskManager.Instance;
    private TaskLimitController limitController;
    [Header("References")]
    [SerializeField] protected Animator characterAnimator;
    protected TaskOptionsUIController taskOptionsUI;
    public virtual void OnTaskStarted(TaskContext context) { }
    public virtual void OnTaskPaused(TaskContext context) { }
    public virtual void OnTaskResumed(TaskContext context) { }
    public virtual void OnTaskEnded(TaskContext context) { }

    public event Action<StateRequest> OnStateRequested;

    // TaskLimits *///*
    public virtual void StartTask()
    {
        TryInitializeLimits();
        ClearEngagementReward();
    }
    public virtual void EndTask()
    {
        CleanupLimits();
    }
    protected void PauseLimits()
    {
        limitController?.Pause();
    }

    protected void ResumeLimits()
    {
        limitController?.Resume();
    }
    private void TryInitializeLimits()
    {
        if (currentTask == null)
            return;

        if (currentTask.limitType == TaskLimitType.None)
            return;

        limitController = new TaskLimitController(currentTask);
        limitController.OnLimitReached += OnLimitReachedInternal;
        limitController.Start();
        PauseLimits();
    }
   
 
    protected virtual void OnLimitReachedInternal(TaskLimitReachedReason reason)
    {
        // DEFAULT behavior for ANY task:
        // Ask FSM to go to dialogue
        RequestState(StateRequest.EnterDialogue);
    }

    private StateRequest pendingStateRequest = StateRequest.None;

    protected void RequestState(StateRequest request)
    {
        pendingStateRequest = request;
        OnStateRequested?.Invoke(request);
    }

    public StateRequest ConsumeStateRequest()
    {
        var req = pendingStateRequest;
        pendingStateRequest = StateRequest.None;
        return req;
    }
    protected void TickLimits(float deltaTime)
    {
        limitController?.Tick(deltaTime);
    }
    protected void NotifyLimitBeat()
    {
        limitController?.OnRhythmStep();
    }
    protected void CleanupLimits()
    {
        if (limitController == null)
            return;

        limitController.OnLimitReached -= OnLimitReachedInternal;
        limitController.Stop();
        limitController = null;
    }
    // TaskLimits *///*

    /// <summary> Инициализация контроллера конкретным заданием. </summary>
    public virtual void Init(TaskData task)
    {
        currentTask = task;
        GameContext.Instance.TaskContext.controller = this;
        ctx = GameContext.Instance.TaskContext;
        Debug.Log($"[BaseTaskController] Initialized with task: {task.id}");
        characterAnimator = ctx?.animator;
        var stateController = FindObjectOfType<StateController>();
        stateController?.OnTaskControllerChanged(this);
        taskOptionsUI = FindFirstObjectByType<TaskOptionsUIController>();
    }
    protected bool IsUiTransitioning()
    {
        var ctx = GameContext.Instance != null ? GameContext.Instance.TaskContext : null;

        if (ctx != null && ctx.dialogueUI != null && ctx.dialogueUI.IsTransitioning)
            return true;

        return taskOptionsUI != null && taskOptionsUI.IsTransitioning;
    }
    /// <summary> Вызывается, когда пользователь выбирает вариант ответа. </summary>
    public virtual bool OnOptionSelected(string answerId, string action)
    {
        Debug.Log($"[BaseTaskController] Option selected: {answerId}, action: {action}");

        // Базовая обработка — если нет спец. логики
        switch (action)
        {
            #region TTFOptions
            case "TTFGoToEnd":
                Debug.Log("[BaseTaskController] FinishSession requested (placeholder).");
                // TODO: jump to final task / final scene when implemented
                // For now: return to task gameplay to avoid dead-end loops
                RequestState(StateRequest.EnterExecutingTask);        
                return true;
            case "TTFBackToTask":
                PlayerPrefs.SetInt("EngagementReward", 0); // safety: ensure no reward is applied
                SetEngagementReward(currentTask != null ? currentTask.engagementReward : 0);
                TaskManager.StartNextTask();
                return true;
            case "TTFCancelSession":
                CancelSessionLogic();
                return true;
            #endregion

            case "NextTask":
                Debug.Log("[InitialTaskController] Starting session...");
                SetEngagementReward(currentTask != null ? currentTask.engagementReward : 0);
                TaskManager.OnTaskCompleted();
                TaskManager.StartNextTask();
                return true;
            case "StartRhythm":
                if (currentTask.rhythmPattern == null)
                {
                    Debug.LogWarning("[BaseTaskController] RhythmPattern is null, cannot start rhythm.");
                    return false;
                }
                return true;
            case "CancelSession":
                CancelSessionLogic();
                return true;
            case "TryFinish":
                HandleTryFinish();
                return true;

            default:
                return false;
        }
    }
    public void CancelSessionLogic()
    {
        Debug.Log("[InitialTaskController] Player cancelled the session.");
        GameContext.Instance.TaskContext.controller = null;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuURP");
    }
    public InTaskOption GetInTaskOptionById(string optionId)
    {
        if (currentInTaskContext == null)
        {
            Debug.LogWarning("[InitialTaskController] Current InTaskContext is null!");
            return null;
        }
        foreach (var option in currentInTaskContext.options)
        {
            if (option.id == optionId)
            {
                return option;
            }
        }
        Debug.LogWarning($"[InitialTaskController] Option with ID '{optionId}' not found in current InTaskContext.");
        return null;
    }
    public DialogueEntry GetDialogueEntryById(string dialogueId)
    {
        if (taskDialogues == null || taskDialogues.dialogues == null)
        {
            return null;
        }
        foreach (var entry in taskDialogues.dialogues)
        {
            if (entry.id == dialogueId)
                return entry;
        }
        Debug.LogWarning($"[DialogueManager] Dialogue entry with ID {dialogueId} not found.");
        return null;
    }
    public PlayerOption GetPlayerOptionById(string optionId)
    {
        DialogueEntry entry = GetDialogueEntryById(currentEntryId);
        if (entry == null || entry.playerOptions == null)
            return null;
        foreach (var option in entry.playerOptions)
        {
            if (option.id == optionId)
                return option;
        }
        Debug.LogWarning($"[DialogueManager] Player option with ID {optionId} not found in dialogue entry {entry.id}.");
        return null;
    }
    public virtual InTaskContext GetInTaskOptions(string contextId)
    {
        if(string.IsNullOrEmpty(contextId))
        {
            contextId = currentInTaskContextId;
        }
        if (currentTask == null || currentTask.inTaskContexts == null)
            return null;

        if (string.IsNullOrEmpty(contextId))
            return null;

        foreach (var inctx in currentTask.inTaskContexts)
        {
            if (inctx != null && inctx.id == contextId)
                return inctx;
        }

        return null;
    }
 
    public virtual InTaskContext GetInTaskOptions()
    {
        return currentInTaskContext;
    }
    public virtual string OnInTaskOptionSelected(string answerId, string action)
    {
        Debug.Log($"[BaseTaskController] Option selected: {answerId}, action: {action}");
        switch(action)
        {
            case "PauseTask":
                PauseTask(answerId);
                return null;
            case "ContinueTask":
                ResumeTask(answerId);
                return null;
        }
         return null;
    }
    protected void  HandleTryFinish()
    {
        var ctx = GameContext.Instance.TaskContext;

        float engagement = ctx.sessionStats != null ? ctx.sessionStats.engagement : 0;
        float elapsedSeconds = ctx.sessionStats != null ? ctx.sessionStats.SessionDurationSeconds : 0f;

        bool allowFinish = GameContext.Instance.Decisions.TryFinishDecision(engagement, elapsedSeconds);

        string entryId = allowFinish ? "FinishTrue" : "FinishFalse";
        DialogueEntry entry = ctx.dialogueManager.LoadDialogueAndGetEntry(
        "TryTofinish",
        entryId);

        if (entry == null)
            return;

        // Передаём entry в UI (как ты обычно это делаешь)
        ctx.dialogueUI.ShowDialogueEntry(entry);
    }
    public void ShowCurrentDialogue()
    {    
        if (currentEntryId == null || string.IsNullOrEmpty(currentEntryId))
        {
            Debug.LogWarning("[InitialTaskController] Next dialogue ID is missing!");
            return;
        }
        ctx.dialogueUI.ShowDialogue(taskDialogues, currentEntryId);
        return;
    }
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
    protected void SetEngagementReward(int value)
    {
        PlayerPrefs.SetInt("EngagementReward", Mathf.Max(0, value));
    }

    protected void ClearEngagementReward()
    {
        PlayerPrefs.SetInt("EngagementReward", 0);
    }
    protected void ShowDialogue(string dialogueId)
    {
        DialogueEntry entry = GetDialogueEntryById(dialogueId);
        if (entry != null)
        {
            RequestState(StateRequest.EnterDialogue);
            ctx.dialogueUI.ShowDialogue(taskDialogues, dialogueId);
            currentEntryId = dialogueId;            
        }
        else
        {
            RequestState(StateRequest.CompleteTask);
        }
    }

}
