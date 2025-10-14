using UnityEngine;

public enum MovementType
{
    Stroke,      // стандартное движение
    Pause,       // пауза
    Alternate,   // чередующееся
    Circular,    // круговое движение
    Custom       // для особых случаев
}

public enum MovementZone
{
    Upper,
    Middle,
    Lower,
    Full
}

[CreateAssetMenu(fileName = "New Rhythm Pattern", menuName = "Game/Rhythm Pattern")]
public class RhythmPattern : ScriptableObject
{
    [Header("Pattern Info")]
    public string title;
    [TextArea] public string description;

    [Header("Rhythm Settings")]
    [Tooltip("Speed of rhythm, higher = faster (in beats per second)")]
    [Range(0.2f, 5f)] public float rhythmSpeed = 1f;

    [Tooltip("Array defining the pattern (1 = beat, 0 = pause)")]
    public int[] pattern = new int[] { 1, 0, 1, 1, 0 };

    [Header("Movement Meta")]
    public MovementType movementType = MovementType.Stroke;
    public MovementZone movementZone = MovementZone.Middle;

    [Header("Engagement impact")]
    [Tooltip("Engagement increase after completing this pattern")]
    public int engagementGain = 5;
}
