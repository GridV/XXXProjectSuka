using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Minimal immutable view of committed Session state used across runtime boundaries.
/// </summary>
public sealed class AISessionSnapshot
{
    private readonly AISessionConversationTurn[] recentTurns;
    private readonly ReadOnlyCollection<AISessionConversationTurn> recentTurnsView;

    public long Version { get; }
    public string SessionId { get; }
    public int TurnIndex { get; }
    public string BlueprintId { get; }
    public string CurrentChapterId { get; }
    public AIInteractionState State { get; }
    public IReadOnlyList<AISessionConversationTurn> RecentTurns => recentTurnsView;

    public AISessionSnapshot(long version, AIInteractionSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        Version = version;
        SessionId = session.SessionId;
        TurnIndex = session.TurnIndex;
        BlueprintId = session.BlueprintId;
        CurrentChapterId = session.CurrentChapterId;
        State = session.State;

        recentTurns = session.CopyRecentTurns();
        recentTurnsView = Array.AsReadOnly(recentTurns);
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
