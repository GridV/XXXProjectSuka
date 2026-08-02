using System;
using System.Collections.Generic;
using UnityEngine;

public class AISessionDirector : MonoBehaviour
{
    [SerializeField]
    private AISessionBlueprint defaultBlueprint;

    [SerializeField]
    private AIBridge aiBridge;

    [SerializeField]
    private bool autoStartOnPlay = true;

    private AIInteractionSession currentSession;
    private AISessionSnapshot currentSnapshot;
    private AISessionBlueprint activeBlueprint;
    private long snapshotVersion;

    public AIInteractionSession CurrentSession => currentSession;
    public AISessionSnapshot CurrentSnapshot => currentSnapshot;
    public AISessionBlueprint ActiveBlueprint => activeBlueprint;
    public bool HasActiveSession => currentSession != null && currentSession.State != AIInteractionState.Finished;

    public void StartDefaultSession()
    {
        Debug.Log("[AISessionDirector] StartDefaultSession called");

        if (defaultBlueprint == null)
        {
            Debug.LogError("[AISessionDirector] Cannot start session: blueprint is null.");
            return;
        }

        if (aiBridge == null)
        {
            Debug.LogError("[AISessionDirector] Cannot start session: AIBridge is not assigned.");
            return;
        }

        Debug.Log($"[AISessionDirector] Starting session: {defaultBlueprint.title ?? defaultBlueprint.blueprintId ?? "<unnamed>"}");
        StartSession(defaultBlueprint);
    }

    private void Start()
    {
        Debug.Log($"[AISessionDirector] Start. autoStartOnPlay={autoStartOnPlay}");

        if (autoStartOnPlay)
            StartDefaultSession();
    }

    public void StartSession(AISessionBlueprint blueprint)
    {
        if (blueprint == null)
        {
            Debug.LogError("[AISessionDirector] Cannot start session: blueprint is null.");
            return;
        }

        if (aiBridge == null)
        {
            Debug.LogError("[AISessionDirector] Cannot start session: AIBridge is not assigned.");
            return;
        }

        if (currentSession != null && currentSession.State != AIInteractionState.Finished)
        {
            EndSession();
        }

        activeBlueprint = blueprint;
        snapshotVersion = 0;
        ReplaceSession(CreateSession(blueprint));

        Debug.Log($"[AISessionDirector] Session created. Id={currentSession.SessionId}");
        Debug.Log($"[AISessionDirector] Start chapter: {currentSession.CurrentChapterId}");

        Debug.Log("[AISessionDirector] Calling AIBridge.StartSession");
        aiBridge.StartSession();
    }

    public void EndSession()
    {
        if (currentSession == null)
            return;

        if (currentSession.State == AIInteractionState.Finished)
            return;

        ReplaceSession(CopySession(state: AIInteractionState.Finished));
        Debug.Log("[AISessionDirector] Session finished.");
    }

    public bool RecordNpcConversationTurn(string text, string intent)
    {
        return AppendConversationTurn("NPC", text, intent);
    }

    public bool RecordPlayerConversationTurn(AIPlayerIntent playerIntent)
    {
        if (playerIntent == null)
        {
            Debug.LogWarning("[AISessionDirector] Cannot record player turn: Player Intent is null.");
            return false;
        }

        return AppendConversationTurn("Player", playerIntent.DisplayText, playerIntent.IntentTag);
    }

    public bool AdvanceTurnIndex()
    {
        if (!CanMutateActiveSession("advance TurnIndex"))
            return false;

        ReplaceSession(CopySession(turnIndex: currentSession.TurnIndex + 1));
        Debug.Log($"[AISessionDirector] Turn index advanced to {currentSession.TurnIndex}.");
        return true;
    }

