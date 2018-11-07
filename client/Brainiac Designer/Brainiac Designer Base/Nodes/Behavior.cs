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
using Brainiac.Design.Attachments.Events;

namespace Brainiac.Design.Nodes
{
	public interface BehaviorNode
	{
		FileManagers.FileManager FileManager { get; set; }
		string MakeAbsolute(string filename);
		string MakeRelative(string filename);
		int ModificationID { get; }
		event Behavior.WasSavedEventDelegate WasSaved;
		event Behavior.WasRenamedEventDelegate WasRenamed;
		void TriggerWasRenamed();
		bool IsModified { get; }
		string GetPathLabel(string behaviorFolder);
		Node.Connector GenericChildren { get; }
	}

	/// <summary>
	/// This node represents the behaviour tree's root node.
	/// </summary>
	public class Behavior : Node, BehaviorNode
	{
		private readonly static Brush __theBackgroundBrush= new SolidBrush( Color.FromArgb(119,147,60) );

		protected Connector _genericChildren;
		public Connector GenericChildren
		{
			get { return _genericChildren; }
		}

		private bool _isModified= false;

		/// <summary>
		/// This flag determines if the behaviour was modified, so it can be saved when closing the editor.
		/// </summary>
		public bool IsModified
		{
			get { return _isModified; }
		}

		protected FileManagers.FileManager _fileManager= null;

		/// <summary>
		/// The file manager the behaviour was saved with. Required so you can simply click save later on.
		/// </summary>
		public FileManagers.FileManager FileManager
		{
			get { return _fileManager; }

			set
			{
				_fileManager= value;

				_isModified= _fileManager ==null || _fileManager.Filename ==string.Empty;

				if(!_isModified)
					Label= Path.GetFileNameWithoutExtension(_fileManager.Filename);

				if(!_isModified && WasSaved !=null)
					WasSaved(this);
			}
		}

		/// <summary>
		/// The filename of the behaviour.
		/// </summary>
		[DesignerString("BehaviorFilename", "BehaviorFilenameDesc", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.ReadOnly|DesignerProperty.DesignerFlags.NoExport|DesignerProperty.DesignerFlags.NoSave)]
		public string Filename
		{
			get { return _fileManager ==null ? string.Empty : _fileManager.Filename; }
		}

		public Behavior(string label, bool createConnector) : base(label, "BehaviorDesc")
		{
			if(createConnector)
				_genericChildren= new ConnectorSingle(_children, string.Empty, "GenericChildren");
		}

		public override bool AcceptsAttachment(Type type)
		{
			return !type.IsSubclassOf(typeof(Event));
		}

		public Behavior(string label) : this(label, true)
		{
		}

		public Behavior() : this(string.Empty)
		{
		}

		public override NodeViewData CreateNodeViewData(NodeViewData parent, BehaviorNode rootBehavior)
		{
			return new NodeViewDataStyled(parent, rootBehavior, this, null, __theBackgroundBrush, _label, _description);
		}

		/// <summary>
		/// Makes a filename relative to the filename of this behaviour. Used for referenced behaviours.
		/// </summary>
		/// <param name="filename">The filename which will become relative to this behaviour.</param>
		/// <returns>Returns the relative filename of the filename parameter.</returns>
		public string MakeRelative(string filename)
		{
			return FileManagers.FileManager.MakeRelative(Path.GetDirectoryName(_fileManager.Filename), filename);
		}

		/// <summary>
		/// Makes a filename absolute which is relative to the filename of this behaviour. Used for referenced behaviours.
		/// </summary>
		/// <param name="filename">The filename which is relative and will become absolute.</param>
		/// <returns>Returns the sbolute filename of the filename parameter.</returns>
		public string MakeAbsolute(string filename)
		{
			return FileManagers.FileManager.MakeAbsolute(Path.GetDirectoryName(_fileManager.Filename), filename);
		}

		public delegate void WasSavedEventDelegate(BehaviorNode node);

		/// <summary>
		/// Is called when the behaviour was saved.
		/// </summary>
		public event WasSavedEventDelegate WasSaved;

		public delegate void WasRenamedEventDelegate(BehaviorNode node);

		/// <summary>
		/// Is called when the behaviour is renamed.
		/// </summary>
		public event WasRenamedEventDelegate WasRenamed;

		public void TriggerWasRenamed()
		{
			if(WasRenamed !=null)
				WasRenamed(this);
		}

		protected int _modificationID= 0;

		/// <summary>
		/// The ID of the last modification. This allows the referenced behaviours to check if they still represent the latest version of the behaviour.
		/// </summary>
		public int ModificationID
		{
			get { return _modificationID; }
		}

		/// <summary>
		/// Marks this behaviour as being modified.
		/// </summary>
		/// <param name="layoutChanged">If true causes the view to recalculate the layout.</param>
		public override void BehaviorWasModified()
		{
			_isModified= true;

			// update the modification ID
			_modificationID++;

			// call WasModified event
			DoWasModified();
		}

		/// <summary>
		/// Returns the relative filename and path of the behaviour.
		/// Used to show a unique behaviour name in the CheckForErrors dialogue.
		/// </summary>
		/// <param name="behaviorFolder">The root folder of the behaviour.</param>
		/// <returns>Returns a string with the relative path of this behaviour.</returns>
		public string GetPathLabel(string behaviorFolder)
		{
			string label= Label;
			if(FileManager !=null && FileManager.Filename !=string.Empty)
			{
				// cut away the behaviour folder
				label= FileManager.Filename.Substring(behaviorFolder.Length +1);

				// remove the extension
				string ext= System.IO.Path.GetExtension(label);
				label= label.Substring(0, label.Length - ext.Length);

				// replace ugly back-slashes with nicer forward-slashes
				label= label.Replace('\\', '/');
			}

			return label;
		}

		public override void CheckForErrors(BehaviorNode rootBehavior, List<ErrorCheck> result)
		{
			// check if the node has any children
			if(_genericChildren.ChildCount <1)
				result.Add( new Node.ErrorCheck(this, ErrorCheckLevel.Error, "BehaviorIsEmptyError") );

			base.CheckForErrors(rootBehavior, result);
		}
	}
}
