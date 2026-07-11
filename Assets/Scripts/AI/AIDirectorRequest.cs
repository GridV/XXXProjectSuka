using System;
using UnityEngine;

[Serializable]
public class AIDirectorRequest
{
    // Session
    public string SessionId;
    public int TurnIndex;
    public string State;

    // Blueprint
    public string BlueprintId;
    public string BlueprintTitle;

    // Current chapter
    public string CurrentChapterId;
    public string CurrentChapterTitle;
    public string ChapterGoal;
    public string ChapterInstructions;
    public string FlowMode;

    // Conversation memory
    public AIConversationTurn[] RecentTurns = new AIConversationTurn[0];

    // Current player input
    public string PlayerIntent;
    public string PlayerText;

    // Flow restrictions
    public string[] AllowedCommands = new string[0];
    public string[] AllowedNextChapterIds = new string[0];

    // Backwards-compatible properties for existing AI pipeline initializers.
    public string SceneId { get; set; }
    public string TaskId { get; set; }
    public string Phase { get; set; }
    public float Engagement { get; set; }
    public float SessionTimeSeconds { get; set; }
    public string PreviousNpcLine { get; set; }
    public string CharacterEmotion { get; set; }
    public string CurrentAnimationState { get; set; }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
