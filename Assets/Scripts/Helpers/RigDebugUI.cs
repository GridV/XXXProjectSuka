using UnityEngine;

public sealed class RigDebugUI : MonoBehaviour
{
    [SerializeField] private float fadeSeconds = 0.2f;

    private CharacterRigService rig;
    private bool lookOn;
    private bool armsOn;

    private void Update()
    {
        // Lazy bind after Env scene is loaded
        if (rig == null)
            rig = FindObjectOfType<CharacterRigService>();
    }

    public void ToggleLook()
    {
        if (rig == null)
        {
            Debug.LogWarning("[RigDebugUI] CharacterRigService not found.");
            return;
        }

        lookOn = !lookOn;
        rig.SetLookWeight(lookOn ? 1f : 0f, fadeSeconds);
        Debug.Log($"[RigDebugUI] Look {(lookOn ? "ON" : "OFF")}");
    }

    public void ToggleArms()
    {
        if (rig == null)
        {
            Debug.LogWarning("[RigDebugUI] CharacterRigService not found.");
            return;
        }

        armsOn = !armsOn;
        rig.SetArmsWeight(armsOn ? 1f : 0f, fadeSeconds);
        Debug.Log($"[RigDebugUI] Arms {(armsOn ? "ON" : "OFF")}");
    }
}
