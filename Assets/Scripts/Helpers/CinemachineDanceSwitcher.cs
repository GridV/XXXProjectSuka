using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public sealed class CinemachineDanceSwitcher : MonoBehaviour
{
    [Header("Dance Cameras")]
    [SerializeField] private List<CinemachineVirtualCameraBase> danceCameras;

    [Header("Default Camera")]
    [SerializeField] private CinemachineVirtualCameraBase defaultCamera;

    [Header("Timing")]
    [SerializeField] private float minHold = 1.5f;
    [SerializeField] private float maxHold = 3.0f;

    [Header("Priority")]
    [SerializeField] private int dancePriority = 20;
    [SerializeField] private int defaultPriority = 10;
    [SerializeField] private int inactivePriority = 0;

    private Coroutine routine;
    private int lastIndex = -1;

    public void StartDance()
    {
        if (danceCameras == null || danceCameras.Count == 0)
        {
            Debug.LogWarning("[CinemachineDanceSwitcher] No dance cameras assigned.");
            return;
        }

        StopRoutineOnly();

        // Ensure default does not win during dance
        SetPriority(defaultCamera, inactivePriority);

        routine = StartCoroutine(SwitchRoutine());
    }

    public void StopDance()
    {
        StopRoutineOnly();

        foreach (var c in danceCameras)
            SetPriority(c, inactivePriority);

        SetPriority(defaultCamera, defaultPriority);
    }

    private void StopRoutineOnly()
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = null;
    }

    private IEnumerator SwitchRoutine()
    {
        while (true)
        {
            int idx = PickRandomNoRepeat(danceCameras.Count, lastIndex);
            lastIndex = idx;

            foreach (var c in danceCameras)
                SetPriority(c, inactivePriority);

            SetPriority(danceCameras[idx], dancePriority);

            float hold = Mathf.Max(0.01f, Random.Range(minHold, maxHold));
            yield return new WaitForSeconds(hold);
        }
    }

    private static void SetPriority(CinemachineVirtualCameraBase cam, int value)
    {
        if (cam == null) return;

        var p = cam.Priority;
        p.Enabled = true;
        p.Value = value;
        cam.Priority = p;
    }

    private static int PickRandomNoRepeat(int count, int last)
    {
        if (count <= 1) return 0;

        int idx;
        do { idx = Random.Range(0, count); }
        while (idx == last);

        return idx;
    }
    public void SwitchToByName(string cameraName)
    {
        StopRoutineOnly();

        bool found = false;

        foreach (var cam in danceCameras)
        {
            if (cam == null) continue;

            bool isTarget = cam.name == cameraName;

            SetPriority(cam, isTarget ? dancePriority : inactivePriority);

            if (isTarget)
                found = true;
        }

        if (!found)
        {
            Debug.LogWarning($"[CinemachineDanceSwitcher] Camera not found: {cameraName}");
            return;
        }

        SetPriority(defaultCamera, inactivePriority);

        Debug.Log($"[CinemachineDanceSwitcher] Switched to: {cameraName}");
    }
}
