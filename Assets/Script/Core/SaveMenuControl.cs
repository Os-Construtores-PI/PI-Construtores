using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveMenuControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartNewGame(int slotIndex)
    {
        DataDirector.Instance.ClearSlot(slotIndex);
        DataDirector.Instance.SetCurrentSlot(slotIndex);

        SceneManager.LoadScene(Constants.SceneNames.Fase0);
    }

    public void ContinueGame(int slotIndex)
    {
        DataDirector.Instance.SetCurrentSlot(slotIndex);

        string lastLevel = DataDirector.Instance.GetLastLevelName(slotIndex);

        if (string.IsNullOrEmpty(lastLevel))
            lastLevel = Constants.SceneNames.Fase0;

        SceneManager.LoadScene(lastLevel);
    }
}
