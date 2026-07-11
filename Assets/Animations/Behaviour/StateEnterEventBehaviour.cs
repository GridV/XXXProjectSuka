using UnityEngine;

public sealed class StateEnterEventBehaviour : StateMachineBehaviour
{
    [SerializeField] private string eventKey = "RhythmStart";

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var router = animator.GetComponent<AnimatorEventRouter>();
        if (router != null)
            router.Raise(eventKey);
    }
}
