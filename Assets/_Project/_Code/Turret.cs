using UnityEngine;

public class Turret : MonoBehaviour
{
    public Laser m_Laser;
    public float m_AngleFall = 30.0f;
    
    private void Update()
    {
        bool l_Fallen = Vector3.Dot(transform.up, Vector3.up) < Mathf.Cos(m_AngleFall * Mathf.Deg2Rad);
        m_Laser.gameObject.SetActive(!l_Fallen);
    }
}