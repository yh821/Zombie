using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attr
{
    public class Formula<T>
        where T : struct
    {
        public T Value { get; set; }

        protected string mTypeName = "";
        public string TypeName
        {
            get { return mTypeName; }
        }

        public Formula()
        {
            Value = default(T);
            mTypeName = typeof(T).Name;
        }

        public Formula(T initVal)
        {
            Value = initVal;
            mTypeName = typeof(T).Name;
        }
    }
}
