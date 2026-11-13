using UnityEngine;

public class Laser : MonoBehaviour
{
    public float m_MaxDistance;
    public LineRenderer m_Laser;
    public LayerMask m_LaserLayerMask;
    public Player m_Player;

    private void Update()
    {
        Transform l_LaserTransform = m_Laser.transform;
        
        Vector3 l_Origin = l_LaserTransform.position;
        Vector3 l_Direction = l_LaserTransform.forward;
        Vector3 l_EndPosition = l_Origin + l_Direction * m_MaxDistance;

        Ray l_Ray = new Ray(l_Origin, l_Direction);
            
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LaserLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            l_EndPosition = l_RaycastHit.point;
            
            if (l_RaycastHit.collider.CompareTag("Player"))
            {
                m_Player.KillPlayer();
            }else if (l_RaycastHit.collider.CompareTag("RefractionCube"))
            {
                l_RaycastHit.collider.GetComponent<RefractionCube>().Refract();
            }
        }
        m_Laser.SetPosition(0, Vector3.zero);
        m_Laser.SetPosition(1, l_LaserTransform.InverseTransformPoint(l_EndPosition));
        m_Laser.gameObject.SetActive(true);
    }
}