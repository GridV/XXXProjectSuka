using UnityEngine;

public sealed class EndingV2Part : TaskPartsBase
{
    private enum V2State
    {
        EnterDelay,
        Countdown,
        NowHold,
        Main
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

    private const string ContextPrep = "FT_S2V2_Prep";
    private const string ContextMain = "FT_S2V2_Main";
    private const string ActionNoOp = "FT_NoOp";
    private const string ActionRuin = "FT_Ruin";
    private const string ActionFailed = "FT_Failed";
    private const string FinalDialogueId = "FT_Final_V2";

    private const float IdleBeforeEndingSeconds = 0f;

    private V2State currentState = V2State.EnterDelay;

    private EndingBranch currentBranch = EndingBranch.Back;
    private BackNode backNode = BackNode.LoopFull;
    private bool backAlternateToggle = false;
    private bool frontIsWall = true;

    private float branchTimer = 0f;
    private int lastShownSeconds = -1;

    private bool hasStartedEndingBranch = false;

    private TaskCountdownUiHelper countdownUi;

    public override string PartId => "EndingV2Part";

    public EndingV2Part(BaseTaskController owner) : base(owner)
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
        currentState = V2State.EnterDelay;

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
            case V2State.EnterDelay:
                localTimer -= deltaTime;

                if (!hasStartedEndingBranch &&
                    localTimer <= finalTask.GetStepEntryUiDelaySeconds())
                {
                    hasStartedEndingBranch = true;
                    StartEndingBranch(finalTask);
                }

                if (localTimer > 0f)
                    return;

                currentState = V2State.Countdown;
                localTimer = finalTask.GetV2CountdownSeconds();
                lastShownSeconds = Mathf.CeilToInt(localTimer);

                countdownUi.Show(ContextPrep, lastShownSeconds, ActionNoOp);
                break;

            case V2State.Countdown:
                localTimer -= deltaTime;

                int secondsLeft = Mathf.CeilToInt(Mathf.Max(0f, localTimer));
                if (secondsLeft != lastShownSeconds)
                {
                    lastShownSeconds = secondsLeft;
                    countdownUi.Update(secondsLeft);
                }

                if (localTimer > 0f)
                    return;

                currentState = V2State.NowHold;
                localTimer = finalTask.GetV2NowHoldSeconds();

                countdownUi.ShowNow(ActionNoOp);
                break;

            case V2State.NowHold:
                localTimer -= deltaTime;
                if (localTimer > 0f)
                    return;

                countdownUi.Clear();
                finalTask.ForceIdle();
                currentState = V2State.Main;
                finalTask.SetDialogueLock(false);
                finalTask.ShowContextById(ContextMain);
                break;

            case V2State.Main:
                break;
        }
    }

    public override string HandleInTaskOption(string answerId, string action)
    {
        if (owner is not FinalTaskController finalTask)
            return null;

        if (currentState != V2State.Main)
            return "handled";

        if (action == ActionRuin)
        {
            finalTask.SetPendingFinalDialogueId(FinalDialogueId);
            EmitSignal(FinalTaskController.TaskPartSignal.ToFinalDialogue);
            return "handled";
        }

        if (action == ActionFailed)
        {
            EmitSignal(FinalTaskController.TaskPartSignal.ToEndingV3);
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