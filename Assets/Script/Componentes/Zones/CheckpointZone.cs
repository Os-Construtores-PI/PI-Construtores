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
        if (!saved && dataSystem != null && other.TryGetComponent(out Player player))
        {
            dataSystem.SaveCheckpoint(GameContext.currentSlot);
            saved = true;
        }
    }
}
