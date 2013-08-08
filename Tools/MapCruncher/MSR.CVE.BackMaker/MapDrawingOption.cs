using System;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	public class MapDrawingOption
	{
		private bool enabled;
		private InvalidatableViewIfc map;
		private ToolStripMenuItem menuItem;
		public bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				this.enabled = value;
				this.map.InvalidateView();
			}
		}
		public MapDrawingOption(InvalidatableViewIfc map, ToolStripMenuItem menuItem, bool default_value)
		{
			this.map = map;
			this.menuItem = menuItem;
			this.enabled = default_value;
			if (this.menuItem != null)
			{
				this.menuItem.Checked = this.enabled;
				this.menuItem.Click += new EventHandler(this.menuItem_Click);
			}
		}
		public void SetInvalidatableView(InvalidatableViewIfc map)
		{
			this.map = map;
		}
		public static bool IsEnabled(MapDrawingOption mdo)
		{
			return mdo != null && mdo.Enabled;
		}
		private void menuItem_Click(object sender, EventArgs e)
		{
			this.enabled = (this.menuItem.Checked = !this.enabled);
			this.map.InvalidateView();
		}
	}
}
