using UnityEngine;

public class UINodesManager : CGameSystem
{
    public static UINodesManager Instance { get; private set; }

    public static Camera UICamera { get; private set; }

    #region 节点Transform
    //UI根节点
    static public Transform UIRoot
    {
        get
        {
            if (m_UIRoot == null)
            {
                GameObject go = GameObject.Find(UINodeName.UIRootName);
                if (go != null)
                    m_UIRoot = go.transform;
            }

            return m_UIRoot;
        }
    }
    static private Transform m_UIRoot;
    #endregion

    #region 2D关键节点设计

    #region Transform

    /// <0>无事件层(第0层)
    /// <1>普通层(第1层)
    /// <2>中间层(第2层)
    /// <3>T1(第3层)
    /// <4>T2(第4层)
    /// <5>T3(第5层)

    /// <summary>
    /// 无事件层(第0层)    
    /// (1)飘血等等
    /// </summary>
    static private Transform m_NoEventsUIRoot;

    public static Transform NoEventsUIRoot
    {
        get
        {
            if (m_NoEventsUIRoot == null && UIRoot != null)
                m_NoEventsUIRoot = UIRoot.Find(UINodeName.NoEventsUIRootName);
            return m_NoEventsUIRoot;
        }
    }

    /// <summary>
    /// [普通层(第1层)]
    /// (1)普通界面,会被其他界面顶掉
    /// </summary>
    static private Transform m_NormalUIRoot;
    static public Transform NormalUIRoot
    {
        get
        {
            if (m_NormalUIRoot == null && UIRoot != null)
                m_NormalUIRoot = UIRoot.Find(UINodeName.NormalUIRootName);
            return m_NormalUIRoot;
        }
    }

    /// <summary>
    /// [中间层(第2层)]
    /// (1)弹出界面,不会被其他界面顶掉
    /// (2)指引界面(在普通层和中间层之上),T1/T2/T3等T节点下的界面不能进行指引
    /// </summary>
    static private Transform m_MiddleUIRoot;
    static public Transform MiddleUIRoot { get { return m_MiddleUIRoot; } }

    /// <summary>
    /// [T1(第3层)]
    /// (1)任务对话框界面
    /// (2)物品提示说明框
    /// (3)奖励面板
    /// (4)升级面板
    /// </summary>
    static private Transform m_TopUIRoot;
    static public Transform TopUIRoot { get { return m_TopUIRoot; } }

    /// <summary>
    /// [T2(第4层)]
    /// 特点：在UI之上
    /// (1)战斗力提升界面
    /// (2)飘字提示界面Float
    /// (3)弹出提示Toast
    /// </summary>
    static private Transform m_T2Root;
    static public Transform T2RootOfSpecial { get { return m_T2Root; } }

    /// <summary>
    /// [T3(第5层)]
    /// 特点：盖住其他所有界面(游戏界面)
    /// (1)Loading界面
    /// (2)PreLoading界面
    /// (3)断线重连框
    /// </summary>
    static private Transform m_T3Root;
    static public Transform T3RootOfSpecial { get { return m_T3Root; } }

    #endregion

    #region Canvas

    static private Canvas cs_NoEventsUIRoot;
    static private Canvas cs_NormalUIRoot;
    static private Canvas cs_MiddleUIRoot;
    static private Canvas cs_TopUIRoot;
    static private Canvas cs_T2RootOfSpecial;
    static private Canvas cs_T3RootOfSpecial;
    static private Canvas cs_T4RootOfSpecial;

    #endregion

    #region 外部调用

    public static bool Is2DCanvasRoot(Transform target)
    {
        if (target == NoEventsUIRoot
            || target == NormalUIRoot
            || target == MiddleUIRoot
            || target == TopUIRoot
            || target == T2RootOfSpecial
            || target == T3RootOfSpecial)
        {
            return true;
        }

        return false;
    }

    #endregion

    #endregion

    #region override

    public override void SysInitial()
    {
        Instance = this;
        InitUICanvas();
        base.SysInitial();
    }

    public override void SysFinalize()
    {
        base.SysFinalize();
        GameObject.Destroy(UIRoot);
    }

    #endregion

    private void InitUICanvas()
    {
        if (UIRoot == null)
            return;
        if (m_MiddleUIRoot != null)//表示已经创建过[重登陆]
            return;

        m_MiddleUIRoot = UGUITools.AddChild(UIRoot.gameObject, NormalUIRoot.gameObject, false, UINodeName.MiddleUIRootName).transform;
        m_TopUIRoot = UGUITools.AddChild(UIRoot.gameObject, NormalUIRoot.gameObject, false, UINodeName.TopUIRootName).transform;
        m_T2Root = UGUITools.AddChild(UIRoot.gameObject, NormalUIRoot.gameObject, false, UINodeName.T2RootName).transform;
        m_T3Root = UGUITools.AddChild(UIRoot.gameObject, NormalUIRoot.gameObject, false, UINodeName.T3RootName).transform;

        //Canvas2D
        SetUICanvas2Ds();

        //Canvas3D
        //SetUICanvas3D(CamerasMgr.CameraMain);
        //Show3DUI(true);

        //UICamera
        SetUICamera();
    }

