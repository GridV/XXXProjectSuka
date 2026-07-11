using UnityEngine;

public enum AISessionFlowMode
{
    Linear,
    DirectorControlled
}

[CreateAssetMenu(menuName = "AI/Session Blueprint", fileName = "AISessionBlueprint")]
public class AISessionBlueprint : ScriptableObject
{
    [Tooltip("Unique identifier for this session blueprint.")]
    public string blueprintId;

    [Tooltip("Human-friendly title for the session blueprint.")]
    public string title;

    [Tooltip("Chapter id where the session begins.")]
    public string startChapterId;

    [Tooltip("The flow mode used by this session blueprint.")]
    public AISessionFlowMode flowMode = AISessionFlowMode.Linear;

    [Tooltip("Chapters that define the session flow.")]
    public AISessionChapter[] chapters = new AISessionChapter[0];

    public AISessionChapter GetChapter(string chapterId)
    {
        if (string.IsNullOrEmpty(chapterId) || chapters == null)
            return null;

        for (int i = 0; i < chapters.Length; i++)
        {
            if (chapters[i] != null && chapters[i].chapterId == chapterId)
                return chapters[i];
        }

        return null;
    }
}

[System.Serializable]
public class AIChapterTransition
{
    public string intentTag;
    public string nextChapterId;
}

[System.Serializable]
public class AISessionChapter
{
    [Header("Identity")]
    public string chapterId;
    public string title;

    [Header("AI")]
    [TextArea]
    public string goal;

    [TextArea]
    public string instructions;

    [Header("Flow")]
    public AIChapterTransition[] transitions = new AIChapterTransition[0];
    public string nextChapterId;
    public string[] allowedNextChapterIds = new string[0];
    public bool waitForPlayerResponse = true;

    [Header("Commands")]
    public string[] allowedCommands = new string[0];
}
