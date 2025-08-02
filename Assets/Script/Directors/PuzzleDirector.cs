using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDirector : MonoBehaviour
{
    [SerializeField] private List<PuzzleLampObject> lamps;
    [SerializeField] private float durationDesired = 3f;

    private List<Color> puzzleColors = new(4) { Color.yellow, Color.red, Color.blue, Color.green };

    public bool canFlash = true;

    private void Start()
    {
        if (puzzleColors.Count != lamps.Count)
        {
            print("Número de Lâmpadas não bate com o número de cores setadas");
            return;
        }
        puzzleColors = StaticRandomizer.ListRandomizer(puzzleColors);
        StartCoroutine(FlashLights());

    }
    IEnumerator FlashLights()
    {
        while (canFlash)
        {
            for (int i = 0; i < lamps.Count; i++)
            {
                lamps[i].SetColor(puzzleColors[i]);
                lamps[i].SetDuration(durationDesired);
                lamps[i].ObjectAction();
                yield return new WaitForSeconds(durationDesired);
            }
        }
    }
}
