using UnityEngine;

public sealed class EndingV3Part : TaskPartsBase
{
    private enum V3State
    {
        EnterDelay,
        Stage1,
        Countdown,
        Stage3,
        Stage4
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

    private const string ContextStage1 = "FT_S2V3_1";
    private const string ContextCountdown = "FT_S2V3_2";
    private const string ContextStage3 = "FT_S2V3_3";
    private const string ContextStage4 = "FT_S2V3_4";

    private const string ActionFailed = "FT_Failed";
    private const string ActionContinueTask = "ContinueTask";
    private const string ActionContinue3 = "FT_V3_Continue_3";
    private const string ActionContinue4 = "FT_V3_Continue_4";
    private const string ActionNoOp = "FT_NoOp";

    private const string FinalDialogueId = "FT_Final_V3";

    private V3State currentState = V3State.EnterDelay;

    private EndingBranch currentBranch = EndingBranch.Back;
    private BackNode backNode = BackNode.LoopFull;
    private bool backAlternateToggle = false;
    private bool frontIsWall = true;

    private float branchTimer = 0f;
    private int lastShownSeconds = -1;

    private TaskCountdownUiHelper countdownUi;

    public override string PartId => "EndingV3Part";

    public EndingV3Part(BaseTaskController owner) : base(owner)
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

        StartEndingBranch(finalTask);

        currentState = V3State.EnterDelay;
        localTimer = finalTask.GetStepEntryUiDelaySeconds();
        lastShownSeconds = -1;
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

        TickEndingBranch(finalTask, deltaTime);

        switch (currentState)
        {
            case V3State.EnterDelay:

                localTimer -= deltaTime;
                if (localTimer > 0f)
                    return;

                currentState = V3State.Stage1;
              
                finalTask.SetDialogueLock(false);
                finalTask.ShowContextById(ContextStage1);
                break;

            case V3State.Countdown:

                localTimer -= deltaTime;

                int secondsLeft = Mathf.CeilToInt(Mathf.Max(0f, localTimer));

                if (secondsLeft != lastShownSeconds)
                {
                    lastShownSeconds = secondsLeft;
                    countdownUi.Update(secondsLeft);
                }

                if (localTimer > 0f)
                    return;

                countdownUi.Clear();

                currentState = V3State.Stage3;
                finalTask.ForceIdle();
                finalTask.SetDialogueLock(false);
                finalTask.ShowContextById(ContextStage3);
                break;
        }
    }

    public override string HandleInTaskOption(string answerId, string action)
    {
        if (owner is not FinalTaskController finalTask)
            return null;

        switch (currentState)
        {
            case V3State.Stage1:

                if (action == ActionFailed || action == ActionContinueTask)
                {
                    currentState = V3State.Countdown;
                    localTimer = finalTask.GetV3CountdownSeconds();

                    lastShownSeconds = Mathf.CeilToInt(localTimer);

                    countdownUi.Show(ContextCountdown, lastShownSeconds, ActionNoOp);

                    return "handled";
                }

                break;

            case V3State.Stage3:

                if (action == ActionContinue3)
                {
                    currentState = V3State.Stage4;
                    finalTask.SetDialogueLock(false);
                    finalTask.ShowContextById(ContextStage4);
                    return "handled";
                }

                break;

            case V3State.Stage4:

                if (action == ActionContinue4)
                {
                    finalTask.SetPendingFinalDialogueId(FinalDialogueId);
                    EmitSignal(FinalTaskController.TaskPartSignal.ToFinalDialogue);
                    return "handled";
                }

                break;
        }

        return "handled";
    }

    private void StartEndingBranch(FinalTaskController finalTask)
    {
        currentBranch = Random.value < 0.5f ? EndingBranch.Back : EndingBranch.Front;
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
        if (currentState == V3State.Stage3 || currentState == V3State.Stage4)
            return;

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