using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 注意：如果子类写了OnDestroy方法，必须Override并且调用基类的OnDestroy方法(保证事件监听的移除)
/// </summary>
public class BaseUIBehaviour : BaseEventListener
{
    #region INIT

    protected bool m_bNameUnique = false;//控件名是否要求唯一
    protected bool m_bInited = false;//是否已经初始化 

    /// <summary>
    /// 面板初始化操作
    /// </summary>
    /// <param name="bcm">BindingContext.BindingContextMode</param>
    /// <param name="nameUnique">控件名是否要求唯一</param>
    protected void AwakeBase(bool nameUnique = false)
    {
        if (!m_bInited)
        {
            m_bInited = true;
            m_bNameUnique = nameUnique;
            DoWidgetToFullName();

            InitUI();

            base.AddListenersWhenAwake();
        }
    }

    //注意: m_myTransform不是脚本挂点的需要重置
    protected void ResetParent()
    {
        m_bInited = false;
    }

    protected override void Awake()
    {
        AwakeBase();
    }

    #endregion

    #region 外部接口

    //通过控件名查找控件
    public Transform FindTransform(string transformName)
    {
        if (m_widgetToFullName == null)
            DoWidgetToFullName();

        Transform find = null;
        if (m_widgetToFullName.ContainsKey(transformName))
        {
            find = m_myTransform.Find(m_widgetToFullName[transformName]);
        }

        if (find == null)
        {
            if (m_myTransform.name == transformName)
                find = m_myTransform;
        }

        return find;
    }

    public void FillTransform2Editor(Transform rootTransform)
    {
        m_myTransform = rootTransform;
        JustDoWidgetToFullName();
    }

    #endregion

    #region 控件字典

    protected Transform m_myTransform;
    private Dictionary<string, string> m_widgetToFullName;

    #endregion

    #region 数据绑定和事件绑定

    protected virtual void InitUI() { } // 初始化UI
    #endregion

    #region 内部方法

    /// <summary>
    /// 填充Widget路径列表
    /// </summary>
    /// <param name="resetWidgetToFullName">如果已经填充过, 是否需要重新填充</param>
    private void DoWidgetToFullName()
    {
        if (m_myTransform == null)
            m_myTransform = transform;

        JustDoWidgetToFullName();
    }

    private void JustDoWidgetToFullName()
    {
        m_widgetToFullName = WidgetPathManager.FillFullNameData(this.GetType().Name, m_myTransform, m_bNameUnique);
    }

    #endregion
}