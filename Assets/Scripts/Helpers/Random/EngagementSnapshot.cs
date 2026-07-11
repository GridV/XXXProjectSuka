using System;

[Serializable]
public struct EngagementSnapshot
{
    /// <summary>
    /// Normalized engagement score in range [0..1].
    /// </summary>
    public float engagement01;

    /// <summary>
    /// Total seconds spent in the current task/session.
    /// </summary>
    public float elapsedSeconds;

    /// <summary>
    /// How many tries/attempts happened in this task/session.
    /// </summary>
    public int attempts;

    public EngagementSnapshot(float engagement01, float elapsedSeconds, int attempts)
    {
        this.engagement01 = engagement01;
        this.elapsedSeconds = elapsedSeconds;
        this.attempts = attempts;
    }
}
