using UnityEngine;

public static class XUtility
{
    public static T AddUniqueComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }

    public static T AddPermanentGameObject<T>(string name) where T : Component
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = CGameRoot.Instance.transform;
        return obj.AddUniqueComponent<T>();
    }

    public static float DistanceNoY(Vector3 a, Vector3 b)
    {
        a.y = 0;
        b.y = 0;
        return Vector3.Distance(a, b);
    }

    public static float DistanceNoY(Vector3 a)
    {
        a.y = 0;
        return a.magnitude;
    }

    public static Vector3 DirectionNoY(Vector3 a)
    {
        a.y = 0;
        return a;
    }

    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - b.x * a.y;
    }

    public static bool IsInTriangle(Vector3 point, Vector3 A, Vector3 B, Vector3 C)
    {
        Vector2 pa = new Vector2(A.x - point.x, A.z - point.z);
        Vector2 pb = new Vector2(B.x - point.x, B.z - point.z);
        Vector2 pc = new Vector2(C.x - point.x, C.z - point.z);
        float t1 = XUtility.Cross(pa, pb);
        float t2 = XUtility.Cross(pb, pc);
        float t3 = XUtility.Cross(pc, pa);
        return t1 * t2 >= 0 && t1 * t3 >= 0;
    }

    public static void DrawCircle(Vector3 center, Vector3 forward, float radius, Color color, float duration = 0f)
    {
        Vector3 startPos = center;
        Vector3 endPos = center + forward * radius;
        if (duration > 0)
            Debug.DrawLine(startPos, endPos, color, duration);
        else
            Debug.DrawLine(startPos, endPos, color);
        for (int angle = 0; angle <= 360; angle += 15)
        {
            startPos = center + Quaternion.AngleAxis(angle, Vector3.up) * forward * radius;
            if (duration > 0)
                Debug.DrawLine(startPos, endPos, color, duration);
            else
                Debug.DrawLine(startPos, endPos, color);
            endPos = startPos;
        }
    }

    public static void DrawSemicircle(Vector3 center, Vector3 forward, float radius, Color color, float duration = 0f)
    {
        Vector3 endPos, startPos = center;
        for (int angle = -90; angle <= 90; angle += 15)
        {
            endPos = center + Quaternion.AngleAxis(angle, Vector3.up) * forward * radius;
            if (duration > 0)
                Debug.DrawLine(startPos, endPos, color, duration);
            else
                Debug.DrawLine(startPos, endPos, color, duration);
            startPos = endPos;
            if (angle == 90)
            {
                if (duration > 0)
                    Debug.DrawLine(startPos, center, color, duration);
                else
                    Debug.DrawLine(startPos, center, color, duration);
            }
        }
    }

    private static RaycastHit mRaycastHit;
    public static bool GetMoveLayerHeight(Vector3 xzPos, out Vector3 pos, float radius = 1f)
    {
        pos = xzPos;
        xzPos.y = 100;
        if (radius > 0)
        {
            if (Physics.SphereCast(new Ray(xzPos, Vector3.down), radius, out mRaycastHit))
            {
                pos = mRaycastHit.point;
                return true;
            }
        }
        else
        {
            if (Physics.Raycast(xzPos, Vector3.down, out mRaycastHit, float.MaxValue, LayerMask.NameToLayer("Default")))
            {
                pos = mRaycastHit.point;
                return true;
            }
        }
        return false;
    }
}
