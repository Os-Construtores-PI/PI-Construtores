using UnityEngine;

public class CheckpointZone : MonoBehaviour
{
    private bool saved = false;
    private DataSystem dataSystem;

    private void Start()
    {
        dataSystem = FindFirstObjectByType<DataSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (saved || dataSystem == null) return;

        if (other.TryGetComponent(out Player firstPlayer))
        {
            // pega a posição do primeiro player que entrou
            Vector3 checkpointPosition = firstPlayer.transform.position;

            // salva o checkpoint usando essa posição para todos os players
            dataSystem.SaveCheckpoint(GameContext.currentSlot, checkpointPosition);

            saved = true;
            Debug.Log($"Checkpoint salvo na posição {checkpointPosition} para todos os players.");
        }
    }
}
