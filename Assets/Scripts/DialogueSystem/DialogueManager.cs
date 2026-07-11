using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    private DialogueData currentDialogue;
    private string currentTaskId;

 

    public DialogueData GetCurrentDialogue()
    {
        return currentDialogue;
    }

    public string GetActionForAnswer(string answerId)
    {
        if (currentDialogue == null) return null;

        foreach (var entry in currentDialogue.dialogues)
            foreach (var opt in entry.playerOptions)
                if (opt.id == answerId)
                    return opt.action;

        return null;
    }
    



    public DialogueEntry GetDialogueEntry(int index)
    {
        if (currentDialogue == null || currentDialogue.dialogues == null)
        {
            Debug.LogWarning("[DialogueManager] No dialogue loaded.");
            return null;
        }

        if (index < 0 || index >= currentDialogue.dialogues.Count)
        {
            Debug.LogWarning($"[DialogueManager] Dialogue index {index} out of range.");
            return null;
        }

        return currentDialogue.dialogues[index];
    }

    public DialogueEntry LoadDialogueAndGetEntry(string dialogueId, string entryId)
    {
        LoadDialogue(dialogueId);

        if (currentDialogue == null)
            return null;

        foreach (var entry in currentDialogue.dialogues)
        {
            if (entry.id == entryId)
                return entry;
        }

        Debug.LogWarning($"[DialogueManager] Entry '{entryId}' not found in dialogue '{dialogueId}'.");
        return null;
    }
    public DialogueData LoadDialogue(string taskId)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Dialogues", taskId + ".json");
        if (!File.Exists(path))
        {
            Debug.LogError($"[DialogueManager] Dialogue file not found: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        currentDialogue = JsonUtility.FromJson<DialogueData>(json);
        currentTaskId = taskId;
        return currentDialogue;
    }

    public DialogueEntry GetRandomEntry()
    {
        if (currentDialogue == null || currentDialogue.dialogues.Count == 0)
            return null;

        int index = Random.Range(0, currentDialogue.dialogues.Count);
        return currentDialogue.dialogues[index];
    }

    public void LoadDialogueFromFile(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Dialogues", fileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"[DialogueManager] Dialogue file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        currentDialogue = JsonUtility.FromJson<DialogueData>(json);

        if (currentDialogue == null)
            Debug.LogError($"[DialogueManager] Failed to parse dialogue JSON: {fileName}");
        else
            Debug.Log($"[DialogueManager] Loaded dialogue: {currentDialogue.taskId}");
    }

    public DialogueData LoadDialogueFromFileReturn(string fileName)
    {
        LoadDialogueFromFile(fileName);
        return currentDialogue;
    }


    public string HandlePlayerAnswer(PlayerOption option)
    {
        return option.id;
    }
}
