using System;

[Serializable]
public class AITagDefinition
{
    // Unique tag identifier.
    public string Id;

    // Description of the tag meaning.
    public string Description;

    // Category such as Animation, Emotion, Camera, or Rhythm.
    public string Category;

    public AITagDefinition() { }

    public AITagDefinition(string id, string description, string category)
    {
        Id = id;
        Description = description;
        Category = category;
    }
}
