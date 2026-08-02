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

    [SerializeField]
    private AISessionDirector sessionDirector;

    private void Awake()
    {
        if (textOptionPresenter == null)
            textOptionPresenter = FindFirstObjectByType<AITextOptionPresenter>();

        if (sessionDirector == null)
            sessionDirector = FindFirstObjectByType<AISessionDirector>();
    }

    public void BindSceneRuntime(SceneRuntimeBinder binder)
    {
        if (binder == null)
        {
            Debug.LogWarning("[AIResponseExecutor] BindSceneRuntime received a null binder.");
            return;
        }

        if (binder.AIAnimationExecutor == null)
        {
            Debug.LogWarning("[AIResponseExecutor] Animation executor missing inside binder; scene runtime not bound.");
            return;
        }

        animationExecutor = binder.AIAnimationExecutor;
        Debug.Log($"[AIResponseExecutor] Scene runtime bound: {binder.name} -> {animationExecutor.name}");
    }

    public void UnbindSceneRuntime(SceneRuntimeBinder binder)
    {
        if (binder == null)
        {
            Debug.LogWarning("[AIResponseExecutor] UnbindSceneRuntime received a null binder.");
            return;
        }

        if (animationExecutor == null)
        {
            Debug.Log("[AIResponseExecutor] Scene runtime unbound: no active animation executor.");
            return;
        }

        if (binder.AIAnimationExecutor != null && animationExecutor == binder.AIAnimationExecutor)
        {
            animationExecutor = null;
            Debug.Log($"[AIResponseExecutor] Scene runtime unbound: {binder.name}");
            return;
        }

        Debug.Log($"[AIResponseExecutor] Scene runtime unbound: {binder.name} (no active binding)");
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
                    Debug.Log($"[AIResponseExecutor] AI selected animation id='{selected.Id}' for tags [{string.Join(", ", tags)}].");
                    try
                    {
                        var animationStarted = animationExecutor.TryPlay(selected);
                        Debug.Log($"[AIResponseExecutor] Animation started: {animationStarted} (id='{selected.Id}').");
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

        // 3) Handle gameplay commands
        if (!string.IsNullOrWhiteSpace(response.GameplayCommand))
        {
            var command = response.GameplayCommand.Trim();
            if (string.Equals(command, "EndSession", StringComparison.Ordinal))
            {
                if (sessionDirector != null)
                {
                    Debug.Log($"[AIResponseExecutor] Forwarding GameplayCommand to AISessionDirector: {command}");
                    sessionDirector.EndSession();
                }
                else
                {
                    Debug.LogError("[AIResponseExecutor] AISessionDirector is not assigned or found; cannot finalize session for GameplayCommand EndSession.");
                }
            }
            else if (command.StartsWith("StartTask", StringComparison.OrdinalIgnoreCase))
            {
                var taskId = ExtractTaskId(command);
                if (!string.IsNullOrWhiteSpace(taskId))
                {         
                        Debug.LogError("[AIResponseExecutor] TaskRunner is not assigned or found; cannot start task for gameplay command.");
                    
                }
                else
                {
                    Debug.LogError($"[AIResponseExecutor] GameplayCommand '{command}' did not include a task id.");
                }
            }
            else
            {
                Debug.Log($"[AIResponseExecutor] Received GameplayCommand: {command}");
            }
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
        if (response == null)
            return Array.Empty<string>();

        // Explicit directive is authoritative.
        if (!string.IsNullOrWhiteSpace(response.AnimationDirective))
        {
            return new[]
            {
            response.AnimationDirective.Trim().ToLowerInvariant()
        };
        }

        // BodyIntent is only a fallback.
        if (!string.IsNullOrWhiteSpace(response.BodyIntent))
        {
            return new[]
            {
            response.BodyIntent.Trim().ToLowerInvariant()
        };
        }

        return Array.Empty<string>();
    }

    private string ExtractTaskId(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return string.Empty;

        var startIndex = command.IndexOf('(');
        var endIndex = command.LastIndexOf(')');
        if (startIndex >= 0 && endIndex > startIndex + 1)
            return command.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();

        if (command.StartsWith("StartTask:", StringComparison.OrdinalIgnoreCase))
            return command.Substring("StartTask:".Length).Trim();

        return string.Empty;
    }
}
