using UnityEngine;

[System.Serializable]
public class AnimationClipInfo
{
    [Tooltip("Name in Animator")]
    public string stateName;
    [Tooltip("Extra pause")]
    public float delayAfter = 0f;
}

[CreateAssetMenu(menuName = "Game/Task", fileName = "Task_")]
public class TaskData : ScriptableObject
{
    [Header("Identification")]
    public string id;
    public string title;
    [TextArea] public string description;

    [Header("Engagement requirements")]
    [Range(0, 100)] public int engagementMin = 0;
    [Range(0, 100)] public int engagementMax = 100;

    [Header("Chance per game phase (0..1)")]
    [Range(0f, 1f)] public float weightEarly = 1f;
    [Range(0f, 1f)] public float weightMid = 1f;
    [Range(0f, 1f)] public float weightLate = 1f;
    [Range(0f, 1f)] public float weightFinal = 1f;

    [Header("Execution type")]
    public TaskType taskType = TaskType.Default;

    [Header("Engagement impact")]
    [Tooltip("Сколько очков вовлечения прибавить при завершении этого задания.")]
    public int engagementReward = 0;


    [Header("If Default (in this scene)")]
    [Tooltip("Animation order")]
    public AnimationClipInfo[] animationSequence;

    [Header("If SpecialScene")]
    [Tooltip("Task scene name")]
    public string sceneName;

    public float GetWeight(TaskPhase phase)
    {
        return phase switch
        {
            TaskPhase.Early => weightEarly,
            TaskPhase.Mid => weightMid,
            TaskPhase.Late => weightLate,
            TaskPhase.Final => weightFinal,
            _ => 1f
        };
    }

    public bool IsAvailable(int engagement)
    {
        return engagement >= engagementMin && engagement <= engagementMax;
    }
}
