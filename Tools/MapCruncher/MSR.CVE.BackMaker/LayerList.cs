using System;
using System.Collections.Generic;
namespace MSR.CVE.BackMaker
{
	public class LayerList
	{
		private const string LayerListTag = "LayerList";
		private List<Layer> layers = new List<Layer>();
		private DirtyEvent dirtyEvent;
		public int Count
		{
			get
			{
				return this.layers.Count;
			}
		}
		public Layer First
		{
			get
			{
				return this.layers[0];
			}
		}
		public LayerList(DirtyEvent parentDirty)
		{
			this.dirtyEvent = parentDirty;
		}
		public LayerList(MashupParseContext context, SourceMap.GetFilenameContext filenameContextDelegate, DirtyEvent parentDirty, DirtyEvent parentReadyToLockEvent)
		{
			this.dirtyEvent = parentDirty;
			XMLTagReader xMLTagReader = context.NewTagReader("LayerList");
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(Layer.GetXMLTag()))
				{
					this.Add(new Layer(context, filenameContextDelegate, this.dirtyEvent, parentReadyToLockEvent));
				}
			}
		}
		public void WriteXML(MashupWriteContext wc)
		{
			wc.writer.WriteStartElement("LayerList");
			foreach (Layer current in this)
			{
				current.WriteXML(wc);
			}
			wc.writer.WriteEndElement();
		}
		internal static string GetXMLTag()
		{
			return "LayerList";
		}
		internal void AddNewLayer()
		{
			this.Add(new Layer(this, this.dirtyEvent));
		}
		public List<Layer>.Enumerator GetEnumerator()
		{
			return this.layers.GetEnumerator();
		}
		public void Add(Layer layer)
		{
			this.layers.Add(layer);
			this.dirtyEvent.SetDirty();
		}
		public void AddAfter(Layer newLayer, Layer refLayer)
		{
			int num = this.layers.FindIndex((Layer l) => l == refLayer);
			if (num < 0)
			{
				throw new IndexOutOfRangeException();
			}
			this.layers.Insert(num + 1, newLayer);
		}
		public void Remove(Layer layer)
		{
			this.layers.Remove(layer);
			this.dirtyEvent.SetDirty();
		}
		internal bool HasLayerNamed(string proposedLayerName)
		{
			return this.layers.Find((Layer layer) => layer.displayName == proposedLayerName) != null;
		}
		internal void RemoveSourceMap(SourceMap sourceMap)
		{
			Layer layer2 = this.layers.Find((Layer layer) => layer.Contains(sourceMap));
			layer2.Remove(sourceMap);
		}
		internal void AutoSelectMaxZooms(MapTileSourceFactory mapTileSourceFactory)
		{
			foreach (Layer current in this.layers)
			{
				current.AutoSelectMaxZooms(mapTileSourceFactory);
			}
		}
		internal bool SomeSourceMapIsReadyToLock()
		{
			foreach (Layer current in this.layers)
			{
				if (current.SomeSourceMapIsReadyToLock())
				{
					return true;
				}
			}
			return false;
		}
	}
}
