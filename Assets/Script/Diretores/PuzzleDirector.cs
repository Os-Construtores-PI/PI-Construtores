using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDirector : MonoBehaviour
{
  [SerializeField]
  private List<ColorPuzzle> puzzles = new();

  [SerializeField]
  float intensityLight = 20f;
  private List<Code> puzzleCode = CodeBaseFour.Codes;

  public bool canFlash = true;

  private void Start()
  {
    foreach (var puzzle in puzzles)
    {
      puzzle.canFlash = true;
      if (puzzleCode.Count != puzzle.lamps.Count)
      {
        print("Número de Lâmpadas não bate com o número de cores setadas");
        continue;
      }
      puzzleCode = StaticRandomizer.ListRandomizer(puzzleCode);
      StartCoroutine(FlashLights(puzzle));
      puzzle.codeCapturer.SetupCode(puzzleCode, puzzle);
    }
  }

  IEnumerator FlashLights(ColorPuzzle puzzle)
  {
    while (puzzle.canFlash)
    {
      for (int i = 0; i < puzzle.lamps.Count; i++)
      {
        puzzle
          .lamps[i]
          .SetupCorDurIntensity(puzzleCode[i].Color, puzzle.durationDesired, intensityLight);
        puzzle.lamps[i].ObjectAction(default);
        yield return new WaitForSeconds(puzzle.durationDesired);
      }
    }
  }
}
