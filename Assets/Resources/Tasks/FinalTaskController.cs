using System;
using UnityEngine;

public class FinalTaskController : BaseTaskController
{
    public enum TaskPartSignal
    {
        None = 0,
        ToIntro,
        ToClothing,
        ToBuildUp,
        ToEndingV1,
        ToEndingV2,
        ToEndingV3,
        ToFinalDialogue,
        CompleteTask,
        CancelTask
    }


    [Header("Animation")]
    [SerializeField] private float step1Speed = 1.0f;
    [SerializeField] private float step2Speed = 1.0f;
    private AnimatorEventRouter router;

    [Header("UI Timing")]
    [SerializeField] private float stepEntryUiDelaySeconds = 2.5f;

    [Header("Step Timing")]
    [SerializeField] private float cooldownSeconds = 10f;
    [SerializeField] private float endingLoopSwitchSeconds = 10f;
    [SerializeField] private float v1WindowSeconds = 10f;
    [SerializeField] private float v2CountdownSeconds = 10f;
    [SerializeField] private float v2NowHoldSeconds = 2f;
    [SerializeField] private float v3CountdownSeconds = 10f;

    private const string TriggerForceIdle = "EL_ForceIdle";
    private const string TriggerEnd = "EL_End";
    private const string ParamSpeed = "EL_Speed";

    private TaskPartsBase currentPart;
    private StateController cachedStateController;

    private bool isPaused = false;
    private bool isDialogueLock = true;
    private string pendingFinalDialogueId = null;

    private ClothingService clothing;
    private FinalTaskClothingPanel clothingPanel;

    public override void Init(TaskData task)
    {
        base.Init(task);

        cachedStateController = FindFirstObjectByType<StateController>();

        var cfg = FindFirstObjectByType<ClothingConfig>();
        if (cfg != null)
        {
            clothing = new ClothingService();
            clothing.Bind(cfg.tshirt, cfg.shorts, cfg.bra, cfg.underwear);
            clothing.ResetSessionState();

            clothingPanel = FindFirstObjectByType<FinalTaskClothingPanel>(FindObjectsInactive.Include);
            if (clothingPanel != null)
            {
                clothingPanel.Bind(clothing);
                clothingPanel.Hide();
            }
        }
    }

    public override void StartTask()
    {
        base.StartTask();

        Debug.Log("[FinalTask] Starting final task.");

        taskDialogues = ctx.dialogueManager.LoadDialogue("FinalTask");

        if (taskDialogues == null)
        {
            Debug.LogWarning("[FinalTask] Dialogue data not found.");
            return;
        }

        UnsubscribeAnimatorEvents();

        router = null;

        if (characterAnimator != null)
        {
            router = characterAnimator.GetComponent<AnimatorEventRouter>();

            if (router == null)
                router = characterAnimator.GetComponentInChildren<AnimatorEventRouter>(true);

            if (router == null)
                router = characterAnimator.GetComponentInParent<AnimatorEventRouter>();
        }

        if (router != null)
        {
            router.OnEvent -= OnAnimatorEvent;
            router.OnEvent += OnAnimatorEvent;

            Debug.Log($"[FinalTask] Subscribed to AnimatorEventRouter on: {router.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[FinalTask] AnimatorEventRouter was not found near character animator.");
        }

        ForceIdle();

        SetPart(new IntroPart(this));
    }

    private void Update()
    {
    
        if (isPaused)
            return;

        if (IsUiTransitioning())
            return;

        currentPart?.Tick(Time.deltaTime);
        ResolvePartSignal();
    }

    public override bool OnOptionSelected(string answerId, string action)
    {
        if (base.OnOptionSelected(answerId, action))
            return true;

        if (currentPart == null)
            return false;

        bool handled = currentPart.HandleDialogueOption(answerId, action);
        ResolvePartSignal();
        return handled;
    }

    public override string OnInTaskOptionSelected(string answerId, string action)
    {
        if (isDialogueLock)
        {
            Debug.Log("[FinalTask] In-task input ignored due to dialogue lock.");
            return null;
        }

        if (currentPart == null)
            return null;

        string result = currentPart.HandleInTaskOption(answerId, action);
        ResolvePartSignal();
        return result;
    }

