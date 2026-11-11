using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shoot")]
    public Camera m_Camera;
    public Transform m_PitchController;
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

    [Header("Grab")]
    public Transform m_GrabTransform;
    public float m_TimeGrab = 0.8f;
    public float m_ShootAttachedObjectForce = 20.0f;
    public KeyCode m_GrabObjectCode = KeyCode.E;

    private bool m_AttachingObject;
    private bool m_ObjectAttached;
    private float m_GrabTime;
    private Rigidbody m_GrabObject;

    private void Awake()
    {
        if (m_PitchController == null)
        {
            PlayerCamera playerCamera = GetComponent<PlayerCamera>();
            if (playerCamera != null && playerCamera.m_PitchController != null)
            {
                m_PitchController = playerCamera.m_PitchController;
            }
            else if (m_Camera != null)
            {
                m_PitchController = m_Camera.transform.parent != null ? m_Camera.transform.parent : m_Camera.transform;
            }
        }

        if (m_GrabTransform == null)
        {
            FPS_Controller fpsController = GetComponent<FPS_Controller>();
            if (fpsController != null && fpsController.m_GrabTransform != null)
            {
                m_GrabTransform = fpsController.m_GrabTransform;
            }
            else if (m_PitchController != null)
            {
                Transform found = m_PitchController.Find("GrabTransform");
                if (found != null)
                {
                    m_GrabTransform = found;
                }
            }
        }
    }

    public bool CanShoot()
    {
        return !(m_ObjectAttached || m_AttachingObject);
    }

    public void OnShootBluePortal(InputValue value)
    {
        if (!enabled) return;

        if (value.isPressed)
        {
            if (CanShoot())
            {
                ActualPortal = m_BluePortal;
                m_Previewing = true;
            }
            else
            {
                m_Previewing = false;
            }
        }
        else
        {
            if (m_ObjectAttached || m_AttachingObject)
            {
                m_Previewing = false;
                ThrowAttachedObject(m_ShootAttachedObjectForce);
                return;
            }

            if (m_Previewing)
            {
                m_Previewing = false;
                if (CanShoot())
                {
                    Preview();
                }
            }
        }
    }

    public void OnShootOrangePortal(InputValue value)
    {
        if (!enabled) return;

        if (!CanShoot())
        {
            m_Previewing = false;
            return;
        }

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
        if (!CanShoot()) return;

        Portal portal = ActualPortal;
        if (portal == null || m_Camera == null) return;

        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxShootDistance, m_ShootLayerMask.value, QueryTriggerInteraction.Ignore))
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

    private bool CanGrabObject()
    {
        return Input.GetKeyDown(m_GrabObjectCode) && !m_AttachingObject && !m_ObjectAttached;
    }

    private void GrabObject()
    {
        if (m_Camera == null) return;

        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxShootDistance, m_ShootLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            if (l_RaycastHit.collider.CompareTag("Cube"))
            {
                GrabObject(l_RaycastHit.collider);
            }
        }
    }

    private void GrabObject(Collider _Collider)
    {
        m_GrabObject = _Collider.GetComponent<Rigidbody>();
        if (m_GrabObject == null) return;

        m_Previewing = false;
        m_GrabObject.isKinematic = true;
        m_AttachingObject = true;
        if (m_GrabTransform != null)
        {
            m_GrabObject.transform.SetParent(m_GrabTransform);
        }

        m_GrabTime = Time.time;
    }

    private void UpdateAttachingObject()
    {
        if (m_AttachingObject)
        {
            if (m_GrabObject == null || m_GrabTransform == null)
            {
                m_AttachingObject = false;
                return;
            }

            float l_DiffTime = Time.time - m_GrabTime;
            float l_Pct = Mathf.Min(1.0f, l_DiffTime / m_TimeGrab);
            m_GrabObject.transform.position =
                Vector3.Lerp(m_GrabObject.transform.position, m_GrabTransform.position, l_Pct);
            m_GrabObject.transform.rotation =
                Quaternion.Lerp(m_GrabObject.transform.rotation, m_GrabTransform.rotation, l_Pct);
            if (l_Pct == 1.0f)
            {
                m_AttachingObject = false;
                m_ObjectAttached = true;
            }
        }
    }

    private void UpdateAttachedObject()
    {
        if (m_ObjectAttached)
        {
            if (Input.GetKeyUp(m_GrabObjectCode))
            {
                ThrowAttachedObject(0.0f);
            }
        }
    }

    private void ThrowAttachedObject(float Force)
    {
        if (m_GrabObject != null)
        {
            m_GrabObject.transform.SetParent(null);
            m_GrabObject.isKinematic = false;
            Transform forwardSource = m_PitchController != null ? m_PitchController : (m_Camera != null ? m_Camera.transform : transform);
            m_GrabObject.linearVelocity = forwardSource.forward * Force;
            m_GrabObject = null;
        }
        m_AttachingObject = false;
        m_ObjectAttached = false;
    }

    void Update()
    {
        if (CanGrabObject())
        {
            GrabObject();
        }

        UpdateAttachingObject();
        UpdateAttachedObject();

        if (m_Previewing && CanShoot())
        {
            Preview();
        }
    }
}
