using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public enum GameState
{
    None = 0,
    Dialogue = 1,
    ExecutingTask = 2,
    Paused = 3,
    Ending = 4
}

public class StateController : MonoBehaviour
{
    [Header("Scene References (assign in Inspector)")]
    [SerializeField] private DialogueUIController dialogueUI;
    [SerializeField] private TaskOptionsUIController taskOptionsUI;
    [SerializeField] private PauseMenuController pauseMenu;

    [Header("Debug")]
    [SerializeField] private GameState currentState = GameState.None;

    private BaseTaskController subscribedController;
    public GameState CurrentState => currentState;

    private void Awake()
    {
        ApplyState(GameState.None);
    }

    public void EnterDialogue()
    {
        ApplyState(GameState.Dialogue);
    }

    public void EnterExecutingTask()
    {
        ApplyState(GameState.ExecutingTask);
    }

    public void EnterPaused()
    {
        ApplyState(GameState.Paused);
    }

    public void EnterEnding()
    {
        ApplyState(GameState.Ending);
    }

    public void OnDialogueAction(string answerId, string action)
    {
        var ctx = GameContext.Instance != null ? GameContext.Instance.TaskContext : null;
        var controller = ctx != null ? ctx.controller : null;

        if (controller == null)
        {
            Debug.LogWarning("[StateController] No active TaskController in GameContext.");
            return;
        }

        controller.OnOptionSelected(answerId, action);

        if (action == "StartRhythm" || action == "StartTask")
        {
            EnterExecutingTask();
            ShowInTaskOptions();
        }
             
    }
    public void OnInTaskAction(string optionId, string action)
    {
        string requestedAction;
        var ctx = GameContext.Instance != null ? GameContext.Instance.TaskContext : null;
        var controller = ctx != null ? ctx.controller : null;
        if (controller == null)
            {
                Debug.LogWarning("[StateController] No active TaskController in GameContext.");
                return;
            }
        
        requestedAction = controller.OnInTaskOptionSelected(optionId, action);
        if (!string.IsNullOrEmpty(requestedAction))
        {
            switch (requestedAction)
            {
                /*TaskRepeatCpontroller*/
                case "reset":
                    EnterExecutingTask();
                    ShowInTaskOptions();
                    return;
                case "handled":
                    return;
            }
        }
        switch (action)
        {
            case "PauseTask":
                EnterPaused();
                ShowInTaskOptions();
                break;
            case "ContinueTask":
                EnterExecutingTask();
                ShowInTaskOptions();
                break; 
            case "NextDialogue":
                ShowInTaskOptions();
                break;
            case "EndSession":
                EnterEnding();
                Debug.Log("[InitialTaskController] Player cancelled the session.");
                GameContext.Instance.TaskContext.controller = null;
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuURP");
                break;

        }
    

    }
    private void ShowInTaskOptions()
    {
        var gameCtx = GameContext.Instance;
        var taskCtx = gameCtx.TaskContext;
        var controller = taskCtx.controller;
        if (controller == null)
            return;

        InTaskContext inTaskContext = controller.GetInTaskOptions();
        if (inTaskContext != null)
        {
            taskOptionsUI.ShowOptions(
                inTaskContext,
                OnInTaskAction
            );
        }
        else
        {
            taskOptionsUI.HideAll();
        }
    }

    private void ApplyState(GameState next)
    {
        if (currentState == next)
            return;

        currentState = next;

        // Baseline: hide optional UI (controllers may show what they need)
        if (dialogueUI != null)
            dialogueUI.HideAllOptions();

        if (taskOptionsUI != null)
            taskOptionsUI.HideAll();

        Debug.Log($"[StateController] State changed to: {currentState}");
    }
    public void OnTaskControllerChanged(BaseTaskController controller)
    {
        if (subscribedController == controller)
            return;

        if (subscribedController != null)
            subscribedController.OnStateRequested -= HandleTaskStateRequest;

        subscribedController = controller;

        if (subscribedController != null)
            subscribedController.OnStateRequested += HandleTaskStateRequest;
    }
    private void HandleTaskStateRequest(BaseTaskController.StateRequest request)
    {
        switch (request)
        {
            case BaseTaskController.StateRequest.EnterDialogue:
                EnterDialogue();
                break;

            case BaseTaskController.StateRequest.EnterExecutingTask:
                EnterExecutingTask();
                ShowInTaskOptions();
                break;

            case BaseTaskController.StateRequest.EnterPaused:
                EnterPaused();
                ShowInTaskOptions();
                break;

            case BaseTaskController.StateRequest.CompleteTask:
                TaskManager.Instance.OnTaskCompleted();
                TaskManager.Instance.StartNextTask();
                break;
        }
    }
}
