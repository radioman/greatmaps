using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
namespace MSR.CVE.BackMaker
{
	public class TransparencyOptions
	{
		public enum TransparencyMode
		{
			Normal,
			Inverted,
			Off
		}
		private const string TransparencyOptionsTag = "TransparencyOptions";
		private const string UseDocumentTransparencyAttr = "UseDocumentTransparency";
		private const string EnabledAttr = "Enabled";
		private const string InvertedAttr = "Inverted";
		public static RangeInt FuzzRange = new RangeInt(0, 255);
		public static RangeInt HaloSizeRange = new RangeInt(0, 5);
		private DirtyEvent dirtyEvent;
		private List<TransparencyColor> _colorList;
		private bool _enabled;
		private bool _inverted;
		private bool _useDocumentTransparency;
		private FadeOptions _fadeOptions;
        public event TransparencyOptionsChangedDelegate transparencyOptionsChangedEvent;

		public List<TransparencyColor> colorList
		{
			get
			{
				return this._colorList;
			}
		}
		public bool useDocumentTransparency
		{
			get
			{
				return this._useDocumentTransparency;
			}
			set
			{
				if (this._useDocumentTransparency != value)
				{
					this._useDocumentTransparency = value;
					this.SetDirty();
				}
			}
		}
		private void Initialize(DirtyEvent dirty)
		{
			this.dirtyEvent = dirty;
			this._colorList = new List<TransparencyColor>();
			this._enabled = true;
			this._inverted = false;
			this._useDocumentTransparency = true;
			this._fadeOptions = new FadeOptions(dirty);
		}
		public TransparencyOptions(TransparencyOptions prototype)
		{
			this._enabled = prototype._enabled;
			this._inverted = prototype._inverted;
			this._useDocumentTransparency = prototype._useDocumentTransparency;
			this._colorList = new List<TransparencyColor>();
			this._colorList.AddRange(prototype._colorList);
			this._fadeOptions = new FadeOptions(prototype._fadeOptions);
		}
		public TransparencyOptions(DirtyEvent dirty)
		{
			this.Initialize(dirty);
		}
		public TransparencyColor AddColor(Pixel color)
		{
			TransparencyColor transparencyColor = new TransparencyColor(color, 2, 0);
			this._colorList.Add(transparencyColor);
			this.SetDirty();
			return transparencyColor;
		}
		public void RemoveColor(TransparencyColor tc)
		{
			this._colorList.Remove(tc);
			this.SetDirty();
		}
		public void SetFuzz(TransparencyColor tc, int newValue)
		{
			if (this._colorList.Contains(tc) && tc.fuzz != newValue)
			{
				this._colorList[this._colorList.IndexOf(tc)] = new TransparencyColor(tc.color, newValue, tc.halo);
				this.SetDirty();
			}
		}
		public void SetHalo(TransparencyColor tc, int newValue)
		{
			if (this._colorList.Contains(tc) && tc.halo != newValue)
			{
				this._colorList[this._colorList.IndexOf(tc)] = new TransparencyColor(tc.color, tc.fuzz, newValue);
				this.SetDirty();
			}
		}
		public TransparencyOptions.TransparencyMode GetMode()
		{
			if (!this._enabled)
			{
				return TransparencyOptions.TransparencyMode.Off;
			}
			if (this._inverted)
			{
				return TransparencyOptions.TransparencyMode.Inverted;
			}
			return TransparencyOptions.TransparencyMode.Normal;
		}
		public void SetNormalTransparency()
		{
			this._enabled = true;
			this._inverted = false;
			this.SetDirty();
		}
		public void SetInvertedTransparency()
		{
			this._enabled = true;
			this._inverted = true;
			this.SetDirty();
		}
		public void SetDisabledTransparency()
		{
			this._enabled = false;
			this.SetDirty();
		}
		public bool ShouldBeTransparent(byte r, byte g, byte b)
		{
			if (!this._enabled)
			{
				return false;
			}
			bool flag = false;
			foreach (TransparencyColor current in this.colorList)
			{
				if (Math.Abs((int)(current.color.r - r)) <= current.fuzz && Math.Abs((int)(current.color.g - g)) <= current.fuzz && Math.Abs((int)(current.color.b - b)) <= current.fuzz)
				{
					flag = true;
					break;
				}
			}
			return flag != this._inverted;
		}
		public void SetDirty()
		{
			if (this.transparencyOptionsChangedEvent != null)
			{
				this.transparencyOptionsChangedEvent();
			}
			this.dirtyEvent.SetDirty();
		}
		public void AccumulateRobustHash(IRobustHash hash)
		{
			hash.Accumulate(this._useDocumentTransparency);
			hash.Accumulate(this._enabled);
			if (this._enabled)
			{
				hash.Accumulate(this._inverted);
				foreach (TransparencyColor current in this._colorList)
				{
					current.AccumulateRobustHash(hash);
				}
			}
			this._fadeOptions.AccumulateRobustHash(hash);
		}
		public TransparencyOptions(MashupParseContext context, DirtyEvent dirty)
		{
			this.Initialize(dirty);
			XMLTagReader xMLTagReader = context.NewTagReader("TransparencyOptions");
			this._useDocumentTransparency = true;
			context.GetAttributeBoolean("UseDocumentTransparency", ref this._useDocumentTransparency);
			this._enabled = context.GetRequiredAttributeBoolean("Enabled");
			this._inverted = context.GetRequiredAttributeBoolean("Inverted");
			while (xMLTagReader.FindNextStartTag())
			{
				if (xMLTagReader.TagIs(TransparencyColor.GetXMLTag()))
				{
					this._colorList.Add(new TransparencyColor(context));
				}
				else
				{
					if (xMLTagReader.TagIs(FadeOptions.GetXMLTag()))
					{
						this._fadeOptions = new FadeOptions(context, dirty);
					}
				}
			}
		}
		internal static string GetXMLTag()
		{
			return "TransparencyOptions";
		}
		internal void WriteXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("TransparencyOptions");
			writer.WriteAttributeString("UseDocumentTransparency", this._useDocumentTransparency.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("Enabled", this._enabled.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("Inverted", this._inverted.ToString(CultureInfo.InvariantCulture));
			foreach (TransparencyColor current in this._colorList)
			{
				current.WriteXML(writer);
			}
			this._fadeOptions.WriteXML(writer);
			writer.WriteEndElement();
		}
		internal bool Effectless()
		{
			return this.colorList.Count == 0 || this.GetMode() == TransparencyOptions.TransparencyMode.Off;
		}
		internal FadeOptions GetFadeOptions()
		{
			return this._fadeOptions;
		}
	}
}
