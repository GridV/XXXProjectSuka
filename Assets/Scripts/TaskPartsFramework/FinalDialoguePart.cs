using UnityEngine;

public sealed class FinalDialoguePart : TaskPartsBase
{
    private const string ActionCompleteTask = "CompleteTask";
    private const string ActionNextTask = "NextTask";

    public override string PartId => "FinalDialoguePart";

    public FinalDialoguePart(BaseTaskController owner) : base(owner)
    {
        availableAnimationKeys.Clear();
        availableSoundKeys.Clear();
        availableEmotionKeys.Clear();
    }

    public override void Enter()
    {
        base.Enter();

        if (owner is not FinalTaskController finalTask)
        {
            Debug.LogError("[FinalDialoguePart] Owner is not FinalTaskController.");
            return;
        }

        string targetDialogueId = finalTask.GetPendingFinalDialogueId();
        if (string.IsNullOrEmpty(targetDialogueId))
            targetDialogueId = "FT_Final_V1";

        finalTask.ForceIdle();
        finalTask.ShowDialogueById(targetDialogueId);
    }

    public override bool HandleDialogueOption(string answerId, string action)
    {
        if (action == ActionCompleteTask || action == ActionNextTask)
        {
            EmitSignal(FinalTaskController.TaskPartSignal.CompleteTask);
            return true;
        }

        if (action == "CancelSession")
        {
            EmitSignal(FinalTaskController.TaskPartSignal.CancelTask);
            return true;
        }

        if (owner is FinalTaskController finalTask && action == "NextDialogue")
        {
            PlayerOption option = owner.GetPlayerOptionById(answerId);
            if (option != null && !string.IsNullOrEmpty(option.nextDialogueId))
            {
                finalTask.ShowDialogueById(option.nextDialogueId);
                return true;
            }
        }

        return false;
    }
}