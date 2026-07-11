using UnityEngine;

public class DebugTaskStarter : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;

    public void StartTaskSequence()
    {
        if (characterAnimator == null)
            return;

        characterAnimator.SetTrigger("ArmsStartSequence");
        characterAnimator.SetTrigger("HeadComeClose");
    }
}
