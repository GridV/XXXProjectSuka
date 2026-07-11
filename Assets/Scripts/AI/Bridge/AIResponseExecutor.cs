using System;
using System.Collections.Generic;
using UnityEngine;

// Dispatches parts of AIDirectorResponse to existing systems (UI, animation, commands placeholders).
public class AIResponseExecutor : MonoBehaviour
{
    [SerializeField]
    private AITextOptionPresenter textOptionPresenter;

    [SerializeField]
    private AIAnimationExecutor animationExecutor;

    private void Awake()
    {
        if (textOptionPresenter == null)
            textOptionPresenter = FindFirstObjectByType<AITextOptionPresenter>();

        if (animationExecutor == null)
            animationExecutor = FindFirstObjectByType<AIAnimationExecutor>();
    }

    // Execute the response: present text/options and trigger animations.
    public void Execute(AIDirectorResponse response, Action<string> onPlayerIntentSelected)
    {
        if (response == null)
        {
            Debug.LogWarning("[AIResponseExecutor] Null response; nothing to execute.");
            return;
        }

        Debug.Log("[AIResponseExecutor] Presenting response.");

        // 1) Present text and player options
        if (textOptionPresenter != null)
        {
            try
            {
                textOptionPresenter.Present(response.TextLine, response.PlayerOptions, onPlayerIntentSelected);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AIResponseExecutor] Error presenting text/options: {ex}");
            }
        }
        else
        {
            Debug.LogWarning("[AIResponseExecutor] No AITextOptionPresenter assigned or found; skipping UI presentation.");
        }

        // 2) Route body/intent to animation pipeline
        var tags = GetRequestedAnimationTags(response);
        if (tags.Length > 0)
        {
            if (animationExecutor != null)
            {
                var selected = AIAnimationSelector.SelectBest(tags);
                if (selected != null)
                {
                    Debug.Log($"[AIResponseExecutor] Executing animation id='{selected.Id}' for tags [{string.Join(", ", tags)}].");
                    try
                    {
                        animationExecutor.TryPlay(selected);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AIResponseExecutor] Error executing animation: {ex}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AIResponseExecutor] No animation mapping found for tags [{string.Join(", ", tags)}].");
                }
            }
            else
            {
                Debug.LogWarning("[AIResponseExecutor] No AIAnimationExecutor assigned or found; skipping animation execution.");
            }
        }

        // 3) Handle command placeholders
        if (!string.IsNullOrWhiteSpace(response.GameplayCommand))
        {
            Debug.Log($"[AIResponseExecutor] Received GameplayCommand (placeholder): {response.GameplayCommand}");
        }

        // If response contains a next chapter hint, log placeholder (field may not exist in current response model)
        var nextChapterField = response.GetType().GetField("NextChapterId");
        if (nextChapterField != null)
        {
            var val = nextChapterField.GetValue(response) as string;
            if (!string.IsNullOrWhiteSpace(val))
                Debug.Log($"[AIResponseExecutor] Received NextChapterId (placeholder): {val}");
        }
    }

    private string[] GetRequestedAnimationTags(AIDirectorResponse response)
    {
        if (response == null) return new string[0];

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
