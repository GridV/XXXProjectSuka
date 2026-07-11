using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AIContextBuilder
{
    public static AIDirectorRequest BuildRequest(
        AIInteractionSession session,
        string playerIntent,
        string playerText,
        AITagDatabase tagDatabase)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        var blueprint = session.Blueprint;
        var chapter = blueprint?.GetChapter(session.CurrentChapterId);

        if (blueprint == null)
        {
            Debug.LogWarning("[AIContextBuilder] session.Blueprint is null — using empty blueprint defaults.");
        }

        if (chapter == null)
        {
            Debug.LogWarning($"[AIContextBuilder] chapter '{session.CurrentChapterId}' not found on blueprint; using empty chapter defaults.");
        }

        return new AIDirectorRequest
        {
            SessionId = session.SessionId ?? string.Empty,
            TurnIndex = session.TurnIndex,

            BlueprintId = blueprint?.blueprintId ?? string.Empty,
            CurrentChapterId = session.CurrentChapterId ?? string.Empty,
            ChapterGoal = chapter?.goal ?? string.Empty,
            ChapterInstructions = chapter?.instructions ?? string.Empty,
            FlowMode = blueprint != null ? blueprint.flowMode.ToString() : AISessionFlowMode.Linear.ToString(),

            RecentTurns = session.RecentTurns != null ? session.RecentTurns.ToArray() : new AIConversationTurn[0],
            PlayerIntent = playerIntent ?? string.Empty,
            PlayerText = playerText ?? string.Empty,

            AllowedCommands = chapter?.allowedCommands != null ? chapter.allowedCommands : new string[0],
            AllowedNextChapterIds = chapter?.allowedNextChapterIds != null ? chapter.allowedNextChapterIds : new string[0]
        };
    }
}
