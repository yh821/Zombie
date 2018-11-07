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
using Brainiac.Design.Nodes;
using Brainiac.Design.Properties;
using Brainiac.Design.Attributes;
using Brainiac.Design.Attachments;

namespace Brainiac.Design
{
	/// <summary>
	/// This enumeration defines the shape of the ode when it is showed in the graph.
	/// </summary>
	public enum NodeShape { Rectangle, RoundedRectangle, Capsule, Ellipse };

	/// <summary>
	/// This class represents a node which is drawn in a view.
	/// </summary>
	public partial class NodeViewData : BaseNode
	{
		/// <summary>
		/// Add a new child but the behaviour does not need to be saved.
		/// Used for collapsed referenced behaviours which show the behaviours they reference.
		/// </summary>
		/// <param name="connector">The connector the node will be added to. Use null for default connector.</param>
		/// <param name="node">The node you want to append.</param>
		/// <returns>Returns true if the child could be added.</returns>
		public bool AddChildNotModified(Connector connector, NodeViewData node)
		{
			Debug.Check(connector !=null && _children.HasConnector(connector));

			if(!connector.AcceptsChild(node.Node.GetType()))
			{
				throw new Exception(Resources.ExceptionNodeHasTooManyChildren);
			}

			if(!connector.AddChild(node))
			{
				return false;
			}

			node._parent= this;

			//TODO: fix this call
			//node.CopyWasModifiedFromParent(this);

			return true;
		}

		protected Node _node;

		/// <summary>
		/// The node this view is for.
		/// </summary>
		public virtual Node Node
		{
			get { return _node; }
		}

		/// <summary>
		/// The parent NodeViewData of this node.
		/// </summary>
		public new NodeViewData Parent
		{
			get { return (NodeViewData)base.Parent; }
		}

		protected BehaviorNode _rootBehavior;

		/// <summary>
		/// The behaviour which owns this view as it is the root of the shown graph.
		/// </summary>
		public BehaviorNode RootBehavior
		{
			get { return _rootBehavior; }
		}

		/// <summary>
		/// Calculates the exact size of a string.
		/// Code taken from http://www.codeproject.com/KB/GDI-plus/measurestring.aspx
		/// </summary>
		/// <param name="graphics">The graphics object used to calculate the string's size.</param>
		/// <param name="font">The font which will be used to draw the string.</param>
		/// <param name="text">The actual string which will be drawn.</param>
		/// <returns>Returns the untransformed size of the string when being drawn.</returns>
		static public SizeF MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
		{
			// set something to generate the minimum size
			bool minimum= false;
			if(text ==string.Empty)
			{
				minimum= true;
				text= " ";
			}

			System.Drawing.StringFormat format = new System.Drawing.StringFormat();
			System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0, 1000, 1000);
			System.Drawing.CharacterRange[] ranges = { new System.Drawing.CharacterRange(0, text.Length) };
			System.Drawing.Region[] regions = new System.Drawing.Region[1];

			format.SetMeasurableCharacterRanges(ranges);

			regions = graphics.MeasureCharacterRanges(text, font, rect, format);
			rect = regions[0].GetBounds(graphics);

			return minimum ? new SizeF(0.0f, rect.Height) : rect.Size;
		}

		protected List<SubItem> _subItems= new List<SubItem>();

		/// <summary>
		/// The list of subitems handled by this node.
		/// </summary>
		public IList<SubItem> SubItems
		{
			get { return _subItems.AsReadOnly(); }
		}

		/// <summary>
		/// Sorts the subitems so the parallel ones are drawn last, otherwise we get glitches with the backgrounds drawn by the non-prallel subitems.
		/// </summary>
		protected void SortSubItems()
		{
			// find the last parallel subitem from the beginning on
			int lastParallelIndex= -1;
			for(int i= 0; i <_subItems.Count; ++i)
			{
				if(_subItems[i].ShowParallelToLabel)
					lastParallelIndex= i;
				else break;  // once we found a subitem which is not parallel we quit
			}

			// sort the subitems
			for(int i= 0; i <_subItems.Count; ++i)
			{
				// if we found a parallel past the last one we sort it
				if(_subItems[i].ShowParallelToLabel && i >lastParallelIndex)
				{
					SubItem parallel= _subItems[i];
					_subItems.RemoveAt(i--);
					_subItems.Insert(++lastParallelIndex, parallel);
				}
			}
		}

		/// <summary>
		/// Attaches a subitem to this node.
		/// </summary>
		/// <param name="sub">The node subitem we want to attach.</param>
		public void AddSubItem(SubItem sub)
		{
			_subItems.Add(sub);

			SortSubItems();

			_labelChanged= true;
		}

		/// <summary>
		/// Attaches a subitem to this node.
		/// </summary>
		/// <param name="sub">The node subitem we want to attach.</param>
		/// <param name="index">The index where you want to insert the subitem.</param>
		public void AddSubItem(SubItem sub, int index)
		{
			_subItems.Insert(index, sub);

			SortSubItems();

			_labelChanged= true;
		}

		/// <summary>
		/// Removes a subitem from the node.
		/// </summary>
		/// <param name="sub">The subitem which will be removed.</param>
		public void RemoveSubItem(SubItem sub)
		{
			int index= _subItems.IndexOf(sub);

			if(sub ==_selectedSubItem)
			{
				_selectedSubItem.IsSelected= false;
				_selectedSubItem= null;
			}

			if(index <0)
				throw new Exception(Resources.ExceptionSubItemIsNoChild);

			_subItems.RemoveAt(index);
		}

