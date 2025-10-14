using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RhythmVisualizerUI : MonoBehaviour
{
    [Header("References")]
    public Image ringFill;            // Круг с Fill Method = Radial360
    public Image ringGlow;            // Светящийся ободок (опционально)
    public GameObject[] stepDots;     // Маленькие точки (индикаторы шагов)

    [Header("Visual Pulse Settings")]
    [Range(1f, 1.5f)] public float pulseScale = 1.1f;
    [Range(0.1f, 0.5f)] public float pulseDuration = 0.25f;
    [Range(1f, 2f)] public float brightnessBoost = 1.3f;

    private Vector3 _baseScale;
    private Color _baseColor;
    private Coroutine _pulseCo;

    void Awake()
    {
        _baseScale = transform.localScale;
        if (ringFill != null)
            _baseColor = ringFill.color;
    }

    // Устанавливает заполнение кольца (0..1)
    public void SetProgress(float t)
    {
        if (ringFill != null)
            ringFill.fillAmount = Mathf.Clamp01(t);
    }

    // Подсветка активной точки
    public void ShowStep(int index)
    {
        if (stepDots == null) return;
        for (int i = 0; i < stepDots.Length; i++)
            if (stepDots[i] != null)
                stepDots[i].SetActive(i == index);
    }

    // Визуальный отклик при ударе (пульс)
    public void OnBeat()
    {
        if (_pulseCo != null)
            StopCoroutine(_pulseCo);
        _pulseCo = StartCoroutine(Pulse());
    }

    private IEnumerator Pulse()
    {
        float t = 0f;
        Vector3 targetScale = _baseScale * pulseScale;

        while (t < pulseDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin((t / pulseDuration) * Mathf.PI);

            //transform.localScale = Vector3.Lerp(_baseScale, targetScale, k);

            if (ringFill != null)
            {
                Color c = _baseColor * Mathf.Lerp(1f, brightnessBoost, k);
                c.a = _baseColor.a;
                ringFill.color = c;
            }

            yield return null;
        }

        //transform.localScale = _baseScale;
        if (ringFill != null)
            ringFill.color = _baseColor;
    }
}
