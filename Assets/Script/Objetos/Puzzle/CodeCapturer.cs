using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CodeCapturer : ActivatableObject
{
    private List<Code> codeDirector = new(4);
    private List<Code> codePlayer = new(4);

    [SerializeField]
    private ActivatableObject objecttoactivate;
    private ColorPuzzle puzzleSetted;
    private readonly UnityEvent<object> codeCorrect = new();
    private readonly UnityEvent<object> codeIncorrect = new();

    private void Start()
    {
        if (!objecttoactivate)
            return;
        codeCorrect.AddListener(objecttoactivate.ObjectAction);
    }

    public void SetupCode(List<Code> puzzleCode, ColorPuzzle puzzleRef)
    {
        codeDirector = puzzleCode;
        puzzleSetted = puzzleRef;
    }

    public override void ObjectAction(object info = null)
    {
        if (info is Code code)
        {
            AddToPlayerCode(code);
        }
    }

    private void AddToPlayerCode(Code code)
    {
        codePlayer.Add(code);
        if (codePlayer.Count == 4)
        {
            if (CompareCodes())
            {
                print("Acertou");
                puzzleSetted.canFlash = false;
                codeCorrect.Invoke(default);
            }
            else
            {
                print("Errou");
                codeIncorrect.Invoke(default);
            }
            codePlayer.Clear();
        }
    }

    private bool CompareCodes()
    {
        print($"CODEDIRECTOR : \n{ListUtils.ToString(GetNumbers(codeDirector))}");
        print($"CODEPLAYER : \n{ListUtils.ToString(GetNumbers(codePlayer))}");
        for (int i = 0; i < codeDirector.Count; i++)
        {
            if (codeDirector[i].Number != codePlayer[i].Number)
            {
                return false;
            }
        }
        return true;
    }

    private List<int> GetNumbers(List<Code> codes)
    {
        List<int> numbers = new();
        foreach (Code code in codes)
        {
            numbers.Add(code.Number);
        }

        return numbers;
    }
}
