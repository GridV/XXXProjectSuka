using UnityEngine;

public sealed class TaskPartsRuntime
{
    public TaskContext Ctx { get; }
    public BaseTaskController Owner { get; }

    public Animator Animator { get; }
    public TaskOptionsUIController TaskOptionsUI { get; }
    public DialogueUIController DialogueUI { get; }
    public StateController StateController { get; }

    public ClothingService Clothing { get; }
    public FinalTaskClothingPanel ClothingPanel { get; }
    //public FinalTaskCinemachineSwitcher CameraSwitcher { get; }

    public float PrimarySpeed { get; set; }
    public float SecondarySpeed { get; set; }
    public float EntryDelaySeconds { get; set; }
    public float CooldownSeconds { get; set; }

    public TaskPartsRuntime(
        BaseTaskController owner,
        TaskContext ctx,
        Animator animator,
        TaskOptionsUIController taskOptionsUI,
        DialogueUIController dialogueUI,
        StateController stateController,
        ClothingService clothing = null,
        FinalTaskClothingPanel clothingPanel = null
       /* FinalTaskCinemachineSwitcher cameraSwitcher = null*/)
    {
        Owner = owner;
        Ctx = ctx;

        Animator = animator;
        TaskOptionsUI = taskOptionsUI;
        DialogueUI = dialogueUI;
        StateController = stateController;

        Clothing = clothing;
        ClothingPanel = clothingPanel;
       // CameraSwitcher = cameraSwitcher;
    }
}