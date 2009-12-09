using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.CacheProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.IO;
using System.Diagnostics;

namespace Demo.WindowsForms
{
   public partial class MainForm : Form
   {
      PointLatLng start;
      PointLatLng end;

      // marker
      GMapMarker currentMarker;
      GMapMarker center;

      // layers
      GMapOverlay top;
      GMapOverlay objects;
      GMapOverlay routes;

      public MainForm()
      {
         InitializeComponent();

         if(!DesignMode)
         {
            // config gmaps
            GMaps.Instance.UseRouteCache = true;
            GMaps.Instance.UseGeocoderCache = true;
            GMaps.Instance.UsePlacemarkCache = true;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            // add your custom map db provider
            //MySQLPureImageCache ch = new MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //GMaps.Instance.ImageCacheSecond = ch;

            // set your proxy here if need
            //GMaps.Instance.Proxy = new WebProxy("10.2.0.100", 8080);
            //GMaps.Instance.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

            // config map 
            MainMap.MapType = MapType.ArcGIS_MapsLT_Map;
            MainMap.MaxZoom = 11;
            MainMap.MinZoom = 2;
            MainMap.Zoom = MainMap.MinZoom + 1;
            MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);
            //MainMap.CurrentPosition = new PointLatLng(29.8741410626414, 121.563806533813); // china test

            // map events
            MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnMarkerClick += new MarkerClick(MainMap_OnMarkerClick);
            MainMap.OnEmptyTileError += new EmptyTileError(MainMap_OnEmptyTileError);
            MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseDown += new MouseEventHandler(MainMap_MouseDown);
            MainMap.MouseUp += new MouseEventHandler(MainMap_MouseUp);

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

            // get zoom  
            trackBar1.Minimum = MainMap.MinZoom;
            trackBar1.Maximum = MainMap.MaxZoom;

#if !DEBUG
            checkBoxDebug.Checked = false;
#endif

            // add custom layers  
            {
               routes = new GMapOverlay(MainMap, "routes");
               MainMap.Overlays.Add(routes);

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
                  myCity.TooltipMode = MarkerTooltipMode.Always;
                  myCity.ToolTipText = "Welcome to Lithuania! ;}";
                  objects.Markers.Add(myCity);
               }
            }

            // add some point in lithuania
            {
               AddLocationLithuania("Klaipėda");
               AddLocationLithuania("Šiauliai");
               AddLocationLithuania("Panevėžys");
               AddLocationLithuania("Kaunas");
            }

            MainMap.ZoomAndCenterMarkers(null);

