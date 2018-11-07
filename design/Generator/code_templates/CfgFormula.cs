/**
 * @file    CfgFormula.cs
 * @author  <code template>
 * @brief   The code template encapsulation.
 */

using System;
using System.Collections.Generic;

using _CFT = GameData.CfgFormulaType;

namespace GameData
{
    /// <summary>
    /// The formula type enumeration.
    /// </summary>
    enum CfgFormulaType
    {
        Accum = 0,
        NegAmend = 1
    }

    /// <summary>
    /// The base formula template class encapsulation.
    /// </summary>
    /// <typeparam name="T">argument type</typeparam>
    public abstract class CfgFormula<T> where T : struct
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CfgFormula()
        {
        }

        /// <summary>
        /// Parameter constructor
        /// </summary>
        /// <param name="initVal">the init parameter</param>
        public CfgFormula(T initVal)
        {
        }

        /// <summary>
        /// Get the formula argument type
        /// </summary>
        public Type Ty
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Get the formula final value, equivalent to f(x1, x2, ...) value.
        /// </summary>
        public abstract T Val
        {
            get;
            set;
        }

        /// <summary>
        /// Add argument to formula
        /// </summary>
        /// <param name="arg">argument value</param>
        public abstract void AddArg(T arg);

        /// <summary>
        /// Remove one argument from formula
        /// </summary>
        /// <param name="arg">argument value</param>
        public abstract void RemoveArg(T arg);

        /// <summary>
        /// Convert value to double type
        /// </summary>
        /// <param name="value">value</param>
        protected static double TemplValToDouble(T value)
        {
            string tyName = value.GetType().Name;
            if (tyName == @"Int32")
                return (int)(object)value;
            else if (tyName == @"Single")
                return (float)(object)value;
            else
                return (double)(object)value;
        }

    }

    /// <summary>
    /// The Accumulation type formula class encapsulation
    /// </summary>
    public class CfgFormulaAccum<T> : CfgFormula<T> where T : struct
    {
        public CfgFormulaAccum()
        {
            val = default(T);
        }

        public CfgFormulaAccum(T initArg)
        {
            val = initArg;
        }

        public override T Val
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
            }
        }

        public override void AddArg(T arg)
        {
            double dbArg = TemplValToDouble(arg);
            double dbVal = TemplValToDouble(val);
            val = (T)Convert.ChangeType((object)(dbVal + dbArg), typeof(T));
        }

        public override void RemoveArg(T arg)
        {
            double dbArg = TemplValToDouble(arg);
            double dbVal = TemplValToDouble(val);
            val = (T)Convert.ChangeType((object)(dbVal - dbArg), typeof(T));
        }

        private T val;
    }

    /// <summary>
    /// The negative amend type formula class encapsulation.
    /// </summary>
    public class CfgFormulaNegAmend<T> : CfgFormula<T> where T : struct
    {
        public CfgFormulaNegAmend()
        {
            val = default(T);
        }

        public CfgFormulaNegAmend(T initArg)
        {
            args.Add(TemplValToDouble(initArg));
            this.Update();
        }

        public override T Val
        {
            get
            {
                return val;
            }
            set
            {
                args.Clear();
                this.AddArg(value);
            }
        }

        public override void AddArg(T arg)
        {
            args.Add(TemplValToDouble(arg));
            this.Update();
        }

        public override void RemoveArg(T arg)
        {
            double dbArg = TemplValToDouble(arg);
            for (int i = 0; i < args.Count; i++)
                if (args[i] == dbArg)
                {
                    args.RemoveAt(i);
                    this.Update();
                    break;
                }
        }

        private void Update()
        {
            double dbVal = 0.0;
            foreach (var arg in args)
            {
                if (arg >= 1.0)
                {
                    dbVal = 1.0;
                    break;
                }
                else if (arg == 0.0)
                {
                    continue;
                }

                dbVal += (1.0 / (1.0 - arg) - 1.0);
            }

            dbVal = 1.0 - 1.0 / (1 + dbVal);
            val = (T)Convert.ChangeType(dbVal, typeof(T));
        }

        private T val;
        private List<double> args = new List<double>();
    }

    /// <summary>
    /// The Formula builder class encapsulation.
    /// </summary>
    /// <typeparam name="T">argument type</typeparam>
    class CfgFormulaBuilder<T> where T : struct
    {
        /// <summary>
        /// Create formula with default argument value.
        /// </summary>
        /// <param name="ty">formula type</param>
        /// <returns></returns>
        public static CfgFormula<T> Build(CfgFormulaType ty)
        {
            return CfgFormulaBuilder<T>.Build(ty, default(T));
        }

        /// <summary>
        /// Create formula with init argument value.
        /// </summary>
        /// <param name="ty">formula type</param>
        /// <param name="initVal">init value</param>
        /// <returns></returns>
        public static CfgFormula<T> Build(CfgFormulaType ty, T initVal)
        {
            switch (ty)
            {
                case _CFT.Accum:
                    return new CfgFormulaAccum<T>(initVal);
                case _CFT.NegAmend:
                    return new CfgFormulaNegAmend<T>(initVal);
                default:
                    throw new Exception("Not supported CfgFormulaType: " + ty);
            }
        }
    }
}