using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

 

public class EdgeLadderTaskController : BaseTaskController
{
    private enum Phase
    {
        None,
        BuildUp,
        Plateau
    }

    #region Animator

    private const string PARAM_SPEED = "EL_Speed";
    private const string TRG_END = "EL_End";
    private const string TRG_FORCE_IDLE = "EL_ForceIdle";

    public enum EdgeAnimType
    {
        JerkBot,
        JerkTop,
        PalmTop,
        ThighJob,
        HandJob,
        BreastJob,
        TwoFingers
    }

    private static readonly Dictionary<EdgeAnimType, string> StartTriggers = new()
    {
        { EdgeAnimType.JerkBot, "EL_Start_JerkBot" },
        { EdgeAnimType.JerkTop, "EL_Start_JerkTop" },
        { EdgeAnimType.PalmTop, "EL_Start_PalmTop" },
        { EdgeAnimType.ThighJob, "EL_Start_ThighJob" },
        { EdgeAnimType.HandJob, "EL_Start_HandJob" },
        { EdgeAnimType.BreastJob, "EL_Start_BreastJob" },
        { EdgeAnimType.TwoFingers, "EL_Start_2Fingers" }
    };

    private EdgeAnimType _currentAnim;

    #endregion

    #region Clothing

    [Header("Clothing Objects (disable to remove)")]
    [SerializeField] private ClothingItem tShirtItem;
    [SerializeField] private ClothingItem braItem;
    [SerializeField] private ClothingItem shortsItem;
    [SerializeField] private ClothingItem underwearItem;


    private bool clothingRemovedThisStep;
 

    private bool IsUpperExposed => clothing != null && clothing.IsUpperExposed;
    private bool IsLowerExposed => clothing != null && clothing.IsLowerExposed;

    #endregion

    #region Tuning

    [Header("Ladder Tuning")]
    [SerializeField] private int minSteps = 4;
    [SerializeField] private int maxSteps = 10;

    [SerializeField] private float plateauBaseSeconds = 5f;

    [Header("Speed Tuning (EL_Speed)")]
    [SerializeField] private float buildUpSpeedBase = 1.15f;
    [SerializeField] private float buildUpSpeedPerStep = 0.10f;
    [SerializeField] private float plateauSpeed = 0.4f;

    [Header("BuildUp Speed")]
    [SerializeField] private float buildUpStartSpeed = 1.0f;
    [SerializeField] private float buildUpMaxSpeed = 3f;
    [SerializeField] private float buildUpRampDuration = 15.0f; // seconds

    private bool isInBuildUp;
    private float buildUpElapsed;

    [Header("Selection")]
    [SerializeField] private bool reselectAnimOnPlateau = true;

    #endregion

    #region Context IDs

    private const string CTX_BUILDUP = "edge_ladder_buildup";
    private const string CTX_PLATEAU = "edge_ladder_plateau";
    private const string CTX_PAUSE = "pauseOpt";

    #endregion

    private Phase currentPhase = Phase.None;

    private EdgeLadderCinemachineSwitcher cameraSwitcher;
    private string lastCameraAnimName;


    private int stepIndex;
    private int stepsTotal;

    private int slipCount;
    private int mercyCount;

    private bool isPaused;
    private bool isDialogueLock;
    private bool gameplayStarted;
    private const int MaxMercyCount = 2;
    private bool pendingRestartStepToBuildUp;

