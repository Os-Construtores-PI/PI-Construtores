using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody),typeof(CharacterController))]
public class MiniPlayerControl : MonoBehaviour
{
    [SerializeField] CharacterController character;
    [SerializeField] MiniGameControl MGC;
    [SerializeField] float _speed = 15;
    [SerializeField] float gvalue = 10f;
    [SerializeField] float friction = 4;
    [SerializeField] float acceleration = 2;
    [SerializeField] float _forceJump = 15;
    public float start_time = 10;
    public float time_to_jump;
    private float hmove;
    private Vector3 movement_vec;
    public bool IsGrounded;
    public bool can_jump;
    private LayerMask layerMask;
    private RaycastHit next_plat;
    public int lastID;
    [SerializeField] AudioSource jumpsound;

    void Awake()
    {
        can_jump = true;
        MGC = GameObject.FindWithTag("MiniGameController").GetComponent<MiniGameControl>();
        layerMask = LayerMask.GetMask("Ground");
        character = GetComponent<CharacterController>();
        time_to_jump = start_time;
    }

    // Update is called once per frame
    void Update()
    {
        IsGrounded = character.isGrounded;
        Gravity();
        PlayerMove();
        SendColor();
    }
    void SendColor()
    {
        bool hit = Physics.Raycast(transform.position, Vector3.up,out next_plat, 100f, layerMask, QueryTriggerInteraction.Collide);
        if(hit)
        {
            if (next_plat.collider.TryGetComponent(out MiniGroundScript miniscript))
            {
                int id_nextplat = miniscript.ground_id;
                MGC.miniMenu.UI_UpdateHUD_Color(id_nextplat);
            }
        }
    }

    public void Jump() // função do pulo
    {
        movement_vec.y = _forceJump;
        jumpsound.Play();
        can_jump = false;
        StartCoroutine(JumpDelay(time_to_jump));
    }
    IEnumerator JumpDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        can_jump = true;
    }

    private void Gravity()
    {
        if (!IsGrounded)
        {
            movement_vec.y -= gvalue * Time.deltaTime;
        }
        else if(hmove==0)
        {
            movement_vec.x = Mathf.Lerp(movement_vec.x, 0, 1 - Mathf.Exp(-Time.deltaTime * friction));
        }
    }
    public void SetMove(InputAction.CallbackContext value)
    {
        hmove = value.ReadValue<Vector2>().x;
    }
    public void Escape(InputAction.CallbackContext value)
    {
        MGC.miniMenu.Pause();
    }
    public void MiniJump(InputAction.CallbackContext value)
    {
        if (can_jump)
        {
        movement_vec.y = _forceJump*.3f;
        jumpsound.Play();
        can_jump = false;
        StartCoroutine(JumpDelay(2));    
        }
    }

    private void PlayerMove()
    {
        movement_vec.x = Mathf.Lerp(movement_vec.x, hmove * _speed, 1 - Mathf.Exp(-Time.deltaTime * acceleration));
        character.Move(movement_vec * Time.deltaTime);
    }
}
