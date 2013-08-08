using MSR.CVE.BackMaker.ImagePipeline;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace MSR.CVE.BackMaker
{
	internal class UserRegionViewController
	{
		private struct State
		{
			public LatLonZoom center;
			public Size size;
			public bool valid;
			public override bool Equals(object o2)
			{
				if (o2 is UserRegionViewController.State)
				{
					UserRegionViewController.State state = (UserRegionViewController.State)o2;
					return this.center == state.center && this.size == state.size && this.valid == state.valid;
				}
				return false;
			}
			public override int GetHashCode()
			{
				return this.center.GetHashCode() ^ this.size.GetHashCode();
			}
			public override string ToString()
			{
				return string.Format("{0} {1}", this.center, this.size);
			}
			public State(UserRegionViewController.State state)
			{
				this.center = state.center;
				this.size = new Size(state.size.Width, state.size.Height);
				this.valid = state.valid;
			}
		}
		private class ClickableThing
		{
			public enum ClickedWhich
			{
				Vertex,
				Segment
			}
			public TracedScreenPoint vertexLocation;
			public GraphicsPath path;
			public UserRegionViewController.ClickableThing.ClickedWhich clickedWhich;
			public int pointIndex;
		}
		private class VertexMouseAction : ViewerControl.MouseAction
		{
			private UserRegionViewController controller;
			private int draggedVertexIndex;
			public VertexMouseAction(UserRegionViewController controller, int draggedVertexIndex)
			{
				this.controller = controller;
				this.draggedVertexIndex = draggedVertexIndex;
			}
			public void Dragged(Point diff)
			{
				this.controller.DragVertex(diff, this.draggedVertexIndex);
			}
			public Cursor GetCursor(bool dragging)
			{
				return Cursors.Arrow;
			}
			public void OnPopup(ContextMenu menu)
			{
				MenuItem menuItem = menu.MenuItems.Add("Remove corner", new EventHandler(this.RemoveCorner));
				menuItem.Enabled = this.controller.RemoveEnabled();
			}
			public void RemoveCorner(object sender, EventArgs e)
			{
				this.controller.RemoveCorner(this.draggedVertexIndex);
			}
		}
		private class SegmentMouseAction : ViewerControl.MouseAction
		{
			private UserRegionViewController controller;
			private int originalVertexIndex;
			private Point clickedPoint;
			private int menuIncarnation;
			private static int menuIncarnationCounter;
			public SegmentMouseAction(UserRegionViewController controller, int originalVertexIndex, Point clickedPoint)
			{
				this.controller = controller;
				this.originalVertexIndex = originalVertexIndex;
				UserRegionViewController.SegmentMouseAction.menuIncarnationCounter++;
				this.menuIncarnation = UserRegionViewController.SegmentMouseAction.menuIncarnationCounter;
				this.clickedPoint = clickedPoint;
			}
			public void Dragged(Point diff)
			{
			}
			public Cursor GetCursor(bool dragging)
			{
				return Cursors.Cross;
			}
			public void OnPopup(ContextMenu menu)
			{
				menu.MenuItems.Add("Add corner", new EventHandler(this.AddCorner));
				D.Say(0, string.Format("Updating menu from incarnation {0}", this.menuIncarnation));
			}
			public void AddCorner(object sender, EventArgs e)
			{
				D.Say(0, string.Format("AddCorner from incarnation {0}", this.menuIncarnation));
				this.controller.AddCorner(this.clickedPoint, this.originalVertexIndex);
			}
		}
		private const int vertexRadius = 3;
		private const int edgeClickWidth = 4;
		private const int vertexClickRadius = 4;
		private CoordinateSystemIfc csi;
		private SVDisplayParams svdp;
		private LatentRegionHolder latentRegionHolder;
		private IDisplayableSource displayableSource;
		private Brush vertexFillBrush;
		private Pen vertexStrokePen;
		private Brush segmentFillBrush;
		private UserRegionViewController.State lastState;
		private UserRegionViewController.ClickableThing[] clickableThings;
		public UserRegionViewController(CoordinateSystemIfc csi, SVDisplayParams svdp, LatentRegionHolder latentRegionHolder, IDisplayableSource unwarpedMapTileSource)
		{
			this.csi = csi;
			this.svdp = svdp;
			this.latentRegionHolder = latentRegionHolder;
			this.displayableSource = unwarpedMapTileSource;
			this.vertexFillBrush = new SolidBrush(Color.LightBlue);
			this.vertexStrokePen = new Pen(Color.DarkBlue, 1f);
			this.segmentFillBrush = new SolidBrush(Color.DarkBlue);
		}
		private void UpdateState(UserRegionViewController.State state)
		{
			if (state.Equals(this.lastState))
			{
				return;
			}
			TracedScreenPoint[] path = this.GetUserRegion().GetPath(CoordinateSystemUtilities.GetBounds(this.csi, state.center, state.size), state.center.zoom, this.csi);
			List<TracedScreenPoint> list = new List<TracedScreenPoint>();
			int length = path.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				D.Assert(path[i].originalIndex >= 0);
				int num = (i + length - 1) % length;
				if (!(path[num].pointf == path[i].pointf))
				{
					list.Add(path[i]);
				}
			}
			list.ToArray();
			int count = list.Count;
			List<UserRegionViewController.ClickableThing> list2 = new List<UserRegionViewController.ClickableThing>();
			List<UserRegionViewController.ClickableThing> list3 = new List<UserRegionViewController.ClickableThing>();
			for (int j = 0; j < count; j++)
			{
				UserRegionViewController.ClickableThing clickableThing = new UserRegionViewController.ClickableThing();
				list2.Add(clickableThing);
				clickableThing.vertexLocation = list[j];
				clickableThing.path = new GraphicsPath();
				clickableThing.path.AddEllipse(list[j].pointf.X - 4f, list[j].pointf.Y - 4f, 8f, 8f);
				clickableThing.clickedWhich = UserRegionViewController.ClickableThing.ClickedWhich.Vertex;
				clickableThing.pointIndex = j;
				UserRegionViewController.ClickableThing clickableThing2 = new UserRegionViewController.ClickableThing();
				list3.Add(clickableThing2);
				clickableThing2.vertexLocation = list[j];
				clickableThing2.path = new GraphicsPath();
				clickableThing2.path.AddLine(list[j].pointf, list[(j + 1) % count].pointf);
				clickableThing2.path.Widen(new Pen(Color.Black, 4f));
				clickableThing2.clickedWhich = UserRegionViewController.ClickableThing.ClickedWhich.Segment;
				clickableThing2.pointIndex = j;
			}
			list2.AddRange(list3);
			this.clickableThings = list2.ToArray();
			this.lastState = new UserRegionViewController.State(state);
		}
		internal RenderRegion GetUserRegion()
		{
			return this.latentRegionHolder.renderRegion;
		}
		internal void Paint(PaintSpecification e, LatLonZoom center, Size size)
		{
			if (this.GetUserRegion() == null)
			{
				return;
			}
			UserRegionViewController.State state;
			state.center = center;
			state.size = size;
			state.valid = true;
			this.UpdateState(state);
			UserRegionViewController.ClickableThing[] array = this.clickableThings;
			for (int i = 0; i < array.Length; i++)
			{
				UserRegionViewController.ClickableThing clickableThing = array[i];
				e.Graphics.FillPath(this.segmentFillBrush, clickableThing.path);
			}
		}
		internal ViewerControl.MouseAction ImminentAction(MouseEventArgs e)
		{
			if (this.clickableThings == null)
			{
				return null;
			}
			int i = 0;
			while (i < this.clickableThings.GetLength(0))
			{
				UserRegionViewController.ClickableThing clickableThing = this.clickableThings[i];
				if (clickableThing.vertexLocation.originalIndex >= 0 && clickableThing.path.IsVisible(e.Location))
				{
					if (clickableThing.clickedWhich == UserRegionViewController.ClickableThing.ClickedWhich.Segment)
					{
						return new UserRegionViewController.SegmentMouseAction(this, clickableThing.vertexLocation.originalIndex, e.Location);
					}
					return new UserRegionViewController.VertexMouseAction(this, clickableThing.vertexLocation.originalIndex);
				}
				else
				{
					i++;
				}
			}
			return null;
		}
		internal void DragVertex(Point diff, int draggedVertexIndex)
		{
			LatLon point = this.GetUserRegion().GetPoint(draggedVertexIndex);
			LatLonZoom center = new LatLonZoom(point.lat, point.lon, this.svdp.MapCenter().zoom);
			diff = new Point(-diff.X, -diff.Y);
			LatLonZoom translationInLatLon = this.csi.GetTranslationInLatLon(center, diff);
			this.GetUserRegion().UpdatePoint(draggedVertexIndex, translationInLatLon.latlon);
			this.Invalidate();
		}
		internal void AddCorner(Point newCornerPoint, int originalVertexIndex)
		{
			LatLonZoom center = this.svdp.MapCenter();
			Point offsetInPixels = new Point(this.svdp.ScreenCenter().X - newCornerPoint.X, this.svdp.ScreenCenter().Y - newCornerPoint.Y);
			LatLonZoom translationInLatLon = this.csi.GetTranslationInLatLon(center, offsetInPixels);
			D.Say(0, string.Format("newCornerPosition= {0}", translationInLatLon));
			this.GetUserRegion().InsertPoint(originalVertexIndex + 1, translationInLatLon.latlon);
			this.Invalidate();
		}
		internal void RemoveCorner(int originalVertexIndex)
		{
			this.GetUserRegion().RemovePoint(originalVertexIndex);
			this.Invalidate();
		}
		internal bool RemoveEnabled()
		{
			return this.GetUserRegion().Count > 3;
		}
		private void Invalidate()
		{
			AsyncRef asyncRef = (AsyncRef)this.displayableSource.GetUserBounds(this.latentRegionHolder, (FutureFeatures)7).Realize("UserRegionViewController.Invalidate");
			asyncRef.ProcessSynchronously();
			asyncRef.Dispose();
			this.svdp.InvalidateView();
			this.lastState.valid = false;
		}
	}
}
