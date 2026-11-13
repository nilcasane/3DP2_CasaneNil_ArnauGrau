using UnityEngine;

public class Turret : MonoBehaviour
{
    public LineRenderer m_Laser;
    public float m_MaxDistance;
    public LayerMask m_LaserLayerMask;
    public Player m_Player;
    public float m_AngleFall = 30.0f;
    
    private void Update()
    {
        
        bool l_Fallen = Vector3.Dot(transform.up, Vector3.up) < Mathf.Cos(m_AngleFall * Mathf.Deg2Rad);
        m_Laser.gameObject.SetActive(!l_Fallen);

        if (!l_Fallen)
        {
            Ray l_Ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);
            float l_Distance = m_MaxDistance;
            
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LaserLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                l_Distance = l_RaycastHit.distance;
            
                if (l_RaycastHit.collider.CompareTag("Player"))
                {
                    m_Player.KillPlayer();
                }
            }
            
            Vector3 l_WorldEndPosition = l_Ray.origin + l_Ray.direction * l_Distance;
            m_Laser.SetPosition(0, Vector3.zero);
            m_Laser.SetPosition(1, m_Laser.transform.InverseTransformPoint(l_WorldEndPosition));
        }
        
    }
}