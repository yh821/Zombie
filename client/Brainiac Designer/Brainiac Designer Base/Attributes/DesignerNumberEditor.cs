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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Brainiac.Design.Attributes
{
	public partial class DesignerNumberEditor : Brainiac.Design.Attributes.DesignerPropertyEditor
	{
		public DesignerNumberEditor()
		{
			InitializeComponent();
		}

		public override void SetProperty(DesignerPropertyInfo property, object obj)
		{
			base.SetProperty(property, obj);

			// check if there is an override for this paroperty
			Nodes.Node node= _object as Nodes.Node;
			if(node !=null && node.HasOverrride(property.Property.Name))
			{
				numericUpDown.Enabled= false;

				return;
			}

			DesignerPropertyInfo restrictions= property;

			bool linkBroken;
			DesignerPropertyInfo linkedProperty= property.Attribute.GetLinkedProperty(obj, out linkBroken);

			// control cannot be used with a broken link
			if(linkBroken)
			{
				numericUpDown.Enabled= false;

				return;
			}

			// if we have a linked property this property will define the restrictions
			if(linkedProperty.Property !=null)
			{
				restrictions= linkedProperty;
			}

			// extract resrictions for float property
			DesignerFloat restFloatAtt= restrictions.Attribute as DesignerFloat;
			if(restFloatAtt !=null)
			{
				numericUpDown.DecimalPlaces= restFloatAtt.Precision;
				numericUpDown.Minimum= (decimal)restFloatAtt.Min;
				numericUpDown.Maximum= (decimal)restFloatAtt.Max;
				numericUpDown.Increment= (decimal)restFloatAtt.Steps;

				unitLabel.Text= restFloatAtt.Units;
			}

			// extract restrictions for int property
			DesignerInteger restIntAtt= restrictions.Attribute as DesignerInteger;
			if(restIntAtt !=null)
			{
				numericUpDown.DecimalPlaces= 0;
				numericUpDown.Minimum= (decimal)restIntAtt.Min;
				numericUpDown.Maximum= (decimal)restIntAtt.Max;
				numericUpDown.Increment= (decimal)restIntAtt.Steps;

				unitLabel.Text= restIntAtt.Units;
			}

			// extract the value
			decimal value= 0;

			DesignerFloat floatAtt= property.Attribute as DesignerFloat;
			if(floatAtt !=null)
			{
				float val= (float)property.Property.GetValue(obj, null);

				value= (decimal)val;
			}

			DesignerInteger intAtt= property.Attribute as DesignerInteger;
			if(intAtt !=null)
			{
				int val= (int)property.Property.GetValue(obj, null);

				value= (decimal)val;
			}

			// assign value within limits
			numericUpDown.Value= Math.Max(numericUpDown.Minimum, Math.Min(numericUpDown.Maximum, value));
		}

		public override void ReadOnly()
		{
			base.ReadOnly();

			numericUpDown.Enabled= false;
		}

		private void numericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if(!_valueWasAssigned)
				return;

			if(_property.Attribute is DesignerFloat)
				_property.Property.SetValue(_object, (float)numericUpDown.Value, null);

			if(_property.Attribute is DesignerInteger)
				_property.Property.SetValue(_object, (int)numericUpDown.Value, null);

			OnValueChanged(_property);
		}
	}
}
