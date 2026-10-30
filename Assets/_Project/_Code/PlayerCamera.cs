using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform m_PitchController;

    float m_Yaw;
    float m_Pitch;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public bool m_InvertedYaw;
    public bool m_InvertedPitch;
    public float m_MinPitch;
    public float m_MaxPitch;

    private float m_MouseX;
    private float m_MouseY; 
    
    private void Start()
    {
        m_Yaw = transform.rotation.eulerAngles.y;
        m_Pitch = m_PitchController.eulerAngles.x;
    }

    void Update()
    {
        m_Yaw += m_MouseX * m_YawSpeed * Time.deltaTime;
        m_Pitch += m_MouseY * m_PitchSpeed * Time.deltaTime;

        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);
    }

    public void ResetLook()
    {
        m_Yaw = transform.eulerAngles.y;
        m_Pitch = 0f;
        m_MouseX = 0f;
        m_MouseY = 0f;
        m_PitchController.localRotation = Quaternion.identity;
    }
    
    public void OnLook(InputValue value)
    {
        m_MouseX = value.Get<Vector2>().x;
        m_MouseY = value.Get<Vector2>().y;

        if (m_InvertedYaw)
            m_MouseX = -m_MouseX;
        if (m_InvertedPitch)
            m_MouseY = -m_MouseY;
    }
}
