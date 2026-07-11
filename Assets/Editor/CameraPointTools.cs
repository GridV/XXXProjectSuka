using UnityEditor;
using UnityEngine;

public static class CameraPointTools
{
    [MenuItem("Tools/Camera Points/Move Main Camera To Selection %#m")] // Ctrl+Shift+M
    private static void MoveMainCameraToSelection()
    {
        var cam = Camera.main;
        var t = Selection.activeTransform;

        if (cam == null)
        {
            Debug.LogWarning("[CameraPointTools] No Main Camera found in the scene.");
            return;
        }

        if (t == null)
        {
            Debug.LogWarning("[CameraPointTools] No transform selected.");
            return;
        }

        Undo.RecordObject(cam.transform, "Move Main Camera To Selection");
        cam.transform.position = t.position;
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/Camera Points/Move Main Camera To Selection And Look At Matching CamLook %#l")] // Ctrl+Shift+L
    private static void MoveMainCameraToSelectionAndLookAt()
    {
        var cam = Camera.main;
        var pos = Selection.activeTransform;

        if (cam == null)
        {
            Debug.LogWarning("[CameraPointTools] No Main Camera found in the scene.");
            return;
        }

        if (pos == null)
        {
            Debug.LogWarning("[CameraPointTools] No transform selected.");
            return;
        }

        // Expect: CamPos_X -> CamLook_X
        if (!pos.name.StartsWith("CamPos_"))
        {
            Debug.LogWarning("[CameraPointTools] Select a CamPos_* transform to use this command.");
            return;
        }

        var lookName = pos.name.Replace("CamPos_", "CamLook_");
        var look = FindTransformByName(lookName);

        if (look == null)
        {
            Debug.LogWarning($"[CameraPointTools] Matching look target not found: {lookName}");
            return;
        }

        Undo.RecordObject(cam.transform, "Move Main Camera And Look At");
        cam.transform.position = pos.position;
        cam.transform.LookAt(look.position, Vector3.up);

        SceneView.RepaintAll();
    }

    private static Transform FindTransformByName(string name)
    {
        // Works in edit mode across the loaded scene.
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name)
                return all[i];
        }
        return null;
    }
}
