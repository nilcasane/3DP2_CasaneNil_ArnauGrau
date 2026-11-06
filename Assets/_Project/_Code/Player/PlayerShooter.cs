using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shoot")]
    public Camera m_Camera;
    public float m_MaxShootDistance = 200.0f;
    public LayerMask m_ShootLayerMask;

    [SerializeField] private Portal m_BluePortal;
    [SerializeField] private Portal m_OrangePortal;
    
    private Portal m_ActualBluePortal;
    private Portal m_ActualOrganePortal;
    
    private Portal m_PreviewBluePortal;
    private Portal m_PreviewOrganePortal;
    
    public Portal ActualPortal { get; private set; }
    private bool m_Previewing;

    
    public void OnShootBluePortal(InputValue value)
    {
        if (value.isPressed)
        {
            ActualPortal = m_BluePortal;
            m_Previewing = true;
        }
        else if (m_Previewing)
        {
            m_Previewing = false;
            Preview();
        }
    }

    public void OnShootOrangePortal(InputValue value)
    {
        if (value.isPressed)
        {
            ActualPortal = m_OrangePortal;
            m_Previewing = true;
        }
        else if (m_Previewing)
        {
            m_Previewing = false;
            Preview();
        }
    }

    private void Preview()
    {
        Portal portal = ActualPortal;
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxShootDistance, m_ShootLayerMask.value))
        {
            if (portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
            {
                portal.gameObject.SetActive(true);
                ActualPortal = portal;
                return;
            }
        }

        portal.gameObject.SetActive(false);
    }

    void Update()
    {
        if (m_Previewing)
        {
            Preview();
        }
    }
}
