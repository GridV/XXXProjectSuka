using System;
using UnityEngine;

public sealed class AnimatorEventRouter : MonoBehaviour
{
    public event Action<string> OnEvent;

    public void Raise(string eventKey)
    {
        if (string.IsNullOrEmpty(eventKey)) return;
        OnEvent?.Invoke(eventKey);
    }
    public void Emit(string eventKey)
    {
        Debug.Log($"[AnimatorEventRouter] Emit called: {eventKey}");

        if (string.IsNullOrWhiteSpace(eventKey))
            return;

        if (OnEvent == null)
        {
            Debug.LogWarning("[AnimatorEventRouter] No listeners subscribed.");
            return;
        }

        OnEvent.Invoke(eventKey);
    }
}
