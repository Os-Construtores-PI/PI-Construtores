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

    public void SetConfig(PlayerContext playerContext)
    {
        playerContext.LiveEntityMaxHealth = vidaMaxima;
        playerContext.LiveEntityDefense = defesa;
        playerContext.CombatEntityEnableRegen = habilitarRegeneracao;
        playerContext.CombatEntityRegenInterval = intervaloRegeneracao;
        playerContext.PlayerSpeed = velocidade;
        playerContext.PlayerRunningSpeed = velocidadeCorrida;
        playerContext.PlayerAcceleration = aceleracao;
        playerContext.PlayerRunningAcceleration = aceleracaoCorrida;
        playerContext.PlayerFriction = friccaoTerra;
        playerContext.PlayerAirFriction = friccaoAr;
        playerContext.PlayerJumpForce = forcaPulo;
        playerContext.PlayerMaxJumpCount = maximoDePulos;
        playerContext.PlayerGravity = gravidade;
        playerContext.PlayerGravityUpMultiplier = gravidademultsubida;
        playerContext.PlayerGravityDownMultiplier = gravidademultdescida;
        playerContext.PlayerMaxFallSpeed = velocidademaximaqueda;
        playerContext.PlayerDashDuration = duracaoDash;
        playerContext.PlayerDashSpeed = velocidadeDash;
        playerContext.PlayerDashCooldown = cooldownDash;
        // TODO: Adicionar Troca de Jogador na Config
        playerContext.PlayerWallSpeedMultiplier = multiplicadorVelocidadeParede;
        playerContext.PlayerWallJumpMultiplier = multiplicadorForcaPuloParede;
        playerContext.PlayerWallExitDuration = duracaoSaidaParede;
        playerContext.PlayerWillAttack = podeAtacar;
        playerContext.PlayerAttackCooldown = cooldownAtaque;
    }
}
