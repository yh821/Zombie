#if UNITY_EDITOR || INNER_VER
#define ENABLE_LOG
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.Text;
using System.Security.Cryptography;
public delegate void VoidDelegate();
public class CUility 
{
	public static string GetFileMD5(string path)
    {
        using (FileStream get_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            MD5CryptoServiceProvider get_md5 = new MD5CryptoServiceProvider();
            byte[] hash_byte = get_md5.ComputeHash(get_file);
            get_file.Close();

            string result = System.BitConverter.ToString(hash_byte);
            result = result.Replace("-", "");
            return result;
        }
    }

    public static object DeSerializerObject(string path, Type type)
    {
        object obj = null;
        if (!File.Exists(path))
        {
            return null;
        }

        using (Stream streamFile = new FileStream(path, FileMode.Open))
        {
            if (streamFile == null)
            {
                Debug.LogError("OpenFile Erro");
                return obj;
            }

            try
            {
                if (streamFile != null)
                {
                    XmlSerializer xs = new XmlSerializer(type);
                    obj = xs.Deserialize(streamFile);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("SerializerObject Erro:" + ex.ToString());
            }
        }

        return obj;
    }

    public static object DeSerializerObjectFromBuff(byte[] buff, Type type)
    {
        object objRet = null;
        using (MemoryStream stream = new MemoryStream(buff))
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(type);
                objRet = xs.Deserialize(stream);
            }
            catch (System.Exception ex)
            {
                Debug.Log("Deserialize Error:" + ex.ToString());
            }
        }
        
        return objRet;
    }

    public static object DeSerializerObjectFromAssert(string path, Type type)
    {
        object objRet = null;
        TextAsset textFile = (TextAsset)Resources.Load(path);
        if (textFile == null)
        {
            return null;
        }

        using (MemoryStream stream = new MemoryStream(textFile.bytes))
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(type);
                objRet = xs.Deserialize(stream);
            }
            catch (System.Exception ex)
            {
                Debug.Log("Deserialize Error:" + ex.ToString());
            }
        }
        Resources.UnloadAsset(textFile);
        return objRet;
    }

    public static void SerializerObject(string path, object obj)
    {
        if (File.Exists(path))
        { // remove exist file to fix unexcept text
            File.Delete(path);
        }

        
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return;
            }            
        }

        using (Stream streamFile = new FileStream(path, FileMode.OpenOrCreate))
        {
            if (streamFile == null)
            {
                Debug.LogError("OpenFile Erro");
                return;
            }

            try
            {
                string strDirectory = Path.GetDirectoryName(path);
                if (!Directory.Exists(strDirectory))
                {
                    Directory.CreateDirectory(strDirectory);
                }

                XmlSerializer xs = new XmlSerializer(obj.GetType());
                TextWriter writer = new StreamWriter(streamFile, Encoding.UTF8);
                xs.Serialize(writer, obj);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("DeSerializerObject Erro:" + ex.ToString());
            }
        }
    }

    public static string PbBytesToString(byte[] bytes)
    {
        if (bytes == null)
        {
            return "";
        }
        
        char[] array = Encoding.UTF8.GetChars(bytes);
        int len = 0;
        for (; len < array.Length; ++len)
            if (array[len] == 0) break;

        return new string(array, 0, len);
    }

    public static byte[] StringToPbBytes(string pbStr)
    {
        if (string.IsNullOrEmpty(pbStr))
        {
            return null;
        }

        return Encoding.UTF8.GetBytes(pbStr);
    }

    public static void SetLayer(GameObject cObject, int nLayer)
    {
        SetLayer(cObject, nLayer, true);
    }

    public static void SetLayer(GameObject cObject, int nLayer, bool bChangeChildren)
    {
        cObject.layer = nLayer;

        if (bChangeChildren)
        {
            Transform[] cChildTransforms = cObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < cChildTransforms.Length; i++)
            {
                cChildTransforms[i].gameObject.layer = nLayer;
            }
        }
    }

    public static string MD5Hash(string path)
    {
        using (FileStream get_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            System.Security.Cryptography.MD5CryptoServiceProvider get_md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash_byte = get_md5.ComputeHash(get_file);
            get_file.Close();

            string result = System.BitConverter.ToString(hash_byte);
            result = result.Replace("-", "");
            return result;
        }
    }

    public static string MD5String(string source)
    {
        using (MD5 md5Hash = MD5.Create()) {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
            string result = System.BitConverter.ToString(data);
            result = result.Replace("-", "");
            return result;
        }
    }

    public static uint BytesToUInt(byte[] numInByte) {
        string strResId = CUility.PbBytesToString(numInByte);
        return Convert.ToUInt32(strResId);
    }

    public static int[] ParseArray(string strArray)
    {
        if (string.IsNullOrEmpty(strArray))
        {
            return null;
        }

        string[] arrSubStr = strArray.Split(',');
        int[] arrValue = new int[arrSubStr.Length]; 
        for (int i = 0; i != arrValue.Length; ++i)
        {
            if (!int.TryParse(arrSubStr[i], out arrValue[i]))
            {
                return null;
            }
        }
        return arrValue;
    }

    public static int[] ParseArray(byte[] bytes)
    {
        return ParseArray(PbBytesToString(bytes));
    }

    public static void Swap<T>(ref T lhs, ref T rhs)
    {
        T t = lhs; 
        lhs = rhs; 
        rhs = t; 
    }

    public static void Log(object message)
    {
#if ENABLE_LOG
        if (CGameRoot.Instance.mShowDebugLog)
        {
            Debug.Log(message);
        }
#endif
    }

    public static void Log(string str, params object[] message)
    {
#if ENABLE_LOG
        if (CGameRoot.Instance.mShowDebugLog)
        {
            Debug.Log(string.Format(str, message));
        }
#endif
    }

    public static void LogError(object message)
    {
#if ENABLE_LOG
        if (CGameRoot.Instance.mShowDebugLog)
        {
            Debug.LogError(message);
        }
#endif
    }

    public static void LogError(string strFormat, params object[] message)
    {
#if ENABLE_LOG
        if (CGameRoot.Instance.mShowDebugLog)
        {
            Debug.LogError(string.Format(strFormat, message));
        }
#endif
    }

    public static RaycastHit m_sRaycastHit;
    public static float GetMoveLayerHeight(Vector3 sOrigin)
    {
        return 0;
        //sOrigin.y = 10000.0f;
        //if (Physics.Raycast(sOrigin, -Vector3.up, out m_sRaycastHit, float.MaxValue, CLayerDefine.GroundMask))
        //{
        //    return m_sRaycastHit.point.y;
        //}
        //else
        //{
        //    Debug.LogError(sOrigin + "不处于地形范围!");
        //    return 0.0f;
        //}
    }
}