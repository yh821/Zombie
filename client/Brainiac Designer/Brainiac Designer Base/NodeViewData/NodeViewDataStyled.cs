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
using TTRider.UI;

namespace Brainiac.Design
{
	public class NodeViewDataStyled : NodeViewData
	{
		protected readonly static Pen __defaultCurrentBorderPen= new Pen(Brushes.Black, 2.0f);
		protected readonly static Pen __defaultSelectedBorderPen= new Pen(Brushes.Black, 2.0f);
		protected readonly static Font __defaultLabelFont= new Font("Calibri,Arial", 8.0f, FontStyle.Regular);

		public static Brush GetDraggedBrush(Brush brush)
		{
			// extract the color
			Color clr= new Pen(brush).Color;

			// generate the hsb color
			HSBColor hsb= new HSBColor(clr);

			// generate the dragged color
			HSBColor draggedhsb= new HSBColor(hsb.A, hsb.H, hsb.S, hsb.B -10.0f);

			// check if we got a solid brush so we also return one
			SolidBrush sb= brush as SolidBrush;
			if(sb !=null)
				return new SolidBrush(draggedhsb.Color);

			// unhandled brush type
			Debug.Check(false);

			// if the brush type was not handled in release mode we return a solid brush by default
			return new SolidBrush(draggedhsb.Color);
		}

		public void ChangeShape(NodeShape shape)
		{
			_shape= shape;
		}

		public NodeViewDataStyled(NodeViewData parent, BehaviorNode rootBehavior, Node node, Pen borderPen, Brush backgroundBrush, string label, string description) :
			base(parent, rootBehavior, node,
				NodeShape.RoundedRectangle,
				new Style(backgroundBrush, null, Brushes.White),
				new Style(null, __defaultCurrentBorderPen, null),
				new Style(null, __defaultSelectedBorderPen, null),
				new Style(GetDraggedBrush(backgroundBrush), null, null),
				label, __defaultLabelFont, 120, 35, description)
		{
		}

		public NodeViewDataStyled(NodeViewData parent, BehaviorNode rootBehavior, Node node, Pen borderPen, Brush backgroundBrush, Brush draggedBackgroundBrush, string label, string description) :
			base(parent, rootBehavior, node,
				NodeShape.RoundedRectangle,
				new Style(backgroundBrush, null, Brushes.White),
				new Style(null, __defaultCurrentBorderPen, null),
				new Style(null, __defaultSelectedBorderPen, null),
				new Style(draggedBackgroundBrush, null, null),
				label, __defaultLabelFont, 120, 35, description)
		{
		}
	}
}
