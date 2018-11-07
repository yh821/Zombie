/**
 * @file    BuffCtrlAttrs.cs
 * @author  <code template>
 * @brief   The code template encapsulation.
 */

using System;

namespace GameData
{
    /// <summary>
    /// The Buff control attrs class encapsulation.
    /// </summary>
    public class BuffCtrlAttrs : object
    {
        /// <summary>
        /// 参数构造
        /// </summary>
        public BuffCtrlAttrs(int elemType)
        {
            this.elemType = elemType;
        }

        /// <summary>
        /// 取得元素类型(地,火,水,雷)
        /// </summary>
        public int ElemType 
        { 
            get { return elemType; } 
        }

        private int elemType = 0;

        # region 添加机率

        /// <summary>
        /// 添加机率加法修正
        /// </summary>
        public float AddProbAddAmend
        { 
            get { return addProbAddAmend; } 
            set { addProbAddAmend = value; } 
        }

        private float addProbAddAmend = 0.0f;

        /// <summary>
        /// 添加机率乘法正修正
        /// </summary>
        public float AddProbMulPosAmend
        {
            get { return addProbMulPosAmend; }
            set { addProbMulPosAmend = value; }
        }

        private float addProbMulPosAmend = 0.0f;

        /// <summary>
        /// 添加机率乘法负修正
        /// </summary>
        public float AddProbMulNegAmend
        {
            get { return addProbMulNegAmend; }
            set { addProbMulNegAmend = value; }
        }

        private float addProbMulNegAmend = 0.0f;

        # endregion

        # region 持续时间

        /// <summary>
        /// 持续时间加法修正(毫秒)
        /// </summary>
        public float DurTimeAddAmend
        {
            get { return durTimeAddAmend; }
            set { durTimeAddAmend = value; }
        }

        private float durTimeAddAmend = 0.0f;

        /// <summary>
        /// 持续时间乘法正修正
        /// </summary>
        public float DurTimeMulPosAmend
        {
            get { return durTimeMulPosAmend; }
            set { durTimeMulPosAmend = value; }
        }

        private float durTimeMulPosAmend = 0.0f;

        /// <summary>
        /// 持续时间乘法负修正
        /// </summary>
        public float DurTimeMulNegAmend
        {
            get { return durTimeMulNegAmend; }
            set { durTimeMulNegAmend = value; }
        }

        private float durTimeMulNegAmend = 0.0f;

        # endregion
    }
}