		/// <summary>
		/// Removes the selected event from the node.
		/// </summary>
		public bool RemoveSelectedSubItem()
		{
			if(!_selectedSubItem.CanBeDeleted)
				return false;

			var attach= _selectedSubItem as SubItemAttachment;
			if(attach !=null)
			{
				_node.RemoveAttachment(attach.Attachment);
			}

			RemoveSubItem(_selectedSubItem);
			return true;
		}

		/// <summary>
		/// The the currently selected subitem.
		/// </summary>
		protected SubItem _selectedSubItem;

		/// <summary>
		/// Returns the currently selected subitem. Is null if no subitem is selected.
		/// </summary>
		public SubItem SelectedSubItem
		{
			get { return _selectedSubItem; }

			set
			{
				if(_selectedSubItem !=null)
					_selectedSubItem.IsSelected= false;

				_selectedSubItem= value;

				if(_selectedSubItem !=null)
					_selectedSubItem.IsSelected= true;
			}
		}

		/// <summary>
		/// The tooltip for this node which is shown if the option is enabled in the settings.
		/// </summary>
		public virtual string ToolTip
		{
			get { return _node.Description; }
		}

		/// <summary>
		/// Returns the first NodeViewData which is associated with the given node. Notice that there might be other NodeViewDatas which are ignored.
		/// </summary>
		/// <param name="node">The node you want to get the NodeViewData for.</param>
		/// <returns>Returns the first NodeViewData found.</returns>
		public virtual NodeViewData FindNodeViewData(Node node)
		{
			// check if this is a fitting view
			if(_node ==node)
				return this;

			// search the children
			foreach(NodeViewData child in _children)
			{
				NodeViewData result= child.FindNodeViewData(node);
				if(result !=null)
					return result;
			}

			return null;
		}

		/// <summary>
		/// Returns whether or not the view data needs to be rebuilt because the tree changed.
		/// </summary>
		/// <returns>Returns true when the tree needs to be rebuilt.</returns>
		protected virtual bool NeedsToSynchronizeWithNode()
		{
			// if the counts do not fit we must rebuild
			bool rebuild= _node.Children.Count !=_children.ChildCount;

			// check if all children are associated to the correct children of the node
			if(!rebuild)
			{
				foreach(Connector connector in _node.Connectors)
				{
					// check if the child count is different
					Connector localConnector= _children.GetConnector(connector.Identifier);
					if(localConnector ==null || connector.ChildCount !=localConnector.ChildCount)
					{
						rebuild= true;
						break;
					}

					// check if the children are still the same
					for(int i= 0; i <localConnector.ChildCount; ++i)
					{
						NodeViewData nvd= (NodeViewData)localConnector.GetChild(i);

						if(nvd.Node !=connector.GetChild(i))
						{
							rebuild= true;
							break;
						}
					}
				}
			}

			return rebuild;
		}

		/// <summary>
		/// Removes all sub items which are used for the connectors.
		/// </summary>
		protected void RemoveAllConnectorSubItems()
		{
			for(int i= 0; i <_subItems.Count; ++i)
			{
				SubItemConnector si= _subItems[i] as SubItemConnector;
				if(si !=null)
				{
					RemoveSubItem(si);
					i--;
				}
			}
		}

		/// <summary>
		/// Creates the view data for a node.
		/// </summary>
		/// <param name="processedBehaviors">The list of processed behaviours to handle circular references.</param>
		public virtual void DoSynchronizeWithNode(ProcessedBehaviors processedBehaviors)
		{
			_children.ClearConnectors();
			RemoveAllConnectorSubItems();

			foreach(Connector connector in _node.Connectors)
				connector.Clone(_children);

			foreach(Connector connector in _children.Connectors)
			{
				Connector nodeConnector= _node.GetConnector(connector.Identifier);
				Debug.Check(nodeConnector !=null);

				for(int i= 0; i <nodeConnector.ChildCount; ++i)
				{
					Node node= (Node)nodeConnector.GetChild(i);

					NodeViewData nvd= node.CreateNodeViewData(this, _rootBehavior);

					Debug.Verify( AddChildNotModified(connector, nvd) );
				}
			}

			GenerateNewLabel();
		}

		/// <summary>
		/// This function adapts the children of the view that they represent the children of the node this view is for.
		/// Children are added and removed.
		/// </summary>
		/// <param name="processedBehaviors">A list of previously processed behaviours to deal with circular references.</param>
		public virtual void SynchronizeWithNode(ProcessedBehaviors processedBehaviors)
		{
			if(processedBehaviors.MayProcess(_node))
			{
				// allow the node to add some final children or remove some
				_node.PreLayoutUpdate(this);

				// check if we must rebuild the child list
				if(NeedsToSynchronizeWithNode())
					DoSynchronizeWithNode(processedBehaviors);

				// synchronise the children as well
				foreach(NodeViewData child in _children)
				{
					Debug.Check(child.RootBehavior ==_rootBehavior);

					child.SynchronizeWithNode(processedBehaviors.Branch(child._node));
				}
			}
		}

		/// <summary>
		/// The label used to generate the final label which can include parameters and other stuff.
		/// </summary>
		protected string BaseLabel
		{
			get { return _node.Label; }
		}

		protected string _label;

		/// <summary>
		/// The label shown on the node.
		/// </summary>
		public string Label
		{
			get { return _label; }
		}

