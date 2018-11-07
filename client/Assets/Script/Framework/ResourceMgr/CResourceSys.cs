using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EResType
{
    ERole,
    EConfig,
    ETable,
    EShader,
    ETexture,
    EUI,
    EItem,
    EEffect,
    EBT,
}

public class CResourceSys : CGameSystem
{

    private static CResourceSys m_cInstance = null;
    public static CResourceSys Instance
    {
        get { return m_cInstance; }
    }

    private AssetBundleManifest m_cManifestBundle = null;

    private Dictionary<string, AssetBundle> m_dicAssetBundle = new Dictionary<string, AssetBundle>();
    private Dictionary<string, Object> m_dicPrefab = new Dictionary<string, Object>();

    public override void SysInitial()
    {
        m_cInstance = this;
        base.SysInitial();
    }

    public static string GetCacheRoot()
    {
#if UNITY_EDITOR && !BUNDLE_MODE
        return "Assets/ResourcesEx/";
#elif UNITY_IPHONE
        return Application.persistentDataPath + "/IOSAssets/";
#else
        return Application.persistentDataPath + "/AndroidAssets/";
#endif
    }

    public void GenManifestBundle()
    {
#if UNITY_IPHONE
        string strPath = Application.persistentDataPath + "/IOSAssets/IOSAssets";
#else
        string strPath = Application.persistentDataPath + "/AndroidAssets/AndroidAssets";
#endif
        AssetBundle cAssetBundle = AssetBundle.LoadFromFile(strPath);
        if (null != cAssetBundle)
        {
            m_cManifestBundle = (AssetBundleManifest)cAssetBundle.LoadAsset("AssetBundleManifest");
        }
    }

    public Object LoadItem(string strFileName)
    {
        string strPath = GetCachePath(EResType.EItem, strFileName);
        return LoadPrefab(strPath, strFileName);
    }

    public Object LoadEffect(string strFileName)
    {
        string strPath = GetCachePath(EResType.EEffect, strFileName);
        return LoadPrefab(strPath, strFileName);
    }

    public Object LoadRole(string strFileName)
    {
        string strPath = GetCachePath(EResType.ERole, strFileName);
        return LoadPrefab(strPath, strFileName);
    }

    public GameObject LoadUI(string strFileName)
    {
        string strPath = GetCachePath(EResType.EUI, strFileName);
        Object cPrefab = LoadPrefab(strPath, strFileName);
        if (null != cPrefab)
        {
            GameObject cObj = GameObject.Instantiate(cPrefab) as GameObject;
            return cObj;
        }
        return null;
    }

    public byte[] LoadTable(string strFileName)
    {
        string strFilePath = GetCachePath(EResType.ETable, strFileName);
        Object cObj = LoadPrefab(strFilePath, strFileName);
        if (null != cObj)
        {
            TextAsset cText = (TextAsset)cObj;
            return cText.bytes;
        }
        return null;
    }

    public byte[] LoadConfig(string strFileName)
    {
        string strFilePath = GetCachePath(EResType.EConfig, strFileName);
        Object cObj = LoadPrefab(strFilePath, strFileName);
        if (null != cObj)
        {
            TextAsset cText = (TextAsset)cObj;
            return cText.bytes;
        }
        return null;
    }

    public TextAsset LoadBehaviourTree(string btName)
    {
        string strFilePath = GetCachePath(EResType.EBT, btName);
        Object cObj = LoadPrefab(strFilePath, btName);
        if (null != cObj)
        {
            return (TextAsset)cObj;
        }
        return null;
    }

    public byte[] LoadShader(string strFileName)
    {
        string strFilePath = GetCachePath(EResType.EShader, strFileName);
        Object cObj = LoadPrefab(strFilePath, strFileName);
        if (null != cObj)
        {
            TextAsset cText = (TextAsset)cObj;
            return cText.bytes;
        }
        return null;
    }

    public Texture2D LoadTexture(string strFileName)
    {
        string strFilePath = GetCachePath(EResType.ETexture, strFileName);
        Object cObj = LoadPrefab(strFilePath, strFileName);
        if (null != cObj)
        {
            return (Texture2D)cObj;
        }
        return null;
    }

    public void UnloadTexture(string strFileName)
    {
        string strPath = GetCachePath(EResType.ETexture, strFileName);

        AssetBundle cAssetBundle = null;
        if (m_dicAssetBundle.TryGetValue(strPath, out cAssetBundle))
        {
            m_dicAssetBundle.Remove(strPath);
            cAssetBundle.Unload(true);
        }
        string strPrefabPath = strPath + "/" + strFileName;
        m_dicPrefab.Remove(strPrefabPath);
    }

    public byte[] LoadBuffFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                byte[] buffSrc = new byte[fs.Length];
                fs.Read(buffSrc, 0, (int)fs.Length);

