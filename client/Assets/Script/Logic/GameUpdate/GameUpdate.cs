using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameUpdateSys : CGameSystem
{
    public class MD5Info
    {
        public string strPath;
        public string strMD5;
    }

    private const string m_cLoadingUI = "UI/LoadingUI/LoadingUI";

    public override void SysInitial()
    {
        base.SysInitial();
    }

    public override bool SysEnter()
    {
        return true;
    }

    private void SetProgress(float fProgress)
    {
    }

    private string GetURLRoot()
    {
        string strURL = null;
#if UNITY_EDITOR 
        strURL = "File:///" + Application.dataPath; // 从PC本机加载，是三个杠
#else
        strURL = Application.streamingAssetsPath; // 手机上从本机加载
#endif
        return strURL;
    }

    private string GetURL(string strPath)
    {
        string strURL = GetURLRoot();

#if UNITY_EDITOR
        strPath = strPath.Replace("StreamingAssets/AndroidAssets", "StreamingAssets/AndroidAssetsCompress");
#elif UNITY_ANDROID
		strPath = strPath.Replace("StreamingAssets/AndroidAssets", "AndroidAssetsCompress");
#else
        strPath = strPath.Replace("StreamingAssets/IOSAssets", "IOSAssetsCompress");
#endif
        return strURL + "/" + strPath;
    }

    public override IEnumerator SysEnterCo()
    {
        SetProgress(0);

#if !UNITY_EDITOR || BUNDLE_MODE
#if UNITY_IPHONE
        string strMd5ConfigURL = GetURLRoot() + "/IOSAssetsMD5Config.xml";
#elif UNITY_EDITOR
		string strMd5ConfigURL = GetURLRoot() + "/StreamingAssets/AndroidAssetsMD5Config.xml";
#else
        string strMd5ConfigURL = GetURLRoot() + "/AndroidAssetsMD5Config.xml";
#endif

        WWW www = new WWW(strMd5ConfigURL);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("下载资源配置出错！");
            yield break;
        }

        List<MD5Info> lstFileMD5 = (List<MD5Info>)CUility.DeSerializerObjectFromBuff(www.bytes, typeof(List<MD5Info>));

        www.Dispose();

        for (int i = 0; i != lstFileMD5.Count; ++i)
        {
            MD5Info md5Info = lstFileMD5[i];
            string strLocalPath = Application.persistentDataPath + md5Info.strPath.Substring(md5Info.strPath.IndexOf('/'));
            bool bNeedUpdate = true;
            if (File.Exists(strLocalPath))
            {
                string strFileMd5 = CUility.GetFileMD5(strLocalPath);
                if (strFileMd5 == md5Info.strMD5)
                {
                    bNeedUpdate = false;
                }
            }
            if (bNeedUpdate)
            {
                string strURL = GetURL(md5Info.strPath);
                www = new WWW(strURL);
                yield return www;

                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError("下载资源出错！");
                    yield break;
                }

                byte[] arrData = GzipHelper.GzipDecompress(www.bytes);
                string strDirectory = strLocalPath.Substring(0, strLocalPath.LastIndexOf('/'));
                if (!Directory.Exists(strDirectory))
                {
                    Directory.CreateDirectory(strDirectory);
                }
                using (FileStream destFile = File.Open(strLocalPath, FileMode.Create, FileAccess.Write))
                {
                    destFile.Write(arrData, 0, arrData.Length);
                    destFile.Close();
                }
                www.Dispose();
            }
        }
        CResourceSys.Instance.GenManifestBundle();
#endif

        SetProgress(1.0f);

#if !UNITY_EDITOR
#if UNITY_IPHONE
        string strCompressDir = Application.dataPath + "/StreamingAssets/IOSAssetsCompress";
#else
        string strCompressDir = Application.dataPath + "/StreamingAssets/AndroidAssetsCompress";
#endif
        if (Directory.Exists(strCompressDir))
        {
            Directory.Delete(strCompressDir, true);
        }
#endif

        CGameRoot.SwitchToState(EStateType.GamePreLoading);

        yield return null;
    }
}
