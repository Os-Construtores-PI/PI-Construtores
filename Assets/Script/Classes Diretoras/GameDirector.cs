using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private DataSystem dataSystem;

    [SerializeField] private PlayerDirector playerDirector;

    private void Start()
    {
        TryGetComponent(out dataSystem);
        // O painel de Start agora é gerenciado por outro script,
        // então não precisamos fazer nada aqui.
    }

    /// <summary>
    /// Inicia o mundo após o painel de Start chamar este método.
    /// </summary>
    public void StartWorld()
    {
        // Apenas ativa os jogadores e HUDs completos
        playerDirector.ActivatePlayers();
    }

    public void ShutdownWorld()
    {
        // Aqui você pode desativar players, limpar câmeras, salvar progresso etc.
    }
}
