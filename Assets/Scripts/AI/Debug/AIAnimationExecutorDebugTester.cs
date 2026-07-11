using UnityEngine;

// Debug tester for AIAnimationExecutor and AIAnimationSelector.
public class AIAnimationExecutorDebugTester : MonoBehaviour
{
    [SerializeField]
    private AIAnimationExecutor executor;

    [SerializeField]
    private string[] requestedTags;

    private void Start()
    {
        if (requestedTags == null || requestedTags.Length == 0)
        {
            requestedTags = new[] { "slow_controlled", "loop" };
            Debug.Log("AIAnimationExecutorDebugTester: No requested tags provided; using default tags.");
        }

        var selected = AIAnimationSelector.SelectBest(requestedTags);
        if (selected == null)
        {
            Debug.Log("AIAnimationExecutorDebugTester: No animation selected for requested tags.");
            return;
        }

        Debug.Log($"AIAnimationExecutorDebugTester: Selected animation id '{selected.Id}' for tags [{string.Join(", ", requestedTags)}].");

        if (executor == null)
        {
            Debug.LogWarning("AIAnimationExecutorDebugTester: Executor reference is missing.");
            return;
        }

        bool requested = executor.TryPlay(selected);
        Debug.Log($"AIAnimationExecutorDebugTester: Playback was requested: {requested}.");
    }
}
