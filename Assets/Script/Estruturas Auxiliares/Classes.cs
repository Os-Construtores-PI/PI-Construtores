using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ColorPuzzle
{
    public string id;
    public bool canFlash;
    public CodeCapturer codeCapturer;
    public float durationDesired;
    public List<PuzzleLampObject> lamps;
}
