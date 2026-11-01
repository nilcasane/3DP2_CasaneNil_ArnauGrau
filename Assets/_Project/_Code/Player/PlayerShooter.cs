using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shoot")]
    public Camera m_Camera;
    public float m_MaxShootDistance = 200.0f;
    public LayerMask m_ShootLayerMask;

    [SerializeField] private Portal m_BluePortal;
    [SerializeField] private Portal m_OrangePortal;
    public Portal LastPlacedPortal { get; private set; }
    public void OnShootBluePortal()
    {
        Shoot(m_BluePortal);
    }

    public void OnShootOrangePortal()
    {
        Shoot(m_OrangePortal);
    }

    private void Shoot(Portal portal)
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxShootDistance, m_ShootLayerMask.value))
        {
            if (portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
            {
                portal.gameObject.SetActive(true);
                LastPlacedPortal = portal;
                return;
            }
        }

        portal.gameObject.SetActive(false);
    }
}
