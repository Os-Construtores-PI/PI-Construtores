using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDirector : MonoBehaviour
{
    [SerializeField] private List<PuzzleLampObject> lamps;
    [SerializeField] private float durationDesired = 3f;
    [SerializeField] float intensityLight = 20f;
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
        GameObject.FindFirstObjectByType<CodeCapturer>().SetupCode(puzzleColors);
        StartCoroutine(FlashLights());

    }
    IEnumerator FlashLights()
    {
        while (canFlash)
        {
            for (int i = 0; i < lamps.Count; i++)
            {
                lamps[i].SetupCorDurIntensity(puzzleColors[i], durationDesired, intensityLight);
                lamps[i].ObjectAction(default);
                yield return new WaitForSeconds(durationDesired);
            }
        }
    }
}
