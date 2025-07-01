using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HUDDirector : MonoBehaviour
{
    [SerializeField] private List<Painel> painelList = new();
    private Dictionary<string, List<GameObject>> painelMap;

    private void Awake()
    {
        // Inicializa o dicionário com os dados da lista serializada
        painelMap = new Dictionary<string, List<GameObject>>();
        foreach (var painel in painelList)
        {
            if (!painelMap.ContainsKey(painel.nome))
            {
                painelMap[painel.nome] = painel.painel;
            }
            else
            {
                Debug.LogWarning($"Painel duplicado: {painel.nome}");
            }
        }
    }

    private void Start()
    {
        DOTween.Init();
        // Esconde os elementos do painel GameOver
        if (painelMap.TryGetValue("GameOver", out var gameOverPainel))
        {
            int lenght = gameOverPainel.Count;
            for (int i = 0; i < lenght; i++)
            {
                if (i == 0 && gameOverPainel[i].TryGetComponent(out Image image))
                {
                    image.DOFade(0f, 0f);
                }
                gameOverPainel[i].transform.localScale = Vector3.zero;
            }
        }
    }

    public void ShowGameOver()
    {
        if (painelMap.TryGetValue("GameOver", out var gameOverPainel))
        {
            int lenght = gameOverPainel.Count;
            for (int i = 0; i < lenght; i++)
            {
                if (i == 0 && gameOverPainel[i].TryGetComponent(out Image imagem))
                {
                    imagem.DOFade(.8f, .25f);
                }
                gameOverPainel[i].transform.DOScale(Vector3.one, 0.25f);
            }
        }
    }

    public void ShakeCamera()
    {
        Camera camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        Vector3 campos = camera.transform.position;
        camera.DOShakePosition(5f);
        camera.transform.position = campos;
    }

}

[System.Serializable]
internal class Painel
{
    public string nome;
    public List<GameObject> painel;
}
