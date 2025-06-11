using UnityEngine;
public class MiniJumpGroundScript : MonoBehaviour
{
    public int jumpground_id;
    public bool jumped = false;
    private LayerMask lm;
    private RaycastHit hitinfo;
    private MiniGroundScript miniground;
    private Material material;
    private Color MaterialColor;
    private float glowIntensity = 2.5f;
    private int offset = 2;

    private void Start()
    {
        lm = LayerMask.GetMask("Ground");
        material = GetComponent<MeshRenderer>().material;
        MaterialColor = material.GetColor("_BaseColor");
        Invoke(nameof(Glow), 1f);
    }
    private void OnTriggerEnter(Collider collider)
    {
        MakeJump(collider);
    }
    private void OnTriggerStay(Collider collider)
    {
        MakeJump(collider);
    }
    private void MakeJump(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            MiniPlayerControl MPC = collider.gameObject.GetComponent<MiniPlayerControl>();
            MiniPlayerEvents MPE = collider.gameObject.GetComponent<MiniPlayerEvents>();
            if (MPC.IsGrounded && !jumped && MPC.can_jump)
            {
                MPC.Jump();
                MPE.AddPontuation(1);
                jumped = true;
                MPC.lastID = jumpground_id;
            }

        }
    }
    private void Glow()
    {
        bool hit = Physics.Raycast(new(transform.position.x, transform.position.y + offset, transform.position.z), Vector3.up, out hitinfo, 10, lm, QueryTriggerInteraction.Collide);
        if (hit)
        {
            if (hitinfo.collider.gameObject.TryGetComponent(out miniground))
            {
                if (miniground.ground_id == jumpground_id)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", MaterialColor * glowIntensity);
                    DynamicGI.UpdateEnvironment();
                }
            }
        }
        else
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", MaterialColor * glowIntensity);
            DynamicGI.UpdateEnvironment();
        }
    }
}
