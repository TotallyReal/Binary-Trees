using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ButtonInsteadAttribute : PropertyAttribute
{
    public string MethodName { get; }
    public string ButtonLabel { get; }

    public ButtonInsteadAttribute(string methodName, string buttonLabel = null)
    {
        MethodName = methodName;
        ButtonLabel = buttonLabel ?? methodName;
    }
}
