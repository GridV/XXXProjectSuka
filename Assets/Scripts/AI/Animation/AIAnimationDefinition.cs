using System;

[Serializable]
public class AIAnimationDefinition
{
    // Unique identifier for this animation mapping.
    public string Id;

    // Logical animator state name (DO NOT allow AI to set this directly).
    public string StateName;

    // Primary tags that indicate a direct match (e.g., "idle", "come_close").
    public string[] PrimaryTags = new string[0];

    // Secondary tags that are less important but increase score.
    public string[] SecondaryTags = new string[0];

    // Optional trigger name used to fire Animator transitions.
    public string TriggerName;

    // Whether this animation can loop.
    public bool CanLoop = false;

    // Approximate duration in seconds (0 means indefinite or loopable).
    public float DurationSeconds = 0f;

    // Tuning weight added to selection score.
    public float Weight = 1f;

    // Optional float parameters to set on the Animator before triggering or crossfading.
    public AIAnimatorFloatParameter[] FloatParameters = new AIAnimatorFloatParameter[0];
}
