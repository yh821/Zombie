using System.Collections.Generic;
using UnityEngine;

public static class BeehiveCell
{
    public static readonly float COS30 = Mathf.Cos(Mathf.PI / 6);
    public static readonly float SQRT3 = Mathf.Sqrt(3);

    /// <summary>
    /// 蜂格边长
    /// </summary>
    public static float Radius { get; set; }

    /// <summary>
    /// 蜂巢层数
    /// </summary>
    public static int Tier { get; private set; }

    public static int Count { get { return mBeehiveList.Count; } }
    private static List<Vector3> mBeehiveList = new List<Vector3>();

    public static Vector3 AddCell(/*float offset = 0.25f*/)
    {
        Tier = CellTier(mBeehiveList.Count + 1);
        float aRadius = (Tier - 1) * Radius * COS30 * 2f;
        Vector3 pos = Vector3.zero;
        if (Tier == 1)
        {
            mBeehiveList.Add(pos);
        }
        else
        {
            int index = mBeehiveList.Count - CellCount(Tier - 1);
            int left_or_right = index % 2 == 0 ? 1 : -1;
            int index2 = (index + 1) / 2;
            float angle = 60f / (Tier - 1) * index2;
            float relaAngle = angle % 60f;
            if (relaAngle > 30)
                relaAngle = 60 - relaAngle;
            float dist = aRadius * Mathf.Cos(relaAngle * Mathf.Deg2Rad);
            pos = GetBeehivePos(Quaternion.AngleAxis(angle * left_or_right, Vector3.up) * (Vector3.right * dist));
            //pos += new Vector3(Random.Range(-offset, offset), 0, Random.Range(-offset, offset));
            mBeehiveList.Add(pos);
        }
        return pos;
    }

    public static void RemoveCell()
    {
        if (mBeehiveList.Count > 0)
            mBeehiveList.RemoveAt(mBeehiveList.Count - 1);
        Tier = CellTier(mBeehiveList.Count);
    }

    public static Vector3 GetBeehivePos(int index)
    {
        if (index < mBeehiveList.Count)
            return mBeehiveList[index];
        return Vector3.zero;
    }

    public static Vector3 GetBeehivePos(Vector3 pos)
    {
        Vector3[] points = new Vector3[3];

        float unit_x = UnitX(Radius);
        float unit_y = UnitY(Radius);

        int tierX = Mathf.FloorToInt(pos.x / unit_x);
        int tierY = Mathf.FloorToInt(pos.z / unit_y);

        points[0].x = unit_x * tierX;
        points[1].x = unit_x * (tierX + 0.5f);
        points[2].x = unit_x * (tierX + 1);

        if (tierY % 2 == 0)
        {
            points[0].z = points[2].z = unit_y * tierY;
            points[1].z = unit_y * (tierY + 1);
        }
        else
        {
            points[0].z = points[2].z = unit_y * (tierY + 1);
            points[1].z = unit_y * tierY;
        }

        int index = 0;
        float dist, mindist = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            dist = XUtility.DistanceNoY(pos, points[i]);
            if (dist < mindist)
            {
                mindist = dist;
                index = i;
            }
        }

        return points[index];
    }

    #region Formula;
    public static float UnitX(float a)
    {
        return a * SQRT3;
    }

    public static float UnitY(float a)
    {
        return a * 1.5f;
    }

    private static int CellTier(int count)
    {
        if (count <= 0)
            return 0;
        else if (count == 1)
            return 1;

        int index = 2;
        while (CellCount(index) < count)
        {
            index++;
        }
        return index;
    }

    public static int CellCount(int tier)
    {
        if (tier <= 0)
            return 0;
        else if (tier == 1)
            return 1;
        else
            return tier * (tier - 1) * 3 + 1;
    }
    #endregion
}