                return buffSrc;
            }
        }
        return null;
    }

    public void UnLoadAllAsset()
    {
        foreach (AssetBundle cBundle in m_dicAssetBundle.Values)
        {
            cBundle.Unload(true);
        }
        m_dicPrefab.Clear();
        m_dicAssetBundle.Clear();
    }

    //public void ResetAnimCtrl(GameObject cObj, string strAnimCtrl)
    //{
    //#if UNITY_EDITOR && !BUNDLE_MODE
    //        strAnimCtrl += ".controller";
    //#endif
    //        string strPath = GetCachePath(EResType.EAnimCtrl, strAnimCtrl);

    //        Object cAnimCtrl = LoadPrefab(strPath, strAnimCtrl);
    //        if (null != cAnimCtrl)
    //        {
    //            Animator cAnimator = cObj.GetComponent<Animator>();
    //            if (null != cAnimator)
    //            {
    //                cAnimator.runtimeAnimatorController = (RuntimeAnimatorController)cAnimCtrl;
    //            }
    //        }
    //}

    private Object LoadPrefab(string strPath, string strFileName)
    {
#if !UNITY_EDITOR || BUNDLE_MODE
        if (null == m_cManifestBundle)
        {
            Debug.LogError("资源尚未初始化");
            return null;
        }

        strFileName = Path.GetFileNameWithoutExtension(strFileName);

#if UNITY_EDITOR && !BUNDLE_MODE
        int nPos = strPath.LastIndexOf('.');
        if (nPos > 0)
        {
            strPath = strPath.Substring(0, nPos);
        }
#endif

        string strPrefabPath = strPath + "/" + strFileName;
#else
        string strPrefabPath = strPath;
#endif

        Object cPrefab = null;
        if (m_dicPrefab.TryGetValue(strPrefabPath, out cPrefab))
        {
            return cPrefab;
        }

#if UNITY_EDITOR && !BUNDLE_MODE
        cPrefab = AssetDatabase.LoadAssetAtPath(strPath, typeof(Object));
#else
        if (File.Exists(strPath))
        {
            AssetBundle cAssetBundle = null;
            string strBundleName = strPath.Replace(GetCacheRoot(), "");
            string[] arrDependsName = m_cManifestBundle.GetAllDependencies(strBundleName);
            for (int i = 0; i < arrDependsName.Length; ++i)
            {
                string strDpBundlePath = GetCacheRoot() + arrDependsName[i];
                cAssetBundle = null;
                if (!m_dicAssetBundle.TryGetValue(strDpBundlePath, out cAssetBundle))
                {
                    cAssetBundle = AssetBundle.LoadFromFile(strDpBundlePath);
                    if (null != cAssetBundle)
                    {
                        m_dicAssetBundle.Add(strDpBundlePath, cAssetBundle);
                    }
                }
            }

            cAssetBundle = null;
            if (!m_dicAssetBundle.TryGetValue(strPath, out cAssetBundle))
            {
                cAssetBundle = AssetBundle.LoadFromFile(strPath);
                if (null != cAssetBundle)
                {
                    m_dicAssetBundle.Add(strPath, cAssetBundle);
                }
            }

            if (null != cAssetBundle)
            {
                cPrefab = cAssetBundle.LoadAsset(strFileName);
            }
    }
#endif

        if (null != cPrefab)
        {
            m_dicPrefab.Add(strPrefabPath, cPrefab);
        }

        return cPrefab;
    }

    private string GetCachePath(EResType eType, string strName)
    {
        string strCachePath = GetCacheRoot();

#if UNITY_EDITOR && !BUNDLE_MODE
        switch (eType)
        {
            case EResType.EConfig:
                return strCachePath + "config/gameconfig/" + strName;

            case EResType.ETable:
                return strCachePath + "table/" + strName;

            case EResType.EShader:
                return strCachePath + "shader/" + strName;

            case EResType.EUI:
                return strCachePath + "prefab/ui/" + strName + ".prefab";

            case EResType.ERole:
                return strCachePath + ("prefab/role/" + strName + ".prefab");

            case EResType.EItem:
                return strCachePath += ("prefab/item/" + strName + ".prefab");

            case EResType.EEffect:
                return strCachePath += ("prefab/effect/" + strName + ".prefab");

            case EResType.ETexture:
                return strCachePath += ("texture/" + strName);

            case EResType.EBT:
                return strCachePath + "behaviourtree/" + strName + ".xml";
        }
#else
		switch (eType) {
		case EResType.EConfig:
			return strCachePath + "config/gameconfig";

		case EResType.ETable:
			return strCachePath + "table";

		case EResType.EShader:
			return strCachePath + "shader";

		case EResType.EUI:
			return strCachePath + "prefab/ui";

		case EResType.ERole:
			return strCachePath += "prefab/role";

		case EResType.EItem:
			return strCachePath += "prefab/item";

		case EResType.EEffect:
			return strCachePath += "prefab/effect";

		case EResType.ETexture:
			return strCachePath += "texture";

        case EResType.EBT:
			return strCachePath + "behaviourtree";
		}
#endif
        return null;
    }
}