            trackBar1.Value = (int) MainMap.Zoom;
         }
      }

      void MainMap_OnMapTypeChanged(MapType type)
      {
         switch(type)
         {
            case MapType.ArcGIS_Map:
            case MapType.ArcGIS_Satellite:
            case MapType.ArcGIS_ShadedRelief:
            case MapType.ArcGIS_Terrain:
            {
               MainMap.MaxZoom = 13;
            }
            break;

            case MapType.ArcGIS_MapsLT_Map_Hybrid:
            case MapType.ArcGIS_MapsLT_Map_Labels:
            case MapType.ArcGIS_MapsLT_Map:
            case MapType.ArcGIS_MapsLT_OrtoFoto:
            {
               MainMap.MaxZoom = 11;
            }
            break;

            case MapType.OpenStreetMapSurfer:
            case MapType.OpenStreetMapSurferTerrain:
            {
               MainMap.MaxZoom = 19;
            }
            break;

            default:
            {
               MainMap.MaxZoom = 17;
            }
            break;
         }

         if(MainMap.Zoom > MainMap.MaxZoom)
         {
            MainMap.Zoom = MainMap.MaxZoom;
         }
         trackBar1.Maximum = MainMap.MaxZoom;

         if(routes.Routes.Count > 0)
         {
            MainMap.ZoomAndCenterRoutes(null);
         }
      }

      // testing my mobile gp log
      void AddGpsMobileLogRoutes(string file)
      {
         try
         {
            DateTime? date = null;
            DateTime? dateEnd = null;
            if(dateTimePickerMobileLog.Checked)
            {
               date = dateTimePickerMobileLog.Value.Date.ToUniversalTime();
               dateEnd = date.Value.AddDays(1);                 
            }

            var log = GMaps.Instance.GetRoutesFromMobileLog(file, date, dateEnd, 3.3);

            if(routes != null)
            {
               foreach(var r in log)
               {
                  {
                     GMapRoute gr = new GMapRoute(r, "");
                     {
                        gr.Color = Color.Blue;
                     }
                     routes.Routes.Add(gr);

                     //GMapMarker m1 = new GMapMarkerGoogleRed(points[points.Count-1]);
                     //m1.ToolTipText = "End";
                     //m1.TooltipMode = MarkerTooltipMode.Always;
                     //objects.Markers.Add(m1);
                  }
                  //else // log
                  {
                     {
                        //{
                        //   GMapMarker m1 = new GMapMarkerGoogleRed(points[0]);
                        //   m1.ToolTipText = "Start";
                        //   m1.TooltipMode = MarkerTooltipMode.Always;
                        //   objects.Markers.Add(m1);
                     }
                  }
               }
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
            GMapMarker m = new GMapMarkerGoogleGreen(pos.Value);
            GMapMarkerRect mBorders = new GMapMarkerRect(pos.Value);
            mBorders.Size = new System.Drawing.Size(100, 100);
            {
               mBorders.ToolTipText = place;
               mBorders.TooltipMode = MarkerTooltipMode.Always;
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

      void UpdateCurrentMarkerPositionText()
      {
         textBoxCurrLat.Text = currentMarker.Position.Lat.ToString(CultureInfo.InvariantCulture);
         textBoxCurrLng.Text = currentMarker.Position.Lng.ToString(CultureInfo.InvariantCulture);
      }

      void MainMap_MouseDown(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left)
         {
            isMouseDown = true;
            currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
            UpdateCurrentMarkerPositionText();
         }
      }

      // move current marker with left holding
      void MainMap_MouseMove(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left && isMouseDown)
         {
            currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
            UpdateCurrentMarkerPositionText();
         }
      }

      // MapZoomChanged
      void MainMap_OnMapZoomChanged()
      {
         trackBar1.Value = (int) (MainMap.Zoom);
      }

      // empty tile displayed
      void MainMap_OnEmptyTileError(int zoom, GMap.NET.Point pos)
      {
         MessageBox.Show("OnEmptyTileError, Zoom: " + zoom + ", " + pos.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }

      // click on some marker
      void MainMap_OnMarkerClick(GMapMarker item)
      {
         MainMap.CurrentPosition = item.Position;
         MainMap.Zoom = 5;
      }

      // loader start loading tiles
      void MainMap_OnTileLoadStart(int loaderId)
      {
         switch(loaderId)
         {
            case 1:
            progressBar1.Show();
            break;

            case 2:
            progressBar2.Show();
            break;

            case 3:
            progressBar3.Show();
            break;
         }

         groupBoxLoading.Invalidate(true);
      }

      // loader end loading tiles
      void MainMap_OnTileLoadComplete(int loaderId)
      {
         switch(loaderId)
         {
            case 1:
            progressBar1.Hide();
            break;

            case 2:
            progressBar2.Hide();
            break;

            case 3:
            progressBar3.Hide();
            break;
         }

         groupBoxLoading.Invalidate(true);
      }

      // current point changed
      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         center.Position = point;
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
            r.Color = Color.Blue;
            routes.Routes.Add(r);

            // add route start/end marks
            GMapMarker m1 = new GMapMarkerGoogleRed(start);
            m1.ToolTipText = "Start: " + route.Name;
            m1.TooltipMode = MarkerTooltipMode.Always;

            GMapMarker m2 = new GMapMarkerGoogleGreen(end);
            m2.ToolTipText = "End: " + end.ToString();
            m2.TooltipMode = MarkerTooltipMode.Always;

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
         GMapMarker m = new GMapMarkerGoogleGreen(currentMarker.Position);
         GMapMarkerRect mBorders = new GMapMarkerRect(currentMarker.Position);
         mBorders.Size = new System.Drawing.Size(100, 100);

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
      }

      // clear routes
      private void button6_Click(object sender, EventArgs e)
      {
         routes.Routes.Clear();
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
         RectLatLng area = MainMap.SelectedArea;
         if(!area.IsEmpty)
         {
            StaticImage st = new StaticImage(MainMap);
            st.Owner = this;
            st.Show();
         }
         else
         {
            MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
         }
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
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            dlg.Filter = "GMap.NET gps log DB files (*.gpsd)|*.gpsd";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
               routes.Routes.Clear();

               // test
               AddGpsMobileLogRoutes(dlg.FileName);

               if(routes.Routes.Count > 0)
               {
                  MainMap.ZoomAndCenterRoutes(null);
               }
            }
         }
      }
   }
}
