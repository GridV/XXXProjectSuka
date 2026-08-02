/// <summary>
/// Immutable player input contract at the Session ownership boundary.
/// </summary>
public sealed class AIPlayerIntent
{
    public string IntentTag { get; }
    public string OptionId { get; }
    public string DisplayText { get; }

    public AIPlayerIntent(string intentTag, string optionId, string displayText)
    {
        IntentTag = intentTag ?? string.Empty;
        OptionId = optionId ?? string.Empty;
        DisplayText = displayText ?? string.Empty;
    }
}
