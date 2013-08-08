using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Globalization;
namespace MSR.CVE.BackMaker
{
	public class Layer : HasDisplayNameIfc, PositionMemoryIfc, IRobustlyHashable, ExpandedMemoryIfc, LastViewIfc
	{
		private const string LayerTag = "Layer";
		private const string LayerDisplayNameAttr = "DisplayName";
		private const string LayerExpandedAttr = "Expanded";
		private const string SimulateTransparencyWithVEBackingLayerAttr = "SimulateTransparencyWithVEBackingLayer";
		private const string NewLayerName = "New Layer";
		private const string LastLayerViewPositionTag_compat = "LastLayerViewPosition";
		private List<SourceMap> sourceMaps = new List<SourceMap>();
		private RenderClip _renderClip = new RenderClip();
		private bool _expanded = true;
		private string _displayName;
		private DirtyEvent dirtyEvent;
		private LayerView _lastView;
		private string _simulateTransparencyWithVEBackingLayer;
		public ICurrentView lastView
		{
			get
			{
				return this._lastView;
			}
		}
		public bool expanded
		{
			get
			{
				return this._expanded;
			}
			set
			{
				this._expanded = value;
			}
		}
		public string displayName
		{
			get
			{
				return this._displayName;
			}
			set
			{
				this._displayName = value;
				this.dirtyEvent.SetDirty();
			}
		}
		public string simulateTransparencyWithVEBackingLayer
		{
			get
			{
				return this._simulateTransparencyWithVEBackingLayer;
			}
		}
		public RenderClip renderClip
		{
			get
			{
				return this._renderClip;
			}
		}
		public SourceMap First
		{
			get
			{
				return this.sourceMaps[0];
			}
		}
		public int Count
		{
			get
			{
				return this.sourceMaps.Count;
			}
		}
		public string GetDisplayName()
		{
			return this.displayName;
		}
		public void SetDisplayName(string value)
		{
			this.displayName = value;
		}
		public Layer(LayerList otherLayers, DirtyEvent parentDirty)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
			int num = 1;
			string text = "New Layer";
			while (otherLayers.HasLayerNamed(text))
			{
				num++;
				text = string.Format("{0} {1}", "New Layer", num);
			}
			this._displayName = text;
		}
		public Layer(MashupParseContext context, SourceMap.GetFilenameContext filenameContextDelegate, DirtyEvent parentDirty, DirtyEvent parentReadyToLockEvent)
		{
			this.dirtyEvent = new DirtyEvent(parentDirty);
			XMLTagReader xMLTagReader = context.NewTagReader("Layer");
			context.ExpectIdentity(this);
			string attribute = context.reader.GetAttribute("DisplayName");
			if (attribute != null)
			{
				this._displayName = attribute;
				context.GetAttributeBoolean("Expanded", ref this._expanded);
				string attribute2 = context.reader.GetAttribute("SimulateTransparencyWithVEBackingLayer");
				if (attribute2 != null)
				{
					this._simulateTransparencyWithVEBackingLayer = attribute2;
				}
				while (xMLTagReader.FindNextStartTag())
				{
					if (xMLTagReader.TagIs(SourceMap.GetXMLTag()))
					{
						this.Add(new SourceMap(context, filenameContextDelegate, this.dirtyEvent, parentReadyToLockEvent));
					}
					else
					{
						if (xMLTagReader.TagIs(LayerView.GetXMLTag()))
						{
							this._lastView = new LayerView(this, context);
						}
						else
						{
							if (xMLTagReader.TagIs("LastLayerViewPosition"))
							{
								XMLTagReader xMLTagReader2 = context.NewTagReader("LastLayerViewPosition");
								MapPosition mapPosition = null;
								while (xMLTagReader2.FindNextStartTag())
								{
									if (xMLTagReader2.TagIs(MapPosition.GetXMLTag(context.version)))
									{
										mapPosition = new MapPosition(context, null, MercatorCoordinateSystem.theInstance);
									}
								}
								if (mapPosition != null)
								{
									this._lastView = new LayerView(this, mapPosition);
								}
							}
							else
							{
								if (xMLTagReader.TagIs(RenderClip.GetXMLTag()))
								{
									this._renderClip = new RenderClip(context);
								}
							}
						}
					}
				}
				return;
			}
			throw new InvalidMashupFile(context, "Expected displayName attribute");
		}
		public void WriteXML(MashupWriteContext wc)
		{
			wc.writer.WriteStartElement("Layer");
			wc.writer.WriteAttributeString("DisplayName", this._displayName);
			wc.writer.WriteAttributeString("Expanded", this._expanded.ToString(CultureInfo.InvariantCulture));
			wc.writer.WriteAttributeString("SimulateTransparencyWithVEBackingLayer", this._simulateTransparencyWithVEBackingLayer);
			wc.WriteIdentityAttr(this);
			foreach (SourceMap current in this)
			{
				current.WriteXML(wc);
			}
			if (this.lastView != null)
			{
				this._lastView.WriteXML(wc.writer);
			}
			this._renderClip.WriteXML(wc);
			wc.writer.WriteEndElement();
		}
		internal static string GetXMLTag()
		{
			return "Layer";
		}
		internal static string GetLayerDisplayNameTag()
		{
			return "DisplayName";
		}
		internal string GetFilesystemName()
		{
			return RenderState.ForceValidFilename(string.Format("Layer_{0}", this.displayName));
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate("Layer(");
			hash.Accumulate(this.displayName);
			foreach (SourceMap current in this.sourceMaps)
			{
				current.AccumulateRobustHash(hash);
			}
			hash.Accumulate(")");
		}
		public void AccumulateRobustHash_PerTile(CachePackage cachePackage, IRobustHash hash)
		{
			hash.Accumulate("Layer(");
			foreach (SourceMap current in this.sourceMaps)
			{
				current.AccumulateRobustHash_PerTile(cachePackage, hash);
			}
			hash.Accumulate(")");
		}
		public List<SourceMap>.Enumerator GetEnumerator()
		{
			return this.sourceMaps.GetEnumerator();
		}
		public List<SourceMap> GetBackToFront()
		{
			List<SourceMap> list = new List<SourceMap>(this.sourceMaps);
			list.Reverse();
			return list;
		}
		public void Add(SourceMap sourceMap)
		{
			this.sourceMaps.Add(sourceMap);
			this.dirtyEvent.SetDirty();
		}
		internal bool Contains(SourceMap sourceMap)
		{
			return this.sourceMaps.Contains(sourceMap);
		}
		internal void Remove(SourceMap sourceMap)
		{
			this.sourceMaps.Remove(sourceMap);
			this.dirtyEvent.SetDirty();
		}
		internal void AutoSelectMaxZooms(MapTileSourceFactory mapTileSourceFactory)
		{
			foreach (SourceMap current in this.sourceMaps)
			{
				current.AutoSelectMaxZoom(mapTileSourceFactory);
			}
		}
		public void NotePositionUnlocked(LatLonZoom sourceMapPosition, MapPosition referenceMapPosition)
		{
			D.Assert(false, "Layers are never unlocked.");
		}
		public void NotePositionLocked(MapPosition referenceMapPosition)
		{
			this._lastView = new LayerView(this, referenceMapPosition);
		}
		internal void AddAt(SourceMap sourceMap, int index)
		{
			this.sourceMaps.Insert(index, sourceMap);
		}
		internal int GetIndexOfSourceMap(SourceMap targetSourceMap)
		{
			return this.sourceMaps.FindIndex((SourceMap item) => item == targetSourceMap);
		}
		internal MapRectangle GetUserBoundingBox(MapTileSourceFactory mapTileSourceFactory)
		{
			MapRectangle mapRectangle = null;
			foreach (SourceMap current in this.sourceMaps)
			{
				mapRectangle = MapRectangle.Union(mapRectangle, current.GetUserBoundingBox(mapTileSourceFactory));
			}
			return mapRectangle;
		}
		internal bool SomeSourceMapIsReadyToLock()
		{
			foreach (SourceMap current in this.sourceMaps)
			{
				if (current.ReadyToLock())
				{
					return true;
				}
			}
			return false;
		}
	}
}
