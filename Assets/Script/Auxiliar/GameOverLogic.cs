using UnityEngine;

public class GameOverLogic : MonoBehaviour
{
    public void Respawn()
    {
        GlobalEventBus.Instance.PLAYERTRIGGERREDRESPAWN.Invoke();
    }
    public void ExitToMainMenu()
    {
        
    }
}
