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
using System.IO;
using Brainiac.Design.Attributes;
using Brainiac.Design.Properties;

namespace Brainiac.Design.Nodes
{
	public interface ReferencedBehaviorNode
	{
		BehaviorNode Reference { get; }
		string ReferenceFilename { get; set; }
		event ReferencedBehavior.ReferencedBehaviorWasModifiedEventDelegate ReferencedBehaviorWasModified;
	}

	/// <summary>
	/// This node represents a referenced behaviour which can be attached to the behaviour tree.
	/// </summary>
	public class ReferencedBehavior : Node, ReferencedBehaviorNode
	{
		private static Brush __defaultBackgroundBrush= new SolidBrush( Color.FromArgb(119,147,60) );

		protected Connector _genericChildren;

		protected BehaviorNode _referencedBehavior;

		/// <summary>
		/// The behaviour which is referenced by this node.
		/// </summary>
		public BehaviorNode Reference
		{
			get { return _referencedBehavior; }
		}

		/// <summary>
		/// The filename of the referenced behaviour.
		/// </summary>
		[DesignerString("ReferencedBehaviorFilename", "ReferencedBehaviorFilenameDesc", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.ReadOnly|DesignerProperty.DesignerFlags.NoExport)]
		public string ReferenceFilename
		{
			get
			{
				// make the path of the reference relative
				string relativeFilename= Behavior.MakeRelative(_referencedBehavior.FileManager.Filename);

				// make sure the behaviour filename is still correct
				Debug.Check(Behavior.MakeAbsolute(relativeFilename) ==_referencedBehavior.FileManager.Filename);
				Debug.Check(!Path.IsPathRooted(relativeFilename));

				return relativeFilename;
			}

			set
			{
				// transform referenced behaviour into an abolute path
				string absoluteFilename= Behavior.MakeAbsolute(value);

				// make sure the behaviour filename is still correct
				Debug.Check(Behavior.MakeRelative(absoluteFilename) ==value);
				Debug.Check(Path.IsPathRooted(absoluteFilename));

				// update the label
				Label= Path.GetFileNameWithoutExtension(absoluteFilename);

				// load the referenced behaviour
				_referencedBehavior= BehaviorManager.Instance.LoadBehavior(absoluteFilename);
				Debug.Check(_referencedBehavior !=null);

				((Node)_referencedBehavior).WasModified+= new WasModifiedEventDelegate(referencedBehavior_WasModified);
				_referencedBehavior.WasRenamed+= new Behavior.WasRenamedEventDelegate(referencedBehavior_WasRenamed);

				// assign the connector of the behaviour
				_genericChildren= _referencedBehavior.GenericChildren;
				_children.SetConnector(_genericChildren);
			}
		}

		/// <summary>
		/// Creates a new referenced behaviour.
		/// </summary>
		/// <param name="rootBehavior">The behaviour this node belongs not. NOT the one is references.</param>
		/// <param name="referencedBehavior">The behaviour you want to reference.</param>
		public ReferencedBehavior(BehaviorNode rootBehavior, BehaviorNode referencedBehavior) : base(((Node)referencedBehavior).Label, Resources.ReferencedBehaviorDesc)
		{
			// when this node is saved, the children won't as they belong to another behaviour
			_saveChildren= false;

			_referencedBehavior= referencedBehavior;

			((Node)_referencedBehavior).WasModified+= new WasModifiedEventDelegate(referencedBehavior_WasModified);
			_referencedBehavior.WasRenamed+= new Behavior.WasRenamedEventDelegate(referencedBehavior_WasRenamed);

			// assign the connector of the behaviour
			_genericChildren= _referencedBehavior.GenericChildren;
			_children.SetConnector(_genericChildren);
		}

