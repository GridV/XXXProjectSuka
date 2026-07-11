using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public sealed class CharacterRigService : MonoBehaviour
{
    [Header("Rig Builder")]
    [SerializeField] private RigBuilder rigBuilder;

    [Header("Rigs")]
    [SerializeField] private Rig lookRig;
    [SerializeField] private Rig armsRig;

    [Header("Targets")]
    [SerializeField] private Transform lookTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftElbowHint;
    [SerializeField] private Transform rightElbowHint;

    private Coroutine lookFade;
    private Coroutine armsFade;

    public Transform LookTarget => lookTarget;
    public Transform LeftHandTarget => leftHandTarget;
    public Transform RightHandTarget => rightHandTarget;
    public Transform LeftElbowHint => leftElbowHint;
    public Transform RightElbowHint => rightElbowHint;

    private void Awake()
    {
        if (rigBuilder == null)
            rigBuilder = GetComponent<RigBuilder>();

        if (rigBuilder == null)
            Debug.LogError("[CharacterRigService] RigBuilder is missing on Camila.");

        SetLookWeight(0f, 0f);
        SetArmsWeight(0f, 0f);
    }

    public void SafeRebuild()
    {
        if (rigBuilder == null)
        {
            Debug.LogWarning("[CharacterRigService] SafeRebuild skipped: RigBuilder is null.");
            return;
        }

        rigBuilder.Build();
        Debug.Log("[CharacterRigService] RigBuilder.Build() executed.");
    }

    public void SetLookTarget(Transform target)
    {
        lookTarget = target;
    }

    public void SetLookWeight(float weight, float fadeSeconds = 0.15f)
    {
        if (lookRig == null)
        {
            Debug.LogWarning("[CharacterRigService] LookRig is not assigned.");
            return;
        }

        weight = Mathf.Clamp01(weight);

        if (fadeSeconds <= 0f)
        {
            lookRig.weight = weight;
            return;
        }

        if (lookFade != null)
            StopCoroutine(lookFade);

        lookFade = StartCoroutine(FadeRigWeight(lookRig, weight, fadeSeconds));
    }

    public void SetArmsWeight(float weight, float fadeSeconds = 0.15f)
    {
        if (armsRig == null)
        {
            Debug.LogWarning("[CharacterRigService] ArmsRig is not assigned.");
            return;
        }

        weight = Mathf.Clamp01(weight);

        if (fadeSeconds <= 0f)
        {
            armsRig.weight = weight;
            return;
        }

        if (armsFade != null)
            StopCoroutine(armsFade);

        armsFade = StartCoroutine(FadeRigWeight(armsRig, weight, fadeSeconds));
    }

    private static IEnumerator FadeRigWeight(Rig rig, float target, float seconds)
    {
        float start = rig.weight;
        float t = 0f;

        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            rig.weight = Mathf.Lerp(start, target, k);
            yield return null;
        }

        rig.weight = target;
    }
}
