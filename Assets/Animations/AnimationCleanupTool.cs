using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AnimationCleanupTool: EditorWindow
{
    private float noiseThreshold = 0.0008f; // мягкая фильтрация
    private string[] armBones = new string[]
    {
        "UpperArm",
        "Shoulder",
        "Arm",
        "ForeArm",
        "Hand",
        "Twist"
    };

    [MenuItem("Tools/Animation Soft Arm Cleanup")]
    static void Init()
    {
        AnimationCleanupTool window = (AnimationCleanupTool)GetWindow(typeof(AnimationCleanupTool));
        window.titleContent = new GUIContent("Soft Arm Cleanup");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Soft Cleanup (arms only)", EditorStyles.boldLabel);
        noiseThreshold = EditorGUILayout.FloatField("Noise Threshold:", noiseThreshold);

        if (GUILayout.Button("Clean Selected Clips (Arms Only)"))
        {
            CleanSelected();
        }
    }

    void CleanSelected()
    {
        Object[] clips = Selection.objects;

        foreach (Object obj in clips)
        {
            AnimationClip clip = obj as AnimationClip;
            if (clip == null) continue;

            CleanClip(clip);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Soft cleanup done.");
    }

    void CleanClip(AnimationClip clip)
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var binding in bindings)
        {
            // фильтруем только нужные кости
            bool isArm = false;
            foreach (var part in armBones)
            {
                if (binding.path.Contains(part))
                {
                    isArm = true;
                    break;
                }
            }

            if (!isArm)
                continue;

            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null) continue;

            // мягкое сглаживание
            List<Keyframe> newKeys = new List<Keyframe>();

            for (int i = 0; i < curve.keys.Length; i++)
            {
                var key = curve.keys[i];

                if (Mathf.Abs(key.value) < noiseThreshold)
                {
                    // сглаживаем мусор, не удаляем полностью
                    key.value = 0f;
                }
                newKeys.Add(key);
            }

            AnimationUtility.SetEditorCurve(clip, binding, new AnimationCurve(newKeys.ToArray()));
        }
    }
}
