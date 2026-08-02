using System;
using System.Linq;
using UnityEngine;

public static class AIContextBuilder
{
    public static AIDirectorRequest BuildRequest(
        AISessionSnapshot snapshot,
        AISessionBlueprint blueprint,
        string playerIntent,
        string playerText,
        AITagDatabase tagDatabase)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        var chapter = blueprint?.GetChapter(snapshot.CurrentChapterId);

        if (blueprint == null)
        {
            Debug.LogWarning("[AIContextBuilder] Resolved Blueprint is null - using empty Blueprint defaults.");
        }
        else if (!string.Equals(snapshot.BlueprintId, blueprint.blueprintId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Resolved Blueprint does not match the Session Snapshot BlueprintId.");
        }

        if (chapter == null)
        {
            Debug.LogWarning($"[AIContextBuilder] Chapter '{snapshot.CurrentChapterId}' not found on Blueprint; using empty Chapter defaults.");
        }

        return new AIDirectorRequest
        {
            SessionId = snapshot.SessionId,
            TurnIndex = snapshot.TurnIndex,

            BlueprintId = snapshot.BlueprintId,
            CurrentChapterId = snapshot.CurrentChapterId,
            ChapterGoal = chapter?.goal ?? string.Empty,
            ChapterInstructions = chapter?.instructions ?? string.Empty,
            FlowMode = blueprint != null ? blueprint.flowMode.ToString() : AISessionFlowMode.Linear.ToString(),

            RecentTurns = snapshot.RecentTurns.Select(turn => turn.ToLegacyDto()).ToArray(),
            PlayerIntent = playerIntent ?? string.Empty,
            PlayerText = playerText ?? string.Empty,

            AllowedCommands = chapter?.allowedCommands ?? Array.Empty<string>(),
            AllowedNextChapterIds = chapter?.allowedNextChapterIds ?? Array.Empty<string>()
        };
    }
}
