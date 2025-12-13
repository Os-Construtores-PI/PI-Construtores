using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DataSystem))]
public class LoadDirector : MonoBehaviour
{
    private DataSystem dataSystem;

    private void Awake()
    {
        dataSystem = GetComponent<DataSystem>();
    }

    /// <summary>
    /// Botão do menu para carregar um slot específico
    /// </summary>
    /// <param name="index">Índice do slot</param>
    public void OnNewSlotButton(int index)
    {
        if (index < 0 || index >= dataSystem.GetMaxSlots()) return;

        GameContext.CurrentSlot = index;
        SavedSlotData savedSlot = dataSystem.GetSlotData(index);

        if (savedSlot != null && !string.IsNullOrEmpty(savedSlot.lastLevelName))
        {
            Debug.Log($"[LoadDirector] Carregando slot {index}: {savedSlot.lastLevelName}");
            SceneManager.LoadScene(savedSlot.lastLevelName);
        }
        else
        {
            Debug.Log($"[LoadDirector] Slot {index} vazio. Iniciando primeira fase (DebugScene).");
            SceneManager.LoadScene(Constants.SceneNames.Fase0);
        }
    }
}