		public override string ToString()
		{
			return _label;
		}

		protected NodeShape _shape;

		/// <summary>
		/// The shape of the node.
		/// </summary>
		public NodeShape Shape
		{
			get { return _shape; }
		}

		protected int _minHeight;

		/// <summary>
		/// The minimum height of the node. Can be expanded by events and the label.
		/// </summary>
		public int MinHeight
		{
			get { return _minHeight; }
		}

		protected int _minWidth;

		/// <summary>
		/// The minimum width of the node. Can be expanded by events and the label.
		/// </summary>
		public int MinWidth
		{
			get { return _minWidth; }
		}

		protected Font _font;
		protected SizeF _labelSize, _realLabelSize;
		protected float _subItemParallelWidth;

		protected Style _defaultStyle;

		/// <summary>
		/// The default style of the node when it is neither hover over or selected.
		/// </summary>
		public Style DefaultStyle
		{
			get { return _defaultStyle; }
		}

		protected Style _currentStyle;

		/// <summary>
		/// The style of the node when the mouse is hovering over it.
		/// </summary>
		public Style CurrentStyle
		{
			get { return _currentStyle; }
		}

		protected Style _selectedStyle;

		/// <summary>
		/// The style of the node when it is selected.
		/// </summary>
		public Style SelectedStyle
		{
			get { return _selectedStyle; }
		}

		protected Style _draggedStyle;

		/// <summary>
		/// The style of the node when it is moved inside the tree and shown as a small tree which is attached to the mouse.
		/// </summary>
		public Style DraggedStyle
		{
			get { return _draggedStyle; }
		}

		protected SizeF _finalSize;

		protected bool _labelChanged= true;

		/// <summary>
		/// Creates a new view for a given node.
		/// </summary>
		/// <param name="parent">The parent of the new NodeViewData.</param>
		/// <param name="rootBehavior">The behaviour which is the root of the graph the given node is shown in.</param>
		/// <param name="node">The node the view is created for.</param>
		/// <param name="shape">The shape of the node when being rendered.</param>
		/// <param name="defaultStyle">The stle of the node when being neither hovered over nor selected.</param>
		/// <param name="currentStyle">The style of the node when the mouse is hovering over it.</param>
		/// <param name="selectedStyle">The style of the node when it is selected.</param>
		/// <param name="draggedStyle">The style of the node when it is attached to the mouse cursor when moving nodes in the graph.</param>
		/// <param name="label">The default label of the node.</param>
		/// <param name="font">The font used for the label.</param>
		/// <param name="minWidth">The minimum width of the node.</param>
		/// <param name="minHeight">The minimum height of the node.</param>
		/// <param name="description">The description of the node shown to the designer.</param>
		public NodeViewData(NodeViewData parent, BehaviorNode rootBehavior, Node node, NodeShape shape, Style defaultStyle, Style currentStyle, Style selectedStyle, Style draggedStyle, string label, Font font, int minWidth, int minHeight, string description)
		{
			Debug.Check(rootBehavior !=null);

			_rootBehavior= rootBehavior;
			_node= node;

			//TODO: fix this call
			//CopyWasModifiedFromParent(parent);

			_node.WasModified+= new Node.WasModifiedEventDelegate(node_WasModified);

			_shape= shape;
			_font= font;
			_minWidth= minWidth;
			_minHeight= minHeight;

			if(defaultStyle ==null)
				throw new Exception(Resources.ExceptionDefaultStyleNull);

			_defaultStyle= defaultStyle;
			_currentStyle= currentStyle;
			_selectedStyle= selectedStyle;
			_draggedStyle= draggedStyle;

			// Add all listed properties
			IList<DesignerPropertyInfo> properties= node.GetDesignerProperties();
			for(int p= 0; p <properties.Count; ++p)
			{
				DesignerProperty att= properties[p].Attribute;

				if(att.Display ==DesignerProperty.DisplayMode.List)
					AddSubItem( new SubItemProperty(node, properties[p].Property, att) );
			}

			// Add all attachments
			foreach(Attachment attach in node.Attachments)
			{
				AddSubItem( attach.CreateSubItem() );
			}

			GenerateNewLabel();
		}

		/// <summary>
		/// Updates the with of the node. For internal use only. Used to give all children the same width.
		/// </summary>
		/// <param name="width">The untransformed with.</param>
		internal void SetWidth(float width)
		{
			_finalSize.Width= width;
		}

		const float Padding= 6.0f;

		/// <summary>
		/// Is called when a property of the selected event of this node was modified.
		/// </summary>
		/// <param name="wasModified">Holds if the node was modified.</param>
		public virtual void OnSubItemPropertyValueChanged(bool wasModified)
		{
			// when the label changes the size of the node might change as well
			_node.DoWasModified();

			if(wasModified)
			{
				_node.BehaviorWasModified();
			}
		}

