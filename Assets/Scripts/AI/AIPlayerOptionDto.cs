using System;
using System.Collections.Generic;

[Serializable]
public class AIPlayerOptionDto
{
    // Unique identifier for the option.
    public string OptionId;

    // The player-visible label for this choice.
    public string Label;

    // Text shown on the UI button for this option (optional).
    public string ButtonText;

    // The intent tag for this choice, used for decision mapping.
    public string IntentTag;
}
