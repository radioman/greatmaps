using System;
using System.Drawing;
namespace MSR.CVE.BackMaker
{
	public class PaintSpecification
	{
		public Graphics Graphics;
		public Rectangle ClipRectangle;
		public Size Size;
		public bool SynchronousTiles;
		public PaintSpecification(Graphics Graphics, Rectangle ClipRectangle, Size Size, bool SynchronousTiles)
		{
			this.Graphics = Graphics;
			this.ClipRectangle = ClipRectangle;
			this.Size = Size;
			this.SynchronousTiles = SynchronousTiles;
		}
		public void ResetClip()
		{
			this.Graphics.SetClip(this.ClipRectangle);
		}
	}
}
