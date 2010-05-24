using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Demo.WindowsForms.CustomMarkers;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System.IO;
using System.Net.NetworkInformation;

namespace Demo.WindowsForms
{
   public partial class MainForm : Form
   {
      PointLatLng start;
      PointLatLng end;

      // marker
      GMapMarker currentMarker;
      GMapMarker center;

      // polygons
      GMapPolygon polygon;

      // layers
      GMapOverlay top;
      internal GMapOverlay objects;
      internal GMapOverlay routes;
      internal GMapOverlay polygons;

      public MainForm()
      {
         InitializeComponent();

         if(!DesignMode)
         {
            // add your custom map db provider
            //MySQLPureImageCache ch = new MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //MainMap.Manager.ImageCacheSecond = ch;

            // set your proxy here if need
            //MainMap.Manager.Proxy = new WebProxy("10.2.0.100", 8080);
            //MainMap.Manager.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeada");

            // set cache mode only if no internet avaible
            try
            {
               System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("www.google.com");
            }
            catch
            {
               MainMap.Manager.Mode = AccessMode.CacheOnly;
               MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET - Demo.WindowsForms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // config map             
            MainMap.MapType = MapType.MapsLT_Map;
            MainMap.MaxZoom = 11;
            MainMap.MinZoom = 1;
            MainMap.Zoom = MainMap.MinZoom + 1;
            MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);

            // map events
            MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnMarkerClick += new MarkerClick(MainMap_OnMarkerClick);
            MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseDown += new MouseEventHandler(MainMap_MouseDown);
            MainMap.MouseUp += new MouseEventHandler(MainMap_MouseUp);
            MainMap.OnMarkerEnter += new MarkerEnter(MainMap_OnMarkerEnter);
            MainMap.OnMarkerLeave += new MarkerLeave(MainMap_OnMarkerLeave);

            // get map type
            comboBoxMapType.DataSource = Enum.GetValues(typeof(MapType));
            comboBoxMapType.SelectedItem = MainMap.MapType;

            // acccess mode
            comboBoxMode.DataSource = Enum.GetValues(typeof(AccessMode));
            comboBoxMode.SelectedItem = GMaps.Instance.Mode;

            // get position
            textBoxLat.Text = MainMap.CurrentPosition.Lat.ToString(CultureInfo.InvariantCulture);
            textBoxLng.Text = MainMap.CurrentPosition.Lng.ToString(CultureInfo.InvariantCulture);

            // get cache modes
            checkBoxUseRouteCache.Checked = GMaps.Instance.UseRouteCache;
            checkBoxUseGeoCache.Checked = GMaps.Instance.UseGeocoderCache;

            MobileLogFrom.Value = DateTime.Today;
            MobileLogTo.Value = DateTime.Now;

            // get zoom  
            trackBar1.Minimum = MainMap.MinZoom;
            trackBar1.Maximum = MainMap.MaxZoom;

#if DEBUG
            checkBoxDebug.Checked = true;
#endif

            // transport demo
            transport.DoWork += new DoWorkEventHandler(transport_DoWork);
            transport.ProgressChanged += new ProgressChangedEventHandler(transport_ProgressChanged);
            transport.WorkerSupportsCancellation = true;
            transport.WorkerReportsProgress = true;

            // Connections
            connectionsWorker.DoWork += new DoWorkEventHandler(connectionsWorker_DoWork);
            connectionsWorker.ProgressChanged += new ProgressChangedEventHandler(connectionsWorker_ProgressChanged);
            connectionsWorker.WorkerSupportsCancellation = true;
            connectionsWorker.WorkerReportsProgress = true;

            // add custom layers  
            {
               routes = new GMapOverlay(MainMap, "routes");
               MainMap.Overlays.Add(routes);

               polygons = new GMapOverlay(MainMap, "polygons");
               MainMap.Overlays.Add(polygons);

               objects = new GMapOverlay(MainMap, "objects");
               MainMap.Overlays.Add(objects);

               top = new GMapOverlay(MainMap, "top");
               MainMap.Overlays.Add(top);
            }

            // set current marker
            currentMarker = new GMapMarkerGoogleRed(MainMap.CurrentPosition);
            top.Markers.Add(currentMarker);

            // map center
            center = new GMapMarkerCross(MainMap.CurrentPosition);
            top.Markers.Add(center);

            // add my city location for demo
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
            {
               PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius", out status);
               if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
               {
                  currentMarker.Position = pos.Value;

                  GMapMarker myCity = new GMapMarkerGoogleGreen(pos.Value);
                  myCity.ToolTipMode = MarkerTooltipMode.Always;
                  myCity.ToolTipText = "Welcome to Lithuania! ;}";
                  objects.Markers.Add(myCity);
               }
            }

            // add some point in lithuania
            //if(false)
            {
               AddLocationLithuania("Kaunas");
               AddLocationLithuania("Klaipėda");
               AddLocationLithuania("Šiauliai");
               AddLocationLithuania("Panevėžys");

               RegeneratePolygon();
            }
         }
      }

