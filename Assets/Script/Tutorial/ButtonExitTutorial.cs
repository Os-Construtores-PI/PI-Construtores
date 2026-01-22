using UnityEngine;

public class ButtonExitTutorial : MonoBehaviour
{
    public void ClosedTutorial()
    {
        TutorialGlobal.Instance.FecharTutorial();
    }
}
