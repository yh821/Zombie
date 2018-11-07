using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorData
{
    public float fRadius;
    public float fAngle;
}

public class SectorMeshCreator
{
    private static Dictionary<SectorData, Mesh> m_dicSector = new Dictionary<SectorData, Mesh>();

    public static Mesh CreateMesh(float fRadius, float fAngle)
    {
        foreach (KeyValuePair<SectorData, Mesh> kv in m_dicSector)
        {
            if (CMath.IsZero(kv.Key.fAngle - fAngle) && CMath.IsZero(kv.Key.fRadius - fRadius))
            {
                return kv.Value;
            }
        }
        Mesh cMesh = CreateImpl(fRadius, fAngle);
        SectorData cData = new SectorData();
        cData.fAngle = fAngle;
        cData.fRadius = fRadius;
        m_dicSector.Add(cData, cMesh);
        return cMesh;
    }

    private static Mesh CreateImpl(float fRadius, float fAngle)
    {
        int segments = (int)(fAngle / 2.0f);
        if (segments <= 0)
        {
            segments = 1;
        }

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[3 + segments - 1];
        vertices[0] = new Vector3(0, 0, 0);

        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = new Vector2(0.5f, 0.5f);//纹理的圆心在中心

        float angle = Mathf.Deg2Rad * fAngle;
        float startAngle = -Mathf.Deg2Rad * fAngle / 2.0f;
        float currAngle = angle + startAngle;
        float deltaAngle = angle / segments;
        for (int i = 1; i < vertices.Length; i++)
        {
            float y = Mathf.Cos(currAngle);
            float x = Mathf.Sin(currAngle);
            vertices[i] = new Vector3(x * fRadius, 0.0f, y * fRadius);
            //纹理的半径就是0.5, 圆心在0.5f, 0.5f的位置
            uvs[i] = new Vector2(x * 0.5f + 0.5f, y * 0.5f + 0.5f);
            currAngle -= deltaAngle;
        }

        int[] triangles = new int[segments * 3];
        for (int i = 0, vi = 1; i < triangles.Length; i += 3, vi++)
        {
            triangles[i] = 0;
            triangles[i + 1] = vi;
            triangles[i + 2] = vi + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }
}