      void RegeneratePolygon()
      {
         List<PointLatLng> polygonPoints = new List<PointLatLng>();

         foreach(GMapMarker m in objects.Markers)
         {
            if(m is GMapMarkerRect)
            {
               m.Tag = polygonPoints.Count;
               polygonPoints.Add(m.Position);
            }
         }

         if(polygon == null)
         {
            polygon = new GMapPolygon(polygonPoints, "polygon test");
            polygons.Polygons.Add(polygon);
         }
         else
         {
            polygon.Points.Clear();
            polygon.Points.AddRange(polygonPoints);

            if(polygons.Polygons.Count == 0)
            {
               polygons.Polygons.Add(polygon);
            }
            else
            {
               MainMap.UpdatePolygonLocalPosition(polygon);
            }
         }
      }

      GMapMarkerRect CurentRectMarker = null;

      void MainMap_OnMarkerLeave(GMapMarker item)
      {
         if(item is GMapMarkerRect)
         {
            CurentRectMarker = null;

            GMapMarkerRect rc = item as GMapMarkerRect;
            rc.Pen.Color = Color.Blue;
            MainMap.Invalidate(false);
         }
      }

      void MainMap_OnMarkerEnter(GMapMarker item)
      {
         if(item is GMapMarkerRect)
         {
            GMapMarkerRect rc = item as GMapMarkerRect;
            rc.Pen.Color = Color.Red;
            MainMap.Invalidate(false);

            CurentRectMarker = rc;
         }
      }

      #region -- performance test --

      double NextDouble(Random rng, double min, double max)
      {
         return min + (rng.NextDouble() * (max - min));
      }

      Random r = new Random();

      int tt = 0;
      void timer_Tick(object sender, EventArgs e)
      {
         var pos = new PointLatLng(NextDouble(r, MainMap.CurrentViewArea.Top, MainMap.CurrentViewArea.Bottom), NextDouble(r, MainMap.CurrentViewArea.Left, MainMap.CurrentViewArea.Right));
         GMapMarker m = new GMapMarkerGoogleGreen(pos);
         {
            m.ToolTipText = (tt++).ToString();
            m.ToolTipMode = MarkerTooltipMode.Always;
            m.Offset = new System.Drawing.Point(-m.Size.Width, -m.Size.Height);
         }

         objects.Markers.Add(m);

         if(tt >= 333)
         {
            timerPerf.Stop();
            tt = 0;
         }
      }

      System.Windows.Forms.Timer timerPerf = new System.Windows.Forms.Timer();
      #endregion

      #region -- transport demo --
      BackgroundWorker transport = new BackgroundWorker();

      readonly List<VehicleData> trolleybus = new List<VehicleData>();
      readonly Dictionary<int, GMapMarker> trolleybusMarkers = new Dictionary<int, GMapMarker>();

      readonly List<VehicleData> bus = new List<VehicleData>();
      readonly Dictionary<int, GMapMarker> busMarkers = new Dictionary<int, GMapMarker>();

