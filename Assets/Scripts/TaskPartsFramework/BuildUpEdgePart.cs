using System.Collections.Generic;
using UnityEngine;

public sealed class BuildUpEdgePart : TaskPartsBase
{
    private enum BuildUpState
    {
        EnterDelay,
        Active,
        Cooldown,
        MercyDialogue
    }

    private const float EdgeStartSpeed = 1.0f;
    private const float EdgeMaxSpeed = 3.0f;
    private const float EdgeRampSeconds = 6.0f;

    private float edgeRampTimer;
    private bool edgeRampActive;

    private const string BuildUpContextId = "FT_buildup";
    private const string CooldownContextId = "FT_cooldown";
    private const string MercyDialogueId = "FT_mercy";
    private const string Mercy2DialogueId = "FT_mercy2";

    private const string ActionOnEdge = "OnEdge";
    private const string ActionMercy = "FT_Mercy";
    private const string ActionContinueTask = "ContinueTask";
    private const string ActionNoOp = "FT_NoOp";

    private readonly List<string> defaultBuildUpAnimations = new List<string>
    {
        "JerkTop",
        "PalmTop",
        "HandJob"
    };

    private readonly List<string> upperExposedAnimations = new List<string>
    {
        "ThighJob"
    };

    private readonly List<string> lowerExposedAnimations = new List<string>
    {
        "BreastJob",
        "2Fingers"
    };

    private BuildUpState currentState = BuildUpState.EnterDelay;
    private float edgedChance = 0.99f; //TODO: Fix it
    private int mercyUsed = 0;
    private int lastShownCountdown = -1;
    private TaskCountdownUiHelper countdownUi;
    private FinalTaskController.TaskPartSignal pendingDialogueSignal = FinalTaskController.TaskPartSignal.None;

    public override string PartId => "BuildUpEdgePart";

    public BuildUpEdgePart(BaseTaskController owner) : base(owner)
    {
        contextId = BuildUpContextId;

        availableAnimationKeys.Clear();
        availableAnimationKeys.AddRange(defaultBuildUpAnimations);
        availableAnimationKeys.AddRange(upperExposedAnimations);
        availableAnimationKeys.AddRange(lowerExposedAnimations);

        availableSoundKeys.Clear();
        availableEmotionKeys.Clear();
    }

    public override void Enter()
    {
        base.Enter();

        if (owner is FinalTaskController finalTask)
            countdownUi = new TaskCountdownUiHelper(finalTask);

        StartBuildUpSequence();
    }

    public override void Exit()
    {
        base.Exit();
        edgeRampActive = false;

        if (owner is FinalTaskController finalTask)
            finalTask.SetAnimatorSpeed(1f);

        countdownUi?.Clear();
    }
    private void TickEdgeSpeedRamp(FinalTaskController finalTask, float deltaTime)
    {
        if (!edgeRampActive)
            return;

        edgeRampTimer += deltaTime;

        float t = Mathf.Clamp01(edgeRampTimer / EdgeRampSeconds);
        float speed = Mathf.Lerp(EdgeStartSpeed, EdgeMaxSpeed, t);

        finalTask.SetAnimatorSpeed(speed);

        if (t >= 1f)
            edgeRampActive = false;
    }
    public override void Tick(float deltaTime)
    {
        if (owner is not FinalTaskController finalTask)
            return;

        switch (currentState)
        {
            case BuildUpState.Active:
                TickEdgeSpeedRamp(finalTask, deltaTime);
                break;
            case BuildUpState.EnterDelay:
                localTimer -= deltaTime;
                if (localTimer > 0f)
                    return;

                currentState = BuildUpState.Active;
                finalTask.SetDialogueLock(false);
                finalTask.ShowContextById(BuildUpContextId);
                Debug.Log("[BuildUpEdgePart] Active state started.");
                break;

            case BuildUpState.Cooldown:
                localTimer -= deltaTime;

                int secondsLeft = Mathf.CeilToInt(Mathf.Max(0f, localTimer));
                if (secondsLeft != lastShownCountdown)
                {
                    lastShownCountdown = secondsLeft;
                    countdownUi?.Update(secondsLeft);
                }

                if (localTimer > 0f)
                    return;

                StartBuildUpSequence();
                break;
        }
    }

