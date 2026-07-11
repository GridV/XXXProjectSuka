using System;
using System.Collections.Generic;

[Serializable]
public class AIAnimationDictionary
{
    public List<AIAnimationDefinition> Definitions = new List<AIAnimationDefinition>();

    private static readonly AIAnimationDictionary defaultDb = new AIAnimationDictionary
    {
        Definitions = new List<AIAnimationDefinition>
        {
            new AIAnimationDefinition
            {
                Id = "idle_basic",
                StateName = "Idle_Basic",
                PrimaryTags = new[] { "idle" },
                SecondaryTags = new[] { "calm" },
                CanLoop = true,
                DurationSeconds = 2f,
                Weight = 1f
            },
            new AIAnimationDefinition
            {
                Id = "arms_come_close",
                StateName = "Arms_ComeClose",
                PrimaryTags = new[] { "come_close" },
                SecondaryTags = new[] { "approach" },
                CanLoop = false,
                DurationSeconds = 1.2f,
                Weight = 1.2f
            },
                // Hand job routine - normal speed
                new AIAnimationDefinition
                {
                    Id = "hand_job_normal",
                    StateName = "",
                    TriggerName = "EL_Start_HandJob",
                    FloatParameters = new[] { new AIAnimatorFloatParameter("EL_Speed", 1.0f) },
                    PrimaryTags = new[] { "hand_job" },
                    SecondaryTags = new[] { "normal", "loop", "routine" },
                    CanLoop = true,
                    DurationSeconds = -1f,
                    Weight = 1.0f
                },

                // Hand job routine - slow
                new AIAnimationDefinition
                {
                    Id = "hand_job_slow",
                    StateName = "",
                    TriggerName = "EL_Start_HandJob",
                    FloatParameters = new[] { new AIAnimatorFloatParameter("EL_Speed", 0.7f) },
                    PrimaryTags = new[] { "hand_job" },
                    SecondaryTags = new[] { "slow", "slow_controlled", "loop", "routine" },
                    CanLoop = true,
                    DurationSeconds = -1f,
                    Weight = 1.0f
                },

                // Hand job routine - fast
                new AIAnimationDefinition
                {
                    Id = "hand_job_fast",
                    StateName = "",
                    TriggerName = "EL_Start_HandJob",
                    FloatParameters = new[] { new AIAnimatorFloatParameter("EL_Speed", 1.3f) },
                    PrimaryTags = new[] { "hand_job" },
                    SecondaryTags = new[] { "fast", "fast_intense", "loop", "routine" },
                    CanLoop = true,
                    DurationSeconds = -1f,
                    Weight = 1.0f
                },

                // End current routine
                new AIAnimationDefinition
                {
                    Id = "end_current_routine",
                    StateName = "",
                    TriggerName = "EL_End",
                    FloatParameters = new AIAnimatorFloatParameter[0],
                    PrimaryTags = new[] { "end_routine" },
                    SecondaryTags = new[] { "end", "stop", "outro" },
                    CanLoop = false,
                    DurationSeconds = 1.0f,
                    Weight = 1.0f
                }
        }
    };

    public static AIAnimationDictionary Default => defaultDb;
}