      bool firstLoadTrasport = true;

      void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         // stops immediate marker/route/polygon invalidations;
         // call Refresh to perform single refresh and reset invalidation state
         MainMap.HoldInvalidation = true;

         lock(trolleybus)
         {
            foreach(VehicleData d in trolleybus)
            {
               GMapMarker marker;

               if(!trolleybusMarkers.TryGetValue(d.Id, out marker))
               {
                  marker = new GMapMarkerGoogleRed(new PointLatLng(d.Lat, d.Lng));
                  marker.Tag = d.Id;
                  marker.ToolTipMode = MarkerTooltipMode.Always;

                  trolleybusMarkers[d.Id] = marker;
                  objects.Markers.Add(marker);
               }
               else
               {
                  marker.Position = new PointLatLng(d.Lat, d.Lng);
                  (marker as GMapMarkerGoogleRed).Bearing = (float?) d.Bearing;
               }
               marker.ToolTipText = d.Line;
            }
         }

         lock(bus)
         {
            foreach(VehicleData d in bus)
            {
               GMapMarker marker;

               if(!busMarkers.TryGetValue(d.Id, out marker))
               {
                  marker = new GMapMarkerGoogleGreen(new PointLatLng(d.Lat, d.Lng));
                  marker.Tag = d.Id;
                  marker.ToolTipMode = MarkerTooltipMode.Always;

                  busMarkers[d.Id] = marker;
                  objects.Markers.Add(marker);
               }
               else
               {
                  marker.Position = new PointLatLng(d.Lat, d.Lng);
                  (marker as GMapMarkerGoogleGreen).Bearing = (float?) d.Bearing;
               }
               marker.ToolTipText = d.Line;
            }
         }

         if(firstLoadTrasport)
         {
            MainMap.ZoomAndCenterMarkers("objects");
            firstLoadTrasport = false;
         }
         MainMap.Refresh();
      }

