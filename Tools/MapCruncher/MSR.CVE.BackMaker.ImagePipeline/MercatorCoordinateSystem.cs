using System;
using System.Collections.Generic;
using System.Drawing;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class MercatorCoordinateSystem : CoordinateSystemIfc
	{
		private const int tileSize = 256;
		private const double EquatorialRadius = 6378137.0;
		private const double EquatorialLength = 40075016.685578488;
		private const double HalfEquatorialLength = 20037508.342789244;
		private RangeInt ZoomRange = new RangeInt(1, 31);
		private RangeDouble LatRange = new RangeDouble(-85.0, 85.0);
		private RangeDouble LonRange = new RangeDouble(-180.0, 180.0);
		private VEAddressLayout addressLayout = new VEAddressLayout();
		public static MercatorCoordinateSystem theInstance = new MercatorCoordinateSystem();
		private static bool beenHere = false;
		private double MetersPerPixel(int zoomLevel)
		{
			return 40075016.685578488 / (Math.Pow(2.0, (double)zoomLevel) * 256.0);
		}
		private Point64 LatLongZoomToPixel(LatLonZoom p)
		{
			double num = Math.Sin(CoordinateSystemUtilities.DegreesToRadians(p.lat));
			double num2 = 6378137.0 * CoordinateSystemUtilities.DegreesToRadians(p.lon);
			double num3 = 3189068.5 * Math.Log((1.0 + num) / (1.0 - num));
			double num4 = this.MetersPerPixel(p.zoom);
			return new Point64((long)Math.Round((20037508.342789244 + num2) / num4), (long)Math.Round((20037508.342789244 - num3) / num4));
		}
		private LatLonZoom PixelToLatLong(Point64 p, int zoomLevel)
		{
			double num = this.MetersPerPixel(zoomLevel);
			double num2 = (double)p.X * num - 20037508.342789244;
			double num3 = 20037508.342789244 - (double)p.Y * num;
			return new LatLonZoom(CoordinateSystemUtilities.RadiansToDegrees(1.5707963267948966 - 2.0 * Math.Atan(Math.Exp(-num3 / 6378137.0))), CoordinateSystemUtilities.RadiansToDegrees(num2 / 6378137.0), zoomLevel);
		}
		private LatLonZoom GetUnclippedDraggedView(LatLonZoom centerMapPosition, Point mouseMotion)
		{
			Point64 point = this.LatLongZoomToPixel(centerMapPosition);
			point.X -= (long)mouseMotion.X;
			point.Y -= (long)mouseMotion.Y;
			return this.PixelToLatLong(point, centerMapPosition.zoom);
		}
		public LatLonZoom GetDefaultView()
		{
			return LatLonZoom.World();
		}
		public LatLonZoom GetTranslationInLatLon(LatLonZoom centerMapPosition, Point mouseMotion)
		{
			LatLonZoom unclippedDraggedView = this.GetUnclippedDraggedView(centerMapPosition, mouseMotion);
			if (unclippedDraggedView.lon > 180.0)
			{
				unclippedDraggedView = new LatLonZoom(unclippedDraggedView.lat, unclippedDraggedView.lon - 360.0, unclippedDraggedView.zoom);
			}
			if (unclippedDraggedView.lon < -180.0)
			{
				unclippedDraggedView = new LatLonZoom(unclippedDraggedView.lat, unclippedDraggedView.lon + 360.0, unclippedDraggedView.zoom);
			}
			if (unclippedDraggedView.lat > 85.0)
			{
				unclippedDraggedView = new LatLonZoom(85.0, unclippedDraggedView.lon, unclippedDraggedView.zoom);
			}
			if (unclippedDraggedView.lat < -85.0)
			{
				unclippedDraggedView = new LatLonZoom(-85.0, unclippedDraggedView.lon, unclippedDraggedView.zoom);
			}
			return unclippedDraggedView;
		}
		public RangeInt GetZoomRange()
		{
			return this.ZoomRange;
		}
		public RangeDouble GetLatRange()
		{
			return this.LatRange;
		}
		public RangeDouble GetLonRange()
		{
			return this.LonRange;
		}
		public Point GetTranslationInPixels(LatLonZoom center, LatLon point)
		{
			Point64 point2 = this.LatLongZoomToPixel(center);
			Point64 point3 = this.LatLongZoomToPixel(new LatLonZoom(point.lat, point.lon, center.zoom));
			return new Point((int)(point3.X - point2.X), (int)(point3.Y - point2.Y));
		}
		public Size GetTileSize()
		{
			return new Size(256, 256);
		}
		public LatLonZoom GetBestViewContaining(MapRectangle newBounds, Size size)
		{
			LatLon center = newBounds.GetCenter();
			int i;
			for (i = this.ZoomRange.max; i >= this.ZoomRange.min; i--)
			{
				LatLonZoom p = new LatLonZoom(newBounds.lat0, newBounds.lon0, i);
				LatLonZoom p2 = new LatLonZoom(newBounds.lat1, newBounds.lon1, i);
				Point64 point = this.LatLongZoomToPixel(p);
				Point64 point2 = this.LatLongZoomToPixel(p2);
				if (point.Y - point2.Y < (long)size.Height)
				{
					break;
				}
			}
			return new LatLonZoom(center.lat, center.lon, i);
		}
		private TileAddress GetTileContainingLatLonZoom(LatLonZoom llz)
		{
			Point64 point = this.LatLongZoomToPixel(llz);
			return new TileAddress((int)Math.Floor((double)point.X / 256.0), (int)Math.Floor((double)point.Y / 256.0), llz.zoom);
		}
		public LatLon GetLatLonOfTileNW(TileAddress ta)
		{
			Point64 p = new Point64((long)ta.TileX * 256L, (long)ta.TileY * 256L);
			return this.PixelToLatLong(p, ta.ZoomLevel).latlon;
		}
		private MapRectangle GetMapWindow(LatLonZoom centerPosition, Size windowSize)
		{
			return new MapRectangle(this.GetUnclippedDraggedView(centerPosition, new Point(windowSize.Width / 2, windowSize.Height / 2)).latlon, this.GetUnclippedDraggedView(centerPosition, new Point(-(windowSize.Width / 2 + 1), -(windowSize.Height / 2 + 1))).latlon);
		}
		public TileDisplayDescriptorArray GetTileArrayDescriptor(LatLonZoom center, Size windowSize)
		{
			TileDisplayDescriptorArray tileDisplayDescriptorArray = new TileDisplayDescriptorArray();
			MapRectangle mapWindow = this.GetMapWindow(center, windowSize);
			tileDisplayDescriptorArray.topLeftTile = this.GetTileContainingLatLonZoom(new LatLonZoom(mapWindow.GetNW(), center.zoom));
			TileAddress tileContainingLatLonZoom = this.GetTileContainingLatLonZoom(new LatLonZoom(mapWindow.GetSE(), center.zoom));
			tileDisplayDescriptorArray.tileCountX = tileContainingLatLonZoom.TileX - tileDisplayDescriptorArray.topLeftTile.TileX + 1;
			tileDisplayDescriptorArray.tileCountY = tileContainingLatLonZoom.TileY - tileDisplayDescriptorArray.topLeftTile.TileY + 1;
			int x = this.GetTranslationInPixels(center, this.GetLatLonOfTileNW(tileDisplayDescriptorArray.topLeftTile)).X;
			int x2 = this.GetTranslationInPixels(center, this.GetLatLonOfTileNW(new TileAddress(tileDisplayDescriptorArray.topLeftTile.TileX + tileDisplayDescriptorArray.tileCountX, tileDisplayDescriptorArray.topLeftTile.TileY, tileDisplayDescriptorArray.topLeftTile.ZoomLevel))).X;
			int num = x2 - x;
			int num2 = windowSize.Width + 512;
			if (num > num2)
			{
				D.Sayf(0, "break", new object[0]);
			}
			tileDisplayDescriptorArray.layout = this.addressLayout;
			tileDisplayDescriptorArray.tileSize = this.GetTileSize();
			Point64 point = this.LatLongZoomToPixel(new LatLonZoom(mapWindow.GetNW(), center.zoom));
			tileDisplayDescriptorArray.topLeftTileOffset = new Point64((long)tileDisplayDescriptorArray.topLeftTile.TileX * 256L - point.X, (long)tileDisplayDescriptorArray.topLeftTile.TileY * 256L - point.Y).ToPoint();
			tileDisplayDescriptorArray.topLeftTile.TileX = VEAddressLayout.WrapLongitude(tileDisplayDescriptorArray.topLeftTile.TileX, tileDisplayDescriptorArray.topLeftTile.ZoomLevel);
			return tileDisplayDescriptorArray;
		}
		public MapRectangle GetUnclippedMapWindow(LatLonZoom centerPosition, Size windowSize)
		{
			return new MapRectangle(this.GetUnclippedDraggedView(centerPosition, new Point(windowSize.Width / 2, windowSize.Height / 2)).latlon, this.GetUnclippedDraggedView(centerPosition, new Point(-(windowSize.Width / 2 + 1), -(windowSize.Height / 2 + 1))).latlon);
		}
		public ITileAddressLayout GetTileAddressLayout()
		{
			return this.addressLayout;
		}
		public RenderBounds MakeRenderBounds(MapRectangle imageBounds)
		{
			RenderBounds renderBounds = new RenderBounds();
			renderBounds.MinZoom = this.ZoomRange.min;
			renderBounds.MaxZoom = this.ZoomRange.max;
			renderBounds.TileSize = this.GetTileSize();
			renderBounds.imageBounds = imageBounds;
			renderBounds.tileRectangle = new TileRectangle[renderBounds.MaxZoom + 1];
			for (int i = renderBounds.MinZoom; i <= renderBounds.MaxZoom; i++)
			{
				renderBounds.tileRectangle[i] = new TileRectangle();
				renderBounds.tileRectangle[i].zoom = i;
				LatLonZoom llz = new LatLonZoom(imageBounds.GetNW().lat, imageBounds.GetNW().lon, i);
				renderBounds.tileRectangle[i].TopLeft = this.GetTileContainingLatLonZoom(llz);
				llz = new LatLonZoom(imageBounds.GetSE().lat, imageBounds.GetSE().lon, i);
				renderBounds.tileRectangle[i].BottomRight = this.GetTileContainingLatLonZoom(llz);
				renderBounds.tileRectangle[i].StrideX = 1;
				renderBounds.tileRectangle[i].StrideY = 1;
			}
			return renderBounds;
		}
		public static LatLon LatLonToMercator(LatLon latlon)
		{
			double num = Math.Sin(CoordinateSystemUtilities.DegreesToRadians(latlon.lat));
			double num2 = 6378137.0 * CoordinateSystemUtilities.DegreesToRadians(latlon.lon);
			double num3 = 3189068.5 * Math.Log((1.0 + num) / (1.0 - num));
			double num4 = 40075016.685578488;
			return new LatLon((20037508.342789244 - num3) / num4, (20037508.342789244 + num2) / num4);
		}
		internal static LatLon MercatorToLatLon(LatLon mercator)
		{
			double num = 40075016.685578488;
			double num2 = mercator.lon * num - 20037508.342789244;
			double num3 = 20037508.342789244 - mercator.lat * num;
			return new LatLon(CoordinateSystemUtilities.RadiansToDegrees(1.5707963267948966 - 2.0 * Math.Atan(Math.Exp(-num3 / 6378137.0))), CoordinateSystemUtilities.RadiansToDegrees(num2 / 6378137.0));
		}
		public static void TestLanLonFuncs()
		{
			if (MercatorCoordinateSystem.beenHere)
			{
				return;
			}
			MercatorCoordinateSystem.beenHere = true;
			foreach (LatLon current in new List<LatLon>
			{
				new LatLon(-85.0, -175.0),
				new LatLon(85.0, -175.0),
				new LatLon(85.0, 175.0),
				new LatLon(-85.0, 175.0),
				new LatLon(1.0, 1.0)
			})
			{
				LatLon latLon = MercatorCoordinateSystem.LatLonToMercator(current);
				LatLon latLon2 = MercatorCoordinateSystem.MercatorToLatLon(latLon);
				D.Sayf(0, "Orig {0} merc {1} back {2}", new object[]
				{
					current,
					latLon,
					latLon2
				});
			}
		}
	}
}
