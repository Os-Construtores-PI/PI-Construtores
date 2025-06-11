using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CharacterController))]
public class MiniPlayerEvents : MonoBehaviour
{
    MiniGameControl MGC;
    MiniPlayerControl PlayerMPC;
    BulletSpawner BS;
    [SerializeField] Transform Start_POS;
    int initial_health = 3;
    private int health;
    public int pontuation;
    private CharacterController character;
    private MiniSoundController audioSourceScript;
    private AudioSource audioSourceComp;
    [SerializeField] private AudioSource damagesound;
    private float mininterval = 1;
    private float curveFactor = .8f;
    private float decayFactor = .07f;
    private SaveSystem saveSystem = new();

    void Awake()
    {
        health = initial_health;
        character = GetComponent<CharacterController>();
        PlayerMPC = GetComponent<MiniPlayerControl>();
        GameObject.FindWithTag("MiniGameController").TryGetComponent(out MGC);
        GameObject.FindWithTag("MiniSpawnerLogic").TryGetComponent(out BS);
        GameObject.FindWithTag("MiniSoundController").TryGetComponent(out audioSourceScript);
        GameObject.FindWithTag("MiniSoundController").TryGetComponent(out audioSourceComp);
    }
    private void DeathPlayer()
    {
        audioSourceComp.Stop();
        BS.CancelInvoke();
        int? compar = saveSystem.LoadInt("score");
        if (pontuation > compar)
        {
            saveSystem.SaveInt("score", pontuation);
        }
        MGC.ShutdownFunc();
    }
    public void ResetPlayer()
    {
        Time.timeScale = 0;
        if (character != null) character.enabled = false;
        transform.position = Start_POS.position;
        if (character != null) character.enabled = true;


        health = initial_health;
        pontuation = 0;
        PlayerMPC.can_jump = true;

        MGC.miniMenu.UI_UpdateHUD_Health(health);
        MGC.miniMenu.UI_UpdateHUD_Score(pontuation);
        MGC.miniMenu.LoadScore();
        Time.timeScale = 1;
        audioSourceComp.Play();
    }

    public void WinPlayer()
    {
        audioSourceComp.Stop();
        BS.CancelInvoke();
        int? compar = saveSystem.LoadInt("score");
        if (pontuation > compar)
        {
            saveSystem.SaveInt("score", pontuation);
        }
        MGC.StartEndCoRo();
    }
    public void DamagePlayer(int damage)
    {
        health -= damage;
        damagesound.Play();
        MGC.miniMenu.UI_UpdateHUD_Health(health);
        if (health <= 0)
        {
            DeathPlayer();
        }
    }
    public void AddPontuation(int points)
    {
        pontuation += points;
        MGC.miniMenu.UI_UpdateHUD_Score(pontuation);
        if (pontuation == 5){BS.StartSpawning();}
        if (pontuation % 5 == 0){audioSourceScript.IncreasePitch();}
        float formula = PlayerMPC.start_time * Mathf.Pow(1 - decayFactor, Mathf.Pow(pontuation, curveFactor));
        PlayerMPC.time_to_jump = Mathf.Max(mininterval, formula);
    }
}