    private void SetUICanvas2Ds()
    {
        //Canvas 2D0
        cs_NoEventsUIRoot = NoEventsUIRoot.GetComponent<Canvas>();
        if (cs_NoEventsUIRoot != null)
        {
            cs_NoEventsUIRoot.gameObject.SetActive(true);
            cs_NoEventsUIRoot.planeDistance = 100;
            cs_NoEventsUIRoot.sortingOrder = ConstOfDepth.ROOT_ORDER_NOEVENTS;
        }

        //Canvas 2D1
        cs_NormalUIRoot = NormalUIRoot.GetComponent<Canvas>();
        if (cs_NormalUIRoot != null)
        {
            cs_NormalUIRoot.gameObject.SetActive(true);
            cs_NormalUIRoot.planeDistance = 200;
            cs_NormalUIRoot.sortingOrder = ConstOfDepth.ROOT_ORDER_NORMAL;
        }

        //Canvas 2D2
        cs_MiddleUIRoot = MiddleUIRoot.GetComponent<Canvas>();
        if (cs_MiddleUIRoot != null)
        {
            cs_MiddleUIRoot.gameObject.SetActive(true);
            cs_MiddleUIRoot.planeDistance = 300;
            cs_MiddleUIRoot.sortingOrder = ConstOfDepth.ROOT_ORDER_MIDDLE;
        }

        //Canvas 2D3
        cs_TopUIRoot = TopUIRoot.GetComponent<Canvas>();
        if (cs_TopUIRoot != null)
        {
            cs_TopUIRoot.gameObject.SetActive(true);
            cs_TopUIRoot.planeDistance = 400;
            cs_TopUIRoot.sortingOrder = ConstOfDepth.ROOT_ORDER_T1;
        }

        //Canvas 2D4
        cs_T2RootOfSpecial = T2RootOfSpecial.GetComponent<Canvas>();
        if (cs_T2RootOfSpecial != null)
        {
            cs_T2RootOfSpecial.gameObject.SetActive(true);
            cs_T2RootOfSpecial.planeDistance = 500;
            cs_T2RootOfSpecial.sortingOrder = ConstOfDepth.ROOT_ORDER_T2;
        }

        //Canvas 2D5
        cs_T3RootOfSpecial = T3RootOfSpecial.GetComponent<Canvas>();
        if (cs_T3RootOfSpecial != null)
        {
            cs_T3RootOfSpecial.gameObject.SetActive(true);
            cs_T3RootOfSpecial.planeDistance = 600;
            cs_T3RootOfSpecial.sortingOrder = ConstOfDepth.ROOT_ORDER_T3;
        }
    }

    public GameObject OpenUI(string prefabName, Transform parent = null, bool forceShow = false)
    {
        GameObject ui = CResourceSys.Instance.LoadUI(prefabName);
        if (parent == null)
            parent = NormalUIRoot;
        UGUITools.SetParent(parent.gameObject, ui, forceShow);
        return ui;
    }

    static private void SetUICamera()
    {
        UICamera = UIRoot.Find(UINodeName.UICamera).GetComponent<Camera>();
        //float width = Screen.width / UIConst.UI_SIZE_WIDTH;
        //float height = Screen.height / UIConst.UI_SIZE_HEIGHT;
        //UIUtils.UIScaleFactor = Mathf.Min(width, height);

        //CamerasMgr.CameraUI.orthographicSize = 1.0f;//CamerasMgr.CameraUI.orthographicSize = 1280.0f;        
        //CamerasMgr.CameraUI.nearClipPlane = -2000.0f;
        //CamerasMgr.CameraUI.farClipPlane = 2000.0f;
    }
}

#region 节点名称
public class UINodeName
{
    public const string UIRootName = "UGUI_Root";
    public const string UICamera = "UICamera";
    public const string UIEventSystem = "EventSystem";

    public const string NoEventsUIRootName = "UICanvasNoEvents";
    public const string NormalUIRootName = "UICanvas";
    public const string MiddleUIRootName = "UICanvasMiddle";
    public const string TopUIRootName = "UICanvasTop";
    public const string T2RootName = "UICanvasT2OfSpeical";
    public const string T3RootName = "UICanvasT3OfSpecial";
}
#endregion

#region 深度管理

public class ConstOfDepth
{
    public const int DEPTH_INTERNVAL = 10;

    public const int ROOT_ORDER_NOEVENTS = 1000;
    public const int ROOT_ORDER_NORMAL = 2000;
    public const int ROOT_ORDER_MIDDLE = 3000;
    public const int ROOT_ORDER_T1 = 14000;
    public const int ROOT_ORDER_T2 = 15000;
    public const int ROOT_ORDER_T3 = 16000;
    public const int ROOT_ORDER_T4 = 17000;

    public const int GuideUILayer1 = 10001;
    public const int GuideWidgetNO = 10002;
    public const int GuideUILayer2 = 10003;
    public const int GuideButtonSkip = 10004;
}

#endregion