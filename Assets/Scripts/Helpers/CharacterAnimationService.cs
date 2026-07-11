using System.Collections;
using UnityEngine;

public sealed class CharacterAnimationService : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Coroutine layerFadeCoroutine;

    public void CrossFadeBase(string stateName, float fade, float normalizedTime = 0f)
    {
        animator.CrossFade(stateName, fade, 0, normalizedTime);
    }

    public void CrossFadeOnLayer(string layerName, string stateName, float fade, float normalizedTime = 0f)
    {
        int layer = animator.GetLayerIndex(layerName);
        if (layer < 0) return;

        animator.CrossFade(stateName, fade, layer, normalizedTime);
    }

    // Safely set an Animator trigger parameter if available.
    // Returns true on success, false on failure (missing animator, missing parameter, or invalid name).
    public bool TrySetTrigger(string triggerName)
    {
        if (animator == null)
        {
            Debug.LogWarning("CharacterAnimationService: Animator is missing.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(triggerName))
        {
            Debug.LogWarning("CharacterAnimationService: triggerName is empty.");
            return false;
        }

        var pars = animator.parameters;
        bool found = false;
        for (int i = 0; i < pars.Length; i++)
        {
            if (pars[i].name == triggerName && pars[i].type == AnimatorControllerParameterType.Trigger)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"CharacterAnimationService: trigger '{triggerName}' not found on Animator.");
            return false;
        }

        animator.SetTrigger(triggerName);
        Debug.Log($"CharacterAnimationService: Trigger set '{triggerName}'.");
        return true;
    }

    public void SetLayerWeight(string layerName, float weight)
    {
        int layer = animator.GetLayerIndex(layerName);
        if (layer < 0) return;

        animator.SetLayerWeight(layer, weight);
    }

    // Safely set a float parameter on the Animator if available.
    // Returns true on success, false on failure (missing animator, missing parameter, or invalid name/type).
    public bool TrySetFloat(string parameterName, float value)
    {
        if (animator == null)
        {
            Debug.LogWarning("CharacterAnimationService: Animator is missing.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(parameterName))
        {
            Debug.LogWarning("CharacterAnimationService: parameterName is empty.");
            return false;
        }

        var pars = animator.parameters;
        bool found = false;
        for (int i = 0; i < pars.Length; i++)
        {
            if (pars[i].name == parameterName && pars[i].type == AnimatorControllerParameterType.Float)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"CharacterAnimationService: float parameter '{parameterName}' not found on Animator.");
            return false;
        }

        animator.SetFloat(parameterName, value);
        Debug.Log($"CharacterAnimationService: Float parameter '{parameterName}' set to {value}.");
        return true;
    }

    public void FadeLayerWeight(string layerName, float targetWeight, float duration)
    {
        int layer = animator.GetLayerIndex(layerName);
        if (layer < 0) return;

        if (layerFadeCoroutine != null)
            StopCoroutine(layerFadeCoroutine);

        layerFadeCoroutine = StartCoroutine(FadeLayerRoutine(layer, targetWeight, duration));
    }

    private IEnumerator FadeLayerRoutine(int layer, float target, float duration)
    {
        float start = animator.GetLayerWeight(layer);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            animator.SetLayerWeight(layer, Mathf.Lerp(start, target, t / duration));
            yield return null;
        }

        animator.SetLayerWeight(layer, target);
        layerFadeCoroutine = null;
    }
}
