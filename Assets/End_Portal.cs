using UnityEngine;

public class End_Portal : BasePortal
{
    private void OnTriggerEnter(Collider other)
    {
        TriggerEndGame();
    }
    private void TriggerEndGame()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.Invoke();
    }
}
