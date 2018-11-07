////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2009, Daniel Kollmann
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted
// provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice, this list of conditions
//   and the following disclaimer.
//
// - Redistributions in binary form must reproduce the above copyright notice, this list of
//   conditions and the following disclaimer in the documentation and/or other materials provided
//   with the distribution.
//
// - Neither the name of Daniel Kollmann nor the names of its contributors may be used to endorse
//   or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Brainiac.Design.Attributes;
using Brainiac.Design.Properties;
using Brainiac.Design.Attachments.Overrides;

namespace Brainiac.Design.Nodes
{
	/// <summary>
	/// This is the class for all nodes which are part of a behaviour tree and are not view data.
	/// </summary>
	public abstract partial class Node : BaseNode, NodeTag.DefaultObject, ICloneable
	{
		/// <summary>
		/// Add a new child but the behaviour does not need to be saved.
		/// Used for collapsed referenced behaviours which show the behaviours they reference.
		/// </summary>
		/// <param name="connector">The connector the node will be added to. Use null for default connector.</param>
		/// <param name="node">The node you want to append.</param>
		/// <returns>Returns true if the child could be added.</returns>
		public virtual bool AddChildNotModified(Connector connector, Node node)
		{
			Debug.Check(connector !=null && _children.HasConnector(connector));

			if(!connector.AcceptsChild(node.GetType()))
			{
				throw new Exception(Resources.ExceptionNodeHasTooManyChildren);
			}

			if(!connector.AddChild(node))
			{
				return false;
			}

			node._parent= this;

			node.CopyWasModifiedFromParent(this);

			return true;
		}

		/// <summary>
		/// Add a new child node.
		/// </summary>
		/// <param name="connector">The connector the node will be added to. Use null for default connector.</param>
		/// <param name="node">The node you want to append.</param>
		/// <returns>Returns true if the child could be added.</returns>
		public virtual bool AddChild(Connector connector, Node node)
		{
			if(!AddChildNotModified(connector, node))
				return false;

			// behaviours must be saved
			BehaviorWasModified();

			return true;
		}

		/// <summary>
		/// Add a new child node.
		/// </summary>
		/// <param name="connector">The connector the node will be added to. Use null for default connector.</param>
		/// <param name="node">The node you want to append.</param>
		/// <param name="index">The index of the new node.</param>
		/// <returns>Returns true if the child could be added.</returns>
		public virtual bool AddChild(Connector connector, Node node, int index)
		{
			Debug.Check(connector !=null && _children.HasConnector(connector));

			if(!connector.AcceptsChild(node.GetType()))
			{
				throw new Exception(Resources.ExceptionNodeHasTooManyChildren);
			}

			if(!connector.AddChild(node, index))
			{
				return false;
			}

			node._parent= this;

			BehaviorWasModified();

			return true;
		}

		/// <summary>
		/// Removes a child node.
		/// </summary>
		/// <param name="connector">The connector the child is attached to.</param>
		/// <param name="node">The child node we want to remove.</param>
		public virtual void RemoveChild(Connector connector, Node node)
		{
			Debug.Check(connector !=null && _children.HasConnector(connector));

			if(!connector.RemoveChild(node))
				throw new Exception(Resources.ExceptionNodeIsNoChild);

			node._parent= null;

			BehaviorWasModified();
		}

		/// <summary>
		/// Determines if an attachment of a certain type is aceepted by this node or not.
		/// </summary>
		/// <param name="type">The type of the attachment we want to add.</param>
		/// <returns>Returns if the attachment may be added or not</returns>
		public virtual bool AcceptsAttachment(Type type)
		{
			return true;
		}

		protected List<Attachments.Attachment> _attachments;

		public IList<Attachments.Attachment> Attachments
		{
			get { return _attachments.AsReadOnly(); }
		}

		public virtual void AddAttachment(Attachments.Attachment attach)
		{
			_attachments.Add(attach);
		}

		public virtual void RemoveAttachment(Attachments.Attachment attach)
		{
			_attachments.Remove(attach);
		}

		protected string _exportName= string.Empty;

		/// <summary>
		/// The name of the node used for the export process.
		/// </summary>
		public virtual string ExportName
		{
			get { return _exportName; }
		}

		protected string _label;

		/// <summary>
		/// The label shown of the node.
		/// </summary>
		public string Label
		{
			get { return _label; }
			set { _label= value; }
		}

		protected readonly string _description;

		/// <summary>
		/// The description of this node.
		/// </summary>
		public string Description
		{
			get { return /*Resources.ResourceManager.GetString(*/_description/*, Resources.Culture)*/; }
		}

		/// <summary>
		/// Creates a new behaviour node.
		/// </summary>
		/// <param name="label">The label of the behaviour node.</param>
		/// <returns>Returns the created behaviour node.</returns>
		public static BehaviorNode CreateBehaviorNode(string label)
		{
			BehaviorNode node= (BehaviorNode)Plugin.BehaviorNodeType.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[] { label });

