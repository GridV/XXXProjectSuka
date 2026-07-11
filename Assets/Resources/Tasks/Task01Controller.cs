using System.Collections;
using UnityEngine;

public class Task01Controller : BaseTaskController
{
  

    [Header("Animation Triggers")]
    [SerializeField] private string armsStartTrigger = "ArmsStartSequence";
    [SerializeField] private string headStartTrigger = "HeadComeClose";
    [SerializeField] private CharacterAnimationService animationService;
    private AnimatorEventRouter router;

    private bool hasStartSequencePlayed;
    private bool rhythmStarted;
  
    public override void Init(TaskData task)
    {
        base.Init(task);
    }
    public override void OnTaskStarted(TaskContext context)
    {
        rhythmStarted = false;

        if (hasStartSequencePlayed)
            return;

        hasStartSequencePlayed = true;
        Animator animatorToUse = null;
        if (characterAnimator != null)
        {
            animatorToUse = characterAnimator;
            router = characterAnimator.GetComponent<AnimatorEventRouter>();
        }
        else { animatorToUse = context?.animator; 
        router = animatorToUse.GetComponent<AnimatorEventRouter>();}



        if (animatorToUse == null && context != null)
        {
            // If your TaskContext contains an Animator reference, assign it here later.
            // For now, keep characterAnimator assigned in the inspector.
        }

        if (animatorToUse == null)
        {
            Debug.LogWarning("[Task01Controller] Character Animator reference is missing.");
            return;
        }

        if (router != null)
        {
            router.OnEvent -= OnAnimatorEvent;
            router.OnEvent += OnAnimatorEvent;
        }


        if (!string.IsNullOrWhiteSpace(armsStartTrigger))
            animatorToUse.SetTrigger(armsStartTrigger);

        string headTriggerToUse = string.IsNullOrWhiteSpace(headStartTrigger) ? armsStartTrigger : headStartTrigger;
        if (!string.IsNullOrWhiteSpace(headTriggerToUse))
            animatorToUse.SetTrigger(headTriggerToUse);
    }
    private void OnAnimatorEvent(string key)
    {
        if (rhythmStarted) return;
        switch(key)
        {
            case "RhythmStart":
                //ctx.cameraMoveService?.StopMove();
                rhythmStarted = true;
                GameContext.Instance.TaskContext.rhythmManager?.StartRhythm(currentTask.rhythmPattern);
                ResumeLimits();
                break;
            case "CamCloser":
                ctx.cameraMoveService?.MoveToDelayed(
                new Vector3(0.71f, 3.21f, 2.85f),
                new Vector3(6.36f, 180f, 0f),
                2f,1f);
                break;
        }
  

    }
    public override void StartTask()
    {
        base.StartTask();

        Debug.Log("[InitialTaskController] Starting initial dialogue sequence...");

        taskDialogues = ctx.dialogueManager.LoadDialogue("Task_01");

        if (taskDialogues != null)
        {
            ctx.dialogueUI.ShowDialogue(taskDialogues, dialogueEntryId);
            currentEntryId = dialogueEntryId;
            Debug.Log("[InitialTaskController] Dialogue shown: Task_01");
        }
        else
        {
            Debug.LogWarning("[InitialTaskController] Dialogue data not found!");
        }
    }
    protected override void OnLimitReachedInternal(TaskLimitReachedReason reason)
    {
       base.OnLimitReachedInternal(reason);
       ctx.characterAnimation.CrossFadeBase("Idle_basic", 0.15f);
       ctx.characterAnimation.FadeLayerWeight("ArmsLayer", 0f, 0.35f);
       TryToFinalDialogue();

    }
    public override string OnInTaskOptionSelected(string answerId, string action)
    {
        base.OnInTaskOptionSelected(answerId, action);
        switch( action)
        {
            case "PauseTask":
                PauseTask(answerId);
                return null;  
            case "ContinueTask":
                ResumeTask(answerId);
                return null;  
            case "CancelTask":
                {
                    TaskManager.Instance.OnTaskCompleted();
                    PauseTask(answerId);
                    return null;  
                }

            case "NextDialogue":
                {
                    var option = GetInTaskOptionById(answerId);
                    currentInTaskContextId = option.nextOption;
                    currentInTaskContext = GetInTaskOptions(currentInTaskContextId);
                    return null;  
                }
            default:
                return null;

        }
    }
    private void OnBeat()
    {
        NotifyLimitBeat();
    }

