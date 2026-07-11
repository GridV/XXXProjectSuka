using UnityEngine;

public sealed class GameDecisionService
{
    private readonly IRng _rng;

    // Tuning (can be moved to ScriptableObject later)
    private readonly float _minChance = 0.02f;
    private readonly float _maxChance = 0.60f;
    private readonly float _engagementPower = 2.0f; // makes high engagement matter more
    private readonly float _minElapsedSeconds = 8f; // anti-skip protection

    public GameDecisionService(IRng rng)
    {
        _rng = rng;
    }

    /// <summary>
    /// Returns true if the game should allow finishing (or start final branch),
    /// based on engagement and session metrics. No side-effects.
    /// </summary>
    public bool TryFinishDecision(float engagement0to100, float elapsedSeconds)
    {
        float e01 = Mathf.Clamp01(engagement0to100 / 100f);

        float timeFactor = elapsedSeconds <= _minElapsedSeconds
            ? Mathf.Clamp01(elapsedSeconds / _minElapsedSeconds)
            : 1f;

        float shaped = Mathf.Pow(e01, _engagementPower);

        float chance = Mathf.Lerp(_minChance, _maxChance, shaped) * timeFactor;
        chance = Mathf.Clamp01(chance);

        float roll = _rng.Next01();
        return roll < chance;
    }
}
