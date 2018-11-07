using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FOWSystem))]
public class FOWSystemEditor : Editor
{
    private TextAsset mAssPath;
    private short[,] mHeights;
    private int _heightRange;

    SerializedProperty mWorldSize;
    SerializedProperty mHeightRange;
    SerializedProperty mPixScale;
    SerializedProperty mUpdateFrequency;
    SerializedProperty mTextureBlendTime;
    SerializedProperty mBlurIterations;
    SerializedProperty mMapSize;

    public void OnEnable()
    {
        mWorldSize = serializedObject.FindProperty("worldSize");
        mHeightRange = serializedObject.FindProperty("heightRange");
        mPixScale = serializedObject.FindProperty("pixScale");
        mUpdateFrequency = serializedObject.FindProperty("updateFrequency");
        mTextureBlendTime = serializedObject.FindProperty("textureBlendTime");
        mBlurIterations = serializedObject.FindProperty("blurIterations");
        mMapSize = serializedObject.FindProperty("mapSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(mWorldSize, new GUIContent("地图刷新尺寸"));
        EditorGUILayout.PropertyField(mHeightRange, new GUIContent("地图高度范围"));
        EditorGUILayout.PropertyField(mPixScale, new GUIContent("像素缩放"));
        EditorGUILayout.PropertyField(mUpdateFrequency, new GUIContent("刷新间隔"));
        EditorGUILayout.PropertyField(mTextureBlendTime, new GUIContent("渐变时间"));
        EditorGUILayout.PropertyField(mBlurIterations, new GUIContent("模糊强度"));
        EditorGUILayout.PropertyField(mMapSize, new GUIContent("地图总尺寸"));
        serializedObject.ApplyModifiedProperties();

        FOWSystem fow = target as FOWSystem;
        _heightRange = Mathf.RoundToInt(fow.heightRange.y - fow.heightRange.x);
        mAssPath = EditorGUILayout.ObjectField("替换的文件", mAssPath, typeof(TextAsset), false) as TextAsset;

        if (GUILayout.Button("生成地图高度数据"))
        {
            string path = "D:\\mapheight.bytes";
            if (mAssPath != null)
            {
                path = AssetDatabase.GetAssetPath(mAssPath);
            }

            mHeights = GetMapHeightInfo(fow);
            WriteToFile(path, fow.mapSize, mHeights);
            Debug.Log("生成bytes成功:" + path);
        }

        if (GUILayout.Button("导出当且数据到JPG"))
        {
            mHeights = GetMapHeightInfo(fow);

            Color[] colors = new Color[fow.mapSize * fow.mapSize];
            for (int y = 0, sizeY = fow.mapSize; y < sizeY; y++)
            {
                int yw = y * sizeY;
                for (int x = 0, sizeX = fow.mapSize; x < sizeX; x++)
                {
                    int pix = mHeights[x, y];
                    if (pix < 8)
                    {
                        colors[x + yw] = Color.white;
                    }
                    else if (pix < 16)
                    {
                        colors[x + yw] = Color.green;
                    }
                    else if (pix < 24)
                    {
                        colors[x + yw] = Color.blue;
                    }
                    else
                    {
                        colors[x + yw] = Color.red;
                    }
                }
            }

            Texture2D tt2d = new Texture2D(fow.mapSize, fow.mapSize);
            tt2d.SetPixels(colors);
            tt2d.Apply();

            byte[] imageBytes = tt2d.EncodeToJPG();
            using (FileStream file = new FileStream("D:\\mapheight.jpg", FileMode.Create))
            {
                file.Write(imageBytes, 0, imageBytes.Length);
                file.Close();
                Debug.Log("生成jpg成功:D:\\mapheight.jpg");
            }
        }
    }

    private void WriteToFile(string fileName, int size, short[,] heights)
    {
        using (FileStream stream = new FileStream(fileName, FileMode.Create))
        {
            stream.Write(BitConverter.GetBytes(size), 0, 4);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    stream.Write(BitConverter.GetBytes(heights[x, y]), 0, 2);
                }
            }
        }
    }

    public short[,] GetMapHeightInfo(FOWSystem self)
    {
        Vector3 origin = self.transform.position - new Vector3(self.mapSize / 2f, 0, self.mapSize / 2f);
        Vector3 pos = origin;
        int heightRange = (int)(self.heightRange.y - self.heightRange.x);
        pos.y += heightRange;
        bool useSphereCast = self.raycastRadius > 0f;
        int textureSize = self.mapSize / self.pixScale;
        short[,] heights = new short[textureSize, textureSize];
        for (int z = 0; z < textureSize; ++z)
        {
            pos.z = origin.z + z * self.pixScale;

            for (int x = 0; x < textureSize; ++x)
            {
                pos.x = origin.x + x * self.pixScale;

                RaycastHit hit;

                if (useSphereCast)
                {
                    if (Physics.SphereCast(new Ray(pos, Vector3.down), self.raycastRadius, out hit, heightRange, self.raycastMask))
                    {
                        heights[x, z] = WorldToGridHeight(pos.y - hit.distance - self.raycastRadius);
                        continue;
                    }
                }
                else if (Physics.Raycast(new Ray(pos, Vector3.down), out hit, heightRange, self.raycastMask))
                {
                    heights[x, z] = WorldToGridHeight(pos.y - hit.distance);
                    continue;
                }
                heights[x, z] = 0;
            }
        }
        return heights;
    }

    public short WorldToGridHeight(float height)
    {
        int val = Mathf.RoundToInt(height / _heightRange * 255f);
        return (short)Mathf.Clamp(val, 0, 255);
    }
}
