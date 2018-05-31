public GMapRoute getRoute(double lat, double lng, double newLat, double newLng)
        {
            PointLatLng start = new PointLatLng(lat, lng);
            PointLatLng end = new PointLatLng(newLat, newLng);
            MapRoute route = BingMapProvider.Instance.GetRoute(start, end, false, false, 13);
            GMapRoute r = new GMapRoute(route.Points, "My route");
            r.Stroke.DashStyle = DashStyle.Dash;
            r.Stroke.Width = 5;
            r.Stroke.Color = Color.LightSeaGreen;
            return r;
        }
        public bool isPointInBoundary(List<PointLatLng> points, string lat, string lng) {
            GMapOverlay polyOverlay = new GMapOverlay();
            GMapPolygon polygon = new GMapPolygon(points, "routePloygon");
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
            polygon.Stroke = new Pen(Color.Red, 1);
            polyOverlay.Polygons.Add(polygon);
            PointLatLng pnt = new PointLatLng(Double.Parse(lat), Double.Parse(lng));
            return polygon.IsInside(pnt);
        
        }