		/// <summary>
		/// Creates a new referenced behaviour. The behaviour which will be referenced is read from the Reference attribute.
		/// </summary>
		public ReferencedBehavior() : base(string.Empty, Resources.ReferencedBehaviorDesc)
		{
			_genericChildren= new ConnectorMultiple(_children, string.Empty, "GenericChildren", 1, int.MaxValue);

			// when this node is saved, the children won't as they belong to another behaviour
			_saveChildren= false;

			_referencedBehavior= null;
		}

		protected override void CopyEventHandlers(Node from)
		{
			base.CopyEventHandlers(from);
			ReferencedBehaviorWasModified= ((ReferencedBehavior)from).ReferencedBehaviorWasModified;
		}

		public delegate void ReferencedBehaviorWasModifiedEventDelegate(ReferencedBehaviorNode node);

		/// <summary>
		/// Event is triggered when the behaviour referenced by this node is modified.
		/// </summary>
		public event ReferencedBehaviorWasModifiedEventDelegate ReferencedBehaviorWasModified;

		/// <summary>
		/// Handles when the behaviour referenced by this node is modified.
		/// </summary>
		/// <param name="node">The referenced behaviour node whose behaviour was modified.</param>
		void referencedBehavior_WasModified(Node node)
		{
			// update the filename and the label
			if( _referencedBehavior !=null &&
				_referencedBehavior.FileManager !=null &&
				_referencedBehavior.FileManager.Filename !=string.Empty )
			{
				Label= Path.GetFileNameWithoutExtension(_referencedBehavior.FileManager.Filename);
			}

			// call the event
			if(ReferencedBehaviorWasModified !=null)
				ReferencedBehaviorWasModified(this);
		}

		void referencedBehavior_WasRenamed(BehaviorNode node)
		{
			BehaviorWasModified();
		}

		public override void CheckForErrors(BehaviorNode rootBehavior, List<ErrorCheck> result)
		{
			// if we have a circular reference we must stop here
			if(_referencedBehavior ==null || _referencedBehavior ==rootBehavior)
			{
				result.Add( new Node.ErrorCheck(this, ErrorCheckLevel.Error, Resources.ReferencedBehaviorCircularReferenceError) );
				return;
			}

			// if our referenced behaviour could be loaded, check it as well for errors
			if(_referencedBehavior !=null)
			{
				foreach(Node child in ((Node)_referencedBehavior).Children)
					child.CheckForErrors(rootBehavior, result);
			}
		}

		/// <summary>
		/// Creates a view for this node. Allows you to return your own class and store additional data.
		/// </summary>
		/// <param name="rootBehavior">The root of the graph of the current view.</param>
		/// <param name="parent">The parent of the NodeViewData created.</param>
		/// <returns>Returns a new NodeViewData object for this node.</returns>
		public override NodeViewData CreateNodeViewData(NodeViewData parent, BehaviorNode rootBehavior)
		{
			return new NodeViewDataReferencedBehavior(parent, rootBehavior, this, null, __defaultBackgroundBrush, Label, Description);
		}

		/// <summary>
		/// Searches a list for NodeViewData for this node. Internal use only.
		/// </summary>
		/// <param name="list">The list which is searched for the NodeViewData.</param>
		/// <returns>Returns null if no fitting NodeViewData could be found.</returns>
		public override NodeViewData FindNodeViewData(List<NodeViewData> list)
		{
			foreach(NodeViewData nvd in list)
			{
				if(nvd.Node is ReferencedBehavior)
				{
					ReferencedBehavior refnode= (ReferencedBehavior)nvd.Node;

						// if both nodes reference the same behaviour we copy the view related data
					if( _referencedBehavior !=null && refnode.Reference ==_referencedBehavior ||
						ReferenceFilename ==refnode.ReferenceFilename )
					{
						NodeViewDataReferencedBehavior nvdrb= (NodeViewDataReferencedBehavior)nvd;
						NodeViewDataReferencedBehavior newdata= (NodeViewDataReferencedBehavior)CreateNodeViewData(nvd.Parent, nvd.RootBehavior);

						// copy data
						newdata.IsExpanded= nvdrb.IsExpanded;

						// return new data
						return newdata;
					}
				}

				if(nvd.Node ==this)
					return nvd;
			}

			return null;
		}

