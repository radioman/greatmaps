using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class Mashup
	{
		private const string mashupFileTag = "MapGrinderMashupFile";
		private bool dirty;
		private bool autoSaveDirty;
		private bool autoSaveFailNotified;
		public DirtyEvent dirtyEvent = new DirtyEvent();
		public DirtyEvent readyToLockEvent = new DirtyEvent();
		private string fileName;
		private RenderOptions renderOptions;
		private LayerList _layerList;
		private ICurrentView _lastView;
		private static string LastViewTag = "LastView";
		private static string LastView_TargetIdAttr = "TargetId";
		public ICurrentView lastView
		{
			get
			{
				return this._lastView;
			}
		}
		public LayerList layerList
		{
			get
			{
				return this._layerList;
			}
		}
		public void SetDirty()
		{
			this.dirtyEvent.SetDirty();
		}
		public Mashup()
		{
			this.dirtyEvent.Add(new DirtyListener(this.SetDirtyFlag));
			this._layerList = new LayerList(this.dirtyEvent);
			this.renderOptions = new RenderOptions(this.dirtyEvent);
		}
		private void SetDirtyFlag()
		{
			this.dirty = true;
			this.autoSaveDirty = true;
		}
		public bool IsDirty()
		{
			return this.dirty;
		}
		private void ClearDirty()
		{
			this.dirty = false;
			this.autoSaveDirty = false;
		}
		public RenderOptions GetRenderOptions()
		{
			return this.renderOptions;
		}
		public void SetFilename(string fileName)
		{
			if (File.Exists(Mashup.GetAutoSaveName(this.fileName)))
			{
				this.RemoveAutoSaveBackup();
				this.autoSaveDirty = true;
			}
			this.fileName = fileName;
			D.Assert(Path.GetFullPath(fileName).ToLower().Equals(fileName.ToLower()));
			if (this.autoSaveDirty)
			{
				this.AutoSaveBackup();
			}
		}
		public string GetFilename()
		{
			return this.fileName;
		}
		public string GetDisplayName()
		{
			string text = this.fileName;
			if (text == null)
			{
				text = "Untitled Mashup";
			}
			else
			{
				text = text.Remove(0, text.LastIndexOf('\\') + 1);
			}
			return text;
		}
		public string GetPublishedFilename()
		{
			string str;
			if (this.GetFilename() == null)
			{
				str = "unsaved";
			}
			else
			{
				str = Path.GetFileName(this.GetFilename());
			}
			return str + ".xml";
		}
		public string GetFilenameContext()
		{
			if (this.fileName == null)
			{
				return "";
			}
			return Path.GetDirectoryName(this.fileName);
		}
		public void WriteXML()
		{
			D.Assert(this.fileName != null);
			this.WriteXML(this.fileName);
			this.ClearDirty();
			this.RemoveAutoSaveBackup();
		}
		public void WriteXML(Stream outStream)
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(outStream, Encoding.UTF8);
			using (xmlTextWriter)
			{
				MashupWriteContext wc = new MashupWriteContext(xmlTextWriter);
				this.WriteXML(wc);
			}
		}
		private void WriteXML(string saveName)
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(saveName, Encoding.UTF8);
			using (xmlTextWriter)
			{
				MashupWriteContext wc = new MashupWriteContext(xmlTextWriter);
				this.WriteXML(wc);
			}
		}
		private void WriteXML(MashupWriteContext wc)
		{
			XmlTextWriter writer = wc.writer;
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument(true);
			writer.WriteStartElement("MapGrinderMashupFile");
			CurrentSchema.schema.WriteXMLAttribute(writer);
			this.renderOptions.WriteXML(writer);
			this._layerList.WriteXML(wc);
			this.WriteXML_LastView(wc);
			writer.WriteEndElement();
			writer.Close();
		}
		private void WriteXML_LastView(MashupWriteContext wc)
		{
			wc.writer.WriteStartElement(Mashup.LastViewTag);
			if (this.lastView != null && !(this.lastView is NoView))
			{
				wc.writer.WriteAttributeString(Mashup.LastView_TargetIdAttr, wc.GetIdentity(this.lastView.GetViewedObject()));
			}
			wc.writer.WriteEndElement();
		}
		public void ReadXML(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("MapGrinderMashupFile");
			context.version = MashupXMLSchemaVersion.ReadXMLAttribute(context.reader);
			SingleMaxZoomForEntireMashupCompatibilityBlob singleMaxZoomForEntireMashupCompatibilityBlob = null;
			string text = null;
			while (xMLTagReader.FindNextStartTag())
			{
				if (context.version != MonolithicMapPositionsSchema.schema && xMLTagReader.TagIs(LayerList.GetXMLTag()))
				{
					this._layerList = new LayerList(context, new SourceMap.GetFilenameContext(this.GetFilenameContext), this.dirtyEvent, this.readyToLockEvent);
				}
				else
				{
					if (context.version == MonolithicMapPositionsSchema.schema && xMLTagReader.TagIs(SourceMap.GetXMLTag()))
					{
						if (this._layerList != null && this._layerList.Count > 0)
						{
							throw new InvalidMashupFile(context, string.Format("Multiple SourceMaps in Version {0} file.", context.version.versionNumberString));
						}
						SourceMap sourceMap = new SourceMap(context, new SourceMap.GetFilenameContext(this.GetFilenameContext), this.dirtyEvent, this.readyToLockEvent);
						this._layerList = new LayerList(this.dirtyEvent);
						this._layerList.AddNewLayer();
						this._layerList.First.Add(sourceMap);
					}
					else
					{
						if (xMLTagReader.TagIs(RenderOptions.GetXMLTag()))
						{
							this.renderOptions = new RenderOptions(context, this.dirtyEvent, ref singleMaxZoomForEntireMashupCompatibilityBlob);
						}
						else
						{
							if (xMLTagReader.TagIs(Mashup.LastViewTag))
							{
								XMLTagReader xMLTagReader2 = context.NewTagReader(Mashup.LastViewTag);
								text = context.reader.GetAttribute(Mashup.LastView_TargetIdAttr);
								xMLTagReader2.SkipAllSubTags();
							}
						}
					}
				}
			}
			this._lastView = new NoView();
			if (text != null)
			{
				object obj = context.FetchObjectByIdentity(text);
				if (obj != null && obj is LastViewIfc)
				{
					this._lastView = ((LastViewIfc)obj).lastView;
				}
			}
			if (this.renderOptions == null)
			{
				if (context.version != MonolithicMapPositionsSchema.schema)
				{
					context.warnings.Add(new MashupFileWarning("RenderOptions tag absent."));
				}
				this.renderOptions = new RenderOptions(this.dirtyEvent);
			}
			if (singleMaxZoomForEntireMashupCompatibilityBlob != null)
			{
				D.Assert(context.version == SingleMaxZoomForEntireMashupSchema.schema);
				foreach (Layer current in this._layerList)
				{
					foreach (SourceMap current2 in current)
					{
						current2.sourceMapRenderOptions.maxZoom = singleMaxZoomForEntireMashupCompatibilityBlob.maxZoom;
					}
				}
			}
		}
		public static Mashup OpenMashupInteractive(string fileName, out MashupFileWarningList warningList)
		{
			if (File.Exists(Mashup.GetAutoSaveName(fileName)))
			{
				RecoverAutoSavedFileDialog recoverAutoSavedFileDialog = new RecoverAutoSavedFileDialog();
				recoverAutoSavedFileDialog.Initialize(Mashup.GetAutoSaveName(fileName));
				DialogResult dialogResult = recoverAutoSavedFileDialog.ShowDialog();
				if (dialogResult == DialogResult.Yes)
				{
					Mashup mashup = new Mashup(Mashup.GetAutoSaveName(fileName), out warningList);
					mashup.fileName = Path.Combine(Path.GetDirectoryName(fileName), "Copy of " + Path.GetFileName(fileName));
					mashup.SetDirty();
					mashup.AutoSaveBackup();
					File.Delete(Mashup.GetAutoSaveName(fileName));
					return mashup;
				}
				if (dialogResult == DialogResult.Ignore)
				{
					File.Delete(Mashup.GetAutoSaveName(fileName));
				}
				else
				{
					if (dialogResult == DialogResult.Cancel)
					{
						warningList = null;
						return null;
					}
					D.Assert(false, "Invalid enum");
				}
			}
			return new Mashup(fileName, out warningList);
		}
		public Mashup(string fileName, out MashupFileWarningList warningList) : this(fileName, File.Open(fileName, FileMode.Open, FileAccess.Read), out warningList)
		{
		}
		private Mashup(string fileName, Stream fromStream, out MashupFileWarningList warningList)
		{
			this.dirtyEvent.Add(new DirtyListener(this.SetDirtyFlag));
			this.fileName = fileName;
			D.Assert(fileName == null || Path.GetFullPath(fileName).ToLower().Equals(fileName.ToLower()));
			bool flag = false;
			XmlTextReader reader = new XmlTextReader(fromStream);
			MashupParseContext mashupParseContext = new MashupParseContext(reader);
			using (mashupParseContext)
			{
				while (mashupParseContext.reader.Read() && !flag)
				{
					if (mashupParseContext.reader.NodeType == XmlNodeType.Element && mashupParseContext.reader.Name == "MapGrinderMashupFile")
					{
						flag = true;
						this.ReadXML(mashupParseContext);
					}
				}
				mashupParseContext.Dispose();
			}
			warningList = null;
			if (mashupParseContext.warnings.Count > 0)
			{
				warningList = mashupParseContext.warnings;
			}
			if (!flag)
			{
				throw new InvalidMashupFile(mashupParseContext, string.Format("{0} doesn't appear to be a valid mashup file.", fileName));
			}
			this.ClearDirty();
		}
		internal void AutoSaveBackup()
		{
			if (!this.autoSaveDirty)
			{
				return;
			}
			try
			{
				this.WriteXML(Mashup.GetAutoSaveName(this.fileName));
				this.autoSaveDirty = false;
				this.autoSaveFailNotified = false;
			}
			catch (Exception ex)
			{
				if (!this.autoSaveFailNotified)
				{
					this.autoSaveFailNotified = true;
					MessageBox.Show(string.Format("Failed to autosave {0}:\n{1}", Mashup.GetAutoSaveName(this.fileName), ex.Message), "AutoSave Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
		private static string GetAutoSaveName(string fileName)
		{
			if (fileName == null)
			{
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				return Path.Combine(folderPath, "backup.Unnamed Crunchup.yum");
			}
			return Path.Combine(Path.GetDirectoryName(fileName), "backup." + Path.GetFileName(fileName));
		}
		private void RemoveAutoSaveBackup()
		{
			try
			{
				File.Delete(Mashup.GetAutoSaveName(this.fileName));
			}
			catch (Exception ex)
			{
				D.Say(0, "Mashup.Close(): " + ex.ToString());
			}
		}
		internal void Close()
		{
			this.RemoveAutoSaveBackup();
		}
		public void SetLastView(ICurrentView lastView)
		{
			this._lastView = lastView;
		}
		internal void AutoSelectMaxZooms(MapTileSourceFactory mapTileSourceFactory)
		{
			this._layerList.AutoSelectMaxZooms(mapTileSourceFactory);
		}
		internal Mashup Duplicate()
		{
			MemoryStream memoryStream = new MemoryStream();
			using (memoryStream)
			{
				this.WriteXML(new MashupWriteContext(new XmlTextWriter(memoryStream, null)));
			}
			MemoryStream memoryStream3 = new MemoryStream(memoryStream.ToArray());
			Mashup result;
			using (memoryStream3)
			{
				MashupFileWarningList mashupFileWarningList;
				Mashup mashup = new Mashup(this.fileName, memoryStream3, out mashupFileWarningList);
				D.Assert(mashupFileWarningList == null);
				result = mashup;
			}
			return result;
		}
		internal bool SomeSourceMapIsReadyToLock()
		{
			return this.layerList.SomeSourceMapIsReadyToLock();
		}
	}
}
