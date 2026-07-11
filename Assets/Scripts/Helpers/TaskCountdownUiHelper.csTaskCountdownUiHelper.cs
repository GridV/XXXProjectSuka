using UnityEngine;

public sealed class TaskCountdownUiHelper
{
    private readonly FinalTaskController owner;

    private string contextId;
    private string optionId;
    private int lastShownSeconds = -999;
    private string actionForFirstOption;

    public TaskCountdownUiHelper(FinalTaskController owner)
    {
        this.owner = owner;
    }

    public void Show(string contextId, int secondsLeft, string action)
    {
        this.contextId = contextId;
        actionForFirstOption = action;
        lastShownSeconds = secondsLeft;

        owner.ShowRuntimeContext(contextId, secondsLeft.ToString(), action);

        InTaskContext context = owner.GetInTaskOptions(contextId);
        if (context != null &&
            context.options != null &&
            context.options.Length > 0 &&
            context.options[0] != null)
        {
            optionId = context.options[0].id;
        }
        else
        {
            optionId = null;
        }
    }

    public void Update(int secondsLeft)
    {
        if (secondsLeft == lastShownSeconds)
            return;

        lastShownSeconds = secondsLeft;

        if (!string.IsNullOrEmpty(optionId))
            owner.UpdateInTaskOptionLabel(optionId, secondsLeft.ToString());
    }

    public void ShowNow(string action = null)
    {
        lastShownSeconds = -1;

        if (string.IsNullOrEmpty(contextId))
            return;

        owner.ShowRuntimeContext(
            contextId,
            "NOW!!",
            string.IsNullOrEmpty(action) ? actionForFirstOption : action
        );

        InTaskContext context = owner.GetInTaskOptions(contextId);
        if (context != null &&
            context.options != null &&
            context.options.Length > 0 &&
            context.options[0] != null)
        {
            optionId = context.options[0].id;
        }
    }

    public void Clear()
    {
        contextId = null;
        optionId = null;
        lastShownSeconds = -999;
        actionForFirstOption = null;
    }
}