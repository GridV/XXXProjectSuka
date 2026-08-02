using System;

public sealed class AIChapterTransitionResolution
{
    public bool Succeeded { get; }
    public string PreviousChapterId { get; }
    public string NextChapterId { get; }
    public string FailureReason { get; }

    private AIChapterTransitionResolution(
        bool succeeded,
        string previousChapterId,
        string nextChapterId,
        string failureReason)
    {
        Succeeded = succeeded;
        PreviousChapterId = previousChapterId ?? string.Empty;
        NextChapterId = nextChapterId ?? string.Empty;
        FailureReason = failureReason ?? string.Empty;
    }

    public static AIChapterTransitionResolution Success(string previousChapterId, string nextChapterId)
    {
        return new AIChapterTransitionResolution(true, previousChapterId, nextChapterId, string.Empty);
    }

    public static AIChapterTransitionResolution Failure(string reason)
    {
        return new AIChapterTransitionResolution(false, string.Empty, string.Empty, reason);
    }
}

/// <summary>
/// Temporary legacy transition resolver. It proposes a chapter; only AISessionDirector commits it.
/// This is not the procedural Decision Engine.
/// </summary>
public sealed class AISessionFlowController
{
    public AIChapterTransitionResolution ResolveNextChapter(
        AIInteractionSession session,
        AISessionBlueprint blueprint,
        string playerIntentTag)
    {
        if (session == null)
            return AIChapterTransitionResolution.Failure("Session is null.");

        if (blueprint == null)
            return AIChapterTransitionResolution.Failure("Blueprint is missing.");

        if (!string.Equals(session.BlueprintId, blueprint.blueprintId, StringComparison.Ordinal))
            return AIChapterTransitionResolution.Failure("Resolved Blueprint does not match the Session BlueprintId.");

        if (string.IsNullOrWhiteSpace(session.CurrentChapterId))
            return AIChapterTransitionResolution.Failure("CurrentChapterId is empty.");

        var currentChapter = blueprint.GetChapter(session.CurrentChapterId);
        if (currentChapter == null)
            return AIChapterTransitionResolution.Failure($"Chapter '{session.CurrentChapterId}' was not found in Blueprint.");

        if (string.IsNullOrWhiteSpace(playerIntentTag))
            return AIChapterTransitionResolution.Failure("Player intent is empty.");

        var normalizedIntent = playerIntentTag.Trim();
        if (currentChapter.transitions != null && currentChapter.transitions.Length > 0)
        {
            for (var i = 0; i < currentChapter.transitions.Length; i++)
            {
                var transition = currentChapter.transitions[i];
                if (transition == null || string.IsNullOrWhiteSpace(transition.intentTag))
                    continue;

                if (!string.Equals(transition.intentTag.Trim(), normalizedIntent, StringComparison.OrdinalIgnoreCase))
                    continue;

                return ResolveDestination(blueprint, currentChapter.chapterId, transition.nextChapterId);
            }

            return AIChapterTransitionResolution.Failure($"No transition found for intent '{playerIntentTag}'.");
        }

        if (!string.IsNullOrWhiteSpace(currentChapter.nextChapterId))
            return ResolveDestination(blueprint, currentChapter.chapterId, currentChapter.nextChapterId);

        return AIChapterTransitionResolution.Failure($"No transition found for intent '{playerIntentTag}'.");
    }

    private static AIChapterTransitionResolution ResolveDestination(
        AISessionBlueprint blueprint,
        string previousChapterId,
        string nextChapterId)
    {
        if (string.IsNullOrWhiteSpace(nextChapterId))
            return AIChapterTransitionResolution.Failure("Destination chapter ID is empty.");

        if (blueprint.GetChapter(nextChapterId) == null)
            return AIChapterTransitionResolution.Failure($"Destination chapter '{nextChapterId}' was not found in Blueprint.");

        return AIChapterTransitionResolution.Success(previousChapterId, nextChapterId);
    }
}
