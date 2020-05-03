namespace GMap.NET.Projections
{
    using System;

    public class SwissTopoProjection : PureProjection
    {
        public override double Axis
        {
            get { return 6378137; }
        }

        public override RectLatLng Bounds
        {
            get { return RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude); }
        }

        public override double Flattening
        {
            get { return (1.0 / 298.257223563); }
        }

        public static readonly SwissTopoProjection Instance = new SwissTopoProjection();
        static readonly double MaxLatitude = 85.05112878;
        static readonly double MaxLongitude = 180;

        static readonly double MinLatitude = -85.05112878;
        static readonly double MinLongitude = -180;

        readonly GSize tileSize = new GSize(256, 256);

        public override GSize TileSize
        {
            get { return tileSize; }
        }

        public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
        {
            GPoint ret = GPoint.Empty;

            lat = Clip(lat, MinLatitude, MaxLatitude);
            lng = Clip(lng, MinLongitude, MaxLongitude);

            double x = (lng + 180) / 360;
            double sinLatitude = Math.Sin(lat * Math.PI / 180);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            GSize s = GetTileMatrixSizePixel(zoom);
            long mapSizeX = s.Width;
            long mapSizeY = s.Height;

            ret.X = (long) Clip(x * mapSizeX + 0.5, 0, mapSizeX - 1);
            ret.Y = (long) Clip(y * mapSizeY + 0.5, 0, mapSizeY - 1);

            return ret;
        }

        public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
        {
            PointLatLng ret = PointLatLng.Empty;

            GSize s = GetTileMatrixSizePixel(zoom);
            double mapSizeX = s.Width;
            double mapSizeY = s.Height;

            double xx = (Clip(x, 0, mapSizeX - 1) / mapSizeX) - 0.5;
            double yy = 0.5 - (Clip(y, 0, mapSizeY - 1) / mapSizeY);

            ret.Lat = 90 - 360 * Math.Atan(Math.Exp(-yy * 2 * Math.PI)) / Math.PI;
            ret.Lng = 360 * xx;

            return ret;
        }

        public override GSize GetTileMatrixMaxXY(int zoom)
        {
            return new GSize(TileMaxLimitsPerZoom[zoom].Width - 1, TileMaxLimitsPerZoom[zoom].Height - 1);
        }

        public override GSize GetTileMatrixMinXY(int zoom)
        {
            return new GSize(0, 0);
        }

        // from https://api3.geo.admin.ch/services/sdiservices.html#wmts
        private static GSize[] TileMaxLimitsPerZoom = new GSize[]
        {
            new GSize(1, 1), /* zoom = 0 */
            new GSize(1, 1), /* zoom = 1 */
            new GSize(1, 1), /* zoom = 2 */
            new GSize(1, 1), /* zoom = 3 */
            new GSize(1, 1), /* zoom = 4 */
            new GSize(1, 1), /* zoom = 5 */
            new GSize(1, 1), /* zoom = 6 */
            new GSize(1, 1), /* zoom = 7 */
            new GSize(1, 1), /* zoom = 8 */
            new GSize(2, 1), /* zoom = 9 */
            new GSize(2, 1), /* zoom = 10 */
            new GSize(2, 1), /* zoom = 11 */
            new GSize(2, 2), /* zoom = 12 */
            new GSize(3, 2), /* zoom = 13 */
            new GSize(3, 2), /* zoom = 14 */
            new GSize(4, 3), /* zoom = 15 */
            new GSize(8, 5), /* zoom = 16 */
            new GSize(19, 13), /* zoom = 17 */
            new GSize(38, 25), /* zoom = 18 */
            new GSize(94, 63), /* zoom = 19 */
            new GSize(188, 125), /* zoom = 20 */
            new GSize(375, 250), /* zoom = 21 */
            new GSize(750, 500), /* zoom = 22 */
            new GSize(938, 625), /* zoom = 23 */
            new GSize(1250, 834), /* zoom = 24 */
            new GSize(1875, 1250), /* zoom = 25 */
            new GSize(3750, 2500), /* zoom = 26 */
            new GSize(7500, 5000), /* zoom = 27 */
            new GSize(18750, 12500), /* zoom = 28 */
        };
    }
}