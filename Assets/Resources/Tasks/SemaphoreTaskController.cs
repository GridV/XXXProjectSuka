using System.Collections;
using UnityEngine;

public sealed class SemaphoreTaskController : BaseTaskController
{
    private enum Phase
    {
        None = 0,
        Green = 1,
        Red = 2
    }

    public enum GreenEventText
    {
        None = 0,
        VariantA = 1,
        VariantB = 2
    }

    private CharacterRigService Rig => ctx?.rigService;
    private FacialExpressionController Face => ctx?.facial;

    private const float RigFade = 0.2f;

    [Header("Loop Settings")]
    [SerializeField] private int minLoops = 1;
    [SerializeField] private int maxLoops = 2;

    [Header("Green Timer")]
    [SerializeField] private float greenMinSeconds = 10f;
    [SerializeField] private float greenMaxSeconds = 16f;

    [Header("Phase Pause")]
    [SerializeField] private float phasePauseSeconds = 3.0f;

    [Header("Red Timer")]
    [SerializeField] private float redMinSeconds = 10f;
    [SerializeField] private float redMaxSeconds = 16f;

    [Header("Transition")]
    [SerializeField] private float transitionSeconds = 1f;

    [Header("In-Task Context Ids")]
    [SerializeField] private string greenContextId = "greenDefault";
    [SerializeField] private string redContextId = "redDefault";

    private Phase phase = Phase.None;
    private bool isActive;
    private bool isFailed;
    private bool isPaused;

    private int targetLoops;
    private int completedLoops;

    private Coroutine loopRoutine;

    // =========================
    // Full-body stop triggers
    // =========================

    [Header("Green Stop Animation")]
    [SerializeField] private string greenStopTrigger = "GreenStop";
    [SerializeField] private float greenStopWaitSeconds = 0.30f;
    private int greenStopTriggerHash;

    [Header("Red Stop Animation")]
    [SerializeField] private string redStopTrigger = "RedStop";
    [SerializeField] private float redStopWaitSeconds = 0.60f;
    private int redStopTriggerHash;

    // =========================
    // Animator - FullBody states
    // =========================

    [Header("Animator - Green FullBody")]
    [SerializeField] private string baseSittingState = "Base Layer.Semaphore.sitting";
    [SerializeField] private string baseGreenPointState = "Base Layer.Semaphore.pointControl";
    [SerializeField] private float greenEnterBlend = 0.20f;
    [SerializeField] private float greenExitBlend = 0.20f;

    [Header("Animator - Red FullBody")]
    [SerializeField] private string baseRedStartState = "Base Layer.Semaphore.Red.selfPstart";
    [SerializeField] private float redEnterBlend = 0.40f;

    // =========================
    // Green event system
    // =========================

    [Header("Green Random Event")]
    [SerializeField, Range(0f, 1f)] private float greenEventChance = 0.25f;

    [Header("Green Event Text")]
    [SerializeField] private GreenEventText greenEventTextVariant = GreenEventText.VariantA;

    [SerializeField, TextArea] private string greenTextTemplateA = "Just stroke as I show you! Until: {0}";
    [SerializeField, TextArea] private string greenTextTemplateB = "Keep going. Stop only when: {0}";

    [SerializeField] private string greenEventWordA = "you edged";
    [SerializeField] private string greenEventWordB = "you cant anymore!";

    [Header("In-Task Option Ids")]
    [SerializeField] private string eventAchievedOptionId = "EventAchieved";

    private bool greenEventAvailable;
    private string greenEventPhrase;
    private bool skipCurrentPhase;
    private bool greenStopTriggered;

    // =========================
    // Task lifecycle
    // =========================

