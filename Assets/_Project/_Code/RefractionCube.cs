using UnityEngine;

public class RefractionCube : MonoBehaviour
{
    public LineRenderer m_Laser;
    private bool m_DisableLaserOnNextFrame = true;

    public void Awake()
    {
        m_Laser.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (m_DisableLaserOnNextFrame)
        {
            m_Laser.gameObject.SetActive(false);
        }   
        m_DisableLaserOnNextFrame = true;
    }

    public void Refract()
    {
        if (!m_DisableLaserOnNextFrame)
        {
            return;
        }
        m_DisableLaserOnNextFrame = false;
        m_Laser.gameObject.SetActive(true);
    }
}