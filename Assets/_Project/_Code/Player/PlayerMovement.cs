using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float m_WalkSpeed = 10f;
    [SerializeField] private float m_RunSpeed = 15f;
    [SerializeField] private float m_JumpImpulse = 5f;

    private CharacterController m_CharacterController;
    private Vector2 m_MoveInput;
    private bool m_SprintHeld;
    private bool m_JumpHeld;
    private float m_VerticalSpeed;
    
    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    public void OnMove(InputValue value)
    {
        m_MoveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        m_SprintHeld = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        m_JumpHeld = value.isPressed;
    }

    public void ResetMovement()
    {
        m_VerticalSpeed = 0f;
        m_SprintHeld = false;
        m_JumpHeld = false;
    }

    private void Update()
    {
        Vector3 l_Direction = transform.right * m_MoveInput.x 
                              + transform.forward * m_MoveInput.y;
        l_Direction.Normalize();

        float l_speed = m_SprintHeld ? m_RunSpeed : m_WalkSpeed;
        if (m_JumpHeld && m_VerticalSpeed == 0f)
            m_VerticalSpeed = m_JumpImpulse;
        
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        l_Direction *= l_speed * Time.deltaTime;
        l_Direction.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Direction);
        if ((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0)
            m_VerticalSpeed = 0.0f;
        else if ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 
                 && m_VerticalSpeed > 0)
            m_VerticalSpeed = 0.0f;
    }
}
