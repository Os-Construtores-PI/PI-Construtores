using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDirector : MonoBehaviour
{
    [SerializeField] private List<PuzzleLampObject> lamps;
    [SerializeField] private float durationDesired = 3f;
    [SerializeField] float intensityLight = 20f;
    private List<Code> puzzleCode = CodeBaseFour.Codes;

    public bool canFlash = true;

    private void Start()
    {
        if (puzzleCode.Count != lamps.Count)
        {
            print("Número de Lâmpadas não bate com o número de cores setadas");
            return;
        }
        puzzleCode = StaticRandomizer.ListRandomizer(puzzleCode);
        FindFirstObjectByType<CodeCapturer>().SetupCode(puzzleCode);
        StartCoroutine(FlashLights());

    }
    IEnumerator FlashLights()
    {
        while (canFlash)
        {
            for (int i = 0; i < lamps.Count; i++)
            {
                lamps[i].SetupCorDurIntensity(puzzleCode[i].color, durationDesired, intensityLight);
                lamps[i].ObjectAction(default);
                yield return new WaitForSeconds(durationDesired);
            }
        }
    }
}