		/// <summary>
		/// Calculates the final size of the node.
		/// </summary>
		/// <param name="graphics">The graphics used to measure the size of the labels.</param>
		/// <param name="rootBehavior">The behaviour this node belongs to.</param>
		public virtual void UpdateFinalSize(Graphics graphics, BehaviorNode rootBehavior)
		{
#if DEBUG
			//ensure consistency
			DebugCheckIntegrity();
#endif

			// find the widest node
			float maxWidth= 0.0f;
			foreach(NodeViewData node in _children)
			{
				node.UpdateFinalSize(graphics, rootBehavior);

				maxWidth= Math.Max(maxWidth, node._finalSize.Width);
			}

			// give all children the same width
			foreach(NodeViewData node in _children)
				node.SetWidth(maxWidth);

			// update the label if it has changed
			if(_labelChanged)
			{
				_labelChanged= false;
				_labelSize= MeasureDisplayStringWidth(graphics, _label, _font);
				_labelSize.Width+= 2.0f;

				// update the subitems
				float subItemHeight= 0.0f;
				float subItemWidth= 0.0f;
				float subItemParallelHeight= 0.0f;
				_subItemParallelWidth= 0.0f;
				foreach(SubItem subitem in _subItems)
				{
					// call update
					subitem.Update(this, graphics);

					// store the required space depending on parallel and non-parallel subitems
					if(subitem.ShowParallelToLabel)
					{
						subItemParallelHeight+= subitem.Height;
						_subItemParallelWidth= Math.Max(_subItemParallelWidth, subitem.Width);
					}
					else
					{
						subItemHeight+= subitem.Height;
						subItemWidth= Math.Max(subItemWidth, subitem.Width);
					}
				}

				// if we have no parallel subitem, we also need no extra padding
				if(_subItemParallelWidth >0.0f)
					_subItemParallelWidth+= Padding;

				// the height of the label is its own height or the height of all the parallel subitems
				_realLabelSize= _labelSize;
				_labelSize.Width= Math.Max(subItemWidth, _labelSize.Width) + _subItemParallelWidth;
				_labelSize.Height= Math.Max(_labelSize.Height + Padding *2.0f, subItemParallelHeight);

				// calculate the final size of the node
				_finalSize.Width= Math.Max(_minWidth, _labelSize.Width + Padding *2.0f);
				_finalSize.Height= Math.Max(_minHeight, _labelSize.Height + subItemHeight);
			}
		}

		/// <summary>
		/// Draws the background and shape of the node
		/// </summary>
		/// <param name="graphics">The grpahics object we render to.</param>
		/// <param name="boundingBox">The untransformed bounding box of the node.</param>
		/// <param name="brush">The brush used for the background.</param>
		public virtual void DrawShapeBackground(Graphics graphics, RectangleF boundingBox, Brush brush)
		{
			switch(_shape)
			{
				case(NodeShape.Rectangle):
					graphics.FillRectangle(brush, boundingBox);
				break;

				case(NodeShape.Ellipse):
					graphics.FillEllipse(brush, boundingBox);
				break;

				case(NodeShape.Capsule):
				case(NodeShape.RoundedRectangle):
					float radius= _shape ==NodeShape.RoundedRectangle ? 10.0f: boundingBox.Height;

					System.Drawing.Extended.ExtendedGraphics extended= new System.Drawing.Extended.ExtendedGraphics(graphics);

					extended.FillRoundRectangle(brush, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height, radius);
				break;

				default: throw new Exception(Resources.ExceptionUnhandledNodeShape);
			}
		}

