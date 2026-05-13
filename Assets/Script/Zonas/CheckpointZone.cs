using UnityEngine;

public class CheckpointZone : MonoBehaviour
{
  private bool saved = false;

  private void OnTriggerEnter(Collider other)
  {
    if (saved || DataDirector.Instance == null)
      return;

    if (other.TryGetComponent(out Player firstPlayer))
    {
      // pega a posição do primeiro player que entrou
      Vector3 checkpointPosition = firstPlayer.transform.position;
      // salva o checkpoint usando essa posição para todos os players
      DataDirector.Instance.SaveCheckpoint(DataDirector.Instance.GetCurrentSlot());
      saved = true;

      // printa que deu tudo certo
      //Debug.Log($"Checkpoint salvo na posição {checkpointPosition} para todos os players.");
    }
  }
}
