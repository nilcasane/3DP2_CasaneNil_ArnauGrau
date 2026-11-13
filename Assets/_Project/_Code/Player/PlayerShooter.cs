using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shoot")]
    public Camera m_Camera;
    public Transform m_PitchController;
    public float m_MaxShootDistance = 200.0f;
    public LayerMask m_ShootLayerMask;

    [SerializeField] private Portal m_BluePortalPrefab;
    [SerializeField] private Portal m_OrangePortalPrefab;
    
    private Portal m_ActualBluePortal;
    private Portal m_ActualOrangePortal;
    private Portal m_PreviewBluePortal;
    private Portal m_PreviewOrangePortal;
    
    public Portal ActualPortal { get; private set; }
    private Portal m_ActivePreview;

    [Header("Grab")]
    [SerializeField] private LayerMask m_GrabLayerMask;
    public Transform m_GrabTransform;
    public float m_TimeGrab = 0.8f;
    public float m_ShootAttachedObjectForce = 20.0f;

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
        
        CreatePortals();
    }

    private void OnDestroy()
    {
        if (m_ActualBluePortal != null) Destroy(m_ActualBluePortal.gameObject);
        if (m_ActualOrangePortal != null) Destroy(m_ActualOrangePortal.gameObject);
        if (m_PreviewBluePortal != null) Destroy(m_PreviewBluePortal.gameObject);
        if (m_PreviewOrangePortal != null) Destroy(m_PreviewOrangePortal.gameObject);
    }
    
    private void CreatePortals()
    {
        m_ActualBluePortal = CreatePortal(m_BluePortalPrefab, false);
        m_ActualOrangePortal = CreatePortal(m_OrangePortalPrefab, false);
        m_ActualBluePortal.m_MirrorPortal = m_ActualOrangePortal;
        m_ActualOrangePortal.m_MirrorPortal = m_ActualBluePortal;
        m_PreviewBluePortal = CreatePortal(m_BluePortalPrefab, true);
        m_PreviewOrangePortal = CreatePortal(m_OrangePortalPrefab, true);
    }

    private Portal CreatePortal(Portal prefab, bool preview)
    {
        Portal portal = Instantiate(prefab);
        portal.gameObject.SetActive(false);
        portal.m_Player = this;
        portal.SetConnected(false);
        if (preview)
        {
            portal.enabled = false;
            foreach (Collider collider in portal.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
        }
        return portal;
    }

    public void OnShootBluePortal(InputValue value)
    {
        HandlePortalInput(true, value.isPressed);
    }

    public void OnShootOrangePortal(InputValue value)
    {
        HandlePortalInput(false, value.isPressed);
    }

    public bool CanShoot()
    {
        return !(m_ObjectAttached || m_AttachingObject);
    }
    
    public void OnGrabObject(InputValue value)
    {
        if (value.isPressed)
        {
            if (!m_AttachingObject && !m_ObjectAttached)
                GrabObject();
        }
        else if (m_GrabObject != null)
        {
            ThrowAttachedObject(0f);
        }
    }
    
    public void OnThrowObject(InputValue value)
    {
        if (m_ObjectAttached) 
            ThrowAttachedObject(m_ShootAttachedObjectForce);
    }

    private void HandlePortalInput(bool blue, bool pressed)
    {
        if (!CanShoot()) return;

        Portal preview = blue ? m_PreviewBluePortal : m_PreviewOrangePortal;
        if (pressed)
        {
            HidePreview();
            ActualPortal = blue ? m_ActualBluePortal : m_ActualOrangePortal;
            m_ActivePreview = preview;
            UpdatePreview();
        }
        else if (preview != null && preview == m_ActivePreview)
        {
            if (UpdatePreview())
            {
                ActualPortal.transform.SetPositionAndRotation(preview.transform.position, preview.transform.rotation);
                ActualPortal.gameObject.SetActive(true);
                bool connected = ActualPortal.m_MirrorPortal.isActiveAndEnabled;
                m_ActualBluePortal.SetConnected(connected);
                m_ActualOrangePortal.SetConnected(connected);
            }
            HidePreview();
        }
    }

    private bool UpdatePreview()
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        bool valid = Physics.Raycast(l_Ray, out RaycastHit hit, m_MaxShootDistance,
                         m_ShootLayerMask, QueryTriggerInteraction.Ignore)
                     && hit.collider.CompareTag("PortalEnabled")
                     && m_ActivePreview.IsValidPosition(hit.point, hit.normal);
        m_ActivePreview.gameObject.SetActive(valid);
        return valid;
    }

    private void HidePreview()
    {
        if (m_ActivePreview != null) m_ActivePreview.gameObject.SetActive(false);
        m_ActivePreview = null;
    }
    
    private void GrabObject()
    {
        if (m_Camera == null) return;

        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // Include the level geometry so walls block the grab ray.
        const int defaultLayerMask = 1 << 0;
        int rayMask = m_GrabLayerMask.value | m_ShootLayerMask.value | defaultLayerMask;
        if (!Physics.Raycast(l_Ray, out RaycastHit hit, m_MaxShootDistance, rayMask, QueryTriggerInteraction.Ignore))
            return;

        if ((m_GrabLayerMask.value & (1 << hit.collider.gameObject.layer)) == 0) return;
        m_GrabObject = hit.rigidbody;
        if (m_GrabObject == null) return;

        HidePreview();
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
        UpdateAttachingObject();

        if (m_ActivePreview != null)
        {
            UpdatePreview();
        }
    }
}
