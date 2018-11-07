using System.Collections.Generic;
using System.Reflection;
using System;
using Mono.Xml;
using System.Security;
using AIRuntime;

//行为树加载
namespace AIMind
{
    public static class BTLoader
    {
        #region 变量

        private static readonly Dictionary<string, BTNode> Container = new Dictionary<string, BTNode>();

        #endregion

        #region 外部调用

        public static BTNode GetBehaviorTree(string id)
        {
            BTNode node = null;
            Container.TryGetValue(id, out node);
            return node;
        }

        public static BTNode LoadBtXml(string xmlName, string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlName) || string.IsNullOrEmpty(xmlContent))
                return null;

            BTNode node = TreeLoader(xmlContent);
            Container.Add(xmlName, node);
            return node;
        }

        #endregion

        #region 内部函数

        private static BTNode TreeLoader(string xmlstring)
        {
            SecurityParser securityParser = new SecurityParser();
            securityParser.LoadXml(xmlstring);
            SecurityElement se = securityParser.ToXml();
            SecurityElement sechild = (SecurityElement)se.Children[0];
            if (sechild.Attribute("Class").Equals("Brainiac.Design.Nodes.Behavior"))
            {
                BTNode rootNode = new BTNode();
                AddChildNode(rootNode, (SecurityElement)sechild.Children[0]);
                return rootNode;
            }
            else
            {
                return null;
            }
        }

        private static void AddChildNode(AIRuntime.Command node, SecurityElement securityElement, int condition = -1)
        {
            if (securityElement.Attribute("Class") != null)
            {
                Type t = Type.GetType(securityElement.Attribute("Class"));
                ConstructorInfo[] constructorInfos = t.GetConstructors();
                ConstructorInfo constructorInfo = constructorInfos[0];
                ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
                Type[] parameterType = new Type[parameterInfos.Length];
                for (int i = 0; i < parameterType.Length; i++)
                {
                    ParameterInfo parameterInfo = parameterInfos[i];
                    parameterType[i] = parameterInfo.ParameterType;
                }
                constructorInfo = t.GetConstructor(parameterType);

                object[] constructorParms = new object[parameterInfos.Length];

                //通过构造函数的参数名字查找xml的属性用以赋值
                for (int i = 0; i < parameterType.Length; i++)
                {
                    ParameterInfo pi = parameterInfos[i];
                    Type typeParameter = parameterType[i];

                    string value = (string)securityElement.Attributes[pi.Name];
                    if (typeParameter.Equals(typeof(string)))
                    {
                        constructorParms[i] = value;
                    }
                    else if (typeParameter.Equals(typeof(int)))
                    {
                        constructorParms[i] = int.Parse(value);
                    }
                    else if (typeParameter.Equals(typeof(float)))
                    {
                        constructorParms[i] = float.Parse(value);
                    }
                    else if (typeParameter.Equals(typeof(bool)))
                    {
                        if (value.Equals("True"))
                        {
                            constructorParms[i] = true;
                        }
                        else
                        {
                            constructorParms[i] = false;
                        }
                    }
                    //else if (typeParameter.Equals(typeof(AIMind.LogicalOperator)))
                    //{
                    //    string[] strEnumNums = value.ToString().Split(':');
                    //    constructorParms[i] = (AIMind.LogicalOperator)int.Parse(strEnumNums[1]);
                    //}
                    //else if (typeParameter.Equals(typeof(AIMind.ComparisonOperator)))
                    //{
                    //    string[] strEnumNums = value.ToString().Split(':');
                    //    constructorParms[i] = (AIMind.ComparisonOperator)int.Parse(strEnumNums[1]);
                    //}
                }

                Command obj = constructorInfo.Invoke(constructorParms) as Command;
                if (node is ConditionConnectors)
                {
                    if (condition == 1)
                        (node as ConditionConnectors).TrueChild = obj;
                    else if (condition == 0)
                        (node as ConditionConnectors).FalseChild = obj;
                }
                else
                    node.AddChild(obj);

                if (securityElement.Children != null)
                {
                    for (int i = 0; i < securityElement.Children.Count; i++)
                    {
                        AddChildNode(obj, (SecurityElement)securityElement.Children[i]);
                    }
                }
            }
            else if (securityElement.Attribute("Identifier") != null)
            {
                if (securityElement.Children != null)
                {
                    string value = securityElement.Attribute("Identifier");
                    if (value == "GenericChildren")
                    {
                        for (int i = 0; i < securityElement.Children.Count; i++)
                        {
                            AddChildNode(node, (SecurityElement)securityElement.Children[i]);
                        }
                    }
                    else if (value == "ConditionTrue")
                    {
                        AddChildNode(node, (SecurityElement)securityElement.Children[0], 1);
                    }
                    else if (value == "ConditionFalse")
                    {
                        AddChildNode(node, (SecurityElement)securityElement.Children[0], 0);
                    }
                }
            }
        }

        #endregion
    }
}