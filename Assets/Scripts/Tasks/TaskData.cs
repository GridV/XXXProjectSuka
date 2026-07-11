using UnityEngine;

[System.Serializable]
public class AnimationClipInfo
{
    [Tooltip("Name of the state in Animator to play")]
    public string stateName;

    [Tooltip("Pause after this clip (seconds)")]
    public float delayAfter = 0f;
}
[System.Serializable]




[CreateAssetMenu(menuName = "Game/Task", fileName = "Task_")]
public class TaskData : ScriptableObject
{
    [Header("In-Task Contexts")]
    public InTaskContext[] inTaskContexts;

    [Tooltip("Context id to show when task execution starts")]
    public string startInTaskContextId;

    [Header("Task Limits")]
    public TaskLimitType limitType = TaskLimitType.None;

    [Tooltip("Time limit in seconds (used if limitType == Time)")]
    public float timeLimitSeconds = 0f;

    [Tooltip("Max rhythm steps (used if limitType == RhythmCount)")]
    public int rhythmStepLimit = 0;


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

    [Header("Animation Sequence (if Default scene)")]
    public AnimationClipInfo[] animationSequence;

    [Header("Special Scene (if taskType == SpecialScene)")]
    public string sceneName;

    [Header("Dialogue")]
    [Tooltip("Имя JSON файла диалога из StreamingAssets/Dialogues")]
    public string dialogueFileName;

    [Header("Rhythm (optional)")]
    [Tooltip("Паттерн ритма, который будет использоваться в этом задании")]
    public RhythmPattern rhythmPattern;

    [Header("Emotions")]
    [Tooltip("Emotion at task start (Animator Trigger)")]
    public string startEmotion;

    [Tooltip("Emotion at task end (Animator Trigger)")]
    public string endEmotion;

    #region Repeatable Task Settings

    [Header("Repeat Count Task Defaults")]
    public int baseRepeatCount;
    public float baseMinTime;
    public float baseMaxTime;

    [Header("Repeat Count Random Range")]
    public int randomMinRepeats = 10;
    public int randomMaxRepeats = 45;

    [Header("Repeat Count Extra Max Time Padding")]
    public float extraMaxTimePadding = 10f;

    #endregion
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

    public bool IsAvailable(float engagement)
    {
        return engagement >= engagementMin && engagement <= engagementMax;
    }
}
