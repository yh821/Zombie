////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011, Daniel Kollmann
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
using Brainiac.Design.Properties;
using Brainiac.Design.Attributes;

namespace Brainiac.Design.Attachments
{
	/// <summary>
	/// This class represents objects that can be attached to nodes like events or overrides.
	/// </summary>
	public abstract class Attachment : NodeTag.DefaultObject
	{
		/// <summary>
		/// Creates an attachment from a given type.
		/// </summary>
		/// <param name="type">The type we want to create an attachment of.</param>
		/// <param name="node">The node this will be added to.</param>
		/// <returns>Returns the created event.</returns>
		public static Attachment Create(Type type, Nodes.Node node)
		{
			Debug.Check(type !=null);

			Attachment atta= (Attachment)type.InvokeMember(string.Empty, System.Reflection.BindingFlags.CreateInstance, null, null, new object[] { node });

			if(atta ==null)
				throw new Exception(Resources.ExceptionMissingEventConstructor);

			return atta;
		}

		protected Nodes.Node _node;

		/// <summary>
		/// The node we are attached to.
		/// </summary>
		public Nodes.Node Node
		{
			get { return _node; }
		}

		private string _label;
		private string _baselabel;
		public string Label
		{
			get { return _label; }

			set
			{
				_label= value; //Resources.ResourceManager.GetString(value, Resources.Culture);

				// store the original label so we can automatically generate a new label when an ttribute changes.
				if(_baselabel ==string.Empty)
					_baselabel= _label;

				// when the label changes the size of the node might change as well
				if(_label !=_baselabel)
					_node.DoWasModified();
			}
		}

		protected string _description;

		/// <summary>
		/// The description of this node.
		/// </summary>
		public string Description
		{
			get { return /*Resources.ResourceManager.GetString(*/_description/*, Resources.Culture)*/; }
		}

		protected Attachment(Nodes.Node node, string label, string description)
		{
			_node= node;
			_label= label;
			_baselabel= label;
			_description= description;
		}

		/// <summary>
		/// Is called when one of the event's proterties were modified.
		/// </summary>
		/// <param name="wasModified">Holds if the event was modified.</param>
		public virtual void OnPropertyValueChanged(bool wasModified)
		{
			_node.OnPropertyValueChanged(wasModified);

			Label= GenerateNewLabel();
		}

		public override string ToString()
		{
			return _label;
		}

		public Attachment Clone(Nodes.Node newnode)
		{
			Attachment atta= Create(GetType(), newnode);

			CloneProperties(atta);

			atta.OnPropertyValueChanged(false);

			return atta;
		}

		protected virtual void CloneProperties(Attachment newattach)
		{
		}

		/// <summary>
		/// Returns a list of all properties which have a designer attribute attached.
		/// </summary>
		/// <returns>A list of all properties relevant to the designer.</returns>
		public IList<DesignerPropertyInfo> GetDesignerProperties()
		{
			return DesignerProperty.GetDesignerProperties(GetType());
		}

		/// <summary>
		/// Returns a list of all properties which have a designer attribute attached.
		/// </summary>
		/// <param name="comparison">The comparison used to sort the design properties.</param>
		/// <returns>A list of all properties relevant to the designer.</returns>
		public IList<DesignerPropertyInfo> GetDesignerProperties(Comparison<DesignerPropertyInfo> comparison)
		{
			return DesignerProperty.GetDesignerProperties(GetType(), comparison);
		}

		/// <summary>
		/// Generates a new label by adding the attributes to the label as arguments
		/// </summary>
		/// <returns>Returns the label with a list of arguments.</returns>
		protected string GenerateNewLabel()
		{
			// generate the new label with the arguments
			string newlabel= _baselabel +"(";
			int paramCount= 0;

			// check all properties for one which must be shown as a parameter on the node
			IList<DesignerPropertyInfo> properties= GetDesignerProperties(DesignerProperty.SortByDisplayOrder);
			for(int p= 0; p <properties.Count; ++p)
			{
				// property must be shown as a parameter on the node
				if(properties[p].Attribute.Display ==DesignerProperty.DisplayMode.Parameter)
				{
					newlabel+= properties[p].GetDisplayValue(this) +", ";
					paramCount++;
				}
			}

			// only return the new label when it contains any parameters
			return paramCount >0 ? newlabel.Substring(0, newlabel.Length -2) +")" : _baselabel;
		}

		public abstract NodeViewData.SubItemAttachment CreateSubItem();
	}
}