    public override void StartTask()
    {
        base.StartTask();

        taskDialogues = ctx.dialogueManager.LoadDialogue("SemaphoreTask");
        if (taskDialogues != null)
        {
            currentEntryId = "InitialDialogue";
            ctx.dialogueUI.ShowDialogue(taskDialogues, currentEntryId);
        }

        redStopTriggerHash = Animator.StringToHash(redStopTrigger);
        greenStopTriggerHash = Animator.StringToHash(greenStopTrigger);

        Debug.Log("[SemaphoreTask] Ready. Waiting for StartTask action.");
    }

    public override bool OnOptionSelected(string answerId, string action)
    {
        if (base.OnOptionSelected(answerId, action))
            return true;

        if (action == "StartTask")
        {
            StartSemaphoreLoop();
            return true;
        }

        return false;
    }

    public override string OnInTaskOptionSelected(string optionId, string action)
    {
        base.OnInTaskOptionSelected(optionId, action);

        switch (action)
        {
            case "PauseTask":
                isPaused = true;
                return null;

            case "ContinueTask":
                isPaused = false;
                RestorePhaseContext();
                RequestState(StateRequest.EnterExecutingTask);
                return null;

            case "EndSession":
                isFailed = true;
                isActive = false;
                StopLoopRoutine();
                return null;

            case "EventAchieved":
                if (phase == Phase.Green && greenEventAvailable)
                {
                    greenEventAvailable = false;
                    skipCurrentPhase = true;
                    TriggerGreenStopOnce();
                }
                return null;

            default:
                return null;
        }
    }

    // =========================
    // Loop logic
    // =========================

    private void StartSemaphoreLoop()
    {
        if (isActive)
            return;

        isActive = true;
        isFailed = false;
        isPaused = false;

        completedLoops = 0;
        targetLoops = Random.Range(minLoops, maxLoops + 1);

        Debug.Log($"[SemaphoreTask] Started. TargetLoops={targetLoops}");

        SetPhase(Phase.Green);

        StopLoopRoutine();
        loopRoutine = StartCoroutine(LoopRoutine());

        RequestState(StateRequest.EnterExecutingTask);
    }

    private IEnumerator LoopRoutine()
    {
        while (isActive && !isFailed)
        {
            if (completedLoops >= targetLoops)
            {
                FinishTask(success: true);
                yield break;
            }

            float duration = GetCurrentPhaseDuration();
            float t = 0f;

            while (t < duration && !skipCurrentPhase)
            {
                if (!isActive || isFailed)
                    yield break;

                if (!isPaused)
                    t += Time.deltaTime;

                yield return null;
            }

            skipCurrentPhase = false;

            float tr = 0f;
            while (tr < transitionSeconds)
            {
                if (!isActive || isFailed)
                    yield break;

                if (!isPaused)
                    tr += Time.deltaTime;

                yield return null;
            }

            if (phase == Phase.Green)
            {
                TriggerGreenStopOnce();

                float stopT = 0f;
                while (stopT < greenStopWaitSeconds)
                {
                    if (!isActive || isFailed) yield break;
                    if (!isPaused) stopT += Time.deltaTime;
                    yield return null;
                }

                float pauseT = 0f;
                while (pauseT < phasePauseSeconds)
                {
                    if (!isActive || isFailed) yield break;
                    if (!isPaused) pauseT += Time.deltaTime;
                    yield return null;
                }

                SetPhase(Phase.Red);
            }
            else
            {
                // Optional: you can remove RedStop if you decide Red does not need a stop gesture.
                TriggerRedStopOnce();

                float stopT = 0f;
                while (stopT < redStopWaitSeconds)
                {
                    if (!isActive || isFailed) yield break;
                    if (!isPaused) stopT += Time.deltaTime;
                    yield return null;
                }

                float pauseT = 0f;
                while (pauseT < phasePauseSeconds)
                {
                    if (!isActive || isFailed) yield break;
                    if (!isPaused) pauseT += Time.deltaTime;
                    yield return null;
                }

                completedLoops++;
                SetPhase(Phase.Green);
            }
        }
    }

