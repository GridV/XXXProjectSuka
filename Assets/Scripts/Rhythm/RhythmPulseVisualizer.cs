using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RhythmPulseVisualizer : MonoBehaviour
{
    [Header("Visual Elements")]
    public Image ringFill;
    public float pulseDuration = 0.4f;  // total time of the pulse
    public float pulseScale = 1.2f;     // how much the ring grows
    public float brightnessBoost = 1.8f; // how bright the color becomes
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // smooth curve

    [Header("Control")]
    public bool enablePulse = false;

    private Color originalColor;
    private Vector3 originalScale;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        if (ringFill != null)
            originalColor = ringFill.color;

        originalScale = transform.localScale;
    }

    public void OnBeat()
    {
        Debug.Log("[Visualizer] OnBeat() called");
        return;
    }

    private IEnumerator Pulse()
    {
        float timer = 0f;

        while (timer < pulseDuration)
        {
            timer += Time.deltaTime;
            float t = timer / pulseDuration;
            float curve = pulseCurve.Evaluate(t);

            // Scale pulsing
            transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, curve);

            // Brightness pulsing
            if (ringFill != null)
            {
                Color boosted = originalColor * Mathf.Lerp(1f, brightnessBoost, curve);
                boosted.a = 1f;
                ringFill.color = boosted;
            }

            yield return null;
        }

        // Return to original state
        transform.localScale = originalScale;
        if (ringFill != null)
            ringFill.color = originalColor;
    }
}
