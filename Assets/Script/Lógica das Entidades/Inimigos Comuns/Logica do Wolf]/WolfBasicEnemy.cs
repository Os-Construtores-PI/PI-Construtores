using System.Collections;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class WolfBasicEnemy : Enemies
{
    private NavMeshAgent _agent; // controla o movimento do inimigo via NavMesh
    private Transform _player; // Referência ao Player (Pandora)
    private EyeWolf _vision; // Script responsável pela visão do Lobo

    [Header("Configurações")]
    public float _patrolRadius = 5f; // distância máxima que o Lobo anda na patrulha
    public float _chaseSpeed = 4f; // velocidade do lobo ao perseguir o Player
    public float _patrolSpeed = 2f; // Velocidade do lobo quando está patrulhando

    [Header("Memoria da Perseguição")]
    public float _chaseMemoryTime = 3f; // tempo (em segundos) que ele continua perseguindo mesmo sem ver o Player
    private float _memoryTimer = 0f; // Contador interno dessa memória 

    private Vector3 _startPosition; // Posição inicial do inimigo, usada como centro da patrulha

    // Estados possíveis do Lobo: patrulhando ou perseguindo
    private enum WolfState { Patrol, Chase }
    private WolfState _currentState = WolfState.Patrol;

    [Header("Confings de Ataque")]
    [SerializeField] private float _prepTime = 0.5f; // tempo de preparação antes do avanço
    [SerializeField] private float _lungeSpeed = 12f; // velocidade do avanço
    [SerializeField] private float _lungeDuration = 0.3f; // tempo que dura o avanço
    [SerializeField] private int _attackDamage = 15;

    private bool _isAttacking = false;



    private void TryAttack()
    {

    }


    protected new void Awake()
    {

        _agent = GetComponent<NavMeshAgent>(); // Pega o NavMeshAgent do Lobo
        _vision = GetComponentInChildren<EyeWolf>(); // Procura o Script EyeWolf em filhos (ex: "cabeça/olhos)
        _startPosition = transform.position; // Salva a posição inicial do inimigo
    }
    protected new void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform; // Localiza o Player pela tag
        Patrol(); // Inicia patrulhando

    }

    // Update is called once per frame
    protected new void Update()
    {
        switch (_currentState)
        {
            case WolfState.Patrol:
                if (_vision._encontrouPlayer && _vision._playerDetectado != null)
                {
                    _currentState = WolfState.Chase; // Muda para perseguição 
                    _memoryTimer = _chaseMemoryTime; // Reseta a memoria de perseguição 
                }
                else if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
                {
                    Patrol(); // Se não tem destina ou já chegou, escolhe novo ponto de patrulha
                }
                break;


            case WolfState.Chase:
                if (_vision._encontrouPlayer && _vision._playerDetectado != null)
                {
                    _memoryTimer = _chaseMemoryTime; // Enquanto vê, reseta o timer
                    Chase(_vision._playerDetectado); // Continua perseguindo
                }
                else
                {
                    // Se perdeu o player, usa a memória
                    if (_memoryTimer > 0)
                    {
                        _memoryTimer -= Time.deltaTime; // Diminui o contador
                        if (_vision != null && _vision._playerDetectado != null) // Continua indo até a ultima posição conhecida
                        {
                            Chase(_vision._playerDetectado);
                        }
                    }

                    else

                    {

                        // Timer acabou -> volta a patrulhar
                        _currentState = WolfState.Patrol;
                        Patrol();
                    }



                }
                break;
        }

    }










    private void Patrol()
    {
        _agent.speed = _patrolSpeed;  // Define velocidade baixa
        Vector3 randomPoint = _startPosition + Random.insideUnitSphere * _patrolRadius;
        randomPoint.y = _startPosition.y; // Mantém no mesmo nível do chão
        _agent.SetDestination(randomPoint); // Move para o ponto aleatório
    }

    private void Chase(Transform target)
    {
        if (target == null) return;
        _agent.speed = _chaseSpeed;
        _agent.SetDestination(target.position);
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _agent.isStopped = true;

        // 1. Preparação (como se fosse carregar o ataque)
        yield return new WaitForSeconds(_prepTime);

        // 2. avanço em direção ao Player 
        float _timer = 0f;
        Vector3 dir = (target.position - transform.position).normalized;

        while (_timer < _lungeDuration)
        {
            _agent.Move(dir * _lungeSpeed * Time.deltaTime);
            _timer += Time.deltaTime;
            yield return null;
        }

        // 3. Checagem de colisão com o Player
        if (target.TryGetComponent(out Player player))
        {
            //ApplyDamage(player);

            // knockback no Player
            Vector3 knockDir = (player.transform.position - transform.position).normalized;
            player.ApplyKnockback(knockDir, KnockBackForce);

        }

        _agent.isStopped = false;
        _isAttacking = false;


    }
            
}

            

