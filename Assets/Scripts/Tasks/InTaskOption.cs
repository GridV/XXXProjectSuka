using System;
using UnityEngine;

[Serializable]


public class InTaskOption
{
    public string id;
    public string? nextOption;
    public string label;
    public string action;
    public InTaskOptionSide side = InTaskOptionSide.Positive;
}
public enum InTaskOptionSide
{
    Positive = 0,
    Negative = 1
}
