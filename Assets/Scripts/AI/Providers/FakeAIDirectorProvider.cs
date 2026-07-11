using System.Collections.Generic;
using UnityEngine;

// MVP fake provider that returns a hardcoded valid AIDirectorResponse.
public class FakeAIDirectorProvider : IAIDirectorProvider
{
    public AIDirectorResponse GetAIDirectorResponse()
    {
        Debug.Log("[FakeAIDirectorProvider] Current chapter: start");

        return new AIDirectorResponse
        {
            TextLine = "Session started.",
            EmotionTag = "neutral",
            ConversationIntent = "Greeting",
            ExpressionTag = "Smile",
            BodyIntent = "Idle",
            GameplayCommand = "MoveToNextChapter",
            CameraIntent = "FocusFace",
            PlayerOptions = new List<AIPlayerOptionDto>
            {
                new AIPlayerOptionDto { OptionId = "opt1", Label = "Continue", ButtonText = "Continue", IntentTag = "continue" }
            }
        };
    }

    public AIDirectorResponse GetAIDirectorResponse(AIDirectorRequest request)
    {
        var chapterId = string.IsNullOrWhiteSpace(request.CurrentChapterId) ? "start" : request.CurrentChapterId;
        Debug.Log($"[FakeAIDirectorProvider] Current chapter: {chapterId}");

        switch (chapterId)
        {
            case "start":
                return new AIDirectorResponse
                {
                    TextLine = "Session started.",
                    EmotionTag = "neutral",
                    ConversationIntent = "Greeting",
                    ExpressionTag = "Smile",
                    BodyIntent = "Idle",
                    GameplayCommand = "MoveToNextChapter",
                    CameraIntent = "FocusFace",
                    PlayerOptions = new List<AIPlayerOptionDto>
                    {
                        new AIPlayerOptionDto { OptionId = "opt1", Label = "Continue", ButtonText = "Continue", IntentTag = "continue" }
                    }
                };

            case "greeting":
                return new AIDirectorResponse
                {
                    TextLine = "Hello. How are you feeling today?",
                    EmotionTag = "neutral",
                    ConversationIntent = "Question",
                    ExpressionTag = "Smile",
                    BodyIntent = "Talk",
                    GameplayCommand = "MoveToNextChapter",
                    CameraIntent = "FocusFace",
                    PlayerOptions = new List<AIPlayerOptionDto>
                    {
                        new AIPlayerOptionDto { OptionId = "opt1", Label = "I'm good.", ButtonText = "I'm good.", IntentTag = "player_good" },
                        new AIPlayerOptionDto { OptionId = "opt2", Label = "I'm tired.", ButtonText = "I'm tired.", IntentTag = "player_tired" }
                    }
                };

            case "check_in":
                return new AIDirectorResponse
                {
                    TextLine = "Thanks for telling me. Let's continue.",
                    EmotionTag = "neutral",
                    ConversationIntent = "Answer",
                    ExpressionTag = "Smile",
                    BodyIntent = "Talk",
                    GameplayCommand = "MoveToNextChapter",
                    CameraIntent = "FocusFace",
                    PlayerOptions = new List<AIPlayerOptionDto>
                    {
                        new AIPlayerOptionDto { OptionId = "opt1", Label = "Okay.", ButtonText = "Okay.", IntentTag = "continue" }
                    }
                };

            case "ending":
                return new AIDirectorResponse
                {
                    TextLine = "Goodbye for now.",
                    EmotionTag = "neutral",
                    ConversationIntent = "Goodbye",
                    ExpressionTag = "Smile",
                    BodyIntent = "Idle",
                    GameplayCommand = "EndSession",
                    CameraIntent = "FocusFace",
                    PlayerOptions = new List<AIPlayerOptionDto>()
                };

            default:
                return new AIDirectorResponse
                {
                    TextLine = "Session started.",
                    EmotionTag = "neutral",
                    ConversationIntent = "Greeting",
                    ExpressionTag = "Smile",
                    BodyIntent = "Idle",
                    GameplayCommand = "MoveToNextChapter",
                    CameraIntent = "FocusFace",
                    PlayerOptions = new List<AIPlayerOptionDto>()
                };
        }
    }
}
