using UnityEngine;

public class ResetTrans : MonoBehaviour
{
    public int frame = 24;
    private int mDeltaFrame = 0;

    void Update()
    {
        mDeltaFrame++;
        if (mDeltaFrame > frame)
        {
            mDeltaFrame = 0;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }
    }
}
