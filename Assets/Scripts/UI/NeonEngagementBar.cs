using UnityEngine;
using UnityEngine.UI;

public class NeonEngagementBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The fill image of the progress bar")]
    public Image fillLine;       // main fill line (if exists)
    [Tooltip("ON dots (bright). Order left to right.")]
    public GameObject[] dotsOn;

    [Header("Colors")]
    public Color reachedColor = Color.white;         // color when level reached
    public Color unreachedColor = new Color(0.6f, 0.6f, 0.6f, 1f); // default grey

    [Header("Engagement thresholds for each dot (0-100)")]
    public int[] thresholds = { 0, 25, 50, 75, 99}; // default 4 steps: 0%, 25%, 50%, 75%

    /// <summary>
    /// Call this whenever engagement changes.
    /// </summary>
    public void UpdateBar(float engagement)
    {
        Debug.Log($"[NeonEngagementBar] UpdateBar called with {engagement}");

        // --- Progress fill ---
        if (fillLine != null)
        {
            fillLine.type = Image.Type.Filled;
            fillLine.fillMethod = Image.FillMethod.Horizontal;
            fillLine.fillOrigin = 0;
            fillLine.fillAmount = Mathf.Clamp01(engagement / 100f);
        }

        // --- Dots ---
        int count = Mathf.Min(thresholds.Length, dotsOn.Length);
        for (int i = 0; i < count; i++)
        {
            bool reached = engagement >= thresholds[i];
            if (dotsOn[i] != null)
                dotsOn[i].SetActive(reached);
        }
    }
}
