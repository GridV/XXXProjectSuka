using UnityEngine;
using System.Collections;

public class RhythmController : MonoBehaviour
{
    [Header("Data")]
    public RhythmPattern currentPattern;   // ScriptableObject с pattern[] и rhythmSpeed

    [Header("View")]
    public RhythmVisualizerUI visualizer;    // UI-компонент из Canvas

    [Header("Playback")]
    public bool autoStart = true;

    Coroutine _loop;
    int _step;            // индекс шага в pattern[]
    float _delay;         // длительность одного «тика» (сек)
    float _t;             // накопленный прогресс между ударами

    void Start()
    {
        if (autoStart && currentPattern != null)
            StartPattern(currentPattern);
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
        if (currentPattern.pattern == null || currentPattern.pattern.Length == 0)
            yield break;

        while (true)
        {
            // прогресс между ударами
            _t += Time.deltaTime;
            float norm = Mathf.Clamp01(_t / _delay);
            if (visualizer != null) visualizer.SetProgress(norm);

            // достигли конца «тика» — двигаем шаг
            if (_t >= _delay)
            {
                _t -= _delay;

                int beat = currentPattern.pattern[_step]; // 1 = удар, 0 = пауза
                if (beat == 1 && visualizer != null)
                    visualizer.OnBeat();

                // следующий шаг
                _step = (_step + 1) % currentPattern.pattern.Length;
                if (visualizer != null)
                    visualizer.ShowStep(_step);
            }

            yield return null;
        }
    }
}
