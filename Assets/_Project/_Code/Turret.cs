using UnityEngine;

public class Turret : MonoBehaviour
{
    public LineRenderer m_Laser;
    public float m_MaxDistance;
    public LayerMask m_LaserLayerMask;
    private void Update()
    {
        Ray l_Ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);
        float l_Distance = m_MaxDistance;

        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LaserLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            l_Distance = l_RaycastHit.distance;
        }

        Vector3 l_WorldEndPosition = l_Ray.origin + l_Ray.direction * l_Distance;
        m_Laser.SetPosition(0, Vector3.zero);
        m_Laser.SetPosition(1, m_Laser.transform.InverseTransformPoint(l_WorldEndPosition));
    }
}