using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class DisplayablePosition
	{
		public enum ErrorMarker
		{
			AsOutlier,
			AsContributor
		}
		private const int NumErrorMarkers = 2;
		private LatLonZoom _pinPosition;
		private ErrorPosition[] _errorPositions = new ErrorPosition[2];
		private bool _invertError;
		public LatLonZoom pinPosition
		{
			get
			{
				return this._pinPosition;
			}
		}
		public bool invertError
		{
			get
			{
				return this._invertError;
			}
			set
			{
				this._invertError = value;
			}
		}
		public DisplayablePosition(LatLonZoom position)
		{
			this._pinPosition = position;
		}
		public static string GetXMLTag(MashupXMLSchemaVersion version)
		{
			if (version == MonolithicMapPositionsSchema.schema)
			{
				return MapPosition.GetXMLTag(version);
			}
			return LatLonZoom.GetXMLTag();
		}
		public DisplayablePosition(MashupParseContext context, CoordinateSystemIfc coordSys)
		{
			if (context.version == MonolithicMapPositionsSchema.schema)
			{
				MapPosition mapPosition = new MapPosition(context, null, coordSys);
				this._pinPosition = mapPosition.llz;
				return;
			}
			this._pinPosition = new LatLonZoom(context, coordSys);
		}
		public ErrorPosition GetErrorPosition(DisplayablePosition.ErrorMarker errorMarker)
		{
			return this._errorPositions[(int)errorMarker];
		}
		public void SetErrorPosition(DisplayablePosition.ErrorMarker errorMarker, LatLon errorPosition)
		{
			this._errorPositions[(int)errorMarker] = new ErrorPosition(errorPosition);
		}
		public void WriteXML(XmlTextWriter writer)
		{
			this._pinPosition.WriteXML(writer);
		}
		public override int GetHashCode()
		{
			return this._pinPosition.GetHashCode();
		}
	}
}
