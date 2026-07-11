using UnityEngine;

public class AdvancedBlink : MonoBehaviour
{
    public Animator animator;

    float timer = 0f;
    float nextBlinkTime;

    void Start()
    {
        ScheduleNextBlink();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextBlinkTime)
        {
            DoBlink();
            ScheduleNextBlink();
        }
    }

    void ScheduleNextBlink()
    {
        nextBlinkTime = Random.Range(3f, 7f); // человек моргает каждые 3–7 секунд
        timer = 0f;
    }

    void DoBlink()
    {
        int r = Random.Range(0, 100);

        if (r < 10)
        {
            // 10% шанс на двойное моргание
            animator.SetTrigger("BlinkNormal");
            Invoke(nameof(SecondBlink), 0.15f);
            return;
        }

        if (r < 60)
        {
            animator.SetTrigger("BlinkNormal");
            return;
        }

        if (r < 80)
        {
            animator.SetTrigger("BlinkCinematic");
            return;
        }

        animator.SetTrigger("BlinkTomne");
    }

    void SecondBlink()
    {
        animator.SetTrigger("BlinkNormal");
    }
}
