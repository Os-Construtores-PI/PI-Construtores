using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CodeCapturer : ActivatableObject
{
    private List<Color> codeDirector = new(4);
    private List<Color> codePlayer = new(4);
    [SerializeField] private ActivatableObject objecttoactivate;
    private readonly UnityEvent<object> codeCorrect = new();
    private readonly UnityEvent<object> codeIncorrect = new();

    private void Start()
    {
        if (!objecttoactivate) return;
        codeCorrect.AddListener(objecttoactivate.ObjectAction);
    }
    public void SetupCode(List<Color> puzzleCode)
    {
        codeDirector = puzzleCode;
    }
    public override void ObjectAction(object info = null)
    {
        if (info is Color color)
        {
            AddToPlayerCode(color);
        }
    }
    private void AddToPlayerCode(Color color)
    {
        codePlayer.Add(color);
        print(color);
        if (codePlayer.Count == 4)
        {
            CompareCodes();
            codePlayer.Clear();
        }
    }
    private void CompareCodes()
    {
        if (ListUtils.ListIdenticalComparison(codePlayer,codeDirector))
        {
            codeCorrect.Invoke(default);
            print("Acertou");
        }
        else
        {
            codeIncorrect.Invoke(default);
            print("Errou");

        }
    }
}
