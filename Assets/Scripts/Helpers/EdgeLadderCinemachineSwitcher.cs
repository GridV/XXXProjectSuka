using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public sealed class EdgeLadderCinemachineSwitcher : MonoBehaviour
{
    [Serializable]
    public struct AnimationCameraBinding
    {
        public string animationName;              // e.g. "HandJob", "PalmTop"
        public EdgeLadderCameraTarget target;     // Default / Face / Breast / Hips
    }

    [Header("Cameras")]
    [SerializeField] private CinemachineVirtualCameraBase defaultCamera;
    [SerializeField] private CinemachineVirtualCameraBase faceCamera;
    [SerializeField] private CinemachineVirtualCameraBase breastCamera;
    [SerializeField] private CinemachineVirtualCameraBase hipsCamera;

    [Header("Bindings")]
    [SerializeField] private List<AnimationCameraBinding> bindings = new();

    [Header("Priority")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int defaultPriority = 10;
    [SerializeField] private int inactivePriority = 0;

    private readonly Dictionary<string, EdgeLadderCameraTarget> map =
        new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        RebuildMap();
        ResetToDefault();
    }

    private void OnValidate()
    {
        RebuildMap();
    }

    public void SwitchForAnimation(string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName))
        {
            ResetToDefault();
            return;
        }

        if (map.TryGetValue(animationName.Trim(), out var target))
            SwitchTo(target);
        else
            SwitchTo(EdgeLadderCameraTarget.Default);
    }

    public void SwitchTo(EdgeLadderCameraTarget target)
    {
        SetAllInactive();

        var cam = GetCamera(target);
        if (cam == null)
        {
            Debug.LogWarning($"[EdgeLadderCinemachineSwitcher] Camera not assigned for target={target}");
            return;
        }

        int prio = (target == EdgeLadderCameraTarget.Default) ? defaultPriority : activePriority;
        SetPriority(cam, prio);

        Debug.Log($"[EdgeLadderCinemachineSwitcher] Switched to {target}");
    }

    public void ResetToDefault()
    {
        SwitchTo(EdgeLadderCameraTarget.Default);
    }

    private void RebuildMap()
    {
        map.Clear();
        if (bindings == null) return;

        foreach (var b in bindings)
        {
            if (string.IsNullOrWhiteSpace(b.animationName))
                continue;

            map[b.animationName.Trim()] = b.target;
        }
    }

    private CinemachineVirtualCameraBase GetCamera(EdgeLadderCameraTarget target)
    {
        return target switch
        {
            EdgeLadderCameraTarget.Face => faceCamera,
            EdgeLadderCameraTarget.Breast => breastCamera,
            EdgeLadderCameraTarget.Hips => hipsCamera,
            _ => defaultCamera
        };
    }

    private void SetAllInactive()
    {
        SetPriority(defaultCamera, inactivePriority);
        SetPriority(faceCamera, inactivePriority);
        SetPriority(breastCamera, inactivePriority);
        SetPriority(hipsCamera, inactivePriority);
    }

    private static void SetPriority(CinemachineVirtualCameraBase cam, int value)
    {
        if (cam == null) return;

        var p = cam.Priority;
        p.Enabled = true;
        p.Value = value;
        cam.Priority = p;
    }
}
