using System.Collections.Generic;
using UnityEngine;

public enum AIInteractionState
{
    NotStarted,
    Running,
    WaitingForPlayer,
    Finished
}

[System.Serializable]
public class AIInteractionSession
{
    public string SessionId;
    public int TurnIndex;

    public AISessionBlueprint Blueprint;
    public string CurrentChapterId;

    public AIInteractionState State = AIInteractionState.NotStarted;
    public List<AIConversationTurn> RecentTurns = new List<AIConversationTurn>();
    public int MaxRecentTurns = 10;

    public void AddConversationTurn(string speaker, string text, string intent)
    {
        if (RecentTurns == null)
            RecentTurns = new List<AIConversationTurn>();

        var turn = new AIConversationTurn
        {
            Speaker = speaker ?? string.Empty,
            Text = text ?? string.Empty,
            Intent = intent ?? string.Empty
        };

        RecentTurns.Add(turn);

        var maxTurns = MaxRecentTurns > 0 ? MaxRecentTurns : 10;
        while (RecentTurns.Count > maxTurns)
            RecentTurns.RemoveAt(0);

        Debug.Log($"[AIInteractionSession] Added {speaker} turn: '{turn.Text}' Intent='{turn.Intent}'");
    }
}
