using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController3D : MonoBehaviour
{

    [SerializeField] Vector3 _movePlayer;
    [SerializeField] CharacterController _controller;
    [SerializeField] Vector3 _playerVelocity;
    [SerializeField] bool _isGrounded;
    [SerializeField] float _playerSpeed = 2.0f;
    [SerializeField] float _jumpHeight = 1.0f;
    private float _gravityValue = -9.7f;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //MoveController();
        MovendoPlayer();
        //Jump();
        Gravity();
        
    }

    

    void MoveController()
    {
       _movePlayer.x = Input.GetAxis("Horizontal");
       _movePlayer.z = Input.GetAxis("Vertical");
    }    


    void MovendoPlayer()
    {
          _isGrounded = _controller.isGrounded;
        if (_isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = 0f;
        }

       // Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _controller.Move(_movePlayer * Time.deltaTime * _playerSpeed);

        if (_movePlayer != Vector3.zero)
        {
            gameObject.transform.forward = _movePlayer;
        }

        
         
        
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
        }
    }
    
    void Gravity()
    {
       

         _playerVelocity.y += _gravityValue * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }

    public void SetMove(InputAction.CallbackContext value)
    {
         _movePlayer.x = value.ReadValue<Vector3>().x;
         _movePlayer.z = value.ReadValue<Vector3>().y;
    }

    public void SetJump(InputAction.CallbackContext value)
    {
         if (_isGrounded == true)
        {
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
        }
    }

   
}