    private float GetCurrentPhaseDuration()
    {
        return phase == Phase.Green
            ? Random.Range(greenMinSeconds, greenMaxSeconds)
            : Random.Range(redMinSeconds, redMaxSeconds);
    }

    // =========================
    // Phase switching
    // =========================

    private void SetPhase(Phase next)
    {
        phase = next;

        switch (phase)
        {
            case Phase.Green:
                greenStopTriggered = false;

                greenEventAvailable = Random.value < greenEventChance;
                greenEventPhrase = greenEventAvailable ? PickGreenEventPhrase() : null;

                ApplyGreenRig();
                ApplyInTaskContext(greenContextId);

                PlayGreenPointControlFullBody();

                Debug.Log($"[SemaphoreTask] Phase=GREEN ({completedLoops}/{targetLoops}) Event={greenEventAvailable} Phrase='{greenEventPhrase}'");
                break;

            case Phase.Red:
                greenEventAvailable = false;
                greenEventPhrase = null;

                ApplyRedRig();
                ApplyInTaskContext(redContextId);

                PlaySemaphoreRed();

                Debug.Log($"[SemaphoreTask] Phase=RED ({completedLoops}/{targetLoops})");
                break;
        }

        RequestState(StateRequest.EnterExecutingTask);
    }

    private void RestorePhaseContext()
    {
        if (phase == Phase.Green)
            ApplyInTaskContext(greenContextId);
        else if (phase == Phase.Red)
            ApplyInTaskContext(redContextId);
    }

    // =========================
    // Rig behavior
    // =========================

    private void ApplyGreenRig()
    {
        // Note: if Green is full-body pointControl, keep rigs off to avoid conflicts.
        Rig?.SetArmsWeight(0f, RigFade);
        Rig?.SetLookWeight(0f, RigFade);
        Face?.SetExpression("commanding");
    }

    private void ApplyRedRig()
    {
        Rig?.SetArmsWeight(0f, RigFade);
        Rig?.SetLookWeight(0f, RigFade);
        Face?.SetExpression("red");
    }

    private void ResetRig()
    {
        Rig?.SetArmsWeight(0f, 0.15f);
        Rig?.SetLookWeight(0f, 0.15f);
        Face?.ResetExpression();
    }

    // =========================
    // In-task UI (event option + text)
    // =========================

    public override InTaskContext GetInTaskOptions(string contextId)
    {
        var baseCtx = base.GetInTaskOptions(contextId);
        if (baseCtx == null)
            return null;

        var ctxCopy = CloneContext(baseCtx);

        bool isGreenDefaultContext = phase == Phase.Green && contextId == greenContextId;

        if (isGreenDefaultContext)
            ctxCopy.text = BuildGreenText(ctxCopy.text);

        if (isGreenDefaultContext && greenEventAvailable)
            return CloneWithExtraOption(ctxCopy, CreateEventAchievedOption());

        return ctxCopy;
    }

    private string BuildGreenText(string baseText)
    {
        if (!greenEventAvailable || string.IsNullOrEmpty(greenEventPhrase))
            return baseText;

        string template = greenEventTextVariant switch
        {
            GreenEventText.VariantB => greenTextTemplateB,
            _ => greenTextTemplateA
        };

        return string.Format(template, greenEventPhrase);
    }

    private string PickGreenEventPhrase()
    {
        return Random.value < 0.5f ? greenEventWordA : greenEventWordB;
    }

    private InTaskOption CreateEventAchievedOption()
    {
        return new InTaskOption
        {
            id = eventAchievedOptionId,
            label = "Event achieved",
            action = "EventAchieved",
            side = InTaskOptionSide.Positive,
            nextOption = string.Empty
        };
    }

    private InTaskContext CloneContext(InTaskContext original)
    {
        if (original == null)
            return null;

        var src = original.options;
        int srcLen = src != null ? src.Length : 0;

        var clone = new InTaskContext
        {
            id = original.id,
            text = original.text,
            options = new InTaskOption[srcLen]
        };

        for (int i = 0; i < srcLen; i++)
            clone.options[i] = src[i];

        return clone;
    }

