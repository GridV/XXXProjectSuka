using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/AI Tag Database", fileName = "AITagDatabase")]
public class AITagDatabase : ScriptableObject
{
    [Header("Conversation Intents")]
    [SerializeField]
    private string[] conversationIntents =
    {
        "Greeting",
        "Goodbye",
        "Explain",
        "Question",
        "Answer",
        "Encourage",
        "Praise",
        "Tease",
        "Comfort",
        "Warn",
        "Command",
        "Apologize",
        "Think",
        "Listen",
        "Silence"
    };

    [Header("Emotion Tags")]
    [SerializeField]
    private string[] emotionTags =
    {
        "Neutral",
        "Happy",
        "Playful",
        "Confident",
        "Dominant",
        "Shy",
        "Embarrassed",
        "Curious",
        "Excited",
        "Disappointed",
        "Angry",
        "Calm",
        "Relaxed",
        "Focused"
    };

    [Header("Expression Tags")]
    [SerializeField]
    private string[] expressionTags =
    {
        "Smile",
        "Laugh",
        "Giggle",
        "Blush",
        "Sigh",
        "LookAway",
        "EyeContact",
        "Blink",
        "Nod",
        "ShakeHead"
    };

    [Header("Body Intents")]
    [SerializeField]
    private string[] bodyIntents =
    {
        "Idle",
        "Talk",
        "LeanForward",
        "LeanBack",
        "Approach",
        "StepBack",
        "Sit",
        "Stand",
        "Wait",
        "Observe"
    };

    [Header("Gameplay Commands")]
    [SerializeField]
    private string[] gameplayCommands =
    {
        "ContinueDialogue",
        "StartTask",
        "PauseTask",
        "ResumeTask",
        "FinishTask",
        "RetryTask",
        "NextTask",
        "StartRhythm",
        "IncreaseSpeed",
        "DecreaseSpeed",
        "Hold",
        "Release",
        "MoveToNextChapter",
        "EndSession"
    };

    [Header("Camera Intents")]
    [SerializeField]
    private string[] cameraIntents =
    {
        "FocusFace",
        "FocusBody",
        "ZoomIn",
        "ZoomOut",
        "RandomDance"
    };

    public IReadOnlyList<string> ConversationIntents => conversationIntents;
    public IReadOnlyList<string> EmotionTags => emotionTags;
    public IReadOnlyList<string> ExpressionTags => expressionTags;
    public IReadOnlyList<string> BodyIntents => bodyIntents;
    public IReadOnlyList<string> GameplayCommands => gameplayCommands;
    public IReadOnlyList<string> CameraIntents => cameraIntents;

    public bool IsValidConversationIntent(string tag) => Contains(conversationIntents, tag);
    public bool IsValidEmotionTag(string tag) => Contains(emotionTags, tag);
    public bool IsValidExpressionTag(string tag) => Contains(expressionTags, tag);
    public bool IsValidBodyIntent(string tag) => Contains(bodyIntents, tag);
    public bool IsValidGameplayCommand(string tag) => Contains(gameplayCommands, tag);
    public bool IsValidCameraIntent(string tag) => Contains(cameraIntents, tag);

    private bool Contains(string[] source, string value)
    {
        if (source == null || string.IsNullOrWhiteSpace(value))
            return false;

        for (int i = 0; i < source.Length; i++)
        {
            if (string.Equals(source[i], value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}