    private float plateauRemainingTime;
    private string resumeContextId;
    [Header("Clothing")]
    private ClothingService clothing = new ClothingService();
    public override void StartTask()
    {
        base.StartTask();

        taskDialogues = ctx.dialogueManager.LoadDialogue("edge_ladder");
        Debug.Log($"[EdgeLadderTask] Dialogue loaded. Entries={(taskDialogues != null && taskDialogues.dialogues != null ? taskDialogues.dialogues.Count : 0)}");

        clothing?.ResetSessionState();

        stepIndex = 1;
        stepsTotal = Random.Range(minSteps, maxSteps + 1); // FIX: correct range

        slipCount = 0;
        mercyCount = 0;

        isPaused = false;
        isDialogueLock = true;
        gameplayStarted = false;
        pendingRestartStepToBuildUp = false;

        clothingRemovedThisStep = false;
        resumeContextId = CTX_BUILDUP;

        currentPhase = Phase.None;

        // Pre-select default gameplay context for when gameplay starts.
        SetContextById(CTX_BUILDUP);
         
        // Keep character safe. Do not enter gameplay state here.
        ForceIdle();

        // Show intro dialogue (state first, then UI).
        ShowDialogueSafe("edge_ladder_intro");
    }
    public override void Init(TaskData task)
    {
        base.Init(task);
        cameraSwitcher = FindFirstObjectByType<EdgeLadderCinemachineSwitcher>();
        if (cameraSwitcher == null)
            Debug.LogWarning("[EdgeLadderTask] EdgeLadderCinemachineSwitcher not found in scene.");

        var cfg = UnityEngine.Object.FindFirstObjectByType<ClothingConfig>();
        if (cfg == null)
        {
            Debug.LogWarning("[EdgeLadder] ClothingConfig not found in scene.");
            return;
        }

        clothing.Bind(cfg.tshirt, cfg.shorts, cfg.bra, cfg.underwear);
        clothing.ResetSessionState();
    }
    private void Update()
    {
        TickLimits(Time.deltaTime);

        if (isPaused || isDialogueLock || !gameplayStarted)
            return;
        if (isInBuildUp)
            TickBuildUpSpeed(Time.deltaTime);
        if (currentPhase != Phase.Plateau)
            return;

        plateauRemainingTime -= Time.deltaTime;

        if (plateauRemainingTime <= 0f)
            OnPlateauCompleted();
        
    }

    #region Dialogue -> Controller

    public override bool OnOptionSelected(string answerId, string action)
    {
        switch (action)
        {
            case "StartTask":
                BeginGameplay();
                return true;

            case "ContinueTask":
                HandleDialogueContinue();
                return true;

            case "CompleteTask":
                HandleDialogueCompleteTask();
                return true;

            default:
                return base.OnOptionSelected(answerId, action);
        }
    }

    #endregion

    #region InTask -> Controller

    public override string OnInTaskOptionSelected(string answerId, string action)
    {
        switch (action)
        {
            case "PauseTask":
                resumeContextId = currentInTaskContextId;
                mercyCount++;
                ForceIdle();
                if (mercyCount >= MaxMercyCount)
                    {
                        MercyFail();
                        return "handled";
                    }   
                isPaused = true;         
                return base.OnInTaskOptionSelected(answerId, action);
                

            case "ContinueTask":
                {
                    isPaused = false;

                    ResumeLimits();
                    ctx.rhythmManager?.ResumePattern();

                    EnterBuildUp(); // same stepIndex, restart buildup

                    RequestState(StateRequest.EnterExecutingTask);
                    return null;
                }

                return base.OnInTaskOptionSelected(answerId, action);

            case "OnEdge":
                if (!isPaused && !isDialogueLock && currentPhase == Phase.BuildUp)
                    EnterPlateau();
                return null;

            case "Slipped":
                if (!isPaused && !isDialogueLock && currentPhase == Phase.Plateau)
                    HandleSlip();
                return null;

            default:
                return null;
        }
    }

    #endregion

    #region Flow

    private void BeginGameplay()
    {
        if (gameplayStarted)
            return;

        gameplayStarted = true;
        isDialogueLock = false;
        isPaused = false;

        EnterBuildUp();
    }

    private void EnterBuildUp()
    {
        
        if (isDialogueLock)
            return;
        SetContextById(CTX_BUILDUP);
        currentPhase = Phase.BuildUp;
        isInBuildUp = true;
        buildUpElapsed = 0f;
        _currentAnim = SelectEdgeAnimation();
        float speed = GetBuildUpSpeedForStep(stepIndex);
        SwitchCameraForAnimation(_currentAnim.ToString());
        PlayEdgeAnimation(_currentAnim, buildUpStartSpeed);
        SetAnimSpeed(buildUpStartSpeed);
        Debug.Log($"[EdgeLadderTask] EnterBuildUp step={stepIndex}/{stepsTotal} anim={_currentAnim} speed={speed:0.00} ctx={currentInTaskContextId}");

        RequestState(StateRequest.EnterExecutingTask);

        
    }

