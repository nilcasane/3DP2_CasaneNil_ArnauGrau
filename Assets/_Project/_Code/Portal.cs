using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public PlayerShooter m_Player;
    public Camera m_Camera;
    public Transform m_OtherPortalTransform;
    public Portal m_MirrorPortal;
    public float m_CameraOffset = 0.5f;
    public List<Transform> m_ValidPositions;
    public float m_RaycastValidationOffsetDistance = 0.1f;
    public float m_MaxAngleValidation = 5.0f;
    public float m_CurrentScale = 1f;
    
    private void Update()
    {
        Camera l_Camera = m_Player.m_Camera;
        Vector3 l_CameraWorldPosition = l_Camera.transform.position;
        Vector3 l_CameraLocalPosition = m_MirrorPortal.transform.InverseTransformPoint(l_CameraWorldPosition);
        
        Vector3 l_DirectionWorldPosition = l_Camera.transform.forward;
        Vector3 l_DirectionLocalPosition = m_MirrorPortal.transform.InverseTransformDirection(l_DirectionWorldPosition);
        
        m_Camera.transform.position = m_OtherPortalTransform.TransformPoint(l_CameraLocalPosition);
        m_Camera.transform.forward = m_OtherPortalTransform.TransformDirection(l_DirectionLocalPosition);

        float l_Distance = Vector3.Distance(m_Camera.transform.position, transform.position);
        m_Camera.nearClipPlane = m_CameraOffset + l_Distance;
    }

    public bool IsValidPosition(Vector3 Position, Vector3 Normal)
    {
        Camera l_Camera = m_Player.m_Camera;
        transform.position = Position;
        transform.rotation = Quaternion.LookRotation(Normal);
        
        foreach (Transform l_ValidPosition in m_ValidPositions)
        {
            Vector3 l_Direction = l_ValidPosition.position - l_Camera.transform.position;
            float l_Distance = l_Direction.magnitude + m_RaycastValidationOffsetDistance;
            l_Direction.Normalize();
            
            Ray l_Ray = new Ray(l_Camera.transform.position, l_Direction);
            float l_MaxValidDotAngle = Mathf.Cos(m_MaxAngleValidation*Mathf.Deg2Rad);
            if (Physics.Raycast(l_Ray, out RaycastHit l_HitInfo, l_Distance, m_Player.m_ShootLayerMask.value,
                    QueryTriggerInteraction.Ignore))
            {
                if (l_HitInfo.collider.CompareTag("PortalEnabled"))
                {
                    float l_DistanceToHitPoint = Vector3.Distance(l_HitInfo.point, l_ValidPosition.position);
                    if (l_DistanceToHitPoint < m_RaycastValidationOffsetDistance)
                    {
                        float l_DotAngle = Vector3.Dot(Normal, l_HitInfo.normal);
                        if (!(l_DotAngle < l_MaxValidDotAngle))
                            continue;
                    }
                }
            }

            return false;
        }
        return true;
    }
}
