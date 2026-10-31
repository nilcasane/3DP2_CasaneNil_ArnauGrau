using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
public class FPS_Controller: MonoBehaviour
{
    public Transform m_PitchController;
    float m_Yaw;
    float m_Pitch;
    public float mYawSpeed;
    public float m_PitchSpeed;
    public bool m_InvertedYaw;
    public bool m_InvertedPitch;
    public float m_MinYaw;
    public float m_MinPitch;
    public float m_MaxPitch;
    public float m_MaxYaw;
    public float m_SprintSpeed = 15.0f;
    public float m_JumpSpeed = 5.0f;
    public bool m_AngleLocked = false;
    public float speed;
    private float m_VerticalSpeed = 0.0f;
    public float m_speed;
    Vector3 m_StartPosition; 
    Quaternion m_StartRotation;
    
    
    [Header("Shoot")] 
    public Camera m_Camera;
    public float m_MaxShootDistance = 200.0f;
    public LayerMask m_ShootLayerMask;
    public GameObject m_HitParticlesPrefab;
    
    [Header("Input")]
    public KeyCode m_LeftKeyCode = KeyCode.A;
    public KeyCode m_RightKeyCode = KeyCode.D;
    public KeyCode m_UpKeyCode = KeyCode.W;
    public KeyCode m_DownKeyCode = KeyCode.S;
    public KeyCode m_SprintKeyCode = KeyCode.LeftShift;
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    public KeyCode m_DebugLockCursorKeyCode =  KeyCode.I;
    public KeyCode m_DebugLockAngleCode = KeyCode.I;
    public int m_ShootBlueButton = 0;
    public int m_ShootOrangeButton = 1;

    [Header("Portals")] 
    public Portal_A m_BluePortal;
    public Portal_A m_OrangePortal;
    
    /*[Header("Animations")] 
    public Animation m_Animation;
    public AnimationClip m_IdleAnimationClip;
    public AnimationClip m_ShootAnimationClip;
    public AnimationClip m_ReloadAnimationClip;*/
    
    CharacterController m_CharacterController;
    
    private void Start()
    {
        m_StartRotation = transform.rotation;
        m_StartPosition = transform.position;
        m_Yaw = transform.rotation.eulerAngles.y;
        m_Pitch = transform.rotation.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
        //SetIdleWeaponAnimation();
    }

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    //x = x0 + v · t distancia
    //v = v0 + a · t velocidad
    private void Update()
    {

        if (Input.GetKeyDown(m_DebugLockAngleCode))
        {
            m_AngleLocked = !m_AngleLocked;
        }
        if (Input.GetKeyDown(m_DebugLockCursorKeyCode))
        {
            Cursor.lockState = Cursor.lockState != CursorLockMode.Locked ? CursorLockMode.Locked : CursorLockMode.None; 
        }
        
        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");
        
        
                
        if (m_InvertedYaw)
            l_MouseX = -l_MouseX;
        if (m_InvertedPitch)
            l_MouseY = -l_MouseY;

        if(!m_AngleLocked)
        {
            m_Yaw = m_Yaw + l_MouseX * mYawSpeed;
            m_Pitch = m_Pitch + l_MouseY * m_PitchSpeed;
        }

        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);

        Vector3 l_Direction = Vector3.zero;
        Vector3 l_Right = transform.right;
        Vector3 l_Forward = transform.forward;
        l_Right.y = 0.0f;
        l_Forward.y = 0.0f;
        l_Right.Normalize();
        l_Forward.Normalize();

        if (Input.GetKey(m_RightKeyCode))
        {
            l_Direction = transform.right;
        }
        else if (Input.GetKey(m_LeftKeyCode))
        {
            l_Direction = -transform.right;
        }

        if (Input.GetKey(m_UpKeyCode))
        {
            l_Direction += transform.forward;
        }
        if (Input.GetKey(m_DownKeyCode))
        {
            l_Direction -= transform.forward;
        }
        float l_Speed = m_speed;
        if (Input.GetKey(m_SprintKeyCode))
            l_Speed = m_SprintSpeed;
        
        if (Input.GetKey(m_JumpKeyCode) && m_VerticalSpeed == 0.0f)
            m_VerticalSpeed = m_JumpSpeed;

        m_VerticalSpeed = m_VerticalSpeed + Physics.gravity.y * Time.deltaTime;

        l_Direction.Normalize();
        l_Direction = l_Direction * l_Speed * Time.deltaTime;
        l_Direction.y = m_VerticalSpeed * Time.deltaTime;
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Direction);
        if ((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0)
            m_VerticalSpeed = 0.0f;
        else if ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0.0f)
            m_VerticalSpeed = 0.0f;

        if (CanShoot() && MustShoot())
        {
            if (Input.GetMouseButtonUp(m_ShootBlueButton))
            {
                Shoot(m_BluePortal);                
            }
            else if(Input.GetMouseButtonUp(m_ShootOrangeButton))
            {
                Shoot(m_OrangePortal);
            }
        }
    }
    bool CanShoot()
    {
        return true;
    }
    bool MustShoot()
    {
        return Input.GetMouseButtonUp(m_ShootBlueButton) || Input.GetMouseButtonUp(m_ShootOrangeButton);
    }
    void Shoot(Portal_A _Portal)
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxShootDistance, m_ShootLayerMask.value))
        {
            if (_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
            {
                _Portal.gameObject.SetActive(true);
            }
            else
            {
                _Portal.gameObject.SetActive(false);
            }
        }
        else
        {
            _Portal.gameObject.SetActive(false);
        }
    }

    /*void SetIdleWeaponAnimation()
    {
        m_Animation.CrossFade(m_IdleAnimationClip.name, 0.1f);
    }
    void SetShootWeaponAnimation()
    {
        m_Animation.CrossFade(m_ShootAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.3f);
    }
    void SetReloadWeaponAnimation()
    {
        m_Animation.CrossFade(m_ReloadAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.3f);
    }*/
    

    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
    
}