    private void EnterPlateau()
    {
        if (isDialogueLock)
            return;

        currentPhase = Phase.Plateau;
        isInBuildUp = false;
        // FIX: Set context BEFORE requesting state
        SetContextById(CTX_PLATEAU);


        SetAnimSpeed(plateauSpeed);

        plateauRemainingTime = plateauBaseSeconds * stepIndex;

        Debug.Log($"[EdgeLadderTask] EnterPlateau step={stepIndex}/{stepsTotal} anim={_currentAnim} speed={plateauSpeed:0.00} timer={plateauRemainingTime:0.0}");

        RequestState(StateRequest.EnterExecutingTask);
    }

    private void OnPlateauCompleted()
    {
        EndEdgeAnimation();

        clothingRemovedThisStep = false;
        clothing.RemoveOneOnPlateauSuccess();
        stepIndex++;

        if (stepIndex > stepsTotal)
        {
            isDialogueLock = true;
            ForceIdle();
            ShowDialogueSafe("edge_ladder_success");
            return;
        }

        EnterBuildUp();
    }

    private void HandleSlip()
    {
        slipCount++;

        ForceIdle();

        if (slipCount == 1)
        {
            isDialogueLock = true;
            pendingRestartStepToBuildUp = true;
            ShowDialogueSafe("edge_ladder_slip_1");
            return;
        }

        isDialogueLock = true;
        ShowDialogueSafe("edge_ladder_fail");
    }
    private void MercyFail()
    {
        isDialogueLock = true;
        ShowDialogueSafe("edge_ladder_fail");
    }
    private void HandleDialogueContinue()
    {
        isDialogueLock = false;

        if (pendingRestartStepToBuildUp)
        {
            pendingRestartStepToBuildUp = false;
            EnterBuildUp();
            return;
        }

        RequestState(StateRequest.EnterExecutingTask);
    }

    private void HandleDialogueCompleteTask()
    {
        isDialogueLock = false;
        isPaused = false;

        ctx.rhythmManager?.StopPattern();
        CleanupLimits();

        ClearEdgeLadderTriggers();
        ForceIdle();

        RequestState(StateRequest.CompleteTask);
    }

    #endregion

    #region Dialogue Safe Show

    private void ShowDialogueSafe(string dialogueId)
    {
        DialogueEntry entry = GetDialogueEntryById(dialogueId);
        if (entry == null)
        {
            Debug.LogWarning($"[EdgeLadderTask] Dialogue entry not found: {dialogueId}");
            RequestState(StateRequest.CompleteTask);
            return;
        }

        // IMPORTANT: request state BEFORE showing UI to avoid ApplyState hiding it after show.
        RequestState(StateRequest.EnterDialogue);

        ctx.dialogueUI.ShowDialogue(taskDialogues, dialogueId);
        currentEntryId = dialogueId;
    }

    #endregion

    #region Context

    private void SetContextById(string contextId)
    {
        currentInTaskContextId = contextId;
        currentInTaskContext = GetInTaskOptions(contextId);

        int optCount = (currentInTaskContext != null && currentInTaskContext.options != null) ? currentInTaskContext.options.Length : 0;

    }

    #endregion

 

    #region Animation Selection

    private bool IsAnimAllowed(EdgeAnimType type)
    {
        switch (type)
        {
            case EdgeAnimType.HandJob:
            case EdgeAnimType.PalmTop:
            case EdgeAnimType.TwoFingers:
                return true;

            case EdgeAnimType.JerkTop:
            case EdgeAnimType.BreastJob:
                return IsUpperExposed;

            case EdgeAnimType.JerkBot:
            case EdgeAnimType.ThighJob:
                return IsLowerExposed;

            default:
                return false;
        }
    }

