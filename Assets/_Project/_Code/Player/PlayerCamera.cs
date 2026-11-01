using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform m_PitchController;

    float m_Yaw;
    float m_Pitch;
    [SerializeField] private float m_HorizontalSensitivity = 10f;
    public float m_PitchSpeed = 5f;
    public bool m_InvertedYaw;
    public bool m_InvertedPitch;
    [SerializeField] private float m_PitchLimit = 60f;

    public bool m_AngleLocked;
    
    private Vector2 m_LookInput;
    private Vector2 m_StickLookInput;
    
    private void Start()
    {
        m_Yaw = transform.rotation.eulerAngles.y;
        m_Pitch = transform.rotation.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!m_AngleLocked)
        {
            Vector2 look = m_LookInput * 0.1f + m_StickLookInput * Time.deltaTime;
            if (m_InvertedYaw) look.x = -look.x;
            if (m_InvertedPitch) look.y = -look.y;
            m_Yaw += look.x * m_HorizontalSensitivity;
            m_Pitch += look.y * m_PitchSpeed;
        }
        m_Pitch = Mathf.Clamp(m_Pitch, -m_PitchLimit, m_PitchLimit);

        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);
    }

    public void ResetLook()
    {
        m_Yaw = transform.eulerAngles.y;
        m_Pitch = 0f;
        m_LookInput = Vector2.zero;
        m_StickLookInput = Vector2.zero;
        m_PitchController.localRotation = Quaternion.identity;
    }
    
    public void OnLook(InputValue value)
    {
        m_LookInput = value.Get<Vector2>();
    }

    public void OnLookStick(InputValue value)
    {
        m_StickLookInput = value.Get<Vector2>();
    }
     
    public void OnDebugLockAngle(InputValue value)
    {
        m_AngleLocked = !m_AngleLocked;
    }
    
    public void OnDebugLockCursor(InputValue value)
    {
        Cursor.lockState = Cursor.lockState != CursorLockMode.Locked 
            ? CursorLockMode.Locked : CursorLockMode.None; 
    }
}
