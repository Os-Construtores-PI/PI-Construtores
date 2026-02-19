using UnityEngine;

public class End_Portal : BasePortal
{
    private bool actived = false;
    private void OnTriggerEnter(Collider other)
    {
        if (actived)
            return;

        if (!other.CompareTag("Player"))
            return;

        actived = true;

        GameDirector director = FindAnyObjectByType<GameDirector>();
        if(director != null)
        {
            director.FinalizarFase();
        }

        gameObject.SetActive(false);

        //TriggerEndGame();
    }
  /*  private void TriggerEndGame()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.Invoke();
    }
  */
}
