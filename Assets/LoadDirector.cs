using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DataSystem))]
public class LoadDirector : MonoBehaviour
{
    private DataSystem dataSystem;
    private void Start()
    {
        dataSystem = GetComponent<DataSystem>();
    }
    public void OnClickButton(int index)
    {
        if (index < 0 || index >= dataSystem.GetMaxSlots) return;
        GameContext.currentSlot = index;
        // --------------------------------------------------- //
        SavedSlotData savedSlot = dataSystem.GetSlotData(index);
        if (savedSlot != null)
        {
            SceneManager.LoadScene(savedSlot.lastLevelName);
        }
        else
        {
            SceneManager.LoadScene(Constants.SceneNames.DebugScene);
        }
    }
}
