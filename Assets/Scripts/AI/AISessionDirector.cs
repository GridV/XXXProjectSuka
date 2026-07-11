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
    private bool hasFinishedSession;

    public AIInteractionSession CurrentSession => currentSession;
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

        currentSession = CreateSession(blueprint);
        hasFinishedSession = false;

        Debug.Log($"[AISessionDirector] Session created. Id={currentSession.SessionId}");
        Debug.Log($"[AISessionDirector] Start chapter: {currentSession.CurrentChapterId}");

        Debug.Log("[AISessionDirector] Calling AIBridge.SetSession");
        aiBridge.SetSession(currentSession);
        Debug.Log("[AISessionDirector] Calling AIBridge.StartSession");
        aiBridge.StartSession();
    }

    public void EndSession()
    {
        if (currentSession == null)
            return;

        if (currentSession.State == AIInteractionState.Finished)
            return;

        currentSession.State = AIInteractionState.Finished;
        hasFinishedSession = true;
        Debug.Log("[AISessionDirector] Session finished.");
    }

    private void Update()
    {
        if (currentSession == null || hasFinishedSession)
            return;

        if (currentSession.State == AIInteractionState.Finished)
        {
            hasFinishedSession = true;
            EndSession();
        }
    }

    private AIInteractionSession CreateSession(AISessionBlueprint blueprint)
    {
        return new AIInteractionSession
        {
            SessionId = Guid.NewGuid().ToString(),
            TurnIndex = 0,
            Blueprint = blueprint,
            CurrentChapterId = blueprint.startChapterId ?? string.Empty,
            State = AIInteractionState.Running,
            RecentTurns = new List<AIConversationTurn>()
        };
    }
}