    public override string HandleInTaskOption(string answerId, string action)
    {
        if (owner is not FinalTaskController finalTask)
            return null;

        if (currentState != BuildUpState.Active)
            return "handled";

        if (action == ActionOnEdge)
        {
            float roll = Random.value;
            bool success = roll <= edgedChance;

            Debug.Log($"[BuildUpEdgePart] OnEdge pressed. Roll={roll:0.00} Chance={edgedChance:0.00} Success={success}");

            if (success)
            {
                EmitSignal(FinalTaskController.TaskPartSignal.ToEndingV1);
                return "handled";
            }

            edgedChance = Mathf.Clamp(edgedChance + 0.10f, 0f, 0.70f);
            BeginCooldown(finalTask);
            return "handled";
        }

        if (action == ActionMercy)
        {
            mercyUsed++;
            finalTask.ForceIdle();
            finalTask.SetDialogueLock(true);
            currentState = BuildUpState.MercyDialogue;
            countdownUi?.Clear();

            if (mercyUsed < 2)
            {
                pendingDialogueSignal = FinalTaskController.TaskPartSignal.ToBuildUp;
                finalTask.ShowDialogueById(MercyDialogueId);
            }
            else
            {
                pendingDialogueSignal = Random.value < 0.5f
                    ? FinalTaskController.TaskPartSignal.ToEndingV2
                    : FinalTaskController.TaskPartSignal.ToEndingV3;

                finalTask.ShowDialogueById(Mercy2DialogueId);
            }

            return "handled";
        }

        return "handled";
    }

    public override bool HandleDialogueOption(string answerId, string action)
    {
        if (owner is not FinalTaskController finalTask)
            return false;

        if (currentState != BuildUpState.MercyDialogue)
            return false;

        if (action == ActionContinueTask)
        {
            if (pendingDialogueSignal == FinalTaskController.TaskPartSignal.ToBuildUp)
            {
                StartBuildUpSequence();
                return true;
            }

            EmitSignal(pendingDialogueSignal);
            return true;
        }

        return false;
    }

    private void StartBuildUpSequence()
    {
        if (owner is not FinalTaskController finalTask)
            return;

        string selectedAnimation = SelectBuildUpAnimation(finalTask);

        finalTask.SetDialogueLock(false);
        finalTask.ClearInTaskContext();
        finalTask.EnterExecutionState();

        edgeRampTimer = 0f;
        edgeRampActive = true;

        finalTask.SetAnimatorSpeed(EdgeStartSpeed);
        finalTask.PlayAnimKey(selectedAnimation, EdgeStartSpeed);

        localTimer = finalTask.GetStepEntryUiDelaySeconds();
        localCounter = 0;
        lastShownCountdown = -1;
        countdownUi?.Clear();
        currentState = BuildUpState.EnterDelay;

        Debug.Log($"[BuildUpEdgePart] Enter delay started. Anim={selectedAnimation} Delay={localTimer:0.00}s");
    }

    private void BeginCooldown(FinalTaskController finalTask)
    {
        finalTask.SetDialogueLock(false);
        finalTask.SetAnimatorSpeed(1f);
        finalTask.ForceIdle();

        currentState = BuildUpState.Cooldown;
        localTimer = finalTask.GetCooldownSeconds();
        lastShownCountdown = Mathf.CeilToInt(localTimer);

        if (countdownUi == null)
            countdownUi = new TaskCountdownUiHelper(finalTask);

        countdownUi.Show(CooldownContextId, lastShownCountdown, ActionNoOp);

        Debug.Log($"[BuildUpEdgePart] Cooldown started. Duration={localTimer:0.00}s");
    }

    private string SelectBuildUpAnimation(FinalTaskController finalTask)
    {
        List<string> pool = new List<string>();
        pool.AddRange(defaultBuildUpAnimations);

        if (finalTask.IsUpperExposed())
            pool.AddRange(upperExposedAnimations);

        if (finalTask.IsLowerExposed())
            pool.AddRange(lowerExposedAnimations);

        if (pool.Count == 0)
            return "JerkTop";

        return pool[Random.Range(0, pool.Count)];
    }
}
