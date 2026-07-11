using System;

[Serializable]
public class AIAnimatorFloatParameter
{
    // Animator parameter name
    public string Name;

    // Value to apply
    public float Value;

    public AIAnimatorFloatParameter() { }

    public AIAnimatorFloatParameter(string name, float value)
    {
        Name = name;
        Value = value;
    }
}
