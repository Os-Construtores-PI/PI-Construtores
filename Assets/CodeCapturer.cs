using System.Collections.Generic;
using UnityEngine;

public class CodeCapturer : ActivatableObject
{
    List<Color> code = new(4);
    List<Color> playerCode = new(4);
    public void SetupCode(List<Color> puzzleCode)
    {
        code = puzzleCode;
    }
    public override void ObjectAction(object info = null)
    {
        if (info is Color color)
        {
            playerCode.Add(color);
        }
    }
}
