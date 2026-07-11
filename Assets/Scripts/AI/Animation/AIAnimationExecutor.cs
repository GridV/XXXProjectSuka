using UnityEngine;

// MVP executor that sends selected AIAnimationDefinition to CharacterAnimationService.
public class AIAnimationExecutor : MonoBehaviour
{
    [SerializeField]
    private CharacterAnimationService characterAnimation;

    [SerializeField]
    private float defaultFadeDuration = 0.15f;

    public bool TryPlay(AIAnimationDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogWarning("AIAnimationExecutor: definition is null.");
            return false;
        }

        if (characterAnimation == null)
        {
            Debug.LogWarning("AIAnimationExecutor: CharacterAnimationService reference is missing.");
            return false;
        }

        // Apply float parameters first
        if (definition.FloatParameters != null)
        {
            foreach (var p in definition.FloatParameters)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.Name))
                {
                    Debug.LogWarning($"AIAnimationExecutor: Ignoring invalid float parameter on animation '{definition.Id}'.");
                    continue;
                }

                bool ok = characterAnimation.TrySetFloat(p.Name, p.Value);
                Debug.Log($"AIAnimationExecutor: Set float '{p.Name}'={p.Value} -> {ok}.");
            }
        }

        bool requested = false;

        // Trigger takes precedence if present
        if (!string.IsNullOrWhiteSpace(definition.TriggerName))
        {
            Debug.Log($"AIAnimationExecutor: Trigger requested='{definition.TriggerName}'.");
            requested = characterAnimation.TrySetTrigger(definition.TriggerName) || requested;
        }
        else if (!string.IsNullOrWhiteSpace(definition.StateName))
        {
            characterAnimation.CrossFadeBase(definition.StateName, defaultFadeDuration);
            requested = true;
        }

        if (!requested)
        {
            Debug.LogWarning($"AIAnimationExecutor: No trigger or state to play for animation '{definition.Id}'.");
            return false;
        }

        // Log summary
        Debug.Log($"AIAnimationExecutor: Requested animation id='{definition.Id}', trigger='{definition.TriggerName}', state='{definition.StateName}'.");
        if (definition.FloatParameters != null && definition.FloatParameters.Length > 0)
        {
            foreach (var p in definition.FloatParameters)
                Debug.Log($"AIAnimationExecutor: FloatParam {p.Name}={p.Value}");
        }

        return true;
    }
}

