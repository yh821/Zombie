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
using Brainiac.Design;
using Brainiac.Design.Attributes;
using Brainiac.Design.Properties;
using Brainiac.Design.Nodes;

namespace Brainiac.Design.Attachments.Overrides
{
	public class OverrideRandom : Override
	{
		public OverrideRandom(Node node) : base(node, Resources.OverrideRandom, Resources.OverrideRandomDesc)
		{
		}

		protected override void CloneProperties(Attachment newattach)
		{
			base.CloneProperties(newattach);

			Override newoverride= (Override)newattach;
		}

		protected float _min;

		[DesignerFloat("RandomMin", "RandomMinDesc", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, "PropertyToOverride", 0, 0, 0, 0, null)]
		public float Min
		{
			get { return _min; }
			set { _min= value; }
		}

		protected float _max;

		[DesignerFloat("RandomMax", "RandomMaxDesc", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 1, DesignerProperty.DesignerFlags.NoFlags, "PropertyToOverride", 0, 0, 0, 0, null)]
		public float Max
		{
			get { return _max; }
			set { _max= value; }
		}
	}
}
