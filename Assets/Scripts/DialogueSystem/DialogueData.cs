using System.Collections.Generic;
using static UnityEngine.InputSystem.DefaultInputActions;

[System.Serializable]
public class DialogueData
{
    public string taskId;
    public List<DialogueEntry> dialogues;

   /* public DialogueEntry GetEntryById(string dialogueId)
    {
        if (string.IsNullOrEmpty(dialogueId))
            return null;

        if (dialogues == null)
            return null;

        for (int i = 0; i < dialogues.Count; i++)
        {
            if (dialogues[i].id == dialogueId)
                return dialogues[i];
        }

        return null;
    }
    public static PlayerOption FindOptionById(this DialogueEntry entry, string optionId)
    {
        if (entry == null || string.IsNullOrEmpty(optionId))
            return null;

        var options = entry.playerOptions;
        if (options == null || options.Count == 0)
            return null;

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null && options[i].id == optionId)
                return options[i];
        }

        return null;
    }*/
}

[System.Serializable]
public class DialogueEntry
{
    public string id;  
    public List<string> npcVariants;
    public List<PlayerOption> playerOptions;
}

[System.Serializable]
public class PlayerOption
{
    public string id;
    public List<string> variants;
    public string type; // Positive, Negative, Neutral и т.д.
    public string action;
    public string nextDialogueId;
}

