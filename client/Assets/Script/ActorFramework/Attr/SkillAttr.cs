using System;
using GameData;

namespace Attr
{
    /// <summary>
    /// 标准属性
    /// </summary>
    public class SkillAttr : ActorAttr, ISkillAttr
    {
        #region 属性
        public int AtkDuration
        {
            get { return mAtkDuration.Value; }
            set { SetValue(AttrType.AtkDuration, value); }
        }
        private Formula<int> mAtkDuration;

        public float AttackDist
        {
            get { return mAttackDist.Value; }
            set { SetValue(AttrType.AttackDist, value); }
        }
        private Formula<float> mAttackDist;

        public float FindDist
        {
            get { return mFindDist.Value; }
            set { SetValue(AttrType.FindDist, value); }
        }
        private Formula<float> mFindDist;

        public float LostDist
        {
            get { return mLostDist.Value; }
            set { SetValue(AttrType.LostDist, value); }
        }
        private Formula<float> mLostDist;

        public int Food
        {
            get { return mFood.Value; }
            set { SetValue(AttrType.Food, value); }
        }
        private Formula<int> mFood;

        public int Bullet
        {
            get { return mBullet.Value; }
            set { SetValue(AttrType.Bullet, value); }
        }
        private Formula<int> mBullet;

        public int HitRatio
        {
            get { return mHitRatio.Value; }
            set { SetValue(AttrType.HitRatio, value); }
        }
        private Formula<int> mHitRatio;

        public int DodgeRatio
        {
            get { return mDodgeRatio.Value; }
            set { SetValue(AttrType.DodgeRatio, value); }
        }
        private Formula<int> mDodgeRatio;

        public int CritRatio
        {
            get { return mCritRatio.Value; }
            set { SetValue(AttrType.CritRatio, value); }
        }
        private Formula<int> mCritRatio;

        public int DecritRatio
        {
            get { return mDecritRatio.Value; }
            set { SetValue(AttrType.DecritRatio, value); }
        }
        private Formula<int> mDecritRatio;
        #endregion

        #region 方法
        public SkillAttr()
        {
            mAttackDist = new Formula<float>(0);
            mAtkDuration = new Formula<int>(0);
            mFindDist = new Formula<float>(0);
            mLostDist = new Formula<float>(0);
            mBullet = new Formula<int>(0);
            mFood = new Formula<int>(0);
            mHitRatio = new Formula<int>(0);
            mDodgeRatio = new Formula<int>(0);
            mCritRatio = new Formula<int>(0);
            mDecritRatio = new Formula<int>(0);
        }

        public SkillAttr(ResSkill data) : base(data)
        {
            mAttackDist = new Formula<float>(data.attack_dist);
            mAtkDuration = new Formula<int>(data.attack_duration);
            mFindDist = new Formula<float>(data.find_dist);
            mLostDist = new Formula<float>(data.lose_dist);
            mBullet = new Formula<int>(data.bullet);
            mFood = new Formula<int>(data.food);
            mHitRatio = new Formula<int>(data.hit_ratio);
            mDodgeRatio = new Formula<int>(data.dodge_ratio);
            mCritRatio = new Formula<int>(data.crit_ratio);
            mDecritRatio = new Formula<int>(data.decrit_ratio);
        }

        protected override Formula<T> GetFormula<T>(AttrType type)
        {
            switch (type)
            {
                case AttrType.AttackDist:
                    return mAttackDist as Formula<T>;
                case AttrType.AtkDuration:
                    return mAtkDuration as Formula<T>;
                case AttrType.FindDist:
                    return mFindDist as Formula<T>;
                case AttrType.LostDist:
                    return mLostDist as Formula<T>;
                case AttrType.Food:
                    return mFood as Formula<T>;
                case AttrType.Bullet:
                    return mBullet as Formula<T>;

                case AttrType.HitRatio:
                    return mHitRatio as Formula<T>;
                case AttrType.DodgeRatio:
                    return mDodgeRatio as Formula<T>;
                case AttrType.CritRatio:
                    return mCritRatio as Formula<T>;
                case AttrType.DecritRatio:
                    return mDecritRatio as Formula<T>;
            }

            return base.GetFormula<T>(type);
        }

        #region 外部调用
        /// <summary>
        /// 增加属性
        /// </summary>
        public void Add(SkillAttr a)
        {
            this.MoveSpeed += a.MoveSpeed;
            this.HpLmt += a.HpLmt;
            this.Atk += a.Atk;
            this.Defence += a.Defence;
            this.AttackDist += a.AttackDist;
            this.AtkDuration += a.AtkDuration;
            this.FindDist += a.FindDist;
            this.LostDist += a.LostDist;
            this.Food += a.Food;
            this.Bullet += a.Bullet;

            this.HitRatio += a.HitRatio;
            this.DodgeRatio += a.DodgeRatio;
            this.CritRatio += a.CritRatio;
            this.DecritRatio += a.DecritRatio;
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
            this.AttackDist -= m.AttackDist;
            this.AtkDuration -= m.AtkDuration;
            this.FindDist -= m.FindDist;
            this.LostDist -= m.LostDist;
            this.Food -= m.Food;
            this.Bullet -= m.Bullet;

            this.HitRatio -= m.HitRatio;
            this.DodgeRatio -= m.DodgeRatio;
            this.CritRatio -= m.CritRatio;
            this.DecritRatio -= m.DecritRatio;
        }

        /// <summary>
        /// 复制属性
        /// </summary>
        public void Copy(SkillAttr c)
        {
            this.MoveSpeed = c.MoveSpeed;
            this.HpLmt = c.HpLmt;
            this.Atk = c.Atk;
            this.Defence = c.Defence;
            this.AttackDist = c.AttackDist;
            this.AtkDuration = c.AtkDuration;
            this.FindDist = c.FindDist;
            this.LostDist = c.LostDist;
            this.Food = c.Food;
            this.Bullet = c.Bullet;

            this.HitRatio = c.HitRatio;
            this.DodgeRatio = c.DodgeRatio;
            this.CritRatio = c.CritRatio;
            this.DecritRatio = c.DecritRatio;
        }

        public void Clear()
        {
            this.MoveSpeed = 0;
            this.HpLmt = 0;
            this.Atk = 0;
            this.Defence = 0;
            this.AttackDist = 0;
            this.AtkDuration = 0;
            this.FindDist = 0;
            this.LostDist = 0;
            this.Food = 0;
            this.Bullet = 0;

            this.HitRatio = 0;
            this.DodgeRatio = 0;
            this.CritRatio = 0;
            this.DecritRatio = 0;
        }
        #endregion

        #endregion
    }
}
