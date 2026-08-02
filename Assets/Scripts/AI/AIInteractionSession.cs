using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public enum AIInteractionState
{
    NotStarted,
    Running,
    WaitingForPlayer,
    Finished
}

/// <summary>
/// Immutable value stored in Session history.
/// </summary>
[Serializable]
public sealed class AISessionConversationTurn
{
    public string Speaker { get; }
    public string Text { get; }
    public string Intent { get; }

    public AISessionConversationTurn(string speaker, string text, string intent)
    {
        Speaker = speaker ?? string.Empty;
        Text = text ?? string.Empty;
        Intent = intent ?? string.Empty;
    }

    public AIConversationTurn ToLegacyDto()
    {
        return new AIConversationTurn
        {
            Speaker = Speaker,
            Text = Text,
            Intent = Intent
        };
    }
}

/// <summary>
/// Immutable authoritative state for one active interaction.
/// AISessionDirector replaces this value whenever committed state changes.
/// </summary>
[Serializable]
public sealed class AIInteractionSession
{
    private readonly AISessionConversationTurn[] recentTurns;
    private readonly ReadOnlyCollection<AISessionConversationTurn> recentTurnsView;

    public string SessionId { get; }
    public int TurnIndex { get; }
    public string BlueprintId { get; }
    public string CurrentChapterId { get; }
    public AIInteractionState State { get; }
    public IReadOnlyList<AISessionConversationTurn> RecentTurns => recentTurnsView;
    public int MaxRecentTurns { get; }

    public AIInteractionSession(
        string sessionId,
        int turnIndex,
        string blueprintId,
        string currentChapterId,
        AIInteractionState state,
        IEnumerable<AISessionConversationTurn> recentTurns = null,
        int maxRecentTurns = 10)
    {
        SessionId = sessionId ?? string.Empty;
        TurnIndex = Math.Max(0, turnIndex);
        BlueprintId = blueprintId ?? string.Empty;
        CurrentChapterId = currentChapterId ?? string.Empty;
        State = state;
        MaxRecentTurns = maxRecentTurns > 0 ? maxRecentTurns : 10;

        var copiedHistory = new List<AISessionConversationTurn>();
        if (recentTurns != null)
        {
            foreach (var turn in recentTurns)
            {
                if (turn == null)
                    continue;

                copiedHistory.Add(new AISessionConversationTurn(turn.Speaker, turn.Text, turn.Intent));
            }
        }

        var overflow = copiedHistory.Count - MaxRecentTurns;
        if (overflow > 0)
            copiedHistory.RemoveRange(0, overflow);

        this.recentTurns = copiedHistory.ToArray();
        recentTurnsView = Array.AsReadOnly(this.recentTurns);
    }

    public AISessionConversationTurn[] CopyRecentTurns()
    {
        var copy = new AISessionConversationTurn[recentTurns.Length];
        for (var i = 0; i < recentTurns.Length; i++)
        {
            var turn = recentTurns[i];
            copy[i] = new AISessionConversationTurn(turn.Speaker, turn.Text, turn.Intent);
        }

        return copy;
    }
}
