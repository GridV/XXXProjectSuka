using System;
using System.Collections.Generic;
using System.Linq;

// Simple selector that chooses the best AIAnimationDefinition for requested tags.
public static class AIAnimationSelector
{
    // Selects the best animation based on requested tags.
    // Returns null if no primary tag matches or if requestedTags is null/empty.
    public static AIAnimationDefinition SelectBest(string[] requestedTags)
    {
        if (requestedTags == null || requestedTags.Length == 0)
            return null;

        var db = AIAnimationDictionary.Default;
        if (db == null || db.Definitions == null || db.Definitions.Count == 0)
            return null;

        // Normalize requested tags to lower-case for case-insensitive matching
        var req = requestedTags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLowerInvariant()).ToArray();
        if (req.Length == 0) return null;

        AIAnimationDefinition best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var def in db.Definitions)
        {
            if (def == null) continue;

            // Primary matches are required; count how many primary tags match
            var prim = def.PrimaryTags ?? new string[0];
            int primaryMatches = prim.Count(p => !string.IsNullOrWhiteSpace(p) && req.Contains(p.Trim().ToLowerInvariant()));
            if (primaryMatches == 0)
                continue; // skip definitions without primary tag match

            // Secondary tag matches add to score
            var sec = def.SecondaryTags ?? new string[0];
            int secondaryMatches = sec.Count(s => !string.IsNullOrWhiteSpace(s) && req.Contains(s.Trim().ToLowerInvariant()));

            // Score: primary matches are weighted heavily, then secondary, then definition weight
            float score = primaryMatches * 100f + secondaryMatches * 10f + def.Weight;

            if (score > bestScore)
            {
                bestScore = score;
                best = def;
            }
        }

        return best;
    }
}