			if(node ==null)
				throw new Exception(Resources.ExceptionMissingNodeConstructor);

			return node;
		}

		/// <summary>
		/// Creates a new referenced behaviour node.
		/// </summary>
		/// <param name="rootBehavior">The behaviour we are adding the reference to.</param>
		/// <param name="referencedBehavior">The behaviour we are referencing.</param>
		/// <returns>Returns the created referenced behaviour node.</returns>
		public static ReferencedBehaviorNode CreateReferencedBehaviorNode(BehaviorNode rootBehavior, BehaviorNode referencedBehavior)
		{
			ReferencedBehaviorNode node= (ReferencedBehaviorNode)Plugin.ReferencedBehaviorNodeType.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[] { rootBehavior, referencedBehavior });

			if(node ==null)
				throw new Exception(Resources.ExceptionMissingNodeConstructor);

			return node;
		}

		/// <summary>
		/// Creates a node from a given type.
		/// </summary>
		/// <param name="type">The type we want to create a node of.</param>
		/// <returns>Returns the created node.</returns>
		public static Node Create(Type type)
		{
			Debug.Check(type !=null);

			// use the type overrides when set
			if(type ==typeof(BehaviorNode))
				type= Plugin.BehaviorNodeType;
			else if(type ==typeof(ReferencedBehaviorNode))
				type= Plugin.ReferencedBehaviorNodeType;

			Debug.Check(type !=null);
			Debug.Check(!type.IsAbstract);

			Node node= (Node)type.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[0]);

			if(node ==null)
				throw new Exception(Resources.ExceptionMissingNodeConstructor);

			return node;
		}

		protected bool _saveChildren= true;

		/// <summary>
		/// Determines if the children of this node will be saved. Required for referenced behaviours.
		/// </summary>
		public bool SaveChildren
		{
			get { return _saveChildren; }
		}

		/// <summary>
		/// The name of the class we want to use for the exporter. This is usually the implemented node of the game.
		/// </summary>
		public virtual string ExportClass
		{
			get { return GetType().FullName; }
		}

		private Comment _comment;

		/// <summary>
		/// The comment object of the node.
		/// </summary>
		public Comment CommentObject
		{
			get { return _comment; }
		}

		/// <summary>
		/// The text of the comment shown for the node and its children.
		/// </summary>
		[DesignerString("NodeCommentText", "NodeCommentTextDesc", "CategoryComment", DesignerProperty.DisplayMode.NoDisplay, 10, DesignerProperty.DesignerFlags.NoExport|DesignerProperty.DesignerFlags.NoSave)]
		public string CommentText
		{
			get { return _comment ==null ? string.Empty : _comment.Text; }

			set
			{
				string str= value.Trim();

				if(str.Length <1)
				{
					_comment= null;
				}
				else
				{
					if(_comment ==null)
						_comment= new Comment(str);
					else _comment.Text= str;
				}
			}
		}

		/// <summary>
		/// The color of the comment shown for the node and its children.
		/// </summary>
		[DesignerEnum("NodeCommentBackground", "NodeCommentBackgroundDesc", "CategoryComment", DesignerProperty.DisplayMode.NoDisplay, 20, DesignerProperty.DesignerFlags.NoExport|DesignerProperty.DesignerFlags.NoSave, null)]
		public CommentColor CommentBackground
		{
			get { return _comment ==null ? CommentColor.NoColor : _comment.Background; }

			set
			{
				if(_comment !=null)
					_comment.Background= value;
			}
		}

		private int _version;

		/// <summary>
		/// The version of this node. Used to update nodes when structure changes.
		/// </summary>
		[DesignerInteger("NodeVersion", "NodeVersionDesc", "CategoryVersion", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.ReadOnly|DesignerProperty.DesignerFlags.NoExport, null, int.MinValue, int.MaxValue, 1, null)]
		public int Version
		{
			get { return _version; }
			set { _version= value; }
		}

		/// <summary>
		/// The version of this node. Used to update nodes when structure changes.
		/// </summary>
		[DesignerInteger("NodeClassVersion", "NodeClassVersionDesc", "CategoryVersion", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.ReadOnly|DesignerProperty.DesignerFlags.NoExport|DesignerProperty.DesignerFlags.NoSave, null, int.MinValue, int.MaxValue, 1, null)]
		public int ClassVersion
		{
			get { return GetClassVersion(); }
		}

		/// <summary>
		/// Returns the current version of the class.
		/// This function should always look like this: return base.GetClassVersion() +your_version;
		/// </summary>
		/// <returns>Version of the class.</returns>
		public virtual int GetClassVersion()
		{
			return 0;
		}

		/// <summary>
		/// Is called when the version of a loaded node is lower than the class version and the node needs to be updated.
		/// </summary>
		/// <returns>Returns if the update was successful and the version number should be updated.</returns>
		public virtual bool UpdateVersion()
		{
			return true;
		}

		/// <summary>
		/// Creates a new node and attaches the default attributes DebugName and ExportType.
		/// </summary>
		/// <param name="label">The default label of the node.</param>
		/// <param name="description">The description of the node shown to the designer.</param>
		protected Node(string label, string description)
		{
			_children= new ConnectedChildren(this);

			_label= label;
			_description= description;
			_attachments= new List<Attachments.Attachment>();
		}

		/// <summary>
		/// Is called when one of the node's properties were modified.
		/// </summary>
		/// <param name="wasModified">Holds if the event was modified.</param>
		public virtual void OnPropertyValueChanged(bool wasModified)
		{
			if(wasModified)
			{
				DoWasModified();
				BehaviorWasModified();
			}
		}

		public delegate void WasModifiedEventDelegate(Node node);

		/// <summary>
		/// Is called when the node was modified.
		/// </summary>
		public event WasModifiedEventDelegate WasModified;

		/// <summary>
		/// For internal use only.
		/// </summary>
		public void DoWasModified()
		{
			if(WasModified !=null)
				WasModified(this);
		}

		/// <summary>
		/// Mark the behaviour this node belongs to as being modified.
		/// </summary>
		public virtual void BehaviorWasModified()
		{
			if(_parent !=null)
				((Node)_parent).BehaviorWasModified();
		}

		/// <summary>
		/// Is called after a property of a node was initialised, allowing further processing.
		/// </summary>
		/// <param name="property">The property which was initialised.</param>
		public virtual void PostPropertyInit(DesignerPropertyInfo property)
		{
		}

		/// <summary>
		/// Is called after the behaviour was loaded.
		/// </summary>
		/// <param name="behavior">The behaviour this node belongs to.</param>
		public virtual void PostLoad(BehaviorNode behavior)
		{
			// update the version of the node if required
			if(_version <GetClassVersion() && UpdateVersion())
				_version= GetClassVersion();
		}

		/// <summary>
		/// Is called before the behaviour is saved.
		/// </summary>
		/// <param name="behavior">The behaviour this node belongs to.</param>
		public virtual void PreSave(BehaviorNode behavior)
		{
		}

		/// <summary>
		/// Returns the name of the node's type for the attribute ExportType.
		/// This is done as the class attribute can be quite long and bad to handle.
		/// </summary>
		/// <returns>Returns the value for ExportType</returns>
		protected virtual string GetExportType()
		{
			return GetType().Name;
		}

		/// <summary>
		/// Checks the current node and its children for errors.
		/// </summary>
		/// <param name="rootBehavior">The behaviour we are currently checking.</param>
		/// <param name="result">The list the errors are added to.</param>
		public virtual void CheckForErrors(BehaviorNode rootBehavior, List<ErrorCheck> result)
		{
			foreach(Node node in _children)
				node.CheckForErrors(rootBehavior, result);
		}

		/// <summary>
		/// Creates a view for this node. Allows you to return your own class and store additional data.
		/// </summary>
		/// <param name="rootBehavior">The root of the graph of the current view.</param>
		/// <param name="parent">The parent of the NodeViewData created.</param>
		/// <returns>Returns a new NodeViewData object for this node.</returns>
		public abstract NodeViewData CreateNodeViewData(NodeViewData parent, BehaviorNode rootBehavior);

		/// <summary>
		/// Searches a list for NodeViewData for this node. Internal use only.
		/// </summary>
		/// <param name="list">The list which is searched for the NodeViewData.</param>
		/// <returns>Returns null if no fitting NodeViewData could be found.</returns>
		public virtual NodeViewData FindNodeViewData(List<NodeViewData> list)
		{
			foreach(NodeViewData nvd in list)
			{
				if(nvd.Node ==this)
					return nvd;
			}

			return null;
		}

		/// <summary>
		/// Copies all the event handlers from one node to this one.
		/// </summary>
		/// <param name="from">Then node you want to copy the event handlers from.</param>
		protected virtual void CopyEventHandlers(Node from)
		{
			WasModified= from.WasModified;
		}

		/// <summary>
		/// Internally used by CloneBranch.
		/// </summary>
		/// <param name="newparent">The parent the clone children will be added to.</param>
		private void CloneChildNodes(Node newparent)
		{
			// we may not clone children of a referenced behavior
			if(newparent is ReferencedBehaviorNode)
				return;

			// for every connector...
			foreach(Connector connector in _children.Connectors)
			{
				// find the one from the new node...
				Connector localconn= newparent.GetConnector(connector.Identifier);
				Debug.Check(localconn !=null);

				// and duplicate its children into the new node's connector
				for(int i= 0; i <connector.ChildCount; ++i)
				{
					Node child= (Node)connector.GetChild(i);

					Node newchild= (Node)child.Clone();
					newparent.AddChild(localconn, newchild);

					// do this for the children as well
					child.CloneChildNodes(newchild);
				}
			}
		}

		/// <summary>
		/// Duplicates a node and all of its children.
		/// </summary>
		/// <returns>New node with new children.</returns>
		public Node CloneBranch()
		{
			Node newnode;
			if(this is ReferencedBehaviorNode)
			{
				// if we want to clone the branch of a referenced behaviour we have to create a new behaviour node for that.
				// this should only be used to visualise stuff, never in the behaviour tree itself!
				newnode= Create(typeof(BehaviorNode));
				//newnode.Label= Label;
			}
			else
			{
				newnode= Create(GetType());
				CloneProperties(newnode);
			}

			CloneChildNodes(newnode);

			newnode.OnPropertyValueChanged(false);

			return newnode;
		}

		/// <summary>
		/// Duplicates this node. Parent and children are not copied.
		/// </summary>
		/// <returns>New node without parent and children.</returns>
		public object Clone()
		{
			Node newnode= Create(GetType());

			CloneProperties(newnode);

			newnode.OnPropertyValueChanged(false);

			return newnode;
		}

		/// <summary>
		/// Used to duplicate all properties. Any property added must be duplicated here as well.
		/// </summary>
		/// <param name="newnode">The new node which is supposed to get a copy of the properties.</param>
		protected virtual void CloneProperties(Node newnode)
		{
			// clone attachements
			foreach(Attachments.Attachment attach in _attachments)
				newnode.AddAttachment(attach.Clone(newnode));

			// clone comment
			if(_comment !=null)
			{
				newnode._comment= _comment.Clone();
			}
		}

		/// <summary>
		/// This node will be removed from its parent and its children. The parent tries to adopt all children.
		/// </summary>
		/// <returns>Returns false if the parent cannot apobt the children and the operation fails.</returns>
		public bool ExtractNode()
		{
			// we cannot adopt children from a referenced behavior
			if(this is ReferencedBehaviorNode)
			{
				((Node)_parent).RemoveChild(_connector, this);
				return true;
			}

			// check if the parent is allowed to adopt the children
			if(ParentCanAdoptChildren)
			{
				Connector conn= _connector;
				Node parent= (Node)_parent;

				int n= conn.GetChildIndex(this);
				Debug.Check(n >=0);

				parent.RemoveChild(conn, this);

				// let the node's parent adopt all the children
				foreach(Connector connector in _children.Connectors)
				{
					for(int i= 0; i <connector.ChildCount; ++i, ++n)
						parent.AddChild(conn, (Node)connector[i], n);

					// remove the adopted children from the old connector. Do NOT clear the _connector member which already points to the new connector.
					connector.ClearChildrenInternal();
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns a list of all properties which have a designer attribute attached.
		/// </summary>
		/// <returns>A list of all properties relevant to the designer.</returns>
		public virtual IList<DesignerPropertyInfo> GetDesignerProperties()
		{
			return DesignerProperty.GetDesignerProperties(GetType());
		}

		/// <summary>
		/// Returns a list of all properties which have a designer attribute attached.
		/// </summary>
		/// <param name="comparison">The comparison used to sort the design properties.</param>
		/// <returns>A list of all properties relevant to the designer.</returns>
		public virtual IList<DesignerPropertyInfo> GetDesignerProperties(Comparison<DesignerPropertyInfo> comparison)
		{
			return DesignerProperty.GetDesignerProperties(GetType(), comparison);
		}

		protected void CopyWasModifiedFromParent(Node parent)
		{
			if(parent !=null)
				WasModified= parent.WasModified;
		}

		public override string ToString()
		{
			return _label;
		}

		/// <summary>
		/// Used when a DesignerNodeProperty property is exported to format the output.
		/// </summary>
		/// <returns>The format string used to write out the value.</returns>
		public virtual string GetNodePropertyExportString()
		{
			return "\"{0}\"";
		}

		/// <summary>
		/// Returns a list of properties that cannot be selected by a DesignerNodeProperty.
		/// </summary>
		/// <returns>Returns names of properties not allowed.</returns>
		public virtual string[] GetNodePropertyExcludedProperties()
		{
			return new string[] { "ClassVersion", "Version" };
		}

		/// <summary>
		/// Checks if this node has an override for a given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property we are checking.</param>
		/// <returns>Returns true if there is an attachement override.</returns>
		public bool HasOverrride(string propertyName)
		{
			foreach(Attachments.Attachment attach in _attachments)
			{
				Override overr= attach as Override;
				if(overr !=null && overr.PropertyToOverride ==propertyName)
				{
					return true;
				}
			}

			return false;
		}
	}
}
