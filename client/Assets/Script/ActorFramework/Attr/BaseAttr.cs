namespace Attr
{
    /// <summary>
    /// 基础属性
    /// </summary>
    public class BaseAttr : AttrExtend, IBaseAttr
    {
        #region 属性
        public int Level
        {
            get { return mLevel.Value; }
            set { SetValue(AttrType.Level, value); }
        }
        private Formula<int> mLevel;
        #endregion

        #region 方法
        public BaseAttr()
        {
            mLevel = new Formula<int>(0);
        }

        protected override Formula<T> GetFormula<T>(AttrType type)
        {
            switch (type)
            {
                case AttrType.Level:
                    return mLevel as Formula<T>;
            }

            return base.GetFormula<T>(type);
        }
        #endregion
    }
}
