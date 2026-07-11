using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AIDirectorResponse
{
    // A single line of text the AI Director outputs.
    public string TextLine;

    public string ConversationIntent;
    public string EmotionTag;
    public string ExpressionTag;
    public string BodyIntent;
    public string GameplayCommand;
    public string NextChapterId;
    public string CameraIntent;

    public List<AIPlayerOptionDto> PlayerOptions = new List<AIPlayerOptionDto>();
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
