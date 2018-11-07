using System;
using GameData;

namespace Attr
{
    /// <summary>
    /// 标准属性
    /// </summary>
    public class ActorAttr : BaseAttr, IActorAttr
    {
        #region 属性
        public long Exp
        {
            get { return mExp.Value; }
            set { SetValue(AttrType.Exp, value); }
        }
        private Formula<long> mExp;

        public float MoveSpeed
        {
            get { return mMoveSpeed.Value; }
            set { SetValue(AttrType.MoveSpeed, value); }
        }
        private Formula<float> mMoveSpeed;

        public int Hp
        {
            get { return mHp.Value; }
            set { SetValue(AttrType.Hp, value); }
        }
        private Formula<int> mHp;

        public int HpLmt
        {
            get { return mHpLmt.Value; }
            set { SetValue(AttrType.HpLmt, value); }
        }
        private Formula<int> mHpLmt;

        public int Atk
        {
            get { return mAtk.Value; }
            set { SetValue(AttrType.Atk, value); }
        }
        private Formula<int> mAtk;

        public int Defence
        {
            get { return mDefence.Value; }
            set { SetValue(AttrType.Defence, value); }
        }
        private Formula<int> mDefence;
        #endregion

        #region 方法
        public ActorAttr()
        {
            mExp = new Formula<long>(0);
            mHp = new Formula<int>(0);
            mHpLmt = new Formula<int>(0);
            mMoveSpeed = new Formula<float>(0);
            mAtk = new Formula<int>(0);
            mDefence = new Formula<int>(0);
        }

        public ActorAttr(ResSkill data)
        {
            mExp = new Formula<long>(0);
            mHp = new Formula<int>(data.hp);
            mHpLmt = new Formula<int>(data.hp_lmt);
            mMoveSpeed = new Formula<float>(data.sub_speed);
            mAtk = new Formula<int>(data.attack);
            mDefence = new Formula<int>(data.defence);
        }

        protected override Formula<T> GetFormula<T>(AttrType type)
        {
            switch (type)
            {
                case AttrType.Exp:
                    return mExp as Formula<T>;
                case AttrType.MoveSpeed:
                    return mMoveSpeed as Formula<T>;
                case AttrType.Hp:
                    return mHp as Formula<T>;
                case AttrType.HpLmt:
                    return mHpLmt as Formula<T>;
                case AttrType.Atk:
                    return mAtk as Formula<T>;
                case AttrType.Defence:
                    return mDefence as Formula<T>;
            }

            return base.GetFormula<T>(type);
        }

        /// <summary>
        /// 增加属性
        /// </summary>
        public void Add(SkillAttr a)
        {
            this.MoveSpeed += a.MoveSpeed;
            this.HpLmt += a.HpLmt;
            this.Atk += a.Atk;
            this.Defence += a.Defence;
        }

        /// <summary>
        /// 减少属性
        /// </summary>
        public void Minus(SkillAttr m)
        {
            this.MoveSpeed -= m.MoveSpeed;
            this.HpLmt -= m.HpLmt;
            this.Atk -= m.Atk;
            this.Defence -= m.Defence;
        }

        /// <summary>
        /// 减少属性
        /// </summary>
        public void Copy(SkillAttr c)
        {
            this.MoveSpeed -= c.MoveSpeed;
            this.HpLmt = c.HpLmt;
            this.Atk = c.Atk;
            this.Defence = c.Defence;
        }
        #endregion
    }
}
