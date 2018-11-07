namespace Attr
{
    public abstract class AttrExtend : IAttrExtend
    {
        public System.Action<AttrType, int, int> AttrIntChangedDelegate = null;
        public System.Action<AttrType, float, float> AttrFloatChangedDelegate = null;
        public System.Action<AttrType, long, long> AttrLongChangedDelegate = null;

        public void OnAttrChanged(AttrType type, int oldValue, int newValue)
        {
            if (null != AttrIntChangedDelegate)
                AttrIntChangedDelegate(type, oldValue, newValue);
        }

        public void OnAttrChanged(AttrType type, float oldValue, float newValue)
        {
            if (null != AttrFloatChangedDelegate)
                AttrFloatChangedDelegate(type, oldValue, newValue);
        }

        public void OnAttrChanged(AttrType type, long oldValue, long newValue)
        {
            if (null != AttrLongChangedDelegate)
                AttrLongChangedDelegate(type, oldValue, newValue);
        }

        public void SetValue(AttrType type, int value)
        {
            Formula<int> temp = GetFormula<int>(type);
            if (null != temp)
            {
                int oldVal = temp.Value;
                if (oldVal != value)
                {
                    temp.Value = value;
                    OnAttrChanged(type, oldVal, value);
                }
            }
        }

        public void SetValue(AttrType type, float value)
        {
            Formula<float> temp = GetFormula<float>(type);
            if (null != temp)
            {
                float oldVal = temp.Value;
                if (oldVal != value)
                {
                    temp.Value = value;
                    OnAttrChanged(type, oldVal, value);
                }
            }
        }

        public void SetValue(AttrType type, long value)
        {
            Formula<long> temp = GetFormula<long>(type);
            if (null != temp)
            {
                long oldVal = temp.Value;
                if (oldVal != value)
                {
                    temp.Value = value;
                    OnAttrChanged(type, oldVal, value);
                }
            }
        }

        protected virtual Formula<T> GetFormula<T>(AttrType type)
            where T : struct
        {
            UnityEngine.Debug.LogError("未找到属性值:" + type);
            return null;
        }
    }
}
