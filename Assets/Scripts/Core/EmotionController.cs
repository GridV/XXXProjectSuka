using UnityEngine;

public class EmotionController : MonoBehaviour
{
    public Animator animator;

    // Имя последней эмоции, чтобы её можно было сбросить
    private string lastEmotion = null;

    public void PlayEmotion(string emotionName)
    {
        if (animator == null)
        {
            Debug.LogWarning("EmotionController: Animator not set");
            return;
        }

        // Сбрасываем предыдущий триггер
        if (!string.IsNullOrEmpty(lastEmotion))
        {
            animator.ResetTrigger(lastEmotion);
        }

        animator.SetTrigger(emotionName);
        lastEmotion = emotionName;
    }
}