		/// <summary>
		/// Draw the border of the node.
		/// </summary>
		/// <param name="graphics">The grpahics object we render to.</param>
		/// <param name="boundingBox">The untransformed bounding box of the node.</param>
		/// <param name="pen">The pen we use.</param>
		protected virtual void DrawShapeBorder(Graphics graphics, RectangleF boundingBox, Pen pen)
		{
			switch(_shape)
			{
				case(NodeShape.Rectangle):
					graphics.DrawRectangle(pen, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
				break;

				case(NodeShape.Ellipse):
					graphics.DrawEllipse(pen, boundingBox);
				break;

				case(NodeShape.Capsule):
				case(NodeShape.RoundedRectangle):
					float radius= _shape ==NodeShape.RoundedRectangle ? 10.0f: boundingBox.Height;

					System.Drawing.Extended.ExtendedGraphics extended= new System.Drawing.Extended.ExtendedGraphics(graphics);

					extended.DrawRoundRectangle(pen, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height, radius);
				break;

				default: throw new Exception(Resources.ExceptionUnhandledNodeShape);
			}
		}

		/// <summary>
		/// Calculates the untransformed bounding box of a subitem.
		/// </summary>
		/// <param name="nodeBoundingBox">The untransformed bounding box of the node.</param>
		/// <param name="n">The index of the subitem.</param>
		/// <returns>Returns the untransformed bounding box of the subitem.</returns>
		protected RectangleF GetSubItemBoundingBox(RectangleF nodeBoundingBox, int n)
		{
			SubItem subitem= _subItems[n];
			float top;
			float width= nodeBoundingBox.Width;

			if(subitem.ShowParallelToLabel)
			{
				// if our subitem is a parallel one, we center it around the middle of the node

				// first we collect some information about parallel shown subitems
				float totalParallelHeight= 0.0f;
				float previousParallelHeight= 0.0f;
				for(int i= 0; i <_subItems.Count; ++i)
				{
					if(_subItems[i].ShowParallelToLabel)
					{
						if(i <n)
							// store the height of all parallel subitems before the requested one
							previousParallelHeight+= _subItems[i].Height;

						// store the height of all available subitems
						totalParallelHeight+= _subItems[i].Height;
					}
					else
					{
						// all parallel subitems must be next to each other
						break;
					}
				}

				// calculate the final top
				top= nodeBoundingBox.Top + (nodeBoundingBox.Height - totalParallelHeight) *0.5f + previousParallelHeight;
			}
			else
			{
				// if our subitem is not parallel we simply add the height of the label and the height of all previous subitems which are not parallel
				top= nodeBoundingBox.Top + _labelSize.Height;
				for(int i= 0; i <n; ++i)
				{
					if(!_subItems[i].ShowParallelToLabel)
						top+= _subItems[i].Height;
				}

				if(_subItemParallelWidth >0.0f)
					width-= _subItemParallelWidth + Padding;
			}

			// return the bounding box of the requested subitem
			return new RectangleF(nodeBoundingBox.X, top, width, subitem.Height);
		}

		/// <summary>
		/// Calculates the untransformed bounding box of all connector subitems of a given connector.
		/// </summary>
		/// <param name="nodeBoundingBox">The untransformed bounding box of the node.</param>
		/// <param name="connector">The connector we want the counding box for.</param>
		/// <returns>Returns the untransformed bounding box of the connector's subitems.</returns>
		public RectangleF GetConnectorBoundingBox(RectangleF nodeBoundingBox, Connector connector)
		{
			// first find the first and last parallel subitem
			int firstParallel= -1;
			int lastParallel= -1;
			for(int i= 0; i <_subItems.Count; ++i)
			{
				if(_subItems[i].ShowParallelToLabel)
				{
					if(firstParallel <0)
						firstParallel= i;

					lastParallel= i;
				}
				else
				{
					// all parallel subitems must be next to each other
					break;
				}
			}

			// ensure our retrieved information is correct
			Debug.Check(firstParallel >=0 && lastParallel >=firstParallel);

			float top= -1.0f;
			float bottom= -1.0f;

			// search all subitems for the connector
			bool inConnector= false;
			for(int i= firstParallel; i <=lastParallel; ++i)
			{
				SubItemConnector subitemConn= _subItems[i] as SubItemConnector;

				// if we found a subitem for our connector and we have found none before, we have found the top of the bounding box
				if(!inConnector && subitemConn !=null && subitemConn.Connector ==connector)
				{
					inConnector= true;

					if(i ==firstParallel)
						top= nodeBoundingBox.Top;  // if this is the first parallel, simply extent it to the full height of the node
					else top= GetSubItemBoundingBox(nodeBoundingBox, i).Top;
				}

				// if we found no subitem for our connector and we have found one before, we have found the bottom of the bounding box
				if(inConnector && (subitemConn ==null || subitemConn.Connector !=connector))
				{
					// the previous subitem was the last one for our connector
					if(i -1 ==lastParallel)
						bottom= nodeBoundingBox.Bottom;  // if this is the first parallel, simply extent it to the full height of the node
					else bottom= GetSubItemBoundingBox(nodeBoundingBox, i -1).Bottom;

					break;
				}

				// when we have reached the last parallel subitem, simply extent the bounding box to the height of the node we are the last parallel subitem
				if(i ==lastParallel)
				{
					bottom= nodeBoundingBox.Bottom;
					break;
				}
			}

			// ensure our retrieved data is valid
			Debug.Check(top >=0.0f && bottom >top);

			// return the bounding box of all subitems belonging to the given connector
			return new RectangleF(nodeBoundingBox.X, top, nodeBoundingBox.Width, bottom - top);
		}

		/// <summary>
		/// Draws the node to the graph.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="nvd">The view data of this node for drawing.</param>
		/// <param name="isCurrent">Determines if the node is currently hovered over.</param>
		/// <param name="isSelected">Determines if the node is selected.</param>
		/// <param name="isDragged">Determines if the node is currently being dragged.</param>
		/// <param name="graphMousePos">The mouse position in the untransformed graph.</param>
		public virtual void Draw(Graphics graphics, NodeViewData nvd, bool isCurrent, bool isSelected, bool isDragged, PointF graphMousePos)
		{
#if DEBUG
			//ensure consistency
			DebugCheckIntegrity();
#endif

			RectangleF boundingBox= nvd.BoundingBox;

			// assemble the correct style
			Style style= _defaultStyle;

			if(isDragged)
				style+= _draggedStyle;
			else if(isCurrent)
				style+= _currentStyle;
			else if(isSelected)
				style+= _selectedStyle;

			if(style.Background !=null)
				DrawShapeBackground(graphics, boundingBox, style.Background);

			// if the node is dragged, do not render the events
			if(!isDragged)
			{
				// if this node is not selected, deselect the event
				if(!isSelected && _selectedSubItem !=null)
				{
					_selectedSubItem.IsSelected= false;
					_selectedSubItem= null;
				}

				if(_subItems.Count >0)
				{
					Region prevreg= graphics.Clip;

					// draw non parallel subitems first
					for(int i= 0; i <_subItems.Count; ++i)
					{
						if(!_subItems[i].ShowParallelToLabel)
						{
							// get the bounding box of the event
							RectangleF newclip= GetSubItemBoundingBox(boundingBox, i);
							graphics.Clip= new Region(newclip);

							_subItems[i].Draw(graphics, nvd, newclip);
						}
					}

					// draw parallel subitems second
					for(int i= 0; i <_subItems.Count; ++i)
					{
						if(_subItems[i].ShowParallelToLabel)
						{
							// get the bounding box of the event
							RectangleF newclip= GetSubItemBoundingBox(boundingBox, i);
							graphics.Clip= new Region(newclip);

							_subItems[i].Draw(graphics, nvd, newclip);
						}
					}

					// restore rendering area
					graphics.Clip= prevreg;
				}

				// draw the label of the node
				if(style.Label !=null)
				{
					// calculate the height of all non-parallel subitems so we can correctly center the label
					float subItemsHeight= 0.0f;
					foreach(SubItem sub in _subItems)
					{
						if(!sub.ShowParallelToLabel)
							subItemsHeight+= sub.Height;
					}

					float x= boundingBox.Left + (boundingBox.Width - _subItemParallelWidth) *0.5f - _realLabelSize.Width *0.5f;
					float y= boundingBox.Top + boundingBox.Height *0.5f - subItemsHeight *0.5f - _realLabelSize.Height *0.5f;
					graphics.DrawString(_label, _font, style.Label, x, y);

					//graphics.DrawRectangle(Pens.Red, boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
					//graphics.DrawRectangle(Pens.Red, x, y, _realLabelSize.Width, _realLabelSize.Height);
					//graphics.DrawRectangle(Pens.Green, x, y, _labelSize.Width, _labelSize.Height);
				}
			}

			// draw the nodes border
			if(style.Border !=null)
				DrawShapeBorder(graphics, boundingBox, style.Border);

			//graphics.DrawRectangle(Pens.Red, nvd.LayoutRectangle.X, nvd.LayoutRectangle.Y, nvd.LayoutRectangle.Width, nvd.LayoutRectangle.Height);
		}

		/// <summary>
		/// Draws the edges connecting the nodes.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="nvd">The view data of this node in the current view.</param>
		/// <param name="edgePen">The pen used for normal connectors.</param>
		/// <param name="edgePenReadOnly">The pen used for read-only connectors.</param>
		public virtual void DrawEdges(Graphics graphics, NodeViewData nvd, Pen edgePen, Pen edgePenReadOnly)
		{
			RectangleF boundingBox= nvd.BoundingBox;

			// calculate an offset so we cannot see the end or beginning of the rendered edge
			float edgePenHalfWidth= edgePen.Width *0.5f;

			foreach(NodeViewData node in nvd.Children)
			{
				RectangleF nodeBoundingBox= node.BoundingBox;

				// calculate the centre between both nodes and of the edge
				float middle= boundingBox.Right + (nodeBoundingBox.Left - boundingBox.Right) *0.5f;

				// end at the middle of the other node
				float nodeHeight= nodeBoundingBox.Top + nodeBoundingBox.Height *0.5f;

				// find the correct connector for this node
				for(int i= 0; i <_subItems.Count; ++i)
				{
					SubItemConnector conn= _subItems[i] as SubItemConnector;
					if(conn !=null && conn.Child ==node)
					{
						// get the bounding box of the event
						RectangleF subitemBoundingBox= GetSubItemBoundingBox(boundingBox, i);

						// start at the middle of the connector
						float connectorHeight= subitemBoundingBox.Top + subitemBoundingBox.Height *0.5f;

						graphics.DrawBezier(conn.Connector.IsReadOnly ? edgePenReadOnly : edgePen,
											boundingBox.Right - edgePenHalfWidth, connectorHeight,
											middle, connectorHeight,
											middle, nodeHeight,
											nodeBoundingBox.Left + edgePenHalfWidth, nodeHeight);

						break;
					}
				}
			}
		}

		/// <summary>
		/// Draws the background of the comment.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="nvd">The view data of this node in the current view.</param>
		/// <param name="renderDepth">The depth which is still rendered.</param>
		/// <param name="padding">The padding between the nodes.</param>
		public void DrawCommentBackground(Graphics graphics, NodeViewData nvd, int renderDepth, SizeF padding)
		{
			if(_node.CommentObject !=null)
				_node.CommentObject.DrawBackground(graphics, nvd, renderDepth, padding);
		}

		/// <summary>
		/// Draws the text of the comment.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="nvd">The view data of this node in the current view.</param>
		public void DrawCommentText(Graphics graphics, NodeViewData nvd)
		{
			if(_node.CommentObject !=null)
				_node.CommentObject.DrawText(graphics, nvd);
		}

		/// <summary>
		/// Is called when the node was double-clicked. Used for referenced behaviours.
		/// </summary>
		/// <param name="nvd">The view data of the node in the current view.</param>
		/// <param name="layoutChanged">Does the layout need to be recalculated?</param>
		/// <returns>Returns if the node handled the double click or not.</returns>
		public virtual bool OnDoubleClick(NodeViewData nvd, out bool layoutChanged)
		{
			layoutChanged= false;
			return false;
		}

		/// <summary>
		/// Generates a new label by adding the attributes to the label as arguments
		/// </summary>
		protected void GenerateNewLabel()
		{
			// generate the new label with the arguments
			string newlabel= BaseLabel +"(";
			int paramCount= 0;

			// check all properties for one which must be shown as a parameter on the node
			IList<DesignerPropertyInfo> properties= _node.GetDesignerProperties(DesignerProperty.SortByDisplayOrder);
			for(int p= 0; p <properties.Count; ++p)
			{
				// property must be shown as a parameter on the node
				if(properties[p].Attribute.Display ==DesignerProperty.DisplayMode.Parameter)
				{
					newlabel+= properties[p].GetDisplayValue(_node) +", ";
					paramCount++;
				}
			}

			// only return the new label when it contains any parameters
			_label= paramCount >0 ? newlabel.Substring(0, newlabel.Length -2) +")" : BaseLabel;

			_labelChanged= true;
		}

		/// <summary>
		/// Is called when a possible selection of an event occured.
		/// </summary>
		/// <param name="nvd">The view data of the node in the current view.</param>
		/// <param name="graphMousePos">The mouse position in the untransformed graph.</param>
		public virtual void ClickEvent(NodeViewData nvd, PointF graphMousePos)
		{
			SubItem newsub= null;

			for(int i= 0; i <_subItems.Count; ++i)
			{
				if(_subItems[i].SelectableObject !=null)
				{
					RectangleF bbox= GetSubItemBoundingBox(nvd.BoundingBox, i);
					if(bbox.Contains(graphMousePos))
					{
						newsub= _subItems[i];
						break;
					}
				}
			}

			SelectedSubItem= newsub;
		}

		protected RectangleF _boundingBox;

		/// <summary>
		/// The untransformed bounding box of the node.
		/// </summary>
		public RectangleF BoundingBox
		{
			get { return _boundingBox; }
		}

		protected RectangleF _displayBoundingBox;

		/// <summary>
		/// The transformed bounding box of the node.
		/// </summary>
		public RectangleF DisplayBoundingBox
		{
			get { return _displayBoundingBox; }
		}

		protected RectangleF _layoutRectangle;

		/// <summary>
		/// The layout rectangle of the node.
		/// </summary>
		public RectangleF LayoutRectangle
		{
			get { return _layoutRectangle; }
		}

		/// <summary>
		/// The upper left corner of the layout rectangle. For internal use only.
		/// </summary>
		protected PointF Location
		{
			get { return _layoutRectangle.Location; }
			set { _layoutRectangle.Location= value; }
		}

		/// <summary>
		/// Returns the node a given location is in.
		/// </summary>
		/// <param name="location">The location you want to check.</param>
		/// <returns>Returns null if the position is not inside any node.</returns>
		public NodeViewData IsInside(PointF location)
		{
			if(_displayBoundingBox.Contains(location))
				return this;

			foreach(NodeViewData node in _children)
			{
				NodeViewData insidenode= node.IsInside(location);
				if(insidenode !=null)
					return insidenode;
			}

			return null;
		}

		/// <summary>
		/// Copies the ode's size as the size of the bounding box.
		/// </summary>
		public virtual void UpdateExtent()
		{
			foreach(NodeViewData node in _children)
				node.UpdateExtent();

			_boundingBox.Size= _finalSize;
		}

		/// <summary>
		/// Adds an offset to the height and Y position of the  layout rectangle.
		/// Used when the parent is higher than the children.
		/// </summary>
		/// <param name="offset">The off set which will be added.</param>
		protected void OffsetLayoutSize(float offset)
		{
			float yoffset= 0.0f;
			foreach(NodeViewData node in _children)
			{
				node._layoutRectangle.Height+= offset;
				node._layoutRectangle.Y+= yoffset;

				yoffset+= offset;

				node.OffsetLayoutSize(offset);
			}
		}

		/// <summary>
		/// Calculates the layout rectangles for this node and its children.
		/// </summary>
		/// <param name="padding">The padding which is used between the nodes.</param>
		public void CalculateLayoutSize(SizeF padding)
		{
			// update size for children
			foreach(NodeViewData node in _children)
				node.CalculateLayoutSize(padding);

			// calculate my layout size
			_layoutRectangle.Height=_boundingBox.Height;
			_layoutRectangle.Width= _boundingBox.Width;

			// calculate the size my children have
			float childHeight= 0.0f;
			foreach(NodeViewData node in _children)
				childHeight+= node.LayoutRectangle.Height;

			// if we have multiple children, add the padding we keep between them.
			if(_children.ChildCount >1)
				childHeight+= (_children.ChildCount -1) * padding.Height;

			if(_layoutRectangle.Height >childHeight)
			{
				_layoutRectangle.Height= _layoutRectangle.Height;

				// if this node is higher than its children we have to update them
				float heightDiff= _layoutRectangle.Height - childHeight;
				float offset= heightDiff / _children.ChildCount;

				OffsetLayoutSize(offset);
			}
			else
			{
				_layoutRectangle.Height= childHeight;
			}
		}

		/// <summary>
		/// Aligns the different layout rectangles in the graph.
		/// </summary>
		/// <param name="padding">The padding you want to keep between the layout rectangles.</param>
		public void Layout(SizeF padding)
		{
			// the upper left position of the children
			PointF pos= new PointF(_layoutRectangle.Right + padding.Width, _layoutRectangle.Y);

			// align children
			foreach(NodeViewData node in _children)
			{
				// set the node to the correct position
				node.Location= pos;

				// adjust the location for the next child to come
				pos.Y+= node.LayoutRectangle.Height + padding.Height;

				// layout the children of this node.
				node.Layout(padding);
			}
		}

		/// <summary>
		/// Centers this and its children node in front of its/their children.
		/// </summary>
		public virtual void UpdateLocation()
		{
			// move the node to the left centre of its layout
			_boundingBox.X= _layoutRectangle.X;
			_boundingBox.Y= _layoutRectangle.Y + _layoutRectangle.Height *0.5f - _boundingBox.Height *0.5f;

			// update the location for the children as well
			foreach(NodeViewData node in _children)
				node.UpdateLocation();
		}

		/// <summary>
		/// Calculates the display bounding box for this node.
		/// </summary>
		/// <param name="offsetX">The X offset of the graph.</param>
		/// <param name="offsetY">The Y offset of the graph.</param>
		/// <param name="scale">The scale of the graph.</param>
		public virtual void UpdateDisplay(float offsetX, float offsetY, float scale)
		{
			// transform the bounding box.
			_displayBoundingBox.X= _boundingBox.X * scale + offsetX;
			_displayBoundingBox.Y= _boundingBox.Y * scale + offsetY;
			_displayBoundingBox.Width= _boundingBox.Width * scale;
			_displayBoundingBox.Height= _boundingBox.Height * scale;

			// transform the children's bounding boxes.
			foreach(NodeViewData node in _children)
				node.UpdateDisplay(offsetX, offsetY, scale);
		}

		/// <summary>
		/// Returns the width of this node and all its child nodes. Internal use only.
		/// </summary>
		/// <param name="paddingWidth">The width kept between the nodes.</param>
		/// <param name="depth">Defines how deep the search will be.</param>
		/// <returns>Returns untransformed width.</returns>
		private float GetTotalWidth(float paddingWidth, int depth)
		{
			if(depth <1)
				return _layoutRectangle.Width;

			float width= _layoutRectangle.Width;

			// if we have children we must keep our distance
			if(_children.ChildCount >0)
				width+= paddingWidth;

			// find the child with the highest width
			float childwidth= 0.0f;
			foreach(NodeViewData node in _children)
				childwidth= Math.Max(childwidth, node.GetTotalWidth(paddingWidth, depth -1));

			// add both
			return width + childwidth;
		}

		/// <summary>
		/// Returns the total size of the node and its child nodes.
		/// </summary>
		/// <param name="paddingWidth">The width kept between the nodes.</param>
		/// <param name="depth">Defines how deep the search will be.</param>
		/// <returns>Returns the untransformed size.</returns>
		public SizeF GetTotalSize(float paddingWidth, int depth)
		{
			return new SizeF(GetTotalWidth(paddingWidth, depth), _layoutRectangle.Height);
		}

		public NodeViewData GetChild(Node node)
		{
			foreach(NodeViewData child in _children)
			{
				if(child.Node ==node)
					return child;
			}

			return null;
		}

		/// <summary>
		/// Draws the edges connecting the nodes.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="edgePen">The pen we use for physical nodes.</param>
		/// <param name="edgePenReadOnly">The pen we use for sub-referenced nodes.</param>
		/// <param name="renderDepth">The depth which is still rendered.</param>
		public virtual void DrawEdges(Graphics graphics, Pen edgePen, Pen edgePenReadOnly, int renderDepth)
		{
			DrawEdges(graphics, this, edgePen, edgePenReadOnly);

			// draw children
			if(renderDepth >0)
			{
				foreach(NodeViewData child in _children)
					child.DrawEdges(graphics, edgePen, edgePenReadOnly, renderDepth -1);
			}
		}

		/// <summary>
		/// Draws the node to the graph.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="currentNode">The current node under the mouse cursor.</param>
		/// <param name="selectedNode">The currently selected node.</param>
		/// <param name="isDragged">Determines if the node is currently being dragged.</param>
		/// <param name="graphMousePos">The mouse position in the untransformed graph.</param>
		/// <param name="renderDepth">The depth which is still rendered.</param>
		public virtual void Draw(Graphics graphics, NodeViewData currentNode, NodeViewData selectedNode, bool isDragged, PointF graphMousePos, int renderDepth)
		{
			Draw(graphics, this, currentNode ==null ? false : this.Node ==currentNode.Node, selectedNode ==null ? false : this.Node ==selectedNode.Node, isDragged, graphMousePos);

			// draw children
			if(renderDepth >0)
			{
				foreach(NodeViewData child in _children)
					child.Draw(graphics, currentNode, selectedNode, isDragged, graphMousePos, renderDepth -1);
			}
		}

		/// <summary>
		/// Draws the background of the node's comment.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="renderDepth">The depth which is still rendered.</param>
		/// <param name="padding">The padding between the nodes.</param>
		public void DrawCommentBackground(Graphics graphics, int renderDepth, SizeF padding)
		{
			// draw comment backgrounds
			DrawCommentBackground(graphics, this, renderDepth, padding);

			// draw children
			if(renderDepth >0)
			{
				foreach(NodeViewData child in _children)
					child.DrawCommentBackground(graphics, renderDepth -1, padding);
			}
		}

		/// <summary>
		/// Draws the text of the node's comment.
		/// </summary>
		/// <param name="graphics">The graphics object we render to.</param>
		/// <param name="renderDepth">The depth which is still rendered.</param>
		public void DrawCommentText(Graphics graphics, int renderDepth)
		{
			// draw comment backgrounds
			DrawCommentText(graphics, this);

			// draw children
			if(renderDepth >0)
			{
				foreach(NodeViewData child in _children)
					child.DrawCommentText(graphics, renderDepth -1);
			}
		}

		protected void node_WasModified(Node node)
		{
			GenerateNewLabel();
		}

		/// <summary>
		/// Returns if any of the node's parents is a given behaviour.
		/// </summary>
		/// <param name="behavior">The behavior we want to check if it is an ancestor of this node.</param>
		/// <returns>Returns true if this node is a descendant of the given behavior.</returns>
		public virtual bool HasParentBehavior(BehaviorNode behavior)
		{
			if(behavior ==null)
				return false;

			if(_node ==behavior)
				return true;

			if(Parent ==null)
				return false;

			return Parent.HasParentBehavior(behavior);
		}
	}
}
