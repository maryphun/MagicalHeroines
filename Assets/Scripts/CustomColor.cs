using System;
using System.Reflection;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using UnityEngine;

public class CustomColor
{
    public static Color damage() { return new Color(1f, 0.75f, 0.33f); }
    public static Color miss() { return Color.yellow; }
    public static Color heal() { return new Color(0.33f, 1f, 0.5f); }
    public static Color invisible() { return new Color(1,1,1,0); }
    public static Color SP() { return new Color(0.75f, 0.75f, 1.00f); }
    public static Color stun() { return new Color(0.5f, 0.00f, 0.00f); }
    public static Color buffvalue() { return new Color(0.996f, 0.87f, 1.00f); }
    public static Color itemName() { return new Color(0.996f, 1.0f, 0.9607f); }
    public static Color abilityName() { return new Color(0.967f, 0.8705f, 1f); }
    public static Color akiho() { return new Color(1f, 0.753f, 0.796f); }
    public static Color rikka() { return new Color(0.678f, 0.847f, 0.902f); }
    public static Color erena() { return new Color(0.945f, 0.898f, 0.675f); }
    public static Color kei() { return new Color(0.678f, 0.847f, 0.902f); }
    public static Color nayuta() { return new Color(0.545f, 0f, 0f); }
    public static Color hisui() { return new Color(0f, 0.5f, 0.5f); }
    public static Color daiya() { return new Color(0.735f, 0.73f, 0.73f); }
    public static Color autobattle() { return new Color(1f, 0.31f, 0/31f); }


    public static string AddColor(int integerString, Color color) { return AddColor(integerString.ToString(), color); }
    public static string AddColor(string text, Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGBA(color); // Get color as a hex code RRGGBBAA
        return "<color=#" + hex + ">" + text + "</color>";
    }
}
