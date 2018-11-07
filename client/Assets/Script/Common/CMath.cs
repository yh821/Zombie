using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMath
{
    public const float EPSILON = 0.0001f;
    public const int INT_MAX = 2147483647;
    public const uint UINT_MAX = 4294967295;

    //浮点数与0比较函数
    public static bool IsZero(float fValue)
    {
        return fValue <= EPSILON && fValue >= -EPSILON;
    }

    public static bool IsZero(Vector3 sVector)
    {
        return IsZero(sVector.x) && IsZero(sVector.y) && IsZero(sVector.z);
    }

    //两个浮点数比较
    public static bool Equals(float fLhs, float fRhs)
    {
        return (fLhs + EPSILON >= fRhs) && (fLhs - EPSILON <= fRhs);
    }

    public static float VectorLenghtSquare(Vector3 sVector)
    {
        return sVector.x * sVector.x + sVector.y * sVector.y + sVector.z * sVector.z;
    }

    public static float VectorLenghtSquareXz(Vector3 vec1, Vector3 vec2)
    {
        float fDeltaX = vec1.x - vec2.x;
        float fDeltaZ = vec1.z - vec2.z;
        return fDeltaX * fDeltaX + fDeltaZ * fDeltaZ;
    }

    //public static Vector3 s_deltaVec = new Vector3();
    public static float VectorLenghtSquareXz(float fDeltaX, float fDeltaZ)
    {
        return fDeltaX * fDeltaX + fDeltaZ * fDeltaZ;
    }

    public static float VectorLengthXz(Vector3 vec1, Vector3 vec2)
    {
        float fDeltaX = vec1.x - vec2.x;
        float fDeltaZ = vec1.z - vec2.z;

        return Mathf.Sqrt(fDeltaX * fDeltaX + fDeltaZ * fDeltaZ);
    }

    public static float VectorLengthXz(float fDeltaX, float fDeltaZ)
    {
        return Mathf.Sqrt(fDeltaX * fDeltaX + fDeltaZ * fDeltaZ);
    }

    public static float VectorLenghtSquareXy(Vector3 vec1, Vector3 vec2)
    {
        float fDeltaX = vec1.x - vec2.x;
        float fDeltaY = vec1.y - vec2.y;

        return fDeltaX * fDeltaX + fDeltaY * fDeltaY;
    }

    public static float VectorLenghtSquareXy(float fDeltaX, float fDeltaY)
    {
        return fDeltaX * fDeltaX + fDeltaY * fDeltaY;
    }

    private static Vector3 s_Dir = new Vector3();
    //优化作用域在函数内的朝向计算，可减少一次new Vector
    public static Vector3 GetDir(Vector3 vec1, Vector3 vec2)
    {
        s_Dir.x = vec1.x - vec2.x;
        s_Dir.y = 0.0f;// vec1.y - vec2.y;
        s_Dir.z = vec1.z - vec2.z;
        return s_Dir;
    }

    //判定一个点是否位于矩形内部
    //@sCenter矩形中心，sDir矩形方向
    public static bool CheckRectangleIntersectPos(Vector3 sCenter, Vector3 sDir, float fWidth, float fHeight, Vector3 sPos)
    {
        //推导链接:http://jingyan.baidu.com/article/2c8c281dfbf3dd0009252a7b.html
        //假设对图片上任意点(x,y)，绕一个坐标点(cx,cy)逆时针旋转a角度后的新的坐标设为(x0, y0)，有公式：
        //x0= (x - cx)*cos(a) - (y - cy)*sin(a) + cx ;
        //y0= (x - cx)*sin(a) + (y - cy)*cos(a) + cy ;

        //注意，当sDir与x轴大于180度的时候计算的是逆时针旋转的角度，小于180顺时针
        sDir.y = 0.0f;
        float fAngle = Vector3.Angle(sDir, Vector3.right);
        if (sDir.z > 0.0f)
        {
            //当计算的角度是顺时针角度需要转换为逆时针角度
            fAngle = 360 - fAngle;
        }

        float fRadian = Mathf.PI * fAngle / 180;
        float fCos = Mathf.Cos(fRadian);
        float fSin = Mathf.Sin(fRadian);
        float deltaX = sPos.x - sCenter.x;
        float deltaZ = sPos.z - sCenter.z;
        float fNewX = deltaX * fCos - deltaZ * fSin + sCenter.x;
        float fNewZ = deltaX * fSin + deltaZ * fCos + sCenter.z;
        float fHalfWidth = fWidth * 0.5f;
        float fHalfHeight = fHeight * 0.5f;

        if ((fNewX < sCenter.x - fHalfHeight) || (fNewX > sCenter.x + fHalfHeight) ||
            (fNewZ < sCenter.z - fHalfWidth) || (fNewZ > sCenter.z + fHalfWidth))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool CheckCicleIntersectLine(Vector3 sOrgPos, Vector3 sOffset, Vector3 sCenter, float fRadius, out Vector3 sCrossPoint)
    {
        //推导过程
        //http://blog.csdn.net/rabbit729/article/details/4285119

        sCrossPoint = sCenter;
        sCenter.y = 0.0f;
        sOrgPos.y = 0.0f;
        sOffset.y = 0.0f;

        float fDis = sOffset.magnitude;
        Vector3 d = sOffset.normalized;
        Vector3 e = sCenter - sOrgPos;
        float a = e.x * d.x + e.z * d.z;
        float f = a * a + fRadius * fRadius - e.sqrMagnitude;

        if (f >= 0.0f)
        {
            f = Mathf.Sqrt(f);
            float t1 = a - f;
            float t2 = a + f;
            if (Mathf.Abs(t1) > Mathf.Abs(t2))
            {
                float fTemp = t1;
                t1 = t2;
                t2 = fTemp;
            }
            if ((t1 >= 0.0f) && (t1 - fDis) <= 0.0f)
            {
                sCrossPoint.x = sOrgPos.x + t1 * d.x;
                sCrossPoint.z = sOrgPos.z + t1 * d.z;
                return true;
            }
            if ((t2 >= 0.0f) && (t2 - fDis) <= 0.0f)
            {
                sCrossPoint.x = sOrgPos.x + t2 * d.x;
                sCrossPoint.z = sOrgPos.z + t2 * d.z;
                return true;
            }

        }
        return false;
    }

    public static bool CheckCicleIntersectLine(Vector3 sOrgPos, ref Vector3 sOffset, Vector3 sCenter, float fRadius)
    {
        Vector3 sCrossPoint;
        if (CheckCicleIntersectPos(sOrgPos, fRadius, sCenter))
        {
            Vector3 sTempDir = sOrgPos - sCenter;
            if (sTempDir == Vector3.zero)
            {
                sTempDir = sOffset.normalized;
            }
            sOffset = sTempDir.normalized * 1000000.0f;
        }
        if (!CheckCicleIntersectLine(sOrgPos, sOffset, sCenter, fRadius, out sCrossPoint))
        {
            return false;
        }
        Vector3 sDir = sCrossPoint - sCenter;
        sDir.Normalize();
        sOffset = sCrossPoint + 0.05f * sDir - sOrgPos;
        return true;
    }

    public static bool CheckCicleIntersectPos(Vector3 vecCenter, float fRadius, Vector3 postion)
    {
        float fDistance = VectorLenghtSquareXz(vecCenter.x - postion.x, vecCenter.z - postion.z);
        return fDistance <= fRadius * fRadius;
    }

    public static bool CheckSectorIntersectPos(Vector3 sCenter, Vector3 forward, float fRadius, float fAngle, Vector3 sPos)
    {
        if (sPos == sCenter)
        {
            return true;
        }
        Vector3 dir = GetDir(sPos, sCenter);
        float tempAngle = Vector3.Angle(dir, forward);

        if (tempAngle <= fAngle / 2.0f)
        {
            float fDis = VectorLenghtSquareXz(sPos.x - sCenter.x, sPos.z - sCenter.z);
            if (fDis < fRadius * fRadius)
            {
                return true;
            }
        }

        return false;
    }

    public static bool CheckRectIntersectPos(Vector2 pos, Rect rect)
    {
        return pos.x >= rect.xMin && pos.x < rect.xMax && pos.y >= rect.yMin && pos.y < rect.yMax;
    }

    public static bool CheckCircleIntersectCircle(Vector3 sCenter, float fRadius, Vector3 sCenter2, float fRadius2)
    {
        float fDeltaX = sCenter.x - sCenter2.x;
        float fDeltaZ = sCenter.z - sCenter2.z;
        float fRadiusAdd = fRadius + fRadius2;
        return fDeltaX * fDeltaX + fDeltaZ * fDeltaZ < fRadiusAdd * fRadiusAdd;
    }

    public static bool CheckCircleIntersectSector(Vector3 sCircleCenter, float fCircleRadius, Vector3 sSectorCenter, Vector3 sSectorForward, float fSectorRadius, float fSectorAngle)
    {
        if (!CheckCircleIntersectCircle(sCircleCenter, fCircleRadius, sSectorCenter, fSectorRadius))
        {
            return false;
        }
        if (CheckSectorIntersectPos(sSectorCenter, sSectorForward, fSectorRadius, fSectorAngle, sCircleCenter))
        {
            return true;
        }

        // 根据夹角 theta/2 计算出旋转矩阵，并将向量v乘该旋转矩阵得出扇形两边的端点p3,p4
        float fRadian = fSectorAngle * 0.5f * Mathf.Deg2Rad;
        float fCos = Mathf.Cos(fRadian);
        float fSin = Mathf.Sin(fRadian);

        Vector3 sPos1 = Vector3.zero;
        Vector3 sPos2 = Vector3.zero;

        sPos1.x = sSectorForward.x * fCos - sSectorForward.z * fSin;
        sPos1.z = sSectorForward.x * fSin + sSectorForward.z * fCos;
        sPos2.x = sSectorForward.x * fCos + sSectorForward.z * fSin;
        sPos2.z = -sSectorForward.x * fSin + sSectorForward.z * fCos;

        Vector3 sOffset = sPos1.normalized * fSectorRadius;
        Vector3 sCrossPos;
        if (CheckCicleIntersectLine(sSectorCenter, sOffset, sCircleCenter, fCircleRadius, out sCrossPos))
        {
            return true;
        }

        sOffset = sPos2.normalized * fSectorRadius;
        if (CheckCicleIntersectLine(sSectorCenter, sOffset, sCircleCenter, fCircleRadius, out sCrossPos))
        {
            return true;
        }

        return false;
    }
}