    public void SetPart(TaskPartsBase nextPart)
    {
        currentPart?.Exit();
        currentPart = nextPart;

        if (currentPart != null)
        {
            Debug.Log($"[FinalTask] Enter part: {currentPart.PartId}");
            currentPart.Enter();
        }
    }

    private void ResolvePartSignal()
    {
        if (currentPart == null)
            return;

        TaskPartSignal signal = currentPart.ConsumeSignal();
        if (signal == TaskPartSignal.None)
            return;

        switch (signal)
        {
            case TaskPartSignal.ToIntro:
                SetPart(new IntroPart(this));
                break;

            case TaskPartSignal.ToClothing:
                SetPart(new ClothingPart(this));
                break;

            case TaskPartSignal.ToBuildUp:
                SetPart(new BuildUpEdgePart(this));
                break;

            case TaskPartSignal.ToEndingV1:
                SetPart(new EndingV1Part(this));
                break;

            case TaskPartSignal.ToEndingV2:
                SetPart(new EndingV2Part(this));
                break;

            case TaskPartSignal.ToEndingV3:
                SetPart(new EndingV3Part(this));
                break;

            case TaskPartSignal.ToFinalDialogue:
                SetPart(new FinalDialoguePart(this));
                break;

            case TaskPartSignal.CompleteTask:
                RequestState(StateRequest.CompleteTask);
                break;

            case TaskPartSignal.CancelTask:
                CancelSessionLogic();
                break;

            default:
                Debug.Log($"[FinalTask] Unhandled signal: {signal}");
                break;
        }
    }

    public void SetDialogueLock(bool value)
    {
        isDialogueLock = value;
    }

    public bool IsDialogueLocked()
    {
        return isDialogueLock;
    }

    public void ShowDialogueById(string targetDialogueId)
    {
        DialogueEntry entry = GetDialogueEntryById(targetDialogueId);
        if (entry == null)
        {
            Debug.LogWarning($"[FinalTask] Dialogue entry not found: {targetDialogueId}");
            RequestState(StateRequest.CompleteTask);
            return;
        }

        isDialogueLock = true;
        RequestState(StateRequest.EnterDialogue);

        if (ctx != null && ctx.dialogueUI != null)
            ctx.dialogueUI.ShowDialogue(taskDialogues, targetDialogueId);

        currentEntryId = targetDialogueId;
    }

    public void EnterExecutionState()
    {
        RequestState(StateRequest.EnterExecutingTask);
    }

    public void SetInTaskContextById(string targetContextId)
    {
        currentInTaskContextId = targetContextId;
        currentInTaskContext = GetInTaskOptions(targetContextId);
    }

    public void ClearInTaskContext()
    {
        currentInTaskContextId = null;
        currentInTaskContext = null;

        if (taskOptionsUI != null)
            taskOptionsUI.HideAll();
    }

    public void ShowCurrentInTaskContext()
    {
        if (taskOptionsUI == null || cachedStateController == null)
            return;

        if (currentInTaskContext == null)
        {
            taskOptionsUI.HideAll();
            return;
        }

        taskOptionsUI.ShowOptions(currentInTaskContext, cachedStateController.OnInTaskAction);
    }

    public void ShowContextById(string targetContextId)
    {
        SetInTaskContextById(targetContextId);
        ShowCurrentInTaskContext();
    }

    public void ShowRuntimeContext(string baseContextId, string firstOptionLabel, string firstOptionAction)
    {
        InTaskContext baseContext = GetInTaskOptions(baseContextId);
        if (baseContext == null)
        {
            Debug.LogWarning($"[FinalTask] Runtime context not found: {baseContextId}");
            return;
        }

        InTaskContext runtimeContext = CloneContext(baseContext);
        if (runtimeContext.options != null && runtimeContext.options.Length > 0 && runtimeContext.options[0] != null)
        {
            runtimeContext.options[0].label = firstOptionLabel;
            runtimeContext.options[0].action = firstOptionAction;
        }

        currentInTaskContextId = baseContextId;
        currentInTaskContext = runtimeContext;

        ShowCurrentInTaskContext();
    }

