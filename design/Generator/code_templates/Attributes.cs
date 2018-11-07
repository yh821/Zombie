/**
 * @file    Attributes.cs
 * @brief   C# 代码模板,用于定义相关自定义属性.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GameData
{
    /// <summary>
    /// 配置indexed记录Attribute
    /// 用于记录配置中标记为indexed的字段名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class CfgIndicesAttribute : Attribute
    {
        public CfgIndicesAttribute(params string[] fieldNames)
        {
            _fieldNames = new List<string>(fieldNames);
        }
    
        /// <summary>
        /// 返回所有indexed的字段名
        /// </summary>
        public List<string> FieldNames
        {
            get { return _fieldNames; }
        }
    
        /// <summary>
        /// 确认指定MemberInfo是否indexed
        /// </summary>
        /// <param name="info">成员反射信息,必须是Property类型的MemberInfo</param>
        /// <returns>如果存在返回true,否则返回false</returns>
        public bool HasField(MemberInfo info)
        {
            if (info.MemberType != MemberTypes.Property)
                return false;
    
            return this.HasField(info.Name);
        }
    
        /// <summary>
        /// 确认指定FieldName是否indexed
        /// </summary>
        /// <param name="fieldName">Property名</param>
        /// <returns>如果存在返回true,否则返回false</returns>
        public bool HasField(string fieldName)
        {
            return _fieldNames.Contains(fieldName);
        }
    
        private List<string> _fieldNames;
    }
}
