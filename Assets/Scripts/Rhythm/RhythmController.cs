using System;
using System.Collections;
using UnityEngine;

public class RhythmController : MonoBehaviour
{

    [Header("Dynamic Rhythm Speed")]
    [SerializeField] private float slowMultiplier = 0.75f;
    [SerializeField] private float normalMultiplier = 1f;
    [SerializeField] private float fastMultiplier = 1.3f;

    [SerializeField] private float minChangeInterval = 10f;
    [SerializeField] private float maxChangeInterval = 15f;

    private Coroutine rhythmSpeedRoutine;
    private RhythmSpeed currentSpeed = RhythmSpeed.Normal;
    private float baseRhythmSpeed;
    protected bool isPaused;

    [Header("Data")]
    public RhythmPattern currentPattern;   // ScriptableObject с pattern[] и rhythmSpeed

    [Header("View")]
    public RhythmVisualizerUI visualizer;    // UI-компонент из Canvas

    [Header("Playback")]
    public bool autoStart = true;

    private int _patternIndex;      // index inside pattern array
    private int _totalStepCount;    // total rhythm steps elapsed

    public event Action<int> OnRhythmStep;
    public event Action OnBeat;
    Coroutine _loop;
    int _step;            // индекс шага в pattern[]
    float _delay;         // длительность одного «тика» (сек)
    float _t;             // накопленный прогресс между ударами
    void Start()
    {
        if (autoStart && currentPattern != null)
            StartPattern(currentPattern);
    }
    public void StartRhythm(RhythmPattern rhythmPattern)
    {
        Debug.Log("[RhythmController] Starting rhythm gameplay...");
        StartPattern(rhythmPattern);
    }

    public void StartPattern(RhythmPattern pattern)
    {
        if (pattern == null) return;
        currentPattern = pattern;

        if (_loop != null) StopCoroutine(_loop);
        _step = 0;
        _delay = 1f / Mathf.Max(0.01f, currentPattern.rhythmSpeed);
        _t = 0f;

        if (visualizer != null)
        {
            visualizer.SetProgress(0f);
            visualizer.ShowStep(_step);
        }

        _loop = StartCoroutine(Loop());
    }

    public void StopPattern()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    IEnumerator Loop()
    {
        if (currentPattern == null || currentPattern.pattern == null || currentPattern.pattern.Length == 0)
            yield break;

        _patternIndex = 0;
        _totalStepCount = 0;
        _t = 0f;

        while (true)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }
            _t += Time.deltaTime;
            float norm = Mathf.Clamp01(_t / _delay);
            if (visualizer != null)
                visualizer.SetProgress(norm);

            if (_t >= _delay)
            {
                _t -= _delay;

                int beat = currentPattern.pattern[_patternIndex];
                if (beat == 1 && visualizer != null)
                {
                    visualizer.OnBeat();
                    OnBeat?.Invoke();
                }
                    
                // advance pattern index (looping)
                _patternIndex = (_patternIndex + 1) % currentPattern.pattern.Length;

                if (visualizer != null)
                    visualizer.ShowStep(_patternIndex);

                // advance global step counter (NO modulo)
                _totalStepCount++;
                OnRhythmStep?.Invoke(_totalStepCount);
            }

            yield return null;
        }
    }

# region randomly change rhythm speed
    public void StartRandomRhythmSpeedVariation()
    {
        if (rhythmSpeedRoutine != null)
            StopCoroutine(rhythmSpeedRoutine);

        if (currentPattern == null)
            return;

        baseRhythmSpeed = currentPattern.rhythmSpeed;
        rhythmSpeedRoutine = StartCoroutine(RandomRhythmSpeedRoutine());
    }
    private IEnumerator RandomRhythmSpeedRoutine()
    {
        while (true)
        {
            float target = UnityEngine.Random.Range(minChangeInterval, maxChangeInterval);
            float elapsed = 0f;

            while (elapsed < target)
            {
                if (!isPaused)
                    elapsed += Time.deltaTime;

                yield return null;
            }

            if (!isPaused)
            {
                RhythmSpeed newSpeed = (RhythmSpeed)UnityEngine.Random.Range(0, 3);
                ApplyRhythmSpeed(newSpeed);
            }
        }
    }
    private void ApplyRhythmSpeed(RhythmSpeed speed)
    {
        currentSpeed = speed;

        float multiplier = speed switch
        {
            RhythmSpeed.Slow => slowMultiplier,
            RhythmSpeed.Fast => fastMultiplier,
            _ => normalMultiplier
        };

        currentPattern.rhythmSpeed = baseRhythmSpeed * multiplier;

        Debug.Log($"[RhythmController] Rhythm speed changed to {speed}");
    }
    public void StopRandomRhythmSpeedVariation()
    {
        if (rhythmSpeedRoutine != null)
        {
            StopCoroutine(rhythmSpeedRoutine);
            rhythmSpeedRoutine = null;
        }

        if (currentPattern != null)
            currentPattern.rhythmSpeed = baseRhythmSpeed;

        currentSpeed = RhythmSpeed.Normal;
    }
    #endregion
    public void PausePattern()
    {
        isPaused = true;

        if (visualizer != null)
            visualizer.SetProgress(0f);
    }

    public void ResumePattern()
    {
        isPaused = false;
    }

}
public enum RhythmSpeed
{
    Slow,
    Normal,
    Fast
}