    public bool CommitLegacyChapterTransition(AIChapterTransitionResolution resolution)
    {
        if (!CanMutateActiveSession("commit chapter transition"))
            return false;

        if (resolution == null || !resolution.Succeeded)
        {
            Debug.LogWarning($"[AISessionDirector] Chapter transition was not committed: {resolution?.FailureReason ?? "resolution is null"}");
            return false;
        }

        if (!string.Equals(currentSession.CurrentChapterId, resolution.PreviousChapterId, StringComparison.Ordinal))
        {
            Debug.LogWarning("[AISessionDirector] Chapter transition was not committed because Session state changed after resolution.");
            return false;
        }

        if (activeBlueprint == null || activeBlueprint.GetChapter(resolution.NextChapterId) == null)
        {
            Debug.LogWarning($"[AISessionDirector] Chapter transition destination '{resolution.NextChapterId}' is unavailable.");
            return false;
        }

        ReplaceSession(CopySession(currentChapterId: resolution.NextChapterId));
        Debug.Log($"[AISessionDirector] Chapter changed: {resolution.PreviousChapterId} -> {resolution.NextChapterId}");
        return true;
    }

    public bool SetSessionWaitingForPlayer()
    {
        return SetLifecycleState(AIInteractionState.WaitingForPlayer, "wait for player");
    }

    public bool SetSessionRunning()
    {
        return SetLifecycleState(AIInteractionState.Running, "resume running");
    }

    public void NotifyTaskCompleted()
    {
        if (currentSession == null)
        {
            Debug.LogWarning("[AISessionDirector] Cannot notify task completion: no active session.");
            return;
        }

        Debug.Log($"[AISessionDirector] Task completed notification received for session {currentSession.SessionId}.");
        Debug.Log("[AISessionDirector] Notifying AIBridge to request the next AI response.");

        if (aiBridge != null)
        {
            aiBridge.RequestAndApply();
        }
        else
        {
            Debug.LogError("[AISessionDirector] AIBridge not assigned; cannot request the next AI response.");
        }
    }

    private AIInteractionSession CreateSession(AISessionBlueprint blueprint)
    {
        return new AIInteractionSession(
            Guid.NewGuid().ToString(),
            0,
            blueprint.blueprintId,
            blueprint.startChapterId,
            AIInteractionState.Running,
            Array.Empty<AISessionConversationTurn>());
    }

    private bool AppendConversationTurn(string speaker, string text, string intent)
    {
        if (!CanMutateActiveSession($"record {speaker} conversation turn"))
            return false;

        var history = new List<AISessionConversationTurn>(currentSession.CopyRecentTurns())
        {
            new AISessionConversationTurn(speaker, text, intent)
        };

        ReplaceSession(CopySession(recentTurns: history));
        Debug.Log($"[AISessionDirector] Recorded {speaker} turn. Intent='{intent ?? string.Empty}'");
        return true;
    }

    private bool SetLifecycleState(AIInteractionState state, string operation)
    {
        if (!CanMutateActiveSession(operation))
            return false;

        if (currentSession.State == state)
            return true;

        ReplaceSession(CopySession(state: state));
        return true;
    }

    private bool CanMutateActiveSession(string operation)
    {
        if (currentSession == null)
        {
            Debug.LogWarning($"[AISessionDirector] Cannot {operation}: no Session exists.");
            return false;
        }

        if (currentSession.State == AIInteractionState.Finished)
        {
            Debug.LogWarning($"[AISessionDirector] Cannot {operation}: Session is finished.");
            return false;
        }

        return true;
    }

    private AIInteractionSession CopySession(
        int? turnIndex = null,
        string currentChapterId = null,
        AIInteractionState? state = null,
        IEnumerable<AISessionConversationTurn> recentTurns = null)
    {
        return new AIInteractionSession(
            currentSession.SessionId,
            turnIndex ?? currentSession.TurnIndex,
            currentSession.BlueprintId,
            currentChapterId ?? currentSession.CurrentChapterId,
            state ?? currentSession.State,
            recentTurns ?? currentSession.CopyRecentTurns(),
            currentSession.MaxRecentTurns);
    }

    private void ReplaceSession(AIInteractionSession replacement)
    {
        if (replacement == null)
            throw new ArgumentNullException(nameof(replacement));

        currentSession = replacement;
        currentSnapshot = new AISessionSnapshot(++snapshotVersion, currentSession);

        // Migration debt: AIBridge caches the immutable Session only to keep the legacy
        // provider/execution path operational. Future phases must consume snapshots directly.
        if (aiBridge != null)
            aiBridge.SetSession(currentSession, activeBlueprint, this);
    }
}
