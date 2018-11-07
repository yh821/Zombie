using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : CGameSystem
{
    private Transform m_cCameraRoot;
    public Transform cameraRoot
    {
        get { return m_cCameraRoot;}
    }
    private Camera m_cCamera;

    private float m_fTurnSpeed = 1.5f;
    private float m_fTiltMax = 45f;
    private float m_fTiltMin = -45f;

    private float m_fLookAngle;
    private float m_fTiltAngle;

    private Vector3 m_sOldEuler;


    private float m_fActualShakeTime;
    private float m_fShakeTime;
    private float m_fShakeFactor;
    private float m_fShakeSpeed = 5.0f;

    public Camera mainCamera
    {
        get { return m_cCamera; }
    }

    private static ChaseCamera m_cInstance;
    public static ChaseCamera instance
    {
        get { return m_cInstance; }
    }

    public override void SysInitial()
    {
        base.SysInitial();
        m_cInstance = this;
    }

    public override bool SysEnter()
    {
        m_cCamera = SceneCfg.instance.cMainCamera;
        m_cCameraRoot = SceneCfg.instance.cameraRoot;
        m_sOldEuler = m_cCameraRoot.rotation.eulerAngles;
        m_fActualShakeTime = -1;
        return false;
    }

    public override void SysUpdate()
    {
        FollowTarget();
        UpdateShake();
    }

    public void Shake(float fTime, float fShakeFactor, float fShakeSpeed)
    {
        m_fShakeTime = fTime;
        m_fActualShakeTime = fTime;
        m_fShakeSpeed = fShakeSpeed;
        m_fShakeFactor = fShakeFactor;
    }

    public Vector3 GetViewVector(Vector3 sVector)
    {
        return m_cCameraRoot.TransformDirection(sVector);
    }

    public Vector2 GetViewVector(Vector2 sVector)
    {
        Vector3 sLogicVec = m_cCameraRoot.TransformDirection(sVector.x, 0.0f, sVector.y);
        Vector2 sTransVec;
        sTransVec.x = sLogicVec.x;
        sTransVec.y = sLogicVec.z;
        return sTransVec;
    }

    private void UpdateShake()
    {
        if (m_fActualShakeTime < 0.0f)
        {
            return;
        }

        m_fActualShakeTime -= Time.deltaTime;

        Vector3 sRandomShakePos;
        sRandomShakePos.x = Random.Range(-m_fShakeFactor, m_fShakeFactor);
        sRandomShakePos.y = Random.Range(-m_fShakeFactor, m_fShakeFactor);
        sRandomShakePos.z = Random.Range(-m_fShakeFactor, m_fShakeFactor);
        sRandomShakePos = sRandomShakePos * (m_fActualShakeTime / m_fShakeTime);

        m_cCameraRoot.position += Vector3.Lerp(Vector3.zero, sRandomShakePos, m_fShakeSpeed * Time.deltaTime);
    }

    private void FollowTarget()
    {
        SceneObject cTarget = GamingSys.Instance.Player;
        if (null == cTarget)
        {
            return;
        }

        m_cCameraRoot.position = cTarget.transform.position;
    }


    private void UpdateRotation()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        if (!CMath.IsZero(x) || !CMath.IsZero(y))
        {
            m_fLookAngle += x * m_fTurnSpeed;
            m_fTiltAngle += y * m_fTurnSpeed;
            m_fTiltAngle = Mathf.Clamp(m_fTiltAngle, m_fTiltMin, m_fTiltMax);

            Vector3 sEuler = Vector3.zero;
            sEuler.x = m_fTiltAngle;
            sEuler.y = m_fLookAngle;
            sEuler.z = m_sOldEuler.z;
            m_cCameraRoot.localEulerAngles = sEuler;

            UpdateChaseRotation();
        }
    }

    private void UpdateChaseRotation()
    {
        SceneObject cTarget = GamingSys.Instance.Player;
        if (null != cTarget && cTarget.IsEnterScene)
        {
            //Vector3 sRoleDir = m_cCameraRoot.position - m_cCamera.transform.position;
            //cTarget.SetForward(sRoleDir, true);
        }
    }
}

