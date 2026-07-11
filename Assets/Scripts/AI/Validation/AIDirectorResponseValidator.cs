using System;
using System.Collections.Generic;
using UnityEngine;

// MVP validator for AIDirectorResponse.
// - Uses AITagDatabase instance from the scene for allowed tags.
// - Collects all validation errors and returns an AIDirectorValidationResult.
// - Does not throw exceptions for normal validation failures.
[Serializable]
public static class AIDirectorResponseValidator
{
    public static AIDirectorValidationResult Validate(AIDirectorResponse response, AITagDatabase tagDatabase)
    {
        var result = new AIDirectorValidationResult();

        if (response == null)
        {
            result.Errors.Add("Response is null.");
            result.IsValid = false;
            return result;
        }

        if (tagDatabase == null)
        {
            result.Errors.Add("Tag database is not assigned.");
            result.IsValid = false;
            return result;
        }

        // Text must not be empty
        if (string.IsNullOrWhiteSpace(response.TextLine))
            result.Errors.Add("TextLine is empty.");

        var db = tagDatabase;

        // Emotion tag
        if (string.IsNullOrWhiteSpace(response.EmotionTag))
        {
            result.Errors.Add("EmotionTag is empty.");
        }
        else if (!TagExists(db.EmotionTags, response.EmotionTag))
        {
            result.Errors.Add($"Unknown EmotionTag: '{response.EmotionTag}'.");
        }

        // Conversation intent
        if (string.IsNullOrWhiteSpace(response.ConversationIntent))
        {
            result.Errors.Add("ConversationIntent is empty.");
        }
        else if (!db.IsValidConversationIntent(response.ConversationIntent))
        {
            result.Errors.Add($"Unknown ConversationIntent: '{response.ConversationIntent}'.");
        }

        // Expression tag
        if (string.IsNullOrWhiteSpace(response.ExpressionTag))
        {
            result.Errors.Add("ExpressionTag is empty.");
        }
        else if (!db.IsValidExpressionTag(response.ExpressionTag))
        {
            result.Errors.Add($"Unknown ExpressionTag: '{response.ExpressionTag}'.");
        }

        // Body intent
        if (string.IsNullOrWhiteSpace(response.BodyIntent))
        {
            result.Errors.Add("BodyIntent is empty.");
        }
        else if (!db.IsValidBodyIntent(response.BodyIntent))
        {
            result.Errors.Add($"Unknown BodyIntent: '{response.BodyIntent}'.");
        }

        // Gameplay command
        if (string.IsNullOrWhiteSpace(response.GameplayCommand))
        {
            result.Errors.Add("GameplayCommand is empty.");
        }
        else if (!db.IsValidGameplayCommand(response.GameplayCommand))
        {
            result.Errors.Add($"Unknown GameplayCommand: '{response.GameplayCommand}'.");
        }

        // Camera intent
        if (string.IsNullOrWhiteSpace(response.CameraIntent))
        {
            result.Errors.Add("CameraIntent is empty.");
        }
        else if (!db.IsValidCameraIntent(response.CameraIntent))
        {
            result.Errors.Add($"Unknown CameraIntent: '{response.CameraIntent}'.");
        }

        // Player options
        if (response.PlayerOptions == null)
        {
            result.Errors.Add("PlayerOptions is null.");
        }
        else
        {
            for (int i = 0; i < response.PlayerOptions.Count; i++)
            {
                var opt = response.PlayerOptions[i];
                if (opt == null)
                {
                    result.Errors.Add($"PlayerOptions[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(opt.OptionId))
                    result.Errors.Add($"PlayerOptions[{i}].OptionId is empty.");

                // Accept either Label or ButtonText depending on DTO usage
                var hasLabel = !string.IsNullOrWhiteSpace(opt.Label);
                var hasButton = !string.IsNullOrWhiteSpace(opt.ButtonText);
                if (!hasLabel && !hasButton)
                    result.Errors.Add($"PlayerOptions[{i}] requires either Label or ButtonText.");

                if (string.IsNullOrWhiteSpace(opt.IntentTag))
                    result.Errors.Add($"PlayerOptions[{i}].IntentTag is empty.");
            }
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    private static bool TagExists(IReadOnlyList<string> list, string tag)
    {
        if (list == null || string.IsNullOrWhiteSpace(tag))
            return false;

        for (int i = 0; i < list.Count; i++)
        {
            if (string.Equals(list[i], tag, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
