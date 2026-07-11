using UnityEngine;

public sealed class EndingV1Part : TaskPartsBase
{
    private enum V1State
    {
        EnterDelay,
        Countdown,
        ReadyToFinish
    }

    private enum EndingBranch
    {
        Back,
        Front
    }

    private enum BackNode
    {
        LoopFull,
        LoopB,
        LoopF
    }

    private const string ContextV1 = "FT_S2V1";
    private const string ActionFinished = "FT_Finished";
    private const string ActionNoOp = "FT_NoOp";
    private const string FinishReadyLabel = "Cumming";
    private const string FinalDialogueId = "FT_Final_V1";

    private const float IdleBeforeEndingSeconds = 1.0f;
    private const float FinishReadySeconds = 1.0f;

    private V1State currentState = V1State.EnterDelay;

    private EndingBranch currentBranch = EndingBranch.Back;
    private BackNode backNode = BackNode.LoopFull;
    private bool backAlternateToggle = false;
    private bool frontIsWall = true;

    private float branchTimer = 0f;
    private int lastShownSeconds = -1;

    private bool hasStartedEndingBranch = false;

    private TaskCountdownUiHelper countdownUi;

    public override string PartId => "EndingV1Part";

    public EndingV1Part(BaseTaskController owner) : base(owner)
    {
        availableAnimationKeys.Clear();
        availableAnimationKeys.Add("FinalStartLoopBack");
        availableAnimationKeys.Add("FinalStartLoopFront");
        availableAnimationKeys.Add("Final_Full_To_B");
        availableAnimationKeys.Add("Final_B_To_F");
        availableAnimationKeys.Add("Final_B_To_Full");
        availableAnimationKeys.Add("Final_F_To_B");
        availableAnimationKeys.Add("Front_handUp");
        availableAnimationKeys.Add("Front_handDown");

        availableSoundKeys.Clear();
        availableEmotionKeys.Clear();
    }

    public override void Enter()
    {
        base.Enter();

        if (owner is not FinalTaskController finalTask)
            return;

        countdownUi = new TaskCountdownUiHelper(finalTask);

        finalTask.ForceRemoveUpperClothing();
        finalTask.SetDialogueLock(false);
        finalTask.ClearInTaskContext();
        finalTask.EnterExecutionState();

        finalTask.ForceIdle();

        hasStartedEndingBranch = false;
        currentState = V1State.EnterDelay;

        localTimer = IdleBeforeEndingSeconds + finalTask.GetStepEntryUiDelaySeconds();
        lastShownSeconds = -1;
        branchTimer = 0f;
    }

    public override void Exit()
    {
        base.Exit();
        countdownUi?.Clear();
    }

    public override void Tick(float deltaTime)
    {
        if (owner is not FinalTaskController finalTask)
            return;

        if (hasStartedEndingBranch)
            TickEndingBranch(finalTask, deltaTime);

        switch (currentState)
        {
            case V1State.EnterDelay:
                localTimer -= deltaTime;

                if (!hasStartedEndingBranch &&
                    localTimer <= finalTask.GetStepEntryUiDelaySeconds())
                {
                    hasStartedEndingBranch = true;
                    StartEndingBranch(finalTask);
                }

                if (localTimer > 0f)
                    return;

                currentState = V1State.Countdown;
                localTimer = finalTask.GetV1WindowSeconds();

                lastShownSeconds = Mathf.CeilToInt(localTimer);
                countdownUi.Show(ContextV1, lastShownSeconds, ActionNoOp);
                break;

            case V1State.Countdown:
                localTimer -= deltaTime;

                int secondsLeft = Mathf.CeilToInt(Mathf.Max(0f, localTimer));

                if (secondsLeft <= 2)
                {
                    currentState = V1State.ReadyToFinish;
                    localTimer = FinishReadySeconds;

                    countdownUi.Clear();
                    finalTask.ShowRuntimeContext(ContextV1, FinishReadyLabel, ActionFinished);
                    return;
                }

                if (secondsLeft != lastShownSeconds)
                {
                    lastShownSeconds = secondsLeft;
                    countdownUi.Update(secondsLeft);
                }

                break;

            case V1State.ReadyToFinish:
                localTimer -= deltaTime;

                if (localTimer > 0f)
                    return;

                EmitSignal(FinalTaskController.TaskPartSignal.ToEndingV2);
                break;
        }
    }

    public override string HandleInTaskOption(string answerId, string action)
    {
        if (owner is not FinalTaskController finalTask)
            return null;

        if (currentState != V1State.ReadyToFinish)
            return "handled";

        if (action == ActionFinished)
        {
            finalTask.SetPendingFinalDialogueId(FinalDialogueId);
            EmitSignal(FinalTaskController.TaskPartSignal.ToFinalDialogue);
            return "handled";
        }

        return "handled";
    }

    private void StartEndingBranch(FinalTaskController finalTask)
    {
        currentBranch = Random.value < 0.5f
            ? EndingBranch.Back
            : EndingBranch.Front;

        branchTimer = finalTask.GetEndingLoopSwitchSeconds();

        if (currentBranch == EndingBranch.Back)
        {
            backNode = BackNode.LoopFull;
            backAlternateToggle = false;

            finalTask.TriggerAnimator("FinalStartLoopBack");
        }
        else
        {
            frontIsWall = true;
            finalTask.TriggerAnimator("FinalStartLoopFront");
        }
    }

    private void TickEndingBranch(FinalTaskController finalTask, float deltaTime)
    {
        branchTimer -= deltaTime;

        if (branchTimer > 0f)
            return;

        branchTimer = finalTask.GetEndingLoopSwitchSeconds();

        if (currentBranch == EndingBranch.Back)
        {
            switch (backNode)
            {
                case BackNode.LoopFull:
                    finalTask.TriggerAnimator("Final_Full_To_B");
                    backNode = BackNode.LoopB;
                    break;

                case BackNode.LoopB:
                    if (!backAlternateToggle)
                    {
                        finalTask.TriggerAnimator("Final_B_To_F");
                        backNode = BackNode.LoopF;
                    }
                    else
                    {
                        finalTask.TriggerAnimator("Final_B_To_Full");
                        backNode = BackNode.LoopFull;
                    }

                    backAlternateToggle = !backAlternateToggle;
                    break;

                case BackNode.LoopF:
                    finalTask.TriggerAnimator("Final_F_To_B");
                    backNode = BackNode.LoopB;
                    break;
            }
        }
        else
        {
            if (frontIsWall)
            {
                finalTask.TriggerAnimator("Front_handUp");
                frontIsWall = false;
            }
            else
            {
                finalTask.TriggerAnimator("Front_handDown");
                frontIsWall = true;
            }
        }
    }
}