using System.Collections.Generic;
using UnityEngine;

// Simple MonoBehaviour to test the AIDirectorResponseValidator with sample responses.
public class AIDirectorValidatorDebugTester : MonoBehaviour
{
    [SerializeField]
    private AITagDatabase tagDatabase;

    void Start()
    {
        TestValidResponse();
        TestInvalidResponse();
    }

    private void TestValidResponse()
    {
        var response = new AIDirectorResponse
        {
            TextLine = "Welcome, traveler.",
            EmotionTag = "neutral",
            ConversationIntent = "Greeting",
            ExpressionTag = "Smile",
            BodyIntent = "Idle",
            GameplayCommand = "ContinueDialogue",
            CameraIntent = "FocusFace",
            PlayerOptions = new List<AIPlayerOptionDto>
            {
                new AIPlayerOptionDto { OptionId = "opt1", Label = "Greet", ButtonText = "Greet", IntentTag = "greet" },
                new AIPlayerOptionDto { OptionId = "opt2", Label = "Ask", ButtonText = "Ask", IntentTag = "ask_question" }
            }
        };

        var result = AIDirectorResponseValidator.Validate(response, tagDatabase);
        Debug.Log($"Valid response valid: {result.IsValid}");
        if (!result.IsValid)
        {
            foreach (var e in result.Errors)
                Debug.LogError("Validation error: " + e);
        }
    }

    private void TestInvalidResponse()
    {
        var bad = new AIDirectorResponse
        {
            TextLine = "", // empty text
            EmotionTag = "unknown_emotion",
            ConversationIntent = "unknown_intent",
            ExpressionTag = "unknown_expression",
            BodyIntent = "unknown_body",
            GameplayCommand = "unknown_command",
            CameraIntent = "unknown_camera",
            PlayerOptions = new List<AIPlayerOptionDto>
            {
                new AIPlayerOptionDto { OptionId = "", Label = "", ButtonText = "", IntentTag = "" },
                null
            }
        };

        var result = AIDirectorResponseValidator.Validate(bad, tagDatabase);
        Debug.Log($"Invalid response valid: {result.IsValid}");
        if (!result.IsValid)
        {
            foreach (var e in result.Errors)
                Debug.LogError("Validation error: " + e);
        }
    }
}
