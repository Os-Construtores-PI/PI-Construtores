using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfigData", menuName = "Configs/PlayerConfig")]
public class ConfigPlayer : ScriptableObject
{
  [Header("Vida")]
  [SerializeField]
  private float vidaMaxima;

  [Header("Regeneração")]
  [SerializeField]
  private bool habilitarRegeneracao;

  [SerializeField]
  private float intervaloRegeneracao;

  [Header("Combate")]
  [SerializeField]
  private float cooldownCombate = 2;

  [SerializeField]
  private float cooldownDano = 2;

  [Header("Movimento [ANDAR]")]
  [SerializeField]
  private float velocidade = 30f;

  [SerializeField]
  private float multiplicadorVelocidadeCorrida = 1.5f;

  [SerializeField]
  private float aceleracao;

  [SerializeField]
  private float multiplicadorAceleracaoCorrida = 1.5f;

  [SerializeField]
  private float friccaoTerra;

  [SerializeField]
  private float friccaoAr;

  [Header("Movimento [PULO]")]
  [SerializeField]
  private float forcaPulo;

  [SerializeField]
  private int maximoDePulos;

  [SerializeField]
  private float gravidade;

  [SerializeField]
  private float gravidademultsubida;

  [SerializeField]
  private float gravidademultdescida;

  [SerializeField]
  private float velocidademaximaqueda;

  [Header("Movimento [DASH]")]
  [SerializeField]
  private float velocidadeDash;

  [SerializeField]
  private float duracaoDash;

  [SerializeField]
  private float cooldownDash;

  [SerializeField]
  private float distanciaDash;

  [SerializeField]
  private int maximoDeDashes;

  [Header("MECÂNICA [TROCA DE JOGADOR]")]
  [SerializeField]
  private float cooldownTrocaJogador;

  [Header("MECÂNICA [CORRIDA NA PAREDE]")]
  [SerializeField]
  private float multiplicadorVelocidadeParede;

  [SerializeField]
  private float multiplicadorForcaPuloParede;

  [SerializeField]
  private float duracaoSaidaParede;

  [Header("MECÂNICA [ATAQUE]")]
  [SerializeField]
  private bool podeAtacar;

  [SerializeField]
  private float cooldownAtaque;

  [Header("Pontuação")]
  [SerializeField]
  private int _pontuacaoTempo;

  [Tooltip("Eixo X = Tempo (segundos). Eixo Y = Pontos (0 a 1).")]
  [SerializeField]
  private AnimationCurve _pontuacaoCurva = AnimationCurve.EaseInOut(0f, 1f, 60f, 0f);

  public void SetConfig(Player player)
  {
    player.MaxHealth = vidaMaxima;
    player.EnableRegen = habilitarRegeneracao;
    player.RegenerationInterval = intervaloRegeneracao;
    player.Speed = velocidade;
    player.RunSpeedMultiplier = multiplicadorVelocidadeCorrida;
    player.Acceleration = aceleracao;
    player.RunAccelMultiplier = multiplicadorAceleracaoCorrida;
    player.Friction = friccaoTerra;
    player.AirFriction = friccaoAr;
    player.JumpForce = forcaPulo;
    player.MaxJumpCount = maximoDePulos;
    player.GravityValue = gravidade;
    player.GravityUpMultiplier = gravidademultsubida;
    player.GravityDownMultiplier = gravidademultdescida;
    player.MaxFallSpeed = velocidademaximaqueda;
    player.DashDuration = duracaoDash;
    player.DashSpeed = velocidadeDash;
    player.DashCooldown = cooldownDash;
    player.DashDistance = distanciaDash;
    player.MaxDashCount = maximoDeDashes;
    player.AttackCooldown = cooldownAtaque;
    player.CombatCooldown = cooldownCombate;
    player.DamagedCooldown = cooldownDano;
    player.WallSpeedMultiplier = multiplicadorVelocidadeParede;
    player.WallJumpMultiplier = multiplicadorForcaPuloParede;
    player.WallExitDuration = duracaoSaidaParede;
    player.WillAttack = podeAtacar;
    player.AttackCooldown = cooldownAtaque;
    player.MaxTimeScore = _pontuacaoTempo;
    player.TimeScoreCurve = _pontuacaoCurva;
  }
}