      void transport_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!transport.CancellationPending)
         {
            try
            {
               lock(trolleybus)
               {
                  MainMap.Manager.GetVilniusTransportData(TransportType.TrolleyBus, string.Empty, trolleybus);
               }

               lock(bus)
               {
                  MainMap.Manager.GetVilniusTransportData(TransportType.Bus, string.Empty, bus);
               }

               transport.ReportProgress(100);
            }
            catch(Exception ex)
            {
               Debug.WriteLine("transport_DoWork: " + ex.ToString());
            }
            Thread.Sleep(3333);
         }
         trolleybusMarkers.Clear();
         busMarkers.Clear();
      }

      #endregion

      #region -- tcp/ip connections demo --
      BackgroundWorker connectionsWorker = new BackgroundWorker();

      readonly Dictionary<string, GMapMarker> tcpConnections = new Dictionary<string, GMapMarker>();
      bool firstLoadConnections = true;

      void connectionsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         // stops immediate marker/route/polygon invalidations;
         // call Refresh to perform single refresh and reset invalidation state
         MainMap.HoldInvalidation = true;

         lock(tcpConnections)
         {
            foreach(var d in tcpConnections)
            {
               GMapMarker marker;

               if(!tcpConnections.TryGetValue("192.168.1.1", out marker))
               {
                  //marker = new GMapMarkerGoogleRed(new PointLatLng(d.Lat, d.Lng));
                  //marker.Tag = d.Id;
                  //marker.ToolTipMode = MarkerTooltipMode.Always;

                  //trolleybusMarkers[d.Id] = marker;
                  //objects.Markers.Add(marker);
               }
               else
               {
                  //marker.Position = new PointLatLng(d.Lat, d.Lng);
               }
               //marker.ToolTipText = d.Line;
            }
         }

         if(firstLoadConnections)
         {
            MainMap.ZoomAndCenterMarkers("objects");
            firstLoadConnections = false;
         }
         MainMap.Refresh();
      }

      void connectionsWorker_DoWork(object sender, DoWorkEventArgs e)
      {
         IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

         while(!connectionsWorker.CancellationPending)
         {
            try
            {
               // http://ipinfodb.com/ip_location_api.php

               // http://ipinfodb.com/ip_query2.php?ip=74.125.45.100,206.190.60.37&timezone=false

               //<?xml version="1.0" encoding="UTF-8"?>
               //<Locations>
               //  <Location id="0">
               //    <Ip>74.125.45.100</Ip>
               //    <Status>OK</Status>
               //    <CountryCode>US</CountryCode>
               //    <CountryName>United States</CountryName>
               //    <RegionCode>06</RegionCode>
               //    <RegionName>California</RegionName>
               //    <City>Mountain View</City>
               //    <ZipPostalCode>94043</ZipPostalCode>
               //    <Latitude>37.4192</Latitude>
               //    <Longitude>-122.057</Longitude>
               //  </Location>
               //  <Location id="1">
               //    <Ip>206.190.60.37</Ip>
               //    <Status>OK</Status>
               //    <CountryCode>US</CountryCode>
               //    <CountryName>United States</CountryName>
               //    <RegionCode>06</RegionCode>
               //    <RegionName>California</RegionName>
               //    <City>Sunnyvale</City>
               //    <ZipPostalCode>94089</ZipPostalCode>
               //    <Latitude>37.4249</Latitude>
               //    <Longitude>-122.007</Longitude>
               //  </Location>
               //</Locations>

               lock(tcpConnections)
               {
                  TcpConnectionInformation[] tcpInfoList = properties.GetActiveTcpConnections();

                  int c = 0;

                  foreach(TcpConnectionInformation i in tcpInfoList)
                  {
                     Debug.WriteLine(c++ + ": " + i.State + " -> " + i.RemoteEndPoint.Address + ":" + i.RemoteEndPoint.Port);
                  }

                  tcpInfoList = null;
               }

               connectionsWorker.ReportProgress(100);
            }
            catch(Exception ex)
            {
               Debug.WriteLine("connectionsWorker_DoWork: " + ex.ToString());
            }
            Thread.Sleep(3333);
         }
         tcpConnections.Clear();
      }

      #endregion

      void MainMap_OnMapTypeChanged(MapType type)
      {
         trackBar1.Minimum = MainMap.MinZoom;
         trackBar1.Maximum = MainMap.MaxZoom;

         if(routes.Routes.Count > 0)
         {
            MainMap.ZoomAndCenterRoutes(null);
         }

         if(radioButtonTransport.Checked)
         {
            MainMap.ZoomAndCenterMarkers("objects");
         }
      }

      string mobileGpsLog = string.Empty;

      // testing my mobile gps log
      void AddGpsMobileLogRoutes(string file)
      {
         try
         {
            DateTime? date = null;
            DateTime? dateEnd = null;

            if(MobileLogFrom.Checked)
            {
               date = MobileLogFrom.Value.ToUniversalTime();
            }

            if(MobileLogTo.Checked)
            {
               dateEnd = MobileLogTo.Value.ToUniversalTime();
            }

            var log = GMaps.Instance.GetRoutesFromMobileLog(file, date, dateEnd, 3.3);

            if(routes != null)
            {
               List<PointLatLng> track = new List<PointLatLng>();

               var sessions = new List<List<GpsLog>>(log);

               foreach(var session in sessions)
               {
                  track.Clear();

                  foreach(var point in session)
                  {
                     track.Add(point.Position);
                  }

                  GMapRoute gr = new GMapRoute(track, "");

                  routes.Routes.Add(gr);
               }

               sessions.Clear();
               sessions = null;

               track.Clear();
               track = null;
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("AddGpsMobileLogRoutes: " + ex.ToString());
         }
      }

      /// <summary>
      /// adds marker using geocoder
      /// </summary>
      /// <param name="place"></param>
      void AddLocationLithuania(string place)
      {
         GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
         PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, " + place, out status);
         if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
         {
            GMapMarkerGoogleGreen m = new GMapMarkerGoogleGreen(pos.Value);
            m.ToolTip = new GMapRoundedToolTip(m);

            GMapMarkerRect mBorders = new GMapMarkerRect(pos.Value);
            {
               mBorders.InnerMarker = m;
               mBorders.ToolTipText = place;
               mBorders.ToolTipMode = MarkerTooltipMode.Always;
            }

            objects.Markers.Add(m);
            objects.Markers.Add(mBorders);
         }
      }

      bool isMouseDown = false;
      void MainMap_MouseUp(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left)
         {
            isMouseDown = false;
         }
      }

      void MainMap_MouseDown(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left)
         {
            isMouseDown = true;
            currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
         }
      }

      // move current marker with left holding
      void MainMap_MouseMove(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left && isMouseDown)
         {
            if(CurentRectMarker == null)
            {
               currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
            }
            else // move rect marker
            {
               PointLatLng pnew = MainMap.FromLocalToLatLng(e.X, e.Y);

               int? pIndex = (int?) CurentRectMarker.Tag;
               if(pIndex.HasValue)
               {
                  if(pIndex < polygon.Points.Count)
                  {
                     polygon.Points[pIndex.Value] = pnew;
                     MainMap.UpdatePolygonLocalPosition(polygon);
                  }
               }

               currentMarker.Position = pnew;
               CurentRectMarker.Position = pnew;

               if(CurentRectMarker.InnerMarker != null)
               {
                  CurentRectMarker.InnerMarker.Position = pnew;
               }
            }
         }
      }

      // MapZoomChanged
      void MainMap_OnMapZoomChanged()
      {
         trackBar1.Value = (int) (MainMap.Zoom);
      }

      // click on some marker
      void MainMap_OnMarkerClick(GMapMarker item)
      {
         if(item is GMapMarkerRect)
         {
            Placemark pos = GMaps.Instance.GetPlacemarkFromGeocoder(item.Position);
            if(pos != null)
            {
               GMapMarkerRect v = item as GMapMarkerRect;
               {
                  v.ToolTipText = pos.Address;
               }
               MainMap.Invalidate(false);
            }
         }
      }

      // loader start loading tiles
      void MainMap_OnTileLoadStart()
      {
         MethodInvoker m = delegate()
         {
            progressBar1.Visible = true;
            toolStripStatusLabelLoading.Visible = true;
         };
         try
         {
            BeginInvoke(m);
         }
         catch
         {
         }
      }

      // loader end loading tiles
      void MainMap_OnTileLoadComplete()
      {
         MethodInvoker m = delegate()
         {
            progressBar1.Visible = false;
            toolStripStatusLabelLoading.Visible = false;

            toolStripStatusLabelMemoryCache.Text = string.Format("MemoryCache: {0:0.00}MB of {1:0.00}MB", MainMap.Manager.MemoryCacheSize, MainMap.Manager.MemoryCacheCapacity);
         };
         try
         {
            BeginInvoke(m);
         }
         catch
         {
         }
      }

      // current point changed
      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         center.Position = point;
         toolStripStatusLabelCurrentPosition.Text = "CurrentPosition: " + point;
      }

      // change map type
      private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.MapType = (MapType) comboBoxMapType.SelectedValue;
      }

      // change mdoe
      private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
      {
         GMaps.Instance.Mode = (AccessMode) comboBoxMode.SelectedValue;
         MainMap.ReloadMap();
      }

      // zoom
      private void trackBar1_ValueChanged(object sender, EventArgs e)
      {
         MainMap.Zoom = (trackBar1.Value);
      }

      // go to
      private void button8_Click(object sender, EventArgs e)
      {
         double lat = double.Parse(textBoxLat.Text, CultureInfo.InvariantCulture);
         double lng = double.Parse(textBoxLng.Text, CultureInfo.InvariantCulture);

         MainMap.CurrentPosition = new PointLatLng(lat, lng);
      }

      // goto by geocoder
      private void textBoxGeo_KeyPress(object sender, KeyPressEventArgs e)
      {
         if((Keys) e.KeyChar == Keys.Enter)
         {
            GeoCoderStatusCode status = MainMap.SetCurrentPositionByKeywords(textBoxGeo.Text);
            if(status != GeoCoderStatusCode.G_GEO_SUCCESS)
            {
               MessageBox.Show("Google Maps Geocoder can't find: '" + textBoxGeo.Text  + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
         }
      }

      // reload map
      private void button1_Click(object sender, EventArgs e)
      {
         MainMap.ReloadMap();
      }

      // cache config
      private void checkBoxUseCache_CheckedChanged(object sender, EventArgs e)
      {
         GMaps.Instance.UseRouteCache = checkBoxUseRouteCache.Checked;
         GMaps.Instance.UseGeocoderCache = checkBoxUseGeoCache.Checked;
         GMaps.Instance.UsePlacemarkCache = GMaps.Instance.UseGeocoderCache;
      }

      // clear cache
      private void button2_Click(object sender, EventArgs e)
      {
         if(MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
         {
            try
            {
               System.IO.Directory.Delete(MainMap.CacheLocation, true);
            }
            catch(Exception ex)
            {
               MessageBox.Show(ex.Message);
            }
         }
      }

      // add test route
      private void button3_Click(object sender, EventArgs e)
      {
         MapRoute route = GMaps.Instance.GetRouteBetweenPoints(start, end, false, (int) MainMap.Zoom);
         if(route != null)
         {
            // add route
            GMapRoute r = new GMapRoute(route.Points, route.Name);
            routes.Routes.Add(r);

            // add route start/end marks
            GMapMarker m1 = new GMapMarkerGoogleRed(start);
            m1.ToolTipText = "Start: " + route.Name;
            m1.ToolTipMode = MarkerTooltipMode.Always;

            GMapMarker m2 = new GMapMarkerGoogleGreen(end);
            m2.ToolTipText = "End: " + end.ToString();
            m2.ToolTipMode = MarkerTooltipMode.Always;

            objects.Markers.Add(m1);
            objects.Markers.Add(m2);

            MainMap.ZoomAndCenterRoute(r);

            // testing kml support
            //KmlType info = GMaps.Instance.GetRouteBetweenPointsKml(start, end, false);
            //if(info != null)
            //{

            //}
         }
      }

      // add marker on current position
      private void button4_Click(object sender, EventArgs e)
      {
         GMapMarkerGoogleGreen m = new GMapMarkerGoogleGreen(currentMarker.Position);
         GMapMarkerRect mBorders = new GMapMarkerRect(currentMarker.Position);
         {
            mBorders.InnerMarker = m;
            mBorders.Tag = polygon.Points.Count;
            mBorders.ToolTipMode = MarkerTooltipMode.Always;
         }

         Placemark p = null;
         if(checkBoxPlacemarkInfo.Checked)
         {
            p = GMaps.Instance.GetPlacemarkFromGeocoder(currentMarker.Position);
         }

         if(p != null)
         {
            mBorders.ToolTipText = p.Address;
         }
         else
         {
            mBorders.ToolTipText = currentMarker.Position.ToString();
         }

         objects.Markers.Add(m);
         objects.Markers.Add(mBorders);

         RegeneratePolygon();
      }

      // clear routes
      private void button6_Click(object sender, EventArgs e)
      {
         routes.Routes.Clear();
      }

      // clear polygons
      private void button15_Click(object sender, EventArgs e)
      {
         polygons.Polygons.Clear();
      }

      // clear markers
      private void button5_Click(object sender, EventArgs e)
      {
         objects.Markers.Clear();
      }

      // show current marker
      private void checkBoxCurrentMarker_CheckedChanged(object sender, EventArgs e)
      {
         if(checkBoxCurrentMarker.Checked)
         {
            top.Markers.Add(currentMarker);
         }
         else
         {
            top.Markers.Remove(currentMarker);
         }
      }

      // can drag
      private void checkBoxCanDrag_CheckedChanged(object sender, EventArgs e)
      {
         MainMap.CanDragMap = checkBoxCanDrag.Checked;
      }

      // set route start
      private void buttonSetStart_Click(object sender, EventArgs e)
      {
         start = currentMarker.Position;
      }

      // set route end
      private void buttonSetEnd_Click(object sender, EventArgs e)
      {
         end = currentMarker.Position;
      }

      // zoom to max for markers
      private void button7_Click(object sender, EventArgs e)
      {
         MainMap.ZoomAndCenterMarkers("objects");
      }

      // expord map data
      private void button9_Click(object sender, EventArgs e)
      {
         MainMap.ShowExportDialog();
      }

      // import map data
      private void button10_Click(object sender, EventArgs e)
      {
         MainMap.ShowImportDialog();
      }

      // prefetch
      private void button11_Click(object sender, EventArgs e)
      {
         RectLatLng area = MainMap.SelectedArea;
         if(!area.IsEmpty)
         {
            for(int i = (int) MainMap.Zoom; i <= MainMap.MaxZoom; i++)
            {
               List<GMap.NET.Point> x = MainMap.Projection.GetAreaTileList(area, i, 0);

               DialogResult res = MessageBox.Show("Ready ripp at Zoom = " + i + " ? Total => " + x.Count, "GMap.NET", MessageBoxButtons.YesNoCancel);

               if(res == DialogResult.Yes)
               {
                  TilePrefetcher obj = new TilePrefetcher();
                  obj.ShowCompleteMessage = true;
                  obj.Start(x, i, MainMap.MapType, 100);
               }
               else if(res == DialogResult.No)
               {
                  continue;
               }
               else if(res == DialogResult.Cancel)
               {
                  break;
               }

               x.Clear();
            }
         }
         else
         {
            MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
         }
      }

      // saves current map view 
      private void button12_Click(object sender, EventArgs e)
      {
         try
         {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
               sfd.Filter = "PNG (*.png)|*.png";
               sfd.FileName = "GMap.NET image";

               Image tmpImage = MainMap.ToImage();
               if(tmpImage != null)
               {
                  using(tmpImage)
                  {
                     if(sfd.ShowDialog() == DialogResult.OK)
                     {
                        tmpImage.Save(sfd.FileName);

                        MessageBox.Show("Image saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            MessageBox.Show("Image failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      // debug
      private void checkBoxDebug_CheckedChanged(object sender, EventArgs e)
      {
         MainMap.ShowTileGridLines = checkBoxDebug.Checked;
      }

      private void button13_Click(object sender, EventArgs e)
      {
         StaticImage st = new StaticImage(this);
         st.Owner = this;
         st.Show();
      }

      // add gps log from mobile
      private void button14_Click(object sender, EventArgs e)
      {
         using(FileDialog dlg = new OpenFileDialog())
         {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gpsd";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: open gps log generated in your windows mobile";
            dlg.Filter = "GMap.NET gps log DB files (*.gpsd)|*.gpsd";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
               routes.Routes.Clear();

               mobileGpsLog = dlg.FileName;

               // test
               AddGpsMobileLogRoutes(dlg.FileName);

               if(routes.Routes.Count > 0)
               {
                  MainMap.ZoomAndCenterRoutes(null);
               }
            }
         }
      }

      private void MainForm_Load(object sender, EventArgs e)
      {
         MainMap.ZoomAndCenterMarkers(null);
         trackBar1.Value = (int) MainMap.Zoom;
      }

      // ensure focus on map, trackbar can have it too
      private void MainMap_MouseEnter(object sender, EventArgs e)
      {
         MainMap.Focus();
      }

      private void MainForm_KeyUp(object sender, KeyEventArgs e)
      {
         int offset = 22;

         if(e.KeyCode == Keys.Left)
         {
            MainMap.Offset(-offset, 0);
         }
         else if(e.KeyCode == Keys.Right)
         {
            MainMap.Offset(offset, 0);
         }
         else if(e.KeyCode == Keys.Up)
         {
            MainMap.Offset(0, -offset);
         }
         else if(e.KeyCode == Keys.Down)
         {
            MainMap.Offset(0, offset);
         }
         else if(e.KeyCode == Keys.Delete)
         {
            if(CurentRectMarker != null)
            {
               objects.Markers.Remove(CurentRectMarker);

               if(CurentRectMarker.InnerMarker != null)
               {
                  objects.Markers.Remove(CurentRectMarker.InnerMarker);
               }
               CurentRectMarker = null;

               RegeneratePolygon();
            }
         }
      }

      private void RealTimeChanged(object sender, EventArgs e)
      {
         objects.Markers.Clear();
         objects.Routes.Clear();
         polygons.Polygons.Clear();

         // start performance test
         if(radioButtonPerf.Checked)
         {
            timerPerf.Interval = 44;
            timerPerf.Tick += new EventHandler(timer_Tick);
            timerPerf.Start();
         }
         else
         {
            // stop performance test
            timerPerf.Stop();
         }

         // start realtime transport tracking demo
         if(radioButtonTransport.Checked)
         {
            if(!transport.IsBusy)
            {
               firstLoadTrasport = true;
               transport.RunWorkerAsync();
            }
         }
         else
         {
            if(transport.IsBusy)
            {
               transport.CancelAsync();
            }
         }
      }

      // export mobile gps log to gpx file
      private void buttonExportToGpx_Click(object sender, EventArgs e)
      {
         try
         {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
               sfd.Filter = "GPX (*.gpx)|*.gpx";
               sfd.FileName = "mobile gps log";

               DateTime? date = null;
               DateTime? dateEnd = null;

               if(MobileLogFrom.Checked)
               {
                  date = MobileLogFrom.Value.ToUniversalTime();

                  sfd.FileName += " from " + MobileLogFrom.Value.ToString("yyyy-MM-dd HH-mm");
               }

               if(MobileLogTo.Checked)
               {
                  dateEnd = MobileLogTo.Value.ToUniversalTime();

                  sfd.FileName += " to " + MobileLogTo.Value.ToString("yyyy-MM-dd HH-mm");
               }

               if(sfd.ShowDialog() == DialogResult.OK)
               {
                  var log = GMaps.Instance.GetRoutesFromMobileLog(mobileGpsLog, date, dateEnd, 3.3);
                  if(log != null)
                  {
                     if(MainMap.Manager.ExportGPX(log, sfd.FileName))
                     {
                        MessageBox.Show("GPX saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            MessageBox.Show("GPX failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      // load gpx file
      private void button16_Click(object sender, EventArgs e)
      {
         using(FileDialog dlg = new OpenFileDialog())
         {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gpx";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: open gpx log";
            dlg.Filter = "gpx files (*.gpx)|*.gpx";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
               try
               {
                  string gpx = File.ReadAllText(dlg.FileName);

                  gpxType r = GMaps.Instance.DeserializeGPX(gpx);
                  if(r != null)
                  {
                     if(r.trk.Length > 0)
                     {
                        foreach(var trk in r.trk)
                        {
                           List<PointLatLng> points = new List<PointLatLng>();

                           foreach(var seg in trk.trkseg)
                           {
                              foreach(var p in seg.trkpt)
                              {
                                 points.Add(new PointLatLng((double) p.lat, (double) p.lon));
                              }
                           }

                           GMapRoute rt = new GMapRoute(points, string.Empty);
                           {
                              rt.Stroke = new Pen(Color.FromArgb(144, Color.Red));
                              rt.Stroke.Width = 5;
                              rt.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                           }
                           routes.Routes.Add(rt);
                        }

                        MainMap.ZoomAndCenterRoutes(null);
                     }
                  }
               }
               catch(Exception ex)
               {
                  Debug.WriteLine("GPX import: " + ex.ToString());
                  MessageBox.Show("Error importing gpx: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }
            }
         }
      }
   }
}
