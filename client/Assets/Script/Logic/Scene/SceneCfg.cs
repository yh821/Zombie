using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCfg : MonoBehaviour
{
    public Camera cMainCamera;

    //public FOWSystem FowSystem;

    private Transform m_cCamerRoot;
    public Transform cameraRoot
    {
        get { return m_cCamerRoot; }
    }

    private static SceneCfg m_cInstance = null;
    public static SceneCfg instance
    {
        get { return m_cInstance; }
    }

    void Awake()
    {
        m_cInstance = this;
        m_cCamerRoot = cMainCamera.transform.parent;
    }
}