		/// <summary>
		/// Adds subitems for all children in the current connector.
		/// </summary>
		protected void AddSubItemsForConnector()
		{
			/*int count= Math.Max(_genericChildren.ChildCount, _genericChildren.MinCount);
			for(int i= 0; i <count; ++i)
			{
				Node child= i <_genericChildren.ChildCount ? _genericChildren.GetChild(i) : null;
				AddSubItem(new SubItemConnector(_genericChildren, child, i));
			}*/
		}

		/// <summary>
		/// Removes all subitems for all children in the current connector.
		/// </summary>
		protected void RemoveSubItemsForConnector()
		{
			/*for(int i= 0; i <_subItems.Count; ++i)
			{
				SubItemConnector subconn= _subItems[i] as SubItemConnector;
				if(subconn !=null && subconn.Connector ==_genericChildren)
				{
					RemoveSubItem(subconn);
					--i;
				}
			}*/
		}

		public override bool AddChild(Connector connector, Node node)
		{
			Debug.Check(connector.Owner ==this || connector.Owner ==_referencedBehavior);

			if(connector.Owner ==this)
				return base.AddChild(connector, node);

			if(((Node)_referencedBehavior).AddChild(connector, node))
			{
				_children.RequiresRebuild();
				RemoveSubItemsForConnector();
				AddSubItemsForConnector();

				return true;
			}

			return false;
		}

		public override bool AddChild(Connector connector, Node node, int index)
		{
			Debug.Check(connector.Owner ==this || connector.Owner ==_referencedBehavior);

			if(connector.Owner ==this)
				return base.AddChild(connector, node, index);

			if(((Node)_referencedBehavior).AddChild(connector, node, index))
			{
				_children.RequiresRebuild();
				RemoveSubItemsForConnector();
				AddSubItemsForConnector();

				return true;
			}

			return false;
		}

		public override bool AddChildNotModified(Node.Connector connector, Node node)
		{
			Debug.Check(connector.Owner ==this || connector.Owner ==_referencedBehavior);

			if(connector.Owner ==this)
				return base.AddChildNotModified(connector, node);

			if(((Node)_referencedBehavior).AddChildNotModified(connector, node))
			{
				_children.RequiresRebuild();
				RemoveSubItemsForConnector();
				AddSubItemsForConnector();

				return true;
			}

			return false;
		}

		public override void RemoveChild(Connector connector, Node node)
		{
			Debug.Check(connector.Owner ==this || connector.Owner ==_referencedBehavior);

			if(connector.Owner ==this)
			{
				base.RemoveChild(connector, node);
				return;
			}

			((Node)_referencedBehavior).RemoveChild(connector, node);

			_children.RequiresRebuild();
			RemoveSubItemsForConnector();
		}

		protected static Brush _defaultBrushCollapsed= new SolidBrush( Color.FromArgb(158, 190, 94) );

		/*public override void Draw(Graphics graphics, NodeViewData nvd, bool isCurrent, bool isSelected, bool isDragged, PointF graphMousePos)
		{
			Brush defBrush= _defaultStyle.Background;

			NodeViewDataReferencedBehavior nvdrb= (NodeViewDataReferencedBehavior) nvd;
			if(_genericChildren.IsReadOnly)
				_defaultStyle.Background= _defaultBrushCollapsed;

			base.Draw(graphics, nvd, isCurrent, isSelected, isDragged, graphMousePos);

			_defaultStyle.Background= defBrush;
		}*/

		protected override void CloneProperties(Node newnode)
		{
			base.CloneProperties(newnode);

			ReferencedBehavior refbehav= (ReferencedBehavior)newnode;
			refbehav._referencedBehavior= _referencedBehavior;
			refbehav.Label= Label;
		}
	}
}
