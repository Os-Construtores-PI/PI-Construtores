using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
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
        GlobalEventBus.Instance.ObjectWasSeen.AddListener(InteractionPopup);
        // Esconde os elementos do painel GameOver
        DisablePanel(ConstantNames.GameOver);
        DisablePanel(ConstantNames.InteractionPopup);
    }
    private void DisablePanel(string panel_name)
    {
        if (painelMap.TryGetValue(panel_name, out var panel))
        {
            int lenght = panel.Count;
            for (int i = 0; i < lenght; i++)
            {
                if (i == 0 && panel[i].TryGetComponent(out Image image))
                {
                    image.DOFade(0f, 0f);
                }
                panel[i].transform.localScale = Vector3.zero;
            }
        }
    }

    public void ShowFade(string panel_name)
    {
        if (painelMap.TryGetValue(panel_name, out var gameOverPainel))
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
        print("dano");
        if (GameObject.FindWithTag("CinemachineCamera1").TryGetComponent<CinemachineBasicMultiChannelPerlin>(out var noisecomp))
        {
            noisecomp.AmplitudeGain = 1;
            StartCoroutine(StopShaking(noisecomp));
        }
    }
    IEnumerator StopShaking(CinemachineBasicMultiChannelPerlin noise)
    {
        yield return new WaitForSecondsRealtime(.25f);
        noise.AmplitudeGain = 0;
    }
    public void InteractionPopup(bool seeing, InteractableObject obj, int id)
    {
        print($"{seeing}, {obj}, {id}");
    }

}

[System.Serializable]
internal class Painel
{
    public string nome;
    public List<GameObject> painel;
}
