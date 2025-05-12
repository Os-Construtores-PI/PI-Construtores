using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer : MonoBehaviour
{
    [SerializeField] Vector3 _movePlayer;
    [SerializeField] CharacterController _controller;
    [SerializeField] float _firstJumpForce = 6.0f;    // Força do primeiro pulo
    [SerializeField] float _doubleJumpForce = 4.0f;  // Força do segundo pulo (menor)
    [SerializeField] float _gravity = -9.81f;
    [SerializeField] float _airControl = 0.5f;
    [SerializeField] float _playerSpeed = 6.0f;

    private Vector3 _velocity;
    [SerializeField] bool _groundedPlayer;
    private int _availableJumps = 2;  // Permite 2 pulos (duplo)



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //_jumpsRemaining = _maxJumps;
        _controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        //InputMovePlayer();
        MoveController();
        Gravidade();
        //Pulo();
    }

    void MoveController()
    {

        _groundedPlayer = _controller.isGrounded;

        if (_groundedPlayer && _velocity.y < 0)
        {
            _velocity.y = -2f;
            _availableJumps = 2;  
        }


        // Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _controller.Move(_movePlayer * Time.deltaTime * _playerSpeed);

        if (_movePlayer != Vector3.zero)
        {
            gameObject.transform.forward = _movePlayer;
        }

        
    }
       
        

        

    void Gravidade()
    {
       
        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    void Pulo()
    {
        if (Input.GetButtonDown("Jump") && _availableJumps > 0)
        {
            // Escolhe a força baseada no tipo de pulo
            float jumpForce = (_availableJumps == 2) ? _firstJumpForce : _doubleJumpForce;

            _velocity.y = Mathf.Sqrt(jumpForce * -2f * _gravity);
            _availableJumps--;
        }
    }

    
    void InputMovePlayer()
    {
        _movePlayer.x = Input.GetAxis("Horizontal");
        _movePlayer.z = Input.GetAxis("Vertical");
    }

    public void SetMove(InputAction.CallbackContext value)
    {
        _movePlayer.x = value.ReadValue<Vector3>().x;
        _movePlayer.z = value.ReadValue<Vector3>().y;
    }

    public void SetJump(InputAction.CallbackContext value)
    {
        if (value.started && _availableJumps > 0)
        {
            
            float jumpForce = (_availableJumps == 2) ? _firstJumpForce : _doubleJumpForce;

            _velocity.y = Mathf.Sqrt(jumpForce * -2f * _gravity);
            _availableJumps--;
        }

    }
}
