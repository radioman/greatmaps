using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace MSR.CVE.BackMaker
{
	public class LegendList
	{
		private SourceMap _sourceMap;
		private List<Legend> list = new List<Legend>();
		public DirtyEvent dirtyEvent;
		private DirtyEvent parentBoundsChangedEvent;

        //[CompilerGenerated]
        //private static Converter<Legend, string> <>9__CachedAnonymousMethodDelegate1;

		public LegendList(SourceMap sourceMap, DirtyEvent parentEvent, DirtyEvent parentBoundsChangedEvent)
		{
			this._sourceMap = sourceMap;
			this.dirtyEvent = new DirtyEvent(parentEvent);
			this.parentBoundsChangedEvent = parentBoundsChangedEvent;
		}
		public LegendList(SourceMap sourceMap, MashupParseContext context, DirtyEvent parentEvent)
		{
			this._sourceMap = sourceMap;
			this.dirtyEvent = new DirtyEvent(parentEvent);
			this.parentBoundsChangedEvent = parentEvent;
			XMLTagReader xMLTagReader = context.NewTagReader(LegendList.GetXMLTag());
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(Legend.GetXMLTag()))
				{
					this.list.Add(new Legend(this._sourceMap, context, this.dirtyEvent, this.parentBoundsChangedEvent));
				}
			}
		}
		public static string GetXMLTag()
		{
			return "LegendList";
		}
		public void WriteXML(MashupWriteContext context)
		{
			context.writer.WriteStartElement(LegendList.GetXMLTag());
			foreach (Legend current in this)
			{
				current.WriteXML(context);
			}
			context.writer.WriteEndElement();
		}
		internal Legend AddNewLegend()
		{
			Legend legend = new Legend(this._sourceMap, this.dirtyEvent, this.parentBoundsChangedEvent);
			string displayName = legend.displayName;
			int num = 1;
			List<string> list = this.list.ConvertAll<string>((Legend l) => l.displayName);
			while (list.Contains(legend.displayName))
			{
				num++;
				legend.displayName = string.Format("{0} {1}", displayName, num);
			}
			this.list.Add(legend);
			this.dirtyEvent.SetDirty();
			return legend;
		}
		public List<Legend>.Enumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}
		public void RemoveLegend(Legend legend)
		{
			this.list.Remove(legend);
			this.dirtyEvent.SetDirty();
		}
	}
}
