using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick
{
    public delegate void MoveHandler();
    public delegate void MovingHandler(Vector2 sMoveDir);
    public MoveHandler OnMoveStartListener;
    public MovingHandler OnMoveListener;
    public MoveHandler OnMoveEndListener;

    public MoveHandler OnPressListener;
    public MoveHandler OnPressEndListener;

    private JoystickUICtrl m_cCtrl;
    private Transform m_baseTransform;
    public Transform transBase
    {
        get { return m_baseTransform; }
    }
    private Transform m_touchTransform;
    public Transform transTouch
    {
        get { return m_touchTransform; }
    }
    private float m_fBaseRadius;
    private float m_fTouchRadius;

    private Vector2 m_sOrgPos;

    private float m_fActiveLeft;
    private float m_fActiveRight;

    private int m_nCurFingerId;
    private bool m_bStartMove;

    private bool m_bCanUseKey = false;
    private bool m_bUseKey = false;

    public void Init(Vector3 sPos, float fActiveLeft, float fActiveRight, bool bCanUseKey)
    {
        GameObject cObj = CResourceSys.Instance.LoadUI("Joystick");
        if (null != cObj)
        {
            m_cCtrl = cObj.AddComponent<JoystickUICtrl>();
        }

        m_baseTransform = m_cCtrl.sprBase.transform;
        m_touchTransform = m_cCtrl.sprTouch.transform;

        m_baseTransform.position = sPos;
        m_touchTransform.position = sPos;

        m_fBaseRadius = m_cCtrl.sprBase.preferredWidth / 2.0f;
        m_fTouchRadius = m_cCtrl.sprTouch.preferredWidth / 2.0f;

        m_fActiveLeft = fActiveLeft;
        m_fActiveRight = fActiveRight;

        m_bCanUseKey = bCanUseKey;
        m_sOrgPos = sPos;
        Reset();
    }

    public void Release()
    {
        OnMoveEndListener = null;
        OnMoveListener = null;
        OnMoveStartListener = null;

        if (null != m_cCtrl)
        {
            GameObject.Destroy(m_cCtrl.gameObject);
            m_cCtrl = null;
        }
    }

    public void Update()
    {
#if UNITY_EDITOR
        Vector2 sMoveDir = Vector2.zero;
        if (m_bCanUseKey)
        {
            sMoveDir.y = Input.GetAxis("Vertical");
            sMoveDir.x = Input.GetAxis("Horizontal");
        }
#endif

        if (m_nCurFingerId < 0)
        {
#if UNITY_EDITOR
            if (!CMath.IsZero(sMoveDir.y) || !CMath.IsZero(sMoveDir.x))
            {
                Begin(m_sOrgPos, 0);
                m_bUseKey = true;
            }
            else if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && 
                Input.mousePosition.x >= m_fActiveLeft && Input.mousePosition.x < m_fActiveRight)
            {
                Begin(Input.mousePosition, 0);
            }
#else
            if (Input.touchCount > 0)
            {
                for (int i = 0; i != Input.touchCount; ++i)
                {
                    Touch cTouch = Input.GetTouch(i);
                    if (TouchPhase.Began == cTouch.phase && !EventSystem.current.IsPointerOverGameObject(cTouch.fingerId))
                    {
                        if (cTouch.position.x >= m_fActiveLeft && cTouch.position.x < m_fActiveRight)
                        {
                            Begin(cTouch.position, cTouch.fingerId);
                        }
                    }
                }
            }
#endif
        }
        else
        {
#if UNITY_EDITOR
            if (!CMath.IsZero(sMoveDir.y) || !CMath.IsZero(sMoveDir.x) || Input.GetMouseButton(0))
            {
                Vector3 sScreenPos = m_sOrgPos;
                if (m_bUseKey)
                {
                    Vector2 sOffset = sMoveDir.normalized * m_fBaseRadius;
                    sScreenPos.x += sOffset.x;
                    sScreenPos.y += sOffset.y;
                }
                else
                {
                    sScreenPos = Input.mousePosition;
                }
#else
            int nTouchIndex = -1;
            for (int i = 0; i != Input.touchCount; ++i)
            {
                Touch cTouch = Input.GetTouch(i);
                if (cTouch.fingerId == m_nCurFingerId)
                {
                    nTouchIndex = i;
                    break;
                }
            }

            if (((Input.touchCount > nTouchIndex) && (TouchPhase.Moved == Input.GetTouch(nTouchIndex).phase || TouchPhase.Stationary == Input.GetTouch(nTouchIndex).phase)))
            {
                Vector3 sScreenPos = Input.GetTouch(nTouchIndex).position;
#endif
                UpdateTouchPos(sScreenPos);
                UpdateBasePos();

                Vector2 sDir = m_touchTransform.position - m_baseTransform.position;
                if (!m_bStartMove && sDir.magnitude > 10.0f)
                {
                    m_bStartMove = true;
                    if (null != OnMoveStartListener)
                    {
                        OnMoveStartListener();
                    }
                }

                if (null != OnPressListener)
                {
                    OnPressListener();
                }

                if (m_bStartMove && (null != OnMoveListener))
                {
                    sDir.Normalize();
                    OnMoveListener(sDir);
                }
            }

#if UNITY_EDITOR
            if ((m_bUseKey && CMath.IsZero(sMoveDir.y) && CMath.IsZero(sMoveDir.x)) || (!m_bUseKey && !Input.GetMouseButton(0)))
#else
            if (Input.touchCount <= 0 || (TouchPhase.Ended == Input.GetTouch(nTouchIndex).phase))
#endif
            {
                if (m_bStartMove && null != OnMoveEndListener)
                {
                    OnMoveEndListener();
                }
                if (null != OnPressEndListener)
                {
                    OnPressEndListener();
                }
                Reset();
            }
        }
    }

    private void Begin(Vector2 sMousePos, int nFingerId)
    {
        if (m_nCurFingerId >= 0)
        {
            return;
        }

        m_nCurFingerId = nFingerId;
        m_baseTransform.position = GetLimitPos(sMousePos, m_fBaseRadius);
        m_touchTransform.position = GetLimitPos(sMousePos, m_fTouchRadius);
    }

    private void Reset()
    {
        m_bUseKey = false;
        m_nCurFingerId = -1;
        m_bStartMove = false;
        m_baseTransform.position = m_sOrgPos;
        m_touchTransform.position = m_sOrgPos;
    }

    private Vector2 GetLimitPos(Vector2 sScreenPos, float fBorder)
    {
        if (sScreenPos.x < fBorder)
        {
            sScreenPos.x = fBorder;
        }
        else if (sScreenPos.x > Screen.width - fBorder)
        {
            sScreenPos.x = Screen.width - fBorder;
        }

        if (sScreenPos.y < fBorder)
        {
            sScreenPos.y = fBorder;
        }
        else if (sScreenPos.y > Screen.height - fBorder)
        {
            sScreenPos.y = Screen.height - fBorder;
        }
        return sScreenPos;
    }

    private void UpdateTouchPos(Vector3 sScreenPos)
    {
        m_touchTransform.position = sScreenPos;
    }

    private void UpdateBasePos()
    {
        float fRadius = m_fBaseRadius + m_fTouchRadius;
        Vector3 sBaseLocalPos = m_baseTransform.position;
        Vector3 sTouchLocalPos = m_touchTransform.position;
        Vector3 sOffset = sTouchLocalPos - sBaseLocalPos;
        sOffset.z = 0.0f;

        float fDis = sOffset.magnitude;
        if (fDis > fRadius)
        {
            float fRadian = Vector3.Angle(sTouchLocalPos - sBaseLocalPos, Vector3.right) * Mathf.Deg2Rad;
            float fOffsetY = Mathf.Sin(fRadian) * fRadius;
            float fOffsetX = Mathf.Cos(fRadian) * fRadius;

            sBaseLocalPos.x = sTouchLocalPos.x - fOffsetX;
            if (sTouchLocalPos.y > sBaseLocalPos.y)
            {
                sBaseLocalPos.y = sTouchLocalPos.y - fOffsetY;
            }
            else
            {
                sBaseLocalPos.y = sTouchLocalPos.y + fOffsetY;
            }
            m_baseTransform.position = sBaseLocalPos;
        }
    }
}
