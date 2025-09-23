using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private DataSystem dataSystem;

    [SerializeField] private PlayerDirector playerDirector;
    [SerializeField] private GameObject startPanel; // Painel de Start UI

    private void Start()
    {
        TryGetComponent(out dataSystem);
        if (startPanel != null)
            startPanel.SetActive(true); // Garante que o painel esteja visível
    }

    /// <summary>
    /// Chame este método ao apertar Start
    /// </summary>
    public void StartWorld()
    {
        // Remove o painel temporário
        if (playerDirector.startPanelInstance != null)
            Destroy(playerDirector.startPanelInstance);

        // Ativa players e HUDs completos
        playerDirector.ActivatePlayers();
    }




    public void ShutdownWorld()
    {
        // Desativar players, limpar câmeras, etc.
    }
}
