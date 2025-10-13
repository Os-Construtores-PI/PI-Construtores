using UnityEngine;

public class ConfigPlayer : ScriptableObject
{
    [Header("Vida")]
    [SerializeField] private float vidaMaxima;
    [SerializeField] private float defesa;

    [Header("Movimento [ANDAR]")]
    [SerializeField] private float velocidade;
    [SerializeField] private float aceleracao;
    [SerializeField] private float friccaoTerra;
    [SerializeField] private float friccaoAr;

    [Header("Movimento [PULO]")]
    [SerializeField] private float forcaPulo;
    [SerializeField] private float pulosmaximos;
    [SerializeField] private float gravidade;

    [Header("Movimento [DASH]")]
    [SerializeField] private float velocidadeDash;
    [SerializeField] private float distanciaDash;
    [SerializeField] private float cooldownDash;

    [Header("SCANNER [INIMIGOS]")]
    [SerializeField] private float raioScanner;
    [SerializeField] private float cooldownInimigoScanner;

    [Header("SCANNER [OBJETOS]")]
    [SerializeField] private float cooldownObjetoScanner;

    [Header("MECÂNICA [TROCA DE JOGADOR]")]
    [SerializeField] private float cooldownTrocaJogador;

    [Header("MECÂNICA [CORRIDA NA PAREDE]")]
    [SerializeField] private float multiplicadorForcaPuloParede;
    [SerializeField] private float duracaoSaidaParede;

    [Header("MECÂNICA [ATAQUE]")]
    [SerializeField] private float cooldownAtaque;

}
