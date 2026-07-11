using UnityEngine;

public class DialogueActionAdapter : MonoBehaviour
{
    public StateController stateController;

    public void OnOptionSelected(string answerId, string action)
    {
        Debug.Log($"[DialogueActionAdapter] Action received: {action}");

        if (stateController != null)
        {
            stateController.OnDialogueAction(answerId, action);
        }
        else
        {
            Debug.LogError("[DialogueActionAdapter] StateController is not assigned!");
        }
    }
}
