using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProtoBuf;
using UnityEngine;

public class TableResMgr : CGameSystem
{
    public static TableResMgr Instance { get { return m_cTableResMgr; } }
    private static TableResMgr m_cTableResMgr;

    private List<Type> m_lstPreLoad = new List<Type>();

    public override void SysInitial()
    {
        m_cTableResMgr = this;
        base.SysInitial();
    }

    /// <summary>
    /// 预加载配置表
    /// 1.常用的可以预先加载
    /// 2.其他的用到再加载
    /// </summary>
    public void PreLoading()
    {

    }

    private void RegisterTable<T>()
        where T : class, new()
    {
        var t = typeof(DataReader<T>);
        var loadMethod = t.GetMethod("LoadData", BindingFlags.Public | BindingFlags.Static);
        if (m_lstPreLoad.Exists(e => e == t))
        {
            Debug.LogErrorFormat("注册了重复的表格: {0}", typeof(T).Name);
        }
        else
        {
            var data = (TextAsset)loadMethod.Invoke(null, null);
            if (data != null)
            {
                m_lstPreLoad.Add(t);
            }
        }
    }
}

/// <summary>
/// todo.建议在第一次进入主城前[采用多线程]预先统一初始化
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class DataReader<T>
    where T : class, new()
{
    #region 变量

    private static List<T> m_dataList;
    public static List<T> DataList
    {
        get
        {
            Init();
            return m_dataList;
        }
    }

    private static Dictionary<int, T> m_intDataMap = new Dictionary<int, T>();
    private static Dictionary<string, T> m_strDataMap = new Dictionary<string, T>();

    #endregion

    #region INIT

    /// <summary>
    /// 初始化加载指定配置表
    /// </summary>
    public static void Init()
    {
        if (m_dataList != null)
            return;//表示已经初始化, 直接返回

        var data = LoadData();
        if (data != null)
        {
            InitData(data);
            UnloadAsset();
        }
    }

    private static string GetFileName()
    {
        var dataType = typeof(T);
        var arr = dataType.ToString().Split('.');
        var fileName = arr[arr.Length - 1];
        return fileName;
    }

    private static string GetFilePath(string fileName)
    {
        return string.Format("cc_{0}.bytes", fileName);
    }

    public static byte[] LoadData()
    {
        if (m_dataList != null)
            return null;//表示已经初始化, 直接返回

        var fileName = GetFileName();

        var data = CResourceSys.Instance.LoadTable(GetFilePath(fileName));
        if (data == null)
        {
            Debug.LogError("加载 " + fileName + " 失败");
        }

        return data;
    }

    public static void UnloadAsset()
    {
        //================Unload====================
        //==========================================
    }

    public static void InitData(byte[] data)
    {
        Type dataType = typeof(T);
        string fileName = GetFileName();

        if (data == null)
        {
            Debug.LogError("加载 " + fileName + " 失败");
            return;
        }
        try
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                //需要消耗1.5MB的GC和90ms的开销
                m_dataList = Serializer.Deserialize<List<T>>(memoryStream);
            }
        }
        catch (ProtoException ex)
        {
            Debug.LogError("表格数据解析错误 " + fileName + ": " + ex);
            return;
        }

        PropertyInfo keyProp = dataType.GetProperties(BindingFlags.Instance | BindingFlags.Public)[0];
        if (keyProp.PropertyType == typeof(Int32) ||
            keyProp.PropertyType == typeof(UInt32) ||
            keyProp.PropertyType == typeof(String))
        {
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                if (keyProp.PropertyType == typeof(Int32) ||
                    keyProp.PropertyType == typeof(UInt32))
                {
                    int dataKey = Convert.ToInt32(keyProp.GetValue(m_dataList[i], null));

                    if (!m_intDataMap.ContainsKey(dataKey))
                        m_intDataMap.Add(dataKey, m_dataList[i]);
                }
                else if (keyProp.PropertyType == typeof(String))
                {
                    string dataKey = Convert.ToString(keyProp.GetValue(m_dataList[i], null));
                    if (!m_strDataMap.ContainsKey(dataKey))
                        m_strDataMap.Add(dataKey, m_dataList[i]);
                }
            }
        }
    }

    #endregion

    #region Get

    public static T Get(int key)
    {
        Init();

        if (m_intDataMap.ContainsKey(key))
            return m_intDataMap[key];

        Debug.LogError("此表  " + typeof(T) + " 没有包含为  key  " + key + "  对应的数据");
        return null;
    }

    public static T Get(string key)
    {
        Init();

        if (m_strDataMap.ContainsKey(key))
            return m_strDataMap[key];

        Debug.LogError("此表  " + typeof(T) + " 没有包含为  key  " + key + "  对应的数据");
        return null;
    }

    #endregion

    #region Contains

    public static bool Contains(int key)
    {
        Init();
        return m_intDataMap.ContainsKey(key);
    }

    public static bool Contains(string key)
    {
        Init();
        return m_strDataMap.ContainsKey(key);
    }

    #endregion
}