    private InTaskContext CloneContext(InTaskContext src)
    {
        if (src == null)
            return null;

        InTaskContext clone = new InTaskContext();
        clone.id = src.id;
        clone.text = src.text;

        if (src.options == null)
        {
            clone.options = Array.Empty<InTaskOption>();
            return clone;
        }

        clone.options = new InTaskOption[src.options.Length];
        for (int i = 0; i < src.options.Length; i++)
        {
            InTaskOption original = src.options[i];
            if (original == null)
            {
                clone.options[i] = null;
                continue;
            }

            InTaskOption copy = new InTaskOption();
            copy.id = original.id;
            copy.label = original.label;
            copy.action = original.action;
            copy.nextOption = original.nextOption;
            copy.side = original.side;

            clone.options[i] = copy;
        }

        return clone;
    }

    public float GetPrimarySpeed()
    {
        return step1Speed;
    }

    public float GetSecondarySpeed()
    {
        return step2Speed;
    }

    public float GetStepEntryUiDelaySeconds()
    {
        return stepEntryUiDelaySeconds;
    }

    public float GetCooldownSeconds()
    {
        return cooldownSeconds;
    }

    public float GetEndingLoopSwitchSeconds()
    {
        return endingLoopSwitchSeconds;
    }

    public float GetV1WindowSeconds()
    {
        return v1WindowSeconds;
    }

    public float GetV2CountdownSeconds()
    {
        return v2CountdownSeconds;
    }

    public float GetV2NowHoldSeconds()
    {
        return v2NowHoldSeconds;
    }

    public float GetV3CountdownSeconds()
    {
        return v3CountdownSeconds;
    }

    public void ForceIdle()
    {
        Debug.Log("[FinalTask] ForceIdle TRIGGERED");

        if (characterAnimator == null)
            return;

        Debug.Log($"[FinalTask] Current State BEFORE idle: {characterAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash}");

        characterAnimator.SetTrigger("EL_ForceIdle");
    }

    public void PlayAnimKey(string key, float speed)
    {
        if (characterAnimator == null)
        {
            Debug.LogError("[FinalTask] Animator is null.");
            return;
        }

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[FinalTask] Empty animation key.");
            return;
        }

        characterAnimator.ResetTrigger(TriggerEnd);
        characterAnimator.ResetTrigger(TriggerForceIdle);
        characterAnimator.SetFloat(ParamSpeed, speed);
        characterAnimator.SetTrigger("EL_Start_" + key);

        Debug.Log($"[FinalTask] PlayAnimKey key={key} speed={speed:0.00}");
    }
    public void TriggerAnimator(string triggerName)
    {
        if (characterAnimator == null || string.IsNullOrEmpty(triggerName))
            return;

        characterAnimator.SetTrigger(triggerName);
    }

    public void SetPendingFinalDialogueId(string dialogueId)
    {
        pendingFinalDialogueId = dialogueId;
    }

    public string GetPendingFinalDialogueId()
    {
        return pendingFinalDialogueId;
    }

    public void ShowClothingPanel()
    {
        if (clothingPanel != null)
            clothingPanel.Show();
    }

    private const string SpeedParameter = "EL_Speed";

    public void SetAnimatorSpeed(float speed)
    {
        if (characterAnimator == null)
            return;

        characterAnimator.SetFloat(SpeedParameter, speed);
    }

    private void OnAnimatorEvent(string key)
    {
        Debug.Log($"[FinalTask] Animator event received: {key}");

        switch (key)
        {
            case "Cam_A_Out":
                ctx.danceCameraService?.SwitchToByName("CinemachineCamera_a_out");
                break;
            case "Cam_Reset":
                ctx.danceCameraService?.StopDance();
                break;
        }
    }
    public void HideClothingPanel()
    {
        if (clothingPanel != null)
            clothingPanel.Hide();
    }

    public bool IsUpperExposed()
    {
        return clothing != null && clothing.IsUpperExposed;
    }

    public bool IsLowerExposed()
    {
        return clothing != null && clothing.IsLowerExposed;
    }

    public void ForceRemoveUpperClothing()
    {
        if (clothing != null)
            clothing.ForceRemoveUpper();
    }
    public void UpdateInTaskOptionLabel(string optionId, string label)
    {
        if (taskOptionsUI == null)
            return;

        taskOptionsUI.UpdateOptionLabel(optionId, label);
    }
    private void UnsubscribeAnimatorEvents()
    {
        if (router == null)
            return;

        router.OnEvent -= OnAnimatorEvent;
        router = null;
    }

    private void OnDestroy()
    {
        UnsubscribeAnimatorEvents();
    }


}