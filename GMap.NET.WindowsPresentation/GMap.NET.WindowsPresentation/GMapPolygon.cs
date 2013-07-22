
namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Windows.Shapes;

    public class GMapPolygon : GMapMarker, IShapable
    {
        public readonly List<PointLatLng> Points = new List<PointLatLng>();

        public GMapPolygon(IEnumerable<PointLatLng> points)
        {
            Points.AddRange(points);
            if (Points.Count > 0)
            {
                Position = Points[0];
                RegenerateShape(null);
            }
        }

        /// <summary>
        /// regenerates shape of polygon
        /// </summary>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null)
            {
                Map = map;

                if (Points.Count > 1)
                {
                    Position = Points[0];

                    var localPath = new List<System.Windows.Point>();
                    var offset = Map.FromLatLngToLocal(Points[0]);
                    foreach (var i in Points)
                    {
                        var p = Map.FromLatLngToLocal(new PointLatLng(i.Lat, i.Lng));
                        localPath.Add(new System.Windows.Point(p.X - offset.X, p.Y - offset.Y));
                    }

                    var shape = map.CreatePolygonPath(localPath);

                    if (this.Shape != null && this.Shape is Path)
                    {
                        (this.Shape as Path).Data = shape.Data;
                    }
                    else
                    {
                        this.Shape = shape;
                    }
                }
                else
                {
                    this.Shape = null;
                }
            }
        }
    }
}
