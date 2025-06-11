using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;




public class MiniGameControl : MonoBehaviour
{
    [SerializeField] Transform GroundBase;
    [SerializeField] float tempload = .1f;
    [SerializeField] int Ydistance = 10;
    [SerializeField] int Xdistance;
    public Dictionary<string, Color> colors = new() { { "Azul Claro", new(0.153f, 0.561f, 0.847f) }, { "Laranja", new(0.925f, 0.502f, 0.075f) }, { "Vermelho", new(1, 0.06987041f, 0) } };
    public Dictionary<int, string> nameColors = new() { { 0, "Azul Claro" }, { 1, "Laranja" }, { 2, "Vermelho" } };
    public int selected_numb = 3;
    public MiniMenuControl miniMenu;
    bool is_Running;
    void Start()
    {
        MiniGroundPool miniGroundPool = GameObject.FindWithTag("MiniPool").GetComponent<MiniGroundPool>();
        int amount = miniGroundPool.amountToPool;
        miniMenu = GetComponent<MiniMenuControl>();
        StartCoroutine(CarregarPlataformas(amount, tempload));
    }
    IEnumerator CarregarPlataformas(int amount, float duration)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject plataforma = GameObject.FindWithTag("MiniPool").GetComponent<MiniGroundPool>().GetPooledObject();
            if (plataforma != null)
            {
                SetupPlataforma(plataforma,amount,i);
                GroundBase = plataforma.transform;
                yield return new WaitForSeconds(duration);
            }
        }
        miniMenu.FinishedLoading();
    }
    void SetupPlataforma(GameObject ground, int amount, int iteraator)
    {
        ground.transform.position = new Vector3(GroundBase.transform.position.x + Xdistance, Ydistance + GroundBase.transform.position.y, 0);
        if (iteraator == (amount - 1))
        {
            SetupEndGame(ground);
        }
        ground.SetActive(true);
    }
    void SetupEndGame(GameObject ground)
    {
        GameObject EndObj = ground.transform.GetChild(3).gameObject;
        EndObj.SetActive(true);
    }
    public void ShutdownFunc()
    {
        StartCoroutine(Shutdown());
    }
    IEnumerator Shutdown()
    {
        is_Running = false;
        miniMenu.DeathMessage();
        SetPlayerActive(false);
        yield return new WaitForSeconds(2);
        miniMenu.UI_Control(true,false);
    }
    public void StartGame()
    {
        is_Running = true;
        ActiveDeadPlayers();
        List<GameObject> Players = GameObject.FindGameObjectsWithTag("Player").ToList();
        foreach(GameObject player in Players)
        {
            player.GetComponent<MiniPlayerEvents>().ResetPlayer();
        }
        List<GameObject> Grounds = GameObject.FindGameObjectsWithTag("MiniJumpGround").ToList();
        foreach (GameObject ground in Grounds)
        {
            if (ground.TryGetComponent(out MiniJumpGroundScript minijumpscr))
            {
                minijumpscr.jumped = false;
            }
        }
        miniMenu.UI_Control(false,false);
        miniMenu.UI_HUDStart();
    }
    public void EndGameFinal()
    {
        SceneManager.LoadScene("CenaMinijogo");
    }
    public void GoToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
    public void StartEndCoRo()
    {
        StartCoroutine(EndGame());
    }
    IEnumerator EndGame()
    {
        is_Running = false;
        miniMenu.WinMessage();
        SetPlayerActive(false);
        yield return new WaitForSeconds(2);
        miniMenu.UI_Control(false, true);
    }

    private void ActiveDeadPlayers()
    {
        List<GameObject> Players = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(go => !go.activeInHierarchy).Where(gj => gj.CompareTag("Player")).ToList();
        foreach (GameObject player in Players)
        {
            player.SetActive(true);
        }
    }
    private void SetPlayerActive(bool set)
    {    
        List<GameObject> Players = GameObject.FindGameObjectsWithTag("Player").ToList();
        foreach(GameObject player in Players)
        {
            player.SetActive(set);
        }
    }








































}
