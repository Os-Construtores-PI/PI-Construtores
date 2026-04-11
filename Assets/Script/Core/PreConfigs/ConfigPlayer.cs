using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfigData", menuName = "Configs/PlayerConfig")]
public class ConfigPlayer : ScriptableObject
{
  [Header("Vida")]
  [SerializeField]
  private float vidaMaxima;

  [SerializeField]
  private float defesa;

  [Header("Regeneração")]
  [SerializeField]
  private bool habilitarRegeneracao;

  [SerializeField]
  private float intervaloRegeneracao;

  [Header("Combate")]
  [SerializeField]
  private float cooldownCombate;

  [SerializeField]
  private float cooldownDano;

  [Header("Movimento [ANDAR]")]
  [SerializeField]
  private float velocidade;

  [SerializeField]
  private float velocidadeCorrida;

  [SerializeField]
  private float aceleracao;

  [SerializeField]
  private float aceleracaoCorrida;

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

  [Header("MECÂNICA [TROCA DE JOGADOR]")]
  [SerializeField]
  private float cooldownTrocaJogador;

  [Header("MECÂNICA [CORRIDA NA PAREDE]")]
  [SerializeField]
  private QualityTier multiplicadorVelocidadeParede;

  [SerializeField]
  private float multiplicadorForcaPuloParede;

  [SerializeField]
  private float duracaoSaidaParede;

  [Header("MECÂNICA [ATAQUE]")]
  [SerializeField]
  private bool podeAtacar;

  [SerializeField]
  private float cooldownAtaque;

  public void SetConfig(Player player)
  {
    player.MaxHealth = vidaMaxima;
    player.Defense = defesa;
    player.EnableRegen = habilitarRegeneracao;
    player.RegenerationInterval = intervaloRegeneracao;
    player.Speed = velocidade;
    player.RunningSpeed = velocidadeCorrida;
    player.Acceleration = aceleracao;
    player.AccelerationRunning = aceleracaoCorrida;
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
    // TODO: Adicionar Troca de Jogador na Config
    player.WallSpeedMultiplier = multiplicadorVelocidadeParede;
    player.WallJumpMultiplier = multiplicadorForcaPuloParede;
    player.WallExitDuration = duracaoSaidaParede;
    player.WillAttack = podeAtacar;
    player.AttackCooldown = cooldownAtaque;
  }
}