    private InTaskContext CloneWithExtraOption(InTaskContext original, InTaskOption extra)
    {
        if (original == null)
            return null;

        var src = original.options;
        int srcLen = src != null ? src.Length : 0;

        var clone = new InTaskContext
        {
            id = original.id,
            text = original.text,
            options = new InTaskOption[srcLen + 1]
        };

        for (int i = 0; i < srcLen; i++)
            clone.options[i] = src[i];

        clone.options[srcLen] = extra;
        return clone;
    }

    // =========================
    // Animator calls
    // =========================

    private void PlaySemaphoreRed()
    {
        if (characterAnimator == null)
            return;

        Rig?.SetArmsWeight(0f, 0.15f);
        Rig?.SetLookWeight(0f, 0.15f);

        characterAnimator.CrossFadeInFixedTime(baseRedStartState, redEnterBlend, 0, 0f);
    }

    private void PlayGreenPointControlFullBody()
    {
        if (characterAnimator == null)
            return;

        Rig?.SetArmsWeight(0f, 0.15f);
        Rig?.SetLookWeight(0f, 0.15f);

        characterAnimator.CrossFadeInFixedTime(baseGreenPointState, greenEnterBlend, 0, 0f);
    }

    private void TriggerGreenStopOnce()
    {
        if (greenStopTriggered)
            return;

        greenStopTriggered = true;

        if (characterAnimator == null)
            return;

        Rig?.SetArmsWeight(0f, 0.15f);
        Rig?.SetLookWeight(0f, 0.15f);

        characterAnimator.ResetTrigger(greenStopTriggerHash);
        characterAnimator.SetTrigger(greenStopTriggerHash);

        Debug.Log("[SemaphoreTask] GreenStop trigger fired.");
    }

    private void TriggerRedStopOnce()
    {
        if (characterAnimator == null)
            return;

        Rig?.SetArmsWeight(0f, 0.15f);
        Rig?.SetLookWeight(0f, 0.15f);

        characterAnimator.ResetTrigger(redStopTriggerHash);
        characterAnimator.SetTrigger(redStopTriggerHash);

        Debug.Log("[SemaphoreTask] RedStop trigger fired.");
    }

    // =========================
    // Finish / helpers
    // =========================

    private void FinishTask(bool success)
    {
        isActive = false;
        StopLoopRoutine();
        ResetRig();
        ResetAnimatorToIdle();

        if (success)
            SetEngagementReward(currentTask != null ? currentTask.engagementReward : 0);
        else
            ClearEngagementReward();

        Debug.Log($"[SemaphoreTask] Finished. Success={success}");

        if (taskDialogues != null && GetDialogueEntryById("FinalDialogue") != null)
        {
            currentEntryId = "FinalDialogue";
            RequestState(StateRequest.EnterDialogue);
            ctx.dialogueUI.ShowDialogue(taskDialogues, currentEntryId);
        }
        else
        {
            RequestState(StateRequest.CompleteTask);
        }
    }

    private void StopLoopRoutine()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
    }

    private void ApplyInTaskContext(string contextId)
    {
        currentInTaskContextId = contextId;
        currentInTaskContext = GetInTaskOptions(contextId);
    }
    private void ResetAnimatorToIdle()
    {
        if (characterAnimator == null)
            return;

        // Clear triggers that could keep transitions alive
        characterAnimator.ResetTrigger(greenStopTriggerHash);
        characterAnimator.ResetTrigger(redStopTriggerHash);

        // Force a known safe base state
        characterAnimator.CrossFadeInFixedTime(baseSittingState, 0.15f, 0, 0f);

        Debug.Log("[SemaphoreTask] Animator reset to idle.");
    }

}
