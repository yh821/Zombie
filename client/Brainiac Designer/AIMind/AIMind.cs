using Brainiac.Design;

namespace AIMind
{
    public class AIMind:Plugin
    {
        public AIMind()
        {
            //AddResourceManager(Resources.ResourceManager);

            _fileManagers.Add(new FileManagerInfo(typeof(Brainiac.Design.FileManagers.FileManagerXML), "Behavior XML (*.xml)|*.xml", ".xml"));

            _exporters.Add(new ExporterInfo(typeof(Brainiac.Design.Exporters.ExporterCs), "C# Behavior Exporter (Assign Properties)", true, "C#Properties"));
            _exporters.Add(new ExporterInfo(typeof(Brainiac.Design.Exporters.ExporterCsUseParameters), "C# Behavior Exporter (Use Parameters)", true, "C#Parameters"));

            #region 修饰节点
            NodeGroup decoratorGroup = new NodeGroup("修饰节点", NodeIcon.Decorator, null);
            _nodeGroups.Add(decoratorGroup);

            decoratorGroup.Items.Add(typeof(Nodes.NotNode));
            decoratorGroup.Items.Add(typeof(Nodes.TrueNode));
            #endregion

            #region 控制节点
            NodeGroup controlGroup = new NodeGroup("控制节点"/*Resources.ControlNode*/, NodeIcon.Selector, null);
            _nodeGroups.Add(controlGroup);

            controlGroup.Items.Add(typeof(Nodes.SelectorNode));
            controlGroup.Items.Add(typeof(Nodes.SequenceNode));
            controlGroup.Items.Add(typeof(Nodes.ParallelNode));
            #endregion

            #region 条件节点
            NodeGroup conditionGroup = new NodeGroup("条件节点"/*Resources.ConditionNode*/, NodeIcon.Condition, null);
            _nodeGroups.Add(conditionGroup);

            conditionGroup.Items.Add(typeof(Nodes.CheckSelfNode));
            conditionGroup.Items.Add(typeof(Nodes.RandomNode));
            conditionGroup.Items.Add(typeof(Nodes.FindAttackTargetNode));
            conditionGroup.Items.Add(typeof(Nodes.FindFollowTargetNode));
            conditionGroup.Items.Add(typeof(Nodes.CheckAttackRangeNode));
            conditionGroup.Items.Add(typeof(Nodes.TooFarFollowTargetNode));
            #endregion

            #region 行为节点
            NodeGroup actionGroup = new NodeGroup("行为节点"/*Resources.ActionNode*/, NodeIcon.Action, null);
            _nodeGroups.Add(actionGroup);

            actionGroup.Items.Add(typeof(Nodes.MoveToPointNode));
            actionGroup.Items.Add(typeof(Nodes.StareBlanklyInThinkCountNode));
            actionGroup.Items.Add(typeof(Nodes.MoveByForwardInThinkCountNode));
            actionGroup.Items.Add(typeof(Nodes.TurnToRandomDirNode));
            actionGroup.Items.Add(typeof(Nodes.ChangeActionNode));
            actionGroup.Items.Add(typeof(Nodes.MoveToAttackTargetNode));
            actionGroup.Items.Add(typeof(Nodes.MoveToFollowTargetNode));
            actionGroup.Items.Add(typeof(Nodes.AttackTheTargetNode));
            actionGroup.Items.Add(typeof(Nodes.StopMoveToNode));
            #endregion
        }
    }
}
