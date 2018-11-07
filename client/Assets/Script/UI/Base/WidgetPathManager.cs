using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 控件路径管理器
/// 控件名->全路径
/// </summary>
public class WidgetPathManager
{
    #region 变量

    static private Dictionary<string, Dictionary<string, string>> mapWidgetPath 
        = new Dictionary<string, Dictionary<string, string>>();

    #endregion

    #region 解析控件字典

    #region 临时变量

    static private Transform m_myTransform;
    static private Dictionary<string, string> mWidgetToFullName;
    static private bool mNameUnique = false;//控件名是否要求唯一

    #endregion

    #region 外部调用

    /// <summary>
    /// 递归遍历控件，填充控件字典
    /// </summary>
    /// <param name="rootTransform"></param>
    static public Dictionary<string, string> FillFullNameData(string name, Transform rootTransform, bool name_unique)
    {
        if (mapWidgetPath.ContainsKey(name))
            return mapWidgetPath[name];

        m_myTransform = rootTransform;
        mNameUnique = name_unique;
        mWidgetToFullName = new Dictionary<string, string>();

        AddWigetToFullNameData(rootTransform.name, rootTransform.name);
        ToFillFullNameData(rootTransform);

        if (!string.IsNullOrEmpty(name))
            mapWidgetPath[name] = mWidgetToFullName;

        return mWidgetToFullName;
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 递归遍历控件，填充控件字典
    /// </summary>
    /// <param name="rootTransform"></param>
    static private void ToFillFullNameData(Transform rootTransform)
    {
        for (int i = 0; i < rootTransform.childCount; ++i)
        {
            AddWigetToFullNameData(rootTransform.GetChild(i).name, GetFullName(rootTransform.GetChild(i)));
            ToFillFullNameData(rootTransform.GetChild(i));
        }
    }

    /// <summary>
    /// 填充控件字典
    /// </summary>
    /// <param name="widgetName"></param>
    /// <param name="fullName"></param>
    static private void AddWigetToFullNameData(string widgetName, string fullName)
    {
        if (mNameUnique)
        {
            if (mWidgetToFullName.ContainsKey(widgetName))
                Debug.LogError(widgetName);
            else
                mWidgetToFullName.Add(widgetName, fullName);
        }
        else
        {
            mWidgetToFullName[widgetName] = fullName;
        }
    }

    /// <summary>
    /// 获取该控件的完整路径名
    /// </summary>
    /// <param name="currentTran"></param>
    /// <returns></returns>
    static private string GetFullName(Transform currentTran)
    {
        string fullName = "";

        while (currentTran != m_myTransform)
        {
            fullName = currentTran.name + fullName;
            if (currentTran.parent != m_myTransform)
                fullName = "/" + fullName;

            currentTran = currentTran.parent;
        }

        return fullName;
    }

    #endregion

    #endregion
}