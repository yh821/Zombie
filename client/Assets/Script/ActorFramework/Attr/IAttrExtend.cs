using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attr
{
    interface IAttrExtend
    {
        void SetValue(AttrType type, int value);
        void SetValue(AttrType type, float value);
        void SetValue(AttrType type, long value);
        void OnAttrChanged(AttrType type, int oldValue, int newValue);
        void OnAttrChanged(AttrType type, float oldValue, float newValue);
        void OnAttrChanged(AttrType type, long oldValue, long newValue);
    }
}
