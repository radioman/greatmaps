using System;
namespace MSR.CVE.BackMaker
{
	public class SingleMaxZoomForEntireMashupCompatibilityBlob
	{
		private int _minZoom = 1;
		private int _maxZoom;
		public int minZoom
		{
			get
			{
				return this._minZoom;
			}
			set
			{
				if (value != this._minZoom)
				{
					this._minZoom = value;
				}
			}
		}
		public int maxZoom
		{
			get
			{
				return this._maxZoom;
			}
			set
			{
				if (value != this._maxZoom)
				{
					this._maxZoom = value;
				}
			}
		}
	}
}
