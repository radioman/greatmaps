using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.IO;

namespace Demo.WindowsForms
{
   public partial class MainForm : Form
   {
      PointLatLng start;
      PointLatLng end;

      // marker
      GMapMarker currentMarker;

      // layers
      GMapOverlay top;
      GMapOverlay objects;
      GMapOverlay routes;

      // testing projection
      GMap.NET.Projections.LKS94Projection prj = new GMap.NET.Projections.LKS94Projection();

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
            //MSSQLPureImageCache ch = new MSSQLPureImageCache();
            //ch.ConnectionString = @"Data Source=RADIOMAN-PC\SQLEXPRESS;Initial Catalog=Test;Persist Security Info=False;User ID=aa;Password=aa;";
            //GMaps.Instance.ImageCacheSecond = ch;

            // set your proxy here if need
            //GMaps.Instance.Proxy = new WebProxy("10.2.0.100", 8080);
            //GMaps.Instance.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

            // config map 
            MainMap.MapType = MapType.GoogleMap;
            MainMap.MaxZoom = 12;
            MainMap.MinZoom = 1;
            MainMap.Zoom = MainMap.MinZoom;
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
            trackBar1.Value = MainMap.Zoom;

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
               MainMap.MaxZoom = 12;
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
         trackBar1.Value = MainMap.Zoom;
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
         currentMarker.Position = point;
         UpdateCurrentMarkerPositionText();
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
         MainMap.Zoom = trackBar1.Value;
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
         MapRoute route = GMaps.Instance.GetRouteBetweenPoints(start, end, false, MainMap.Zoom);
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
            KmlType info = GMaps.Instance.GetRouteBetweenPointsKml(start, end, false);
            if(info != null)
            {

            }
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
         if(MainMap.ZoomAndCenterMarkers("objects"))
         {
            trackBar1.Value = MainMap.Zoom > trackBar1.Maximum ? trackBar1.Maximum : MainMap.Zoom;
         }
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
         RectLatLng area = MainMap.CurrentViewArea;

         for(int i = MainMap.Zoom; i <= MainMap.MaxZoom; i++)
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
                  if(sfd.ShowDialog() == DialogResult.OK)
                  {
                     tmpImage.Save(sfd.FileName);

                     MessageBox.Show("Image saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            MessageBox.Show("Select map holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
         }
      }
   }
}
