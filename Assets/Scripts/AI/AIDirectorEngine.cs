using System.Collections.Generic;
using UnityEngine;

// MVP director engine: gets responses from a provider, validates, and logs outcome.
public class AIDirectorEngine : MonoBehaviour
{
    [SerializeField]
    private bool runOnStart = true;

    [SerializeField]
    private AITagDatabase tagDatabase;

    [SerializeField]
    private AIAnimationExecutor animationExecutor;

    private IAIDirectorProvider provider;

    void Awake()
    {
        // Use the fake provider for MVP/testing.
        provider = new FakeAIDirectorProvider();
    }

    void Start()
    {
        if (!runOnStart) return;

        if (provider == null)
        {
            Debug.LogError("AIDirectorEngine: provider is null.");
            return;
        }

        var response = provider.GetAIDirectorResponse();
        var validation = AIDirectorResponseValidator.Validate(response, tagDatabase);

        if (!validation.IsValid)
        {
            Debug.LogError("AIDirectorEngine: Response validation failed. Errors:");
            foreach (var e in validation.Errors)
                Debug.LogError(" - " + e);
            return;
        }

        Debug.Log("AIDirectorEngine: Response is valid.");
        Debug.Log("TextLine: " + response.TextLine);
        Debug.Log("EmotionTag: " + response.EmotionTag);
        Debug.Log("ConversationIntent: " + response.ConversationIntent);
        Debug.Log("ExpressionTag: " + response.ExpressionTag);
        Debug.Log("BodyIntent: " + response.BodyIntent);
        Debug.Log("GameplayCommand: " + response.GameplayCommand);
        Debug.Log("CameraIntent: " + response.CameraIntent);

        if (response.PlayerOptions != null && response.PlayerOptions.Count > 0)
        {
            Debug.Log("PlayerOptions:");
            for (int i = 0; i < response.PlayerOptions.Count; i++)
            {
                var opt = response.PlayerOptions[i];
                if (opt == null)
                {
                    Debug.LogWarning($"  PlayerOptions[{i}] is null");
                    continue;
                }
                Debug.Log($"  OptionId={opt.OptionId}, Label={opt.Label}, ButtonText={opt.ButtonText}, IntentTag={opt.IntentTag}");
            }
        }
        else
        {
            Debug.Log("PlayerOptions: <none>");
        }

        // Animation execution via AI animation pipeline
        if (animationExecutor == null)
        {
            Debug.LogWarning("AIDirectorEngine: animationExecutor is missing; skipping animation execution.");
            return;
        }

        var requestedTags = GetRequestedAnimationTags(response);
        var selectedAnimation = AIAnimationSelector.SelectBest(requestedTags);

        if (selectedAnimation == null)
        {
            Debug.LogWarning($"AIDirectorEngine: No animation selected for tags [{string.Join(", ", requestedTags)}].");
        }
        else
        {
            Debug.Log($"AIDirectorEngine: Selected animation id='{selectedAnimation.Id}' for tags [{string.Join(", ", requestedTags)}].");
            bool requested = animationExecutor.TryPlay(selectedAnimation);
            Debug.Log($"AIDirectorEngine: Animation execution was requested: {requested}");
        }
    }

    private string[] GetRequestedAnimationTags(AIDirectorResponse response)
    {
        var tags = new List<string>();
        if (response == null)
            return tags.ToArray();

        if (!string.IsNullOrWhiteSpace(response.BodyIntent))
            tags.Add(response.BodyIntent.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(response.ExpressionTag))
            tags.Add(response.ExpressionTag.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(response.ConversationIntent))
            tags.Add(response.ConversationIntent.Trim().ToLowerInvariant());

        return tags.ToArray();
    }
}
