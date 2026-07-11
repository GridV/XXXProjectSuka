using UnityEngine;

public class InitialTaskController : BaseTaskController
{
    private DialogueManager dialogueManager;
    private DialogueUIController dialogueUI;

    /* BaseTaskController already has these fields:
    protected TaskData currentTask;
    public string currentEntryId;
    protected DialogueData taskDialogues;
    protected string dialogueEntryId = "InitialDIalogue";
     */

    public override void Init(TaskData task)
    {
        base.Init(task);
        dialogueManager = ctx.dialogueManager;
        dialogueUI = ctx.dialogueUI;
    }

    public override void StartTask()
    {
        base.StartTask();

        Debug.Log("[InitialTaskController] Starting initial dialogue sequence...");
        
        taskDialogues = ctx.dialogueManager.LoadDialogue("InitialTask");

        if (taskDialogues != null)
        {
            //ctx.dialogueUI.ShowDialogue(taskDialogues, dialogueEntryId);
            currentEntryId = dialogueEntryId;
            Debug.Log("[InitialTaskController] Dialogue shown: InitialTask");
        }
        else
        {
            Debug.LogWarning("[InitialTaskController] Dialogue data not found!");
        }
    }

    public override bool OnOptionSelected(string answerId, string action)
    {
        Debug.Log($"[InitialTaskController] Option selected: {answerId}, action: {action}");
        var option = GetPlayerOptionById(answerId);

        if (base.OnOptionSelected(answerId, action)) { return true; }
  
        switch (action)
        {

            case "NextDialogue":
                if (option == null || string.IsNullOrEmpty(option.nextDialogueId))
                {
                    Debug.LogWarning("[InitialTaskController] Next dialogue ID is missing!");
                    return false;
                }
                ctx.dialogueUI.ShowDialogue(taskDialogues, option.nextDialogueId);
                currentEntryId = option.nextDialogueId;
                return true;

            case "Retry":
                Debug.Log("[InitialTaskController] Restarting dialogue...");
                StartTask();
                return true;

            default:
                Debug.Log($"[InitialTaskController] No specific handler for action '{action}'");
                return false;
        }
    }
}
