using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController m_CharacterController;

    float m_VerticalSpeed;
    private float m_Speed;
    public float m_MoveSpeed;
    public float m_JumpSpeed;
    public float m_SprintSpeed;

    private Vector3 m_Direction;
    
    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Speed = m_MoveSpeed;
    }

    public void ResetMovement()
    {
        m_VerticalSpeed = 0f;
        m_Speed = m_MoveSpeed;
    }

    private void Update()
    {
        Vector3 l_Direction = Vector3.zero;

        if (m_Direction.x > 0f)
            l_Direction = transform.right;
        else if (m_Direction.x < 0f)
            l_Direction = -transform.right;

        if (m_Direction.y > 0f)
            l_Direction += transform.forward;
        else if (m_Direction.y < 0f)
            l_Direction -= transform.forward;

        l_Direction.Normalize();
        
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        l_Direction *= m_Speed * Time.deltaTime;
        l_Direction.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Direction);
        if ((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0)
            m_VerticalSpeed = 0.0f;
        else if ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0)
            m_VerticalSpeed = 0.0f;
    }
    
    public void OnMove(InputValue value)
    {
        m_Direction = value.Get<Vector2>();
    }
    
    public void OnSprint(InputValue value) 
    {

        if (value.isPressed && m_VerticalSpeed == 0.0f)
            m_Speed = m_SprintSpeed;
        else
            m_Speed = m_MoveSpeed;
    }
    
    public void OnJump(InputValue value) 
    {
        if (value.isPressed && m_VerticalSpeed == 0.0f)
            m_VerticalSpeed = m_JumpSpeed;
    }
}
