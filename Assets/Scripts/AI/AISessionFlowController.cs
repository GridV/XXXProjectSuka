using System;
using UnityEngine;

public class AISessionFlowController
{
    public bool TryMoveToNextChapter(
        AIInteractionSession session,
        string playerIntentTag,
        out string previousChapterId,
        out string nextChapterId)
    {
        previousChapterId = string.Empty;
        nextChapterId = string.Empty;

        if (session == null)
        {
            Debug.LogWarning("[AISessionFlow] Session is null.");
            return false;
        }

        if (session.Blueprint == null)
        {
            Debug.LogWarning("[AISessionFlow] Blueprint is missing.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(session.CurrentChapterId))
        {
            Debug.LogWarning("[AISessionFlow] CurrentChapterId is empty.");
            return false;
        }

        Debug.Log($"[AISessionFlow] Current chapter: {session.CurrentChapterId}");
        Debug.Log($"[AISessionFlow] Player intent: {playerIntentTag}");

        var currentChapter = session.Blueprint.GetChapter(session.CurrentChapterId);
        if (currentChapter == null)
        {
            Debug.LogWarning($"[AISessionFlow] Chapter '{session.CurrentChapterId}' was not found in blueprint.");
            return false;
        }

        previousChapterId = currentChapter.chapterId ?? string.Empty;

        if (string.IsNullOrWhiteSpace(playerIntentTag))
        {
            Debug.LogWarning("[AISessionFlow] No transition found for empty intent.");
            return false;
        }

        var normalizedIntent = playerIntentTag.Trim();
        if (currentChapter.transitions != null && currentChapter.transitions.Length > 0)
        {
            for (int i = 0; i < currentChapter.transitions.Length; i++)
            {
                var transition = currentChapter.transitions[i];
                if (transition == null || string.IsNullOrWhiteSpace(transition.intentTag))
                    continue;

                if (string.Equals(transition.intentTag.Trim(), normalizedIntent, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(transition.nextChapterId))
                    {
                        Debug.LogWarning($"[AISessionFlow] Destination chapter not found: {transition.nextChapterId}");
                        return false;
                    }

                    if (session.Blueprint.GetChapter(transition.nextChapterId) == null)
                    {
                        Debug.LogWarning($"[AISessionFlow] Destination chapter not found: {transition.nextChapterId}");
                        return false;
                    }

                    nextChapterId = transition.nextChapterId;
                    session.CurrentChapterId = nextChapterId;
                    Debug.Log($"[AISessionFlow] Chapter changed: {previousChapterId} -> {nextChapterId}");
                    return true;
                }
            }

            Debug.Log($"[AISessionFlow] No transition found for intent: {playerIntentTag}");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(currentChapter.nextChapterId))
        {
            if (session.Blueprint.GetChapter(currentChapter.nextChapterId) == null)
            {
                Debug.LogWarning($"[AISessionFlow] Destination chapter not found: {currentChapter.nextChapterId}");
                return false;
            }

            nextChapterId = currentChapter.nextChapterId;
            session.CurrentChapterId = nextChapterId;
            Debug.Log($"[AISessionFlow] Chapter changed: {previousChapterId} -> {nextChapterId}");
            return true;
        }

        Debug.Log($"[AISessionFlow] No transition found for intent: {playerIntentTag}");
        return false;
    }
}