    private EdgeAnimType SelectEdgeAnimation()
    {
        List<EdgeAnimType> pool = new();

        foreach (EdgeAnimType type in Enum.GetValues(typeof(EdgeAnimType)))
        {
            if (IsAnimAllowed(type))
                pool.Add(type);
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning("[EdgeLadderTask] No allowed animations, fallback to HandJob.");
            return EdgeAnimType.HandJob;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    private float GetBuildUpSpeedForStep(int step)
    {
        return Mathf.Max(0.05f, buildUpSpeedBase + buildUpSpeedPerStep * Mathf.Max(0, step - 1));
    }

    #endregion
    private void SwitchCameraForAnimation(string animName)
    {
        if (cameraSwitcher == null)
            return;

        if (string.IsNullOrWhiteSpace(animName))
            return;

        if (string.Equals(lastCameraAnimName, animName, System.StringComparison.OrdinalIgnoreCase))
            return;

        lastCameraAnimName = animName;
        cameraSwitcher.SwitchForAnimation(animName);
    }
    private void ResetCameraToDefault()
    {
        if (cameraSwitcher == null)
            return;

        lastCameraAnimName = null;
        cameraSwitcher.ResetToDefault();
    }

    #region Animator Control
    private void TickBuildUpSpeed(float deltaTime)
    {
        buildUpElapsed += deltaTime;

        float t = (buildUpRampDuration <= 0.01f)
            ? 1f
            : Mathf.Clamp01(buildUpElapsed / buildUpRampDuration);

        t = Mathf.SmoothStep(0f, 1f, t);

        float speed = Mathf.Lerp(buildUpStartSpeed, buildUpMaxSpeed, t);
        SetAnimSpeed(speed); // when t==1 => speed stays at max forever
    }

    private void ClearEdgeLadderTriggers()
    {
        if (characterAnimator == null)
        {
            Debug.LogError("[EdgeLadderTask] Animator is null.");
            return;
        }

        characterAnimator.ResetTrigger(TRG_END);
        characterAnimator.ResetTrigger(TRG_FORCE_IDLE);

        foreach (var kv in StartTriggers)
            characterAnimator.ResetTrigger(kv.Value);
    }

    private void PlayEdgeAnimation(EdgeAnimType type, float speed)
    {
        if (characterAnimator == null)
        {
            Debug.LogError("[EdgeLadderTask] Animator is null.");
            return;
        }

        if (!StartTriggers.TryGetValue(type, out string startTrigger) || string.IsNullOrEmpty(startTrigger))
        {
            Debug.LogError($"[EdgeLadderTask] Missing start trigger for anim type: {type}");
            return;
        }

        characterAnimator.SetFloat(PARAM_SPEED, speed);

        ClearEdgeLadderTriggers();
        characterAnimator.SetTrigger(startTrigger);

        Debug.Log($"[EdgeLadderTask] PlayEdgeAnimation type={type} trigger={startTrigger} speed={speed:0.00}");
    }

    private void EndEdgeAnimation()
    {
        if (characterAnimator == null)
            return;

        characterAnimator.ResetTrigger(TRG_FORCE_IDLE);
        characterAnimator.SetTrigger(TRG_END);

        Debug.Log($"[EdgeLadderTask] EndEdgeAnimation trigger={TRG_END}");
    }

    private void ForceIdle()
    {
        ResetCameraToDefault();
        if (characterAnimator == null)
            return;

        characterAnimator.SetFloat(PARAM_SPEED, 1f);
        isInBuildUp = false;
        ClearEdgeLadderTriggers();
        SetAnimSpeed(1f);
        characterAnimator.SetTrigger("EL_ForceIdle");

        Debug.Log($"[EdgeLadderTask] ForceIdle trigger={TRG_FORCE_IDLE}");
    }
    private void SetAnimSpeed(float speed)
    {
        if (characterAnimator == null) return;
        characterAnimator.SetFloat(PARAM_SPEED, Mathf.Max(0.01f, speed));
    }
    #endregion
}
