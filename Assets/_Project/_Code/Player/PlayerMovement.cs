using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float m_WalkSpeed = 10f;
    [SerializeField] private float m_RunSpeed = 15f;
    [SerializeField] private float m_JumpImpulse = 5f;

    [Header("Portals")]
    public float m_TeleportPortalOffset = 1.0f;
    public float m_MaxValidAngle = 40.0f;

    private CharacterController m_CharacterController;
    private Vector2 m_MoveInput;
    private Vector3 m_Direction;
    private bool m_SprintHeld;
    private bool m_JumpHeld;
    private float m_VerticalSpeed;
    
    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    public void OnMove(InputValue value)
    {
        if (!enabled) return;
        m_MoveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        if (!enabled) return;
        m_SprintHeld = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (!enabled) return;
        m_JumpHeld = value.isPressed;
    }

    public void ResetMovement()
    {
        m_VerticalSpeed = 0f;
        m_Direction = Vector3.zero;
        m_SprintHeld = false;
        m_JumpHeld = false;
    }

    private void Update()
    {
        m_Direction = transform.right * m_MoveInput.x 
                      + transform.forward * m_MoveInput.y;
        m_Direction.Normalize();

        float l_speed = m_SprintHeld ? m_RunSpeed : m_WalkSpeed;
        if (m_JumpHeld && m_VerticalSpeed == 0f)
            m_VerticalSpeed = m_JumpImpulse;
        
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        Vector3 l_Direction = m_Direction * l_speed * Time.deltaTime;
        l_Direction.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Direction);
        if ((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0)
            m_VerticalSpeed = 0.0f;
        else if ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 
                 && m_VerticalSpeed > 0)
            m_VerticalSpeed = 0.0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;

        if (other.CompareTag("Portal"))
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if (l_Portal != null && CanTeleport(l_Portal))
            {
                Teleport(l_Portal);
            }
        }
    }

    private bool CanTeleport(Portal l_Portal)
    {
        float l_MaxDotAngle = Mathf.Cos(m_MaxValidAngle * Mathf.Deg2Rad);
        float l_DotAngle = Vector3.Dot(-m_Direction, l_Portal.transform.forward);
        return l_DotAngle >= l_MaxDotAngle && l_Portal.isActiveAndEnabled;
    }

    private void Teleport(Portal _Portal)
    {
        Vector3 l_worldPosition = transform.position + m_Direction * m_TeleportPortalOffset;
        Vector3 l_worldForward = transform.forward;
        Vector3 l_localPosition = _Portal.m_OtherPortalTransform.InverseTransformPoint(l_worldPosition);
        Vector3 l_localForward = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_worldForward);

        m_CharacterController.enabled = false;
        transform.position = _Portal.m_MirrorPortal.transform.TransformPoint(l_localPosition);
        transform.forward = _Portal.m_MirrorPortal.transform.InverseTransformDirection(l_localForward);
        GetComponent<PlayerCamera>()?.SetYaw(transform.rotation.eulerAngles.y);
        m_Direction = Vector3.zero;
        m_CharacterController.enabled = true;
    }
}
