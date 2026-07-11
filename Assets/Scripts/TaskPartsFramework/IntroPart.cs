using UnityEngine;

public sealed class IntroPart : TaskPartsBase
{
    private const string IntroDialogueId = "FT_Entry";
    private const string ActionGoClothing = "FT_GoClothing";

    public override string PartId => "IntroPart";

    public IntroPart(BaseTaskController owner) : base(owner)
    {
        dialogueId = IntroDialogueId;

        availableSoundKeys.Clear();
        availableEmotionKeys.Clear();
        availableAnimationKeys.Clear();
    }

    public override void Enter()
    {
        base.Enter();

        if (owner is not FinalTaskController finalTask)
        {
            Debug.LogError("[IntroPart] Owner is not FinalTaskController.");
            return;
        }

        finalTask.ShowDialogueById(dialogueId);
    }

    public override bool HandleDialogueOption(string answerId, string action)
    {
        if (owner is not FinalTaskController finalTask)
            return false;

        if (action == ActionGoClothing)
        {
            EmitSignal(FinalTaskController.TaskPartSignal.ToClothing);
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