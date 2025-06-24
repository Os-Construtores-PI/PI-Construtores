using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Requer que o GameObject tenha CharacterController para controlar movimento e colisão
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementComponent : ComponentBehaviour
{
    #region Variáveis
    [Header("Movimento")]
    [SerializeField] private float speed = 10f;               // Velocidade máxima do movimento horizontal
    [SerializeField] private float acceleration = 5;           // Aceleração para suavizar mudança de velocidade
    [SerializeField] private float friction = 2f;               // Atrito no chão para desacelerar quando parado
    [SerializeField] private float airfriction = 2f;            // Atrito no ar para desacelerar quando parado no ar

    [Header("Parâmetros de Pulo")]
    [SerializeField] private float jumpForce = 10f;             // Força aplicada no pulo
    [SerializeField] private int maxJumpCount = 2;               // Quantidade máxima de pulos (ex: pulo duplo)
    [SerializeField] private float gravity = -9.81f;             // Gravidade aplicada no personagem

    [Header("Dash Parâmetros")]
    [SerializeField] private float dashSpeed = 30f;              // Velocidade durante o dash
    [SerializeField] private float initialDashDuration = 0.3f; // Duração inicial do dash
    [SerializeField] private float dashCooldown = 5f;            // Tempo de recarga para poder dar outro dash

    [Header("Componentes")]
    [SerializeField] private CharacterController characterController; // Componente CharacterController para movimentação
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;    // Referência à câmera Cinemachine para orientação

    private Vector3 movementVector = Vector3.zero;            // Vetor de movimento atual incluindo gravidade e velocidade
    private Vector3 dir;                                       // Direção do dash
    private Vector2 moveInput;                                // Entrada do jogador para movimentação (Eixo XZ)
    private int currentJumpCount;                             // Quantidade atual de pulos já realizados (para limitar pulos)
    private bool isGrounded;                                  // Flag para saber se o personagem está no chão
    private bool canDash = true;                              // Controla se o dash pode ser executado (não está em cooldown)
    private bool isDashing = false;                           // Controla se o personagem está em estado de dash
    private InputAction move_action;                          // Ação de input para movimento
    private float dashDuration;          // Tempo que dura o dash
    #endregion

    private void Start()
    {
        StartAtributes();    // Inicializa atributos e eventos de mudança
        DOTween.Init();      // Inicializa DOTween para animações
        // Atualiza velocidade e força do pulo caso os atributos mudem dinamicamente
        SubscribeToAttribute(nameof(speed), (newSpeed) =>
        {
            speed = (float)newSpeed;
        });
        SubscribeToAttribute(nameof(jumpForce), (newJumpForce) =>
        {
            jumpForce = (float)newJumpForce;
        });
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        move_action = InputSystem.actions.FindAction("Move");  // Pega a ação de input "Move" do sistema de input
        dashDuration = initialDashDuration;
    }

    private void FixedUpdate()
    {
        isGrounded = characterController.isGrounded; // Verifica se o personagem está tocando o chão

        DashLogica();            // Lida com lógica do dash, movimentação durante dash
        RotationAndMovement();   // Rotaciona o personagem e atualiza movimento normal

        characterController.Move(movementVector * Time.deltaTime);  // Aplica movimento ao personagem
    }

    #region Input
    // Recebe input de movimento (Eixo X e Y)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Recebe input do dash (ativação)
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            // Define a direção do dash baseado no vetor de movimento atual, ou frente se parado
            if (movementVector.x != 0 && movementVector.z != 0)
            {
                dir = new Vector3(movementVector.x, 0, movementVector.z).normalized;
            }
            else
            {
                dir = transform.forward;
            }
            StartDash();    // Começa o dash (seta flags e timers)
            DashSequence();// Animação visual do dash via DOTween
            move_action.Disable(); // Desabilita movimento enquanto dasha
        }
    }

    // Recebe input do pulo
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpLogica();
        }
    }
    #endregion

    #region Dash
    private void StartDash()
    {
        characterController.Move(Vector3.zero); // Para movimento atual
        canDash = false;                        // Marca dash como indisponível até o cooldown
        isDashing = true;                      // Marca personagem como dashing
        Invoke(nameof(ResetDash), dashCooldown); // Agenda reativação do dash após cooldown
    }

    // Lógica executada durante o FixedUpdate para dash e gravidade
    void DashLogica()
    {
        if (isDashing)
        {
            characterController.Move(dashSpeed * Time.deltaTime * dir); // Move na direção do dash
            dashDuration -= Time.deltaTime;                             // Diminui tempo restante do dash

            if (dashDuration <= 0f)
            {
                isDashing = false;         // Termina o dash
                dashDuration = initialDashDuration;       // Reseta duração para próximo dash
            }
        }
        else
        {
            if (!move_action.enabled)
            {
                move_action.Enable();   // Reativa input de movimento após dash
            }
            ApplyGravity(); // Aplica gravidade se não estiver dashando
        }
    }

    private void ResetDash()
    {
        canDash = true;         // Permite dash novamente
    }
    #endregion

    #region Movimento Y (vertical)
    private void ApplyGravity()
    {
        if (isGrounded && movementVector.y < 0)
        {
            movementVector.y = 0f;        // Reseta velocidade vertical se está no chão
            currentJumpCount = 0;         // Reseta contador de pulos
            if (moveInput == Vector2.zero)
            {
                // Aplica atrito para reduzir velocidade horizontal suavemente
                movementVector.x = Interp(movementVector.x, 0, friction);
                movementVector.z = Interp(movementVector.z, 0, friction);
            }
        }
        else
        {
            if (moveInput == Vector2.zero)
            {
                // Aplica atrito no ar para reduzir velocidade horizontal suavemente
                movementVector.x = Interp(movementVector.x, 0, airfriction);
                movementVector.z = Interp(movementVector.z, 0, airfriction);
            }
            movementVector.y += gravity * Time.deltaTime;  // Aplica gravidade vertical
        }
    }

    private void JumpLogica()
    {
        // Permite pular se estiver no chão ou ainda tiver pulos restantes
        if (isGrounded || currentJumpCount < maxJumpCount)
        {
            // Define um fator de redução com base na contagem de pulos
            float jumpMultiplier = 1f - (0.3f * currentJumpCount); // Ex: 1.0, 0.7, 0.4...

            // Garante que o multiplicador não fique negativo
            jumpMultiplier = Mathf.Max(jumpMultiplier, 0.2f); // valor mínimo de força

            // Aplica força do pulo com base na contagem
            movementVector.y = jumpForce * jumpMultiplier;

            // Incrementa o contador de pulos usados
            currentJumpCount++;
        }
    
    }
    #endregion

    #region Movimentos horizontais e rotação
    private void RotationAndMovement()
    {
        if (cinemachineCamera && moveInput != Vector2.zero)
        {
            // Pega direção da câmera para movimentação relativa à câmera
            var cameraTransform = cinemachineCamera.transform;
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calcula vetor direção baseado no input do jogador relativo à câmera
            Vector3 direction = forward * moveInput.y + right * moveInput.x;

            // Rotaciona suavemente o personagem para a direção do movimento
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            // Calcula velocidade alvo nos eixos X e Z baseados na direção e velocidade
            float targetX = direction.x * speed;
            float targetZ = direction.z * speed;

            // Aplica suavização na mudança de velocidade para evitar movimentos bruscos
            targetX = Interp(movementVector.x, targetX, acceleration);
            targetZ = Interp(movementVector.z, targetZ, acceleration);

            // Atualiza vetor de movimento com as velocidades suavizadas
            movementVector = new Vector3(targetX, movementVector.y, targetZ);
        }
    }
    #endregion

    #region Utilitários
    // Função de interpolação suavizada para valores float
    private float Interp(float from, float target, float smooth)
    {
        float newvalue = Mathf.Lerp(from, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        return newvalue;
    }

    // Inicializa os atributos do componente para permitir modificações dinâmicas
    private void StartAtributes()
    {
        SetAttribute(nameof(speed), speed);
        SetAttribute(nameof(jumpForce), jumpForce);
    }
    #endregion

    #region DOTWEEN - animações visuais
    void OnDestroy()
    {
        DOTween.KillAll();  // Mata todas as animações DOTween quando o objeto é destruído para evitar bugs
    }

    // Sequência visual para o dash (pequena animação de escala no eixo Y)
    private void DashSequence()
    {
        Sequence dashsequence = DOTween.Sequence();
        dashsequence.Append(transform.DOScaleY(.65f, dashDuration * .60f));
        dashsequence.Append(transform.DOScaleY(1, dashDuration * .40f));
        dashsequence.SetEase(Ease.InOutSine).SetUpdate(UpdateType.Fixed);
        dashsequence.Play();
    }
    #endregion
}
