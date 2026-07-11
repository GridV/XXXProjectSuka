using UnityEngine;

public sealed class ClothingPart : TaskPartsBase
{
    private const string ClothingDialogueId = "FT_Clothing";
    private const string ActionBeginExecution = "FT_BeginExecution";
    private const string ActionGoClothing = "FT_GoClothing";

    public override string PartId => "ClothingPart";

    public ClothingPart(BaseTaskController owner) : base(owner)
    {
        dialogueId = ClothingDialogueId;

        availableSoundKeys.Clear();
        availableEmotionKeys.Clear();
        availableAnimationKeys.Clear();
    }

    public override void Enter()
    {
        base.Enter();

        if (owner is not FinalTaskController finalTask)
        {
            Debug.LogError("[ClothingPart] Owner is not FinalTaskController.");
            return;
        }

        finalTask.ShowDialogueById(dialogueId);
        finalTask.ShowClothingPanel();
    }

    public override void Exit()
    {
        base.Exit();

        if (owner is FinalTaskController finalTask)
            finalTask.HideClothingPanel();
    }

    public override bool HandleDialogueOption(string answerId, string action)
    {
        if (owner is not FinalTaskController finalTask)
            return false;

        if (action == ActionBeginExecution || action == ActionGoClothing)
        {
            EmitSignal(FinalTaskController.TaskPartSignal.ToBuildUp);
            return true;
        }

        if (action == "NextDialogue")
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