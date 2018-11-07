using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BundleParse
{
    public class BundleItem
    {
        public List<string> lstDir;
        public int nCount;
    }

    private Dictionary<string, BundleItem> m_dicBundleCount = new Dictionary<string, BundleItem>();
 
    public void Parse(string strPath)
    {
        string strAssetPath = "Assets/" + strPath;
        DirectoryInfo cFolder = new DirectoryInfo(strAssetPath);
        FileSystemInfo[] arrFiles = cFolder.GetFileSystemInfos();
        for (int i = 0; i < arrFiles.Length; i++)
        {
            FileSystemInfo cFileInfo = arrFiles[i];
            if (cFileInfo is DirectoryInfo)
            {
                string strTemp = cFileInfo.FullName.Replace('\\', '/');
                strPath = strTemp.Replace(Application.dataPath, "");
                strPath = strPath.Substring(1);
                Parse(strPath);
            }
            else if (!cFileInfo.Name.EndsWith(".meta"))
            {
                string strAsset = strAssetPath + "/" + cFileInfo.Name;
                string[] arrDpPaths = AssetDatabase.GetDependencies(strAsset);
                for (int j = 0; j != arrDpPaths.Length; ++j)
                {
                    AddRef(strAssetPath, arrDpPaths[j]);  
                }
            }
        }
    }

    public int GetCount(string strPath)
    {
        BundleItem cItem;
        m_dicBundleCount.TryGetValue(strPath, out cItem);
        if (null == cItem)
        {
            return 0;
        }
        else
        {
            return cItem.nCount;
        }
    }

    private void AddBundle(string strDir, string strPath)
    {
        BundleItem cItem;
        if (!m_dicBundleCount.TryGetValue(strPath, out cItem))
        {
            cItem = new BundleItem();
            cItem.nCount = 1;
            cItem.lstDir = new List<string>();
            cItem.lstDir.Add(strDir);
            m_dicBundleCount.Add(strPath, cItem);
        }
    }

    private void AddRef(string strDir, string strPath)
    {
        BundleItem cItem;
        if (m_dicBundleCount.TryGetValue(strPath, out cItem))
        {
            for (int i = 0; i != cItem.lstDir.Count; ++i)
            {
                if (cItem.lstDir[i] == strDir)
                {
                    return;
                }
            }
            cItem.nCount += 1;
            cItem.lstDir.Add(strDir);
        }
        else
        {
            AddBundle(strDir, strPath);
        }
    }
}