    public override bool OnOptionSelected(string answerId, string action)
    {
        Debug.Log($"[InitialTaskController] Option selected: {answerId}, action: {action}");
        var option = GetPlayerOptionById(answerId);

        if (base.OnOptionSelected(answerId, action)) 
        {
            switch (action)
            {
                case "StartRhythm":
                    
                   
                    currentInTaskContext = base.GetInTaskOptions(currentInTaskContextId);
                    if (ctx.rhythmManager != null)
                    {
                        ctx.rhythmManager.OnBeat += OnBeat;
                        OnTaskStarted(ctx);
                    }          
                    characterAnimator.Play("Arms_ComeClose", 0, 0f);

                    return true;

                default:
                    Debug.Log($"[InitialTaskController] No specific handler for action '{action}'");
                    return true;
            }
        
        }

        switch (action)
        {

            case "NextDialogue":
                if (option == null || string.IsNullOrEmpty(option.nextDialogueId))
                {
                    Debug.LogWarning("[InitialTaskController] Next dialogue ID is missing!");
                    return false;
                }
                ctx.dialogueUI.ShowDialogue(taskDialogues, option.nextDialogueId);
                currentEntryId = option.nextDialogueId;
                return true;

            case "Retry":
                Debug.Log("[InitialTaskController] Restarting dialogue...");
                StartTask();
                return true;

            default:
                Debug.Log($"[InitialTaskController] No specific handler for action '{action}'");
                return false;
        }
    }
    private void Update()
    {
        if (IsUiTransitioning())
            return;

        TickLimits(Time.deltaTime);

        TickLimits(Time.deltaTime);
    }
    public override void OnTaskEnded(TaskContext context)
    {
        hasStartSequencePlayed = false;
        if (ctx?.rhythmManager != null)
            ctx.rhythmManager.OnBeat -= OnBeat;
    }
    public void PauseTask(string answerId)
    {
        var option = GetInTaskOptionById(answerId);
        currentInTaskContextId = option.nextOption;
        currentInTaskContext = GetInTaskOptions(currentInTaskContextId);
        PauseLimits();
        characterAnimator.CrossFade("Idle_basic", 0.15f, 0, 0f);
        ctx.rhythmManager.PausePattern();
        ctx.characterAnimation.CrossFadeBase("Idle_basic", 0.15f);
        ctx.characterAnimation.FadeLayerWeight("ArmsLayer", 0f, 0.35f);
    }

    public void ResumeTask(string answerId)
    {
        var option = GetInTaskOptionById(answerId);
        currentInTaskContextId = option.nextOption;
        currentInTaskContext = GetInTaskOptions(currentInTaskContextId);
        ResumeLimits();
        ctx.rhythmManager?.ResumePattern();
        ctx.characterAnimation.FadeLayerWeight("ArmsLayer", 1f, 0.25f);
        ctx.characterAnimation.CrossFadeOnLayer("ArmsLayer", "Arms_SlowJerkLoop", 0.12f);
    }
    public void TryToFinalDialogue()
    {
        ctx.cameraMoveService?.ResetToDefault(0.6f);
        DialogueEntry entry = base.GetDialogueEntryById("FinalDialogue");
        if( entry != null)
        {
              ctx.dialogueUI.ShowDialogue(taskDialogues, "FinalDialogue");
        currentEntryId = "FinalDialogue";
            ctx.rhythmManager?.StopPattern();
            base.CleanupLimits();
        }
        else
        {
            RequestState(StateRequest.CompleteTask);
        }

    }
 
}
