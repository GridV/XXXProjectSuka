using System.Collections.Generic;
using UnityEngine;

public abstract class TaskPartsBase
{
    protected readonly BaseTaskController owner;
    protected readonly TaskContext ctx;
    protected readonly Animator animator;
    protected readonly DialogueUIController dialogueUI;
    protected readonly TaskOptionsUIController taskOptionsUI;
    protected readonly StateController stateController;

    protected readonly List<string> availableAnimationKeys;
    protected readonly List<string> availableSoundKeys;
    protected readonly List<string> availableEmotionKeys;

    protected float localTimer;
    protected int localCounter;
    protected bool isEntered;

    protected string dialogueId;
    protected string contextId;

    private FinalTaskController.TaskPartSignal pendingSignal = FinalTaskController.TaskPartSignal.None;

    public abstract string PartId { get; }

    protected TaskPartsBase(BaseTaskController owner)
    {
        this.owner = owner;

        ctx = GameContext.Instance != null ? GameContext.Instance.TaskContext : null;
        animator = ctx != null ? ctx.animator : null;
        dialogueUI = ctx != null ? ctx.dialogueUI : null;

        taskOptionsUI = Object.FindFirstObjectByType<TaskOptionsUIController>();
        stateController = Object.FindFirstObjectByType<StateController>();

        availableAnimationKeys = new List<string>();
        availableSoundKeys = new List<string>();
        availableEmotionKeys = new List<string>();
    }

    public virtual void Enter()
    {
        isEntered = true;
        localTimer = 0f;
        localCounter = 0;
    }

    public virtual void Exit()
    {
    }

    public virtual void Tick(float deltaTime)
    {
    }

    public virtual bool HandleDialogueOption(string answerId, string action)
    {
        return false;
    }

    public virtual string HandleInTaskOption(string answerId, string action)
    {
        return null;
    }

    protected void EmitSignal(FinalTaskController.TaskPartSignal signal)
    {
        pendingSignal = signal;
    }

    public FinalTaskController.TaskPartSignal ConsumeSignal()
    {
        FinalTaskController.TaskPartSignal signal = pendingSignal;
        pendingSignal = FinalTaskController.TaskPartSignal.None;
        return signal;
    }

    public virtual void ClearSignal()
    {
        pendingSignal = FinalTaskController.TaskPartSignal.None;
    }
}