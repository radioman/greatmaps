using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using GMapNET;
using System.Drawing;

namespace Demo.WindowsForms
{
   public partial class MainForm : Form
   {
      PointLatLng start;
      PointLatLng end;

      // marker
      GMapMarker currentMarker;

      // layers
      GMapOverlay ground;
      GMapOverlay objects;
      GMapOverlay routes;

      public MainForm()
      {
         InitializeComponent();

         if(!DesignMode)
         {
            // config gmaps
            GMaps.Instance.Language = "lt";
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
            MainMap.MaxZoom = 17;
            MainMap.MinZoom = 12;
            MainMap.Zoom = MainMap.MinZoom;
            MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);
            //MainMap.CurrentPosition = new PointLatLng(-40.913512576127573, 173.408203125);

            // map events
            MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnMarkerClick += new MarkerClick(MainMap_OnMarkerClick);
            MainMap.OnEmptyTileError += new EmptyTileError(MainMap_OnEmptyTileError);
            MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);
            
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

            // set current marker and get ground layer
            currentMarker = new GMapMarkerGoogleRed(MainMap.CurrentPosition);
            ground = MainMap.Overlays[0] as GMapOverlay;
            ground.Markers.Add(currentMarker);

            // add custom layers
            {
               objects = new GMapOverlay(MainMap, "objects");
               MainMap.Overlays.Add(objects);

               routes = new GMapOverlay(MainMap, "routes");
               MainMap.Overlays.Add(routes);
            }

            // add my city location for demo
            PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius");
            if(pos != null)
            {
               currentMarker.Position = pos.Value;

               GMapMarker myCity = new GMapMarkerGoogleGreen(pos.Value);
               myCity.TooltipMode = MarkerTooltipMode.Always;
               myCity.ToolTipText = "Welcome to Lithuania! ;}";
               ground.Markers.Add(myCity);
            }
         }
      }

      // MapZoomChanged
      void MainMap_OnMapZoomChanged()
      {
         trackBar1.Value = MainMap.Zoom;
      }

      // empty tile displayed
      void MainMap_OnEmptyTileError(int zoom, GMapNET.Point pos)
      {
         MessageBox.Show("OnEmptyTileError, Zoom: " + zoom + ", " + pos.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }

      // on shown, do not forget this! ;}
      private void MainForm_Shown(object sender, EventArgs e)
      {
         MainMap.ReloadMap();
      }

      // click on some marker
      void MainMap_OnMarkerClick(GMapMarker item)
      {
         MessageBox.Show("OnMarkerClick: " + item.Position.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
         textBoxCurrLat.Text = point.Lat.ToString(CultureInfo.InvariantCulture);
         textBoxCurrLng.Text = point.Lng.ToString(CultureInfo.InvariantCulture);

         currentMarker.Position = point;
         MainMap.UpdateMarkerLocalPosition(currentMarker);
      }

      // change map type
      private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.MapType = (MapType) comboBoxMapType.SelectedValue;
         MainMap.ReloadMap();
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
         MainMap.GoToCurrentPosition();
      }

      // goto by geocoder
      private void textBoxGeo_KeyPress(object sender, KeyPressEventArgs e)
      {
         if((Keys) e.KeyChar == Keys.Enter)
         {
            if(!MainMap.SetCurrentPositionByKeywords(textBoxGeo.Text))
            {
               MessageBox.Show("Google Maps Geocoder can't find: " + textBoxGeo.Text, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
               MainMap.GoToCurrentPosition();
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
         List<PointLatLng> route = GMaps.Instance.GetRouteBetweenPoints(start, end, false, MainMap.Zoom);
         if(route != null)
         {
            // add route
            GMapRoute r = new GMapRoute(route, "test");
            r.Color = Color.Blue;
            routes.Routes.Add(r);

            // add route start/end marks
            GMapMarker m1 = new GMapMarkerGoogleRed(start);
            m1.ToolTipText = "Start: " + start.ToString();
            m1.TooltipMode = MarkerTooltipMode.Always;

            GMapMarker m2 = new GMapMarkerGoogleGreen(end);
            m2.ToolTipText = "End: " + end.ToString();
            m2.TooltipMode = MarkerTooltipMode.Always;

            objects.Markers.Add(m1);
            objects.Markers.Add(m2);

            MainMap.ZoomAndCenterRoute(r);
         }
      }

      // add marker on current position
      private void button4_Click(object sender, EventArgs e)
      {
         GMapMarker m = new GMapMarkerGoogleGreen(MainMap.CurrentPosition);
         GMapMarkerRect mBorders = new GMapMarkerRect(MainMap.CurrentPosition);
         mBorders.Size = new GMapNET.Size(100, 100);

         Placemark p = null;
         if(checkBoxPlacemarkInfo.Checked)
         {
            p = GMaps.Instance.GetPlacemarkFromGeocoder(MainMap.CurrentPosition);
         }

         if(p != null)
         {
            mBorders.ToolTipText = p.Address;
         }
         else
         {
            mBorders.ToolTipText = MainMap.CurrentPosition.ToString();
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
            ground.Markers.Add(currentMarker);
         }
         else
         {
            ground.Markers.Remove(currentMarker);
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
         start = MainMap.CurrentPosition;
      }

      // set route end
      private void buttonSetEnd_Click(object sender, EventArgs e)
      {
         end = MainMap.CurrentPosition;
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

         for(int i = MainMap.Zoom; i <= GMaps.Instance.MaxZoom; i++)
         {
            List<GMapNET.Point> x = GMaps.Instance.GetAreaTileList(area, i);

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
   }
}
