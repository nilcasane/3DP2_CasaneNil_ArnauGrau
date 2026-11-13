using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    
    public bool IsDead;
    public event Action Died;

    private Vector3 m_StartPosition;
    private Quaternion m_StartRotation;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Restart()
    {
        Respawn(m_StartPosition, m_StartRotation);
    }

    public void Respawn(Vector3 position, Quaternion rotation)
    {
        CharacterController controller = GetComponent<CharacterController>();
        bool wasEnabled = controller != null && controller.enabled;
        if (wasEnabled) controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        GetComponent<PlayerMovement>()?.ResetMovement();
        GetComponent<PlayerCamera>()?.ResetLook();
        if (wasEnabled) controller.enabled = true;
    }
    
    public void KillPlayer()
    {
        Debug.Log("El jugador ha tocat el làser!");
    }

    private void Die()
    {
        if (IsDead) return;
        Died?.Invoke();
    }
}
