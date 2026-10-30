using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public bool IsDead;
    public event Action Died;
    
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

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Respawn(Vector3 position, Quaternion rotation, bool resetHealth = true)
    {
        CharacterController controller = GetComponent<CharacterController>();
        bool wasEnabled = controller != null && controller.enabled;
        if (wasEnabled) controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        GetComponent<PlayerMovement>()?.ResetMovement();
        GetComponent<PlayerCamera>()?.ResetLook();
        if (wasEnabled) controller.enabled = true;
    }

    private void Die()
    {
        if (IsDead) return;
        Died?.Invoke();
    }
}
