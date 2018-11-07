using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class GameBuild
{
    public class MD5Info
    {
        public string strPath;
        public string strMD5;
    }

    private static string m_strPrefabResourcesDir = "ResourcesEx/Prefab";
    //private static string m_strLuaResourcesDir = "ResourcesEx/Lua";

    private static BundleParse m_cBundleParse;
    private static string[] m_arrAssetBundleNames;
    private static string m_strDirectoryName;
    private static string m_strCompressDirectoryPath;
    private static string m_strBundleDirectoryPath;

    private static List<MD5Info> m_lstOldFileMD5 = null;
    private static List<MD5Info> m_lstNewFileMD5 = null;

    //[MenuItem("Tools/AssetBundles/BuildPC")]
    //public static void BuildPC()
    //{
    //    m_strBundleDirectoryPath = Application.dataPath + "/AssetBundles/PCAssets";
    //    m_strCompressDirectoryPath = Application.dataPath + "/StreamingAssets/PCAssets";
    //    BuildAllAssetBundles(BuildTarget.StandaloneWindows);
    //}

    [MenuItem("Tools/AssetBundles/BuildAndroid")]
    public static void BuildAndroid()
    {
        m_strBundleDirectoryPath = Application.dataPath + "/AssetBundles/AndroidAssets";
        m_strCompressDirectoryPath = Application.dataPath + "/StreamingAssets/AndroidAssets";
        BuildAllAssetBundles(BuildTarget.Android);
    }

    //[MenuItem("Tools/AssetBundles/BuildIOS")]
    //public static void BuildIOS()
    //{
    //    m_strBundleDirectoryPath = Application.dataPath + "/AssetBundles/AndroidAssets";
    //    m_strCompressDirectoryPath = Application.dataPath + "/StreamingAssets/IOSAssets";
    //    BuildAllAssetBundles(BuildTarget.iOS);
    //}

    [MenuItem("Tools/AssetBundles/清理")]
    public static void ClearAndroid()
    {
        m_strBundleDirectoryPath = Application.dataPath + "/AssetBundles/";
        if (Directory.Exists(m_strBundleDirectoryPath))
        {
            Directory.Delete(m_strBundleDirectoryPath, true);
        }

        m_strCompressDirectoryPath = Application.dataPath + "/StreamingAssets/";
        if (Directory.Exists(m_strCompressDirectoryPath))
        {
            Directory.Delete(m_strCompressDirectoryPath, true);
        }

        AssetDatabase.Refresh();
    }

    private static void BuildAllAssetBundles(BuildTarget cPlatform)
    {
        GenAssetBundlesName();

        //lua要单独处理
        //string strLuaTempDir = Application.dataPath + "/" + m_strLuaResourcesDir + "Temp";
        //CopyLuaBytesFiles(Application.dataPath + "/" + m_strLuaResourcesDir, strLuaTempDir);

        AssetDatabase.Refresh();
        //IterFile(strLuaTempDir, GenLuaAssetBundlesNameImpl);

        if (!Directory.Exists(m_strBundleDirectoryPath))
        {
            Directory.CreateDirectory(m_strBundleDirectoryPath);
        }
        BuildPipeline.BuildAssetBundles(m_strBundleDirectoryPath, BuildAssetBundleOptions.UncompressedAssetBundle, cPlatform);

        AssetDatabase.RemoveUnusedAssetBundleNames();

        m_arrAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        m_strDirectoryName = m_strBundleDirectoryPath.Substring(m_strBundleDirectoryPath.LastIndexOf('/') + 1);

        IterFile(m_strBundleDirectoryPath, ClearRemainAssetBundles);
        GenCompressAssetBundles();

        AssetDatabase.Refresh();
        ClearRemainDirectory(m_strBundleDirectoryPath);

        //Directory.Delete(strLuaTempDir, true);

        Debug.Log("打包成功！");

        AssetDatabase.Refresh();
    }

    static void GenCompressAssetBundles()
    {
        m_lstOldFileMD5 = null;
        m_lstNewFileMD5 = null;
        string strMD5Cfg = m_strCompressDirectoryPath + "MD5Config.xml";
        if (File.Exists(strMD5Cfg))
        {
            m_lstOldFileMD5 = (List<MD5Info>)CUility.DeSerializerObject(strMD5Cfg, typeof(List<MD5Info>));
        }

        m_lstNewFileMD5 = new List<MD5Info>();

        IterFile(m_strBundleDirectoryPath, CompressAssetBundles);

        CUility.SerializerObject(strMD5Cfg, m_lstNewFileMD5);
    }

    //static void GenLuaAssetBundlesNameImpl(string strPath)
    //{
    //    string strAssetPath = strPath.Replace(Application.dataPath, "Assets");
    //    AssetImporter cAssetImporter = AssetImporter.GetAtPath(strAssetPath);
    //    if (null != cAssetImporter)
    //    {
    //        cAssetImporter.assetBundleName = "lua";
    //    }
    //}

    static void GenAssetBundlesName()
    {
        ClearAssetBundlesName();
        m_cBundleParse = new BundleParse();
        m_cBundleParse.Parse(m_strPrefabResourcesDir);

        string strAssetPath = "Assets/ResourcesEx";
        DirectoryInfo cFolder = new DirectoryInfo(strAssetPath);
        FileSystemInfo[] arrFiles = cFolder.GetFileSystemInfos();
        for (int i = 0; i != arrFiles.Length; ++i)
        {
            FileSystemInfo cFileInfo = arrFiles[i];
            if (cFileInfo is DirectoryInfo)
            {
                string strPath = cFileInfo.FullName.Replace('\\', '/');
                strPath = strPath.Replace(Application.dataPath, "");
                strPath = strPath.Substring(1);

                if (/*m_strLuaResourcesDir != strPath &&*/ m_strPrefabResourcesDir != strPath)
                {
                    GenAssetBundlesNameImpl(strPath, false);
                }
            }
        }

        GenAssetBundlesNameImpl(m_strPrefabResourcesDir, true);
    }

    static void GenAssetBundlesNameImpl(string strPath, bool bIsPerfab)
    {
        string strSubPath = strPath.Replace("ResourcesEx/", "");
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
                GenAssetBundlesNameImpl(strPath, bIsPerfab);
            }
            else if (!cFileInfo.Name.EndsWith(".meta"))
            {
                string strAsset = strAssetPath + "/" + cFileInfo.Name;
                string strAssetBundleName = strSubPath;
                AssetImporter cAssetImporter = null;
                string[] arrDpPaths = AssetDatabase.GetDependencies(strAsset);
                for (int j = 0; j != arrDpPaths.Length; ++j)
                {
                    if (!arrDpPaths[j].EndsWith(".cs"))
                    {
                        cAssetImporter = AssetImporter.GetAtPath(arrDpPaths[j]);
                        if (null != cAssetImporter && string.IsNullOrEmpty(cAssetImporter.assetBundleName))
                        {
                            if (m_cBundleParse.GetCount(arrDpPaths[j]) > 1 && bIsPerfab)
                            {
                                string strBundleName = Path.GetDirectoryName(arrDpPaths[j]);
                                strBundleName = strBundleName.Replace("Assets/Data/", "");
                                strBundleName = strBundleName.Replace("Assets/ResourcesEx/", "");
                                cAssetImporter.assetBundleName = "common/" + strBundleName + "_c";
                            }
                        }
                    }
                }

                cAssetImporter = AssetImporter.GetAtPath(strAssetPath);
                if (null != cAssetImporter && string.IsNullOrEmpty(cAssetImporter.assetBundleName))
                {
                    cAssetImporter.assetBundleName = strAssetBundleName;
                }
            }
        }
    }

    static void ClearAssetBundlesName()
    {
        string[] arrCurAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        int nLen = arrCurAssetBundleNames.Length;
        string[] arrOldAssetBundleNames = new string[nLen];

        for (int i = 0; i < nLen; ++i)
        {
            arrOldAssetBundleNames[i] = arrCurAssetBundleNames[i];
        }

        for (int j = 0; j < arrOldAssetBundleNames.Length; ++j)
        {
            AssetDatabase.RemoveAssetBundleName(arrOldAssetBundleNames[j], true);
        }
    }

    public delegate void DIterHander(string strPath);
    private static void IterFile(string path, DIterHander hander)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (string directory in Directory.GetDirectories(path))
        {
            string dir = directory.Replace("\\", "/");

            //if (dir.EndsWith(".svn")) continue;

            IterFile(dir, hander);
        }

        foreach (string filePathOri in Directory.GetFiles(path))
        {
            string filePath = filePathOri.Replace('\\', '/');
            if (filePath.EndsWith(".meta") || filePath.EndsWith(".manifest")) continue;

            if (hander != null)
            {
                hander(filePath);
            }
        }
    }

    private static void ClearRemainDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (string directory in Directory.GetDirectories(path))
        {
            string dir = directory.Replace("\\", "/");

            //if (dir.EndsWith(".svn")) continue;

            ClearRemainDirectory(dir);
        }

        string strDir = path;
        if (Directory.GetFiles(strDir).Length <= 0)
        {
            Directory.Delete(strDir);
            File.Delete(strDir + ".meta");
        }

        strDir = GetCompressPath(path);
        if (Directory.GetFiles(strDir).Length <= 0)
        {
            Directory.Delete(strDir);
            File.Delete(strDir + ".meta");
        }
    }

    private static void ClearRemainAssetBundles(string strPath)
    {
        string strBundleName = strPath.Replace(m_strBundleDirectoryPath, "");
        strBundleName = strBundleName.Substring(1);
        if (HaveAssetBundleName(strBundleName))
        {
            return;
        }
        if (File.Exists(strPath))
        {
            File.Delete(strPath);
        }

        string strNewPath = strPath + ".meta";
        if (File.Exists(strPath))
        {
            File.Delete(strPath);
        }

        strNewPath = strPath + ".manifest";
        if (File.Exists(strNewPath))
        {
            File.Delete(strNewPath);
        }

        strNewPath += ".meta";
        if (File.Exists(strNewPath))
        {
            File.Delete(strNewPath);
        }

        string strCompress = GetCompressPath(strPath);
        if (File.Exists(strCompress))
        {
            File.Delete(strCompress);
        }
        strCompress += ".meta";
        if (File.Exists(strCompress))
        {
            File.Delete(strCompress);
        }
    }

    private static bool HaveAssetBundleName(string strName)
    {
        if (null == m_arrAssetBundleNames || strName == m_strDirectoryName)
        {
            return true;
        }

        for (int i = 0; i != m_arrAssetBundleNames.Length; ++i)
        {
            if (strName == m_arrAssetBundleNames[i])
            {
                return true;
            }
        }
        return false;
    }

    private static string GetCompressPath(string strPath)
    {
        return strPath.Replace(m_strBundleDirectoryPath, m_strCompressDirectoryPath + "Compress");
    }

    private static void CompressAssetBundles(string strPath)
    {
        string strMD5Path = strPath.Replace(Application.dataPath, "");
        strMD5Path = strMD5Path.Substring(1);

        string strOldMD5 = GetBundleMD5(strMD5Path);
        string strNewMD5 = CUility.GetFileMD5(strPath);
        if (null == strOldMD5 || strOldMD5 != strNewMD5)
        {
            string strCompress = GetCompressPath(strPath);
            string strDirectory = strCompress.Substring(0, strCompress.LastIndexOf('/'));
            if (!Directory.Exists(strDirectory))
            {
                Directory.CreateDirectory(strDirectory);
            }
            GzipHelper.GZipFile(strPath, strCompress);
        }

        MD5Info cMD5 = new MD5Info();
        cMD5.strMD5 = strNewMD5;
        cMD5.strPath = strMD5Path.Replace("AssetBundles", "StreamingAssets");

        m_lstNewFileMD5.Add(cMD5);
    }

    private static string GetBundleMD5(string strPath)
    {
        if (null == m_lstOldFileMD5)
        {
            return null;
        }
        else
        {
            for (int i = 0; i != m_lstOldFileMD5.Count; ++i)
            {
                MD5Info cMD5Info = m_lstOldFileMD5[i];
                if (cMD5Info.strPath == strPath)
                {
                    return cMD5Info.strMD5;
                }
            }
        }
        return null;
    }

    static void CopyLuaBytesFiles(string sourceDir, string destDir, bool appendext = true, string searchPattern = "*.lua", SearchOption option = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(sourceDir))
        {
            return;
        }

        string[] files = Directory.GetFiles(sourceDir, searchPattern, option);
        int len = sourceDir.Length;

        if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
        {
            --len;
        }

        for (int i = 0; i < files.Length; i++)
        {
            string str = files[i].Remove(0, len);
            string dest = destDir + "/" + str;
            if (appendext) dest += ".bytes";
            string dir = Path.GetDirectoryName(dest);
            Directory.CreateDirectory(dir);
            File.Copy(files[i], dest, true);
        }
    }
}