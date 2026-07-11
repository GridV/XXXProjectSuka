using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// MVP bridge between gameplay and AI response pipeline.
// Mediates AI requests, validation, UI presentation, and animation application.
public class AIBridge : MonoBehaviour
{
    [SerializeField]
    private AIInteractionSession currentSession;

    [SerializeField]
    private AITagDatabase tagDatabase;

    private AIAnimationExecutor animationExecutor;

    private AISessionFlowController sessionFlowController;

    [SerializeField]
    private AITextOptionPresenter textOptionPresenter;
    
    [SerializeField]
    private AIResponseExecutor responseExecutor;

    private IAIDirectorProvider provider;
    private AIDirectorResponse lastResponse;

    private void Awake()
    {
        // Use fake provider for MVP/testing.
        provider = new FakeAIDirectorProvider();
        sessionFlowController = new AISessionFlowController();
    }

    private void Start()
    {
        // AIBridge does not auto-start. Session lifecycle is driven by AISessionDirector.
    }

    public void SetSession(AIInteractionSession session)
    {
        currentSession = session;
        if (session != null)
        {
            Debug.Log($"[AIBridge] Session assigned: {session.SessionId}");
        }
    }

    public void StartSession()
    {
        Debug.Log("[AIBridge] StartSession called");

        if (provider == null)
        {
            Debug.LogError("[AIBridge] provider is null.");
            return;
        }

        if (currentSession == null)
        {
            Debug.LogError("[AIBridge] Cannot start session: session is null.");
            return;
        }

        if (currentSession.Blueprint == null)
        {
            Debug.LogError("[AIBridge] Cannot start session: session.Blueprint is null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(currentSession.CurrentChapterId))
        {
            Debug.LogError("[AIBridge] Cannot start session: CurrentChapterId is empty.");
            return;
        }

        Debug.Log($"[AIBridge] Starting AI session. Chapter={currentSession.CurrentChapterId}");
        RequestAndApply();
    }

    public void RequestAndApply()
    {
        Debug.Log("[AIBridge] RequestAndApply called");
        if (currentSession == null)
        {
            Debug.LogError("[AIBridge] Cannot request AI response: session is null.");
            return;
        }
        if (currentSession.Blueprint == null)
        {
            Debug.LogError("[AIBridge] Cannot request AI response: session.Blueprint is null.");
            return;
        }
        if (string.IsNullOrWhiteSpace(currentSession.CurrentChapterId))
        {
            Debug.LogError("[AIBridge] Cannot request AI response: CurrentChapterId is empty.");
            return;
        }

        var response = provider.GetAIDirectorResponse();
        ProcessResponse(response);
    }

    private void ProcessResponse(AIDirectorResponse response)
    {
        if (response == null)
        {
            Debug.LogError("[AIBridge] Received null response.");
            return;
        }

        var validation = AIDirectorResponseValidator.Validate(response, tagDatabase);
        if (!validation.IsValid)
        {
            Debug.LogError("[AIBridge] Response validation failed:");
            foreach (var error in validation.Errors)
                Debug.LogError(" - " + error);
            return;
        }

        if (currentSession != null)
        {
            currentSession.AddConversationTurn("NPC", response.TextLine, response.ConversationIntent ?? string.Empty);
        }
        else
        {
            Debug.LogWarning("[AIBridge] currentSession is not assigned; NPC turn will not be recorded.");
        }

        lastResponse = response;
        Debug.Log($"[AIBridge] Response text: '{response.TextLine}'");

        // Prefer centralized executor when available to keep AIBridge simple.
        if (responseExecutor != null)
        {
            responseExecutor.Execute(response, HandlePlayerIntentSelected);
        }
        else
        {
            if (textOptionPresenter != null)
            {
                textOptionPresenter.Present(response.TextLine, response.PlayerOptions, HandlePlayerIntentSelected);
            }
            else
            {
                Debug.LogWarning("[AIBridge] textOptionPresenter is not assigned; UI will not update.");
            }

            RouteAnimation(response);
        }
    }

    private void HandlePlayerIntentSelected(string intentTag)
    {
        Debug.Log($"[AIBridge] Player selected intent: {intentTag}");

        if (provider == null)
        {
            Debug.LogError("[AIBridge] provider is null when handling player intent.");
            return;
        }

        if (currentSession == null)
        {
            Debug.LogError("[AIBridge] currentSession is not assigned; cannot build AI request.");
            return;
        }

        var selectedText = FindOptionText(intentTag);
        currentSession.AddConversationTurn("Player", selectedText, intentTag ?? string.Empty);
        currentSession.TurnIndex++;
        Debug.Log($"[AIInteractionSession] Turn index: {currentSession.TurnIndex}");

        string previousChapterId;
        string nextChapterId;
        var moved = sessionFlowController.TryMoveToNextChapter(currentSession, intentTag, out previousChapterId, out nextChapterId);

        var request = AIContextBuilder.BuildRequest(currentSession, intentTag, selectedText, tagDatabase);
        if (request == null)
        {
            Debug.LogWarning("[AIBridge] AIContextBuilder returned null; aborting continuation.");
            return;
        }

        Debug.Log("========== AI REQUEST ==========");
        Debug.Log($"Session: {request.SessionId} (Turn {request.TurnIndex})");
        Debug.Log($"Chapter: {request.CurrentChapterId}");
        Debug.Log($"Goal: {request.ChapterGoal}");
        Debug.Log($"Player Intent: {request.PlayerIntent}");
        Debug.Log($"Allowed Commands: {(request.AllowedCommands != null ? string.Join(", ", request.AllowedCommands) : "<none>")}");
        Debug.Log("================================");

        var response = provider.GetAIDirectorResponse(request);
        Debug.Log("[AIBridge] Received continuation response.");
        ProcessResponse(response);
    }

    private string FindOptionText(string intentTag)
    {
        if (lastResponse?.PlayerOptions == null)
            return string.Empty;

        foreach (var option in lastResponse.PlayerOptions)
        {
            if (option == null)
                continue;

            if (string.Equals(option.IntentTag, intentTag, System.StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(option.ButtonText))
                    return option.ButtonText;
                if (!string.IsNullOrWhiteSpace(option.Label))
                    return option.Label;
            }
        }

        return string.Empty;
    }

    private void RouteAnimation(AIDirectorResponse response)
    {
        // Lazy-resolve animation executor to avoid cross-scene inspector references.
        if (animationExecutor == null)
            animationExecutor = FindFirstObjectByType<AIAnimationExecutor>();

        if (animationExecutor == null)
        {
            Debug.LogWarning("[AIBridge] animationExecutor is missing; skipping animation execution.");
            return;
        }

        var requestedTags = GetRequestedAnimationTags(response);
        var selectedAnimation = AIAnimationSelector.SelectBest(requestedTags);

        if (selectedAnimation == null)
        {
            Debug.LogWarning($"[AIBridge] No animation selected for tags [{string.Join(", ", requestedTags)}].");
            return;
        }

        Debug.Log($"[AIBridge] Selected animation id='{selectedAnimation.Id}'.");
        bool requested = animationExecutor.TryPlay(selectedAnimation);
        Debug.Log($"[AIBridge] Animation execution requested: {requested}");
    }

    private string[] GetRequestedAnimationTags(AIDirectorResponse response)
    {
        if (response == null)
            return new string[0];

        var tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(response.BodyIntent))
            tags.Add(response.BodyIntent.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(response.ExpressionTag))
            tags.Add(response.ExpressionTag.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(response.ConversationIntent))
            tags.Add(response.ConversationIntent.Trim().ToLowerInvariant());
        return tags.ToArray();
    }
}
