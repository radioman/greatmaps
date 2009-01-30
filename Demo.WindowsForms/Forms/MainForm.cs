using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using System.Net;
using System.ComponentModel;
using Demo.WindowsForms.Properties; 
using GMapNET;

namespace Demo.WindowsForms
{
   public partial class MainForm : Form
   {
      PointLatLng start;
      PointLatLng end;

      public MainForm()
      {
         InitializeComponent();

         if(LicenseManager.UsageMode != LicenseUsageMode.Designtime)
         {
            // config gmaps
            GMaps.Instance.Language = "lt";
            GMaps.Instance.UseTileCache = true;
            GMaps.Instance.UseRouteCache = true;
            GMaps.Instance.UseGeocoderCache = true;
            GMaps.Instance.UsePlacemarkCache = true;
            GMaps.Instance.Timeout = 10*1000;

            // set your proxy here if need
            //GMaps.Instance.Proxy = new WebProxy("10.2.0.100", 8080);
            //GMaps.Instance.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

            // config map             
            MainMap.RenderMode = RenderMode.GDI;
            MainMap.MapType = GMapType.GoogleMap;
            MainMap.Zoom = 12;
            MainMap.CurrentMarkerEnabled = true;
            MainMap.CurrentMarkerStyle = CurrentMarkerType.GMap;
            MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);

            // map events
            MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnMarkerClick += new MarkerClick(MainMap_OnMarkerClick);

            // get map type
            comboBoxMapType.DataSource = Enum.GetValues(typeof(GMapType));
            comboBoxMapType.SelectedItem = MainMap.MapType;

            // get position
            textBoxLat.Text = MainMap.CurrentPosition.Lat.ToString(CultureInfo.InvariantCulture);
            textBoxLng.Text = MainMap.CurrentPosition.Lng.ToString(CultureInfo.InvariantCulture);

            // get render type
            comboBoxRenderType.DataSource = Enum.GetValues(typeof(RenderMode));
            comboBoxRenderType.SelectedItem = MainMap.RenderMode;

            // get cache modes
            checkBoxUseTileCache.Checked = GMaps.Instance.UseTileCache;
            checkBoxUseRouteCache.Checked = GMaps.Instance.UseRouteCache;
            checkBoxUseGeoCache.Checked = GMaps.Instance.UseGeocoderCache;

            // get zoom
            trackBar1.Maximum = GMaps.Instance.MaxZoom;
            trackBar1.Value = MainMap.Zoom;
         }
      }

      // click on some marker
      void MainMap_OnMarkerClick(Marker item)
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
      }

      // change rendering mode
      private void comboBoxRenderType_DropDownClosed(object sender, EventArgs e)
      {
         RenderMode mode =  (RenderMode) comboBoxMapType.SelectedValue;

         if(mode != RenderMode.WPF)
         {
            MainMap.RenderMode = mode;
            MainMap.ReloadMap();
         }

         comboBoxRenderType.SelectedItem = MainMap.RenderMode;
      }

      // change map type
      private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.MapType = (GMapType) comboBoxMapType.SelectedValue;
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

      // get draw mode sometimes...
      private void MainMap_MouseDown(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Right)
         {
            comboBoxRenderType.SelectedItem = MainMap.RenderMode;
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
         GMaps.Instance.UseTileCache = checkBoxUseTileCache.Checked;
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
         // klaipeda - vilnius
         //PointLatLng start = new PointLatLng(55.70466, 21.2261);
         //PointLatLng end = new PointLatLng(54.654769, 25.224609);

         // testing route
         //PointLatLng start = new PointLatLng(54.7290810525512, 25.2708721160889);
         //PointLatLng end = new PointLatLng(54.710441321148, 25.314474105835);

         List<PointLatLng> route = GMaps.Instance.GetRouteBetweenPoints(start, end, false, MainMap.Zoom);
         if(route != null)
         {
            // add route
            Route r = new Route(route, "test");
            MainMap.AddRoute(r);

            // add route start/end marks
            Marker m1 = new Marker(start, MarkerType.Medium, MarkerColor.Green);
            m1.Text = "Start: " + start.ToString();
            m1.TooltipMode = MarkerTooltipMode.Always;

            Marker m2 = new Marker(end, MarkerType.Medium, MarkerColor.Yellow);
            m2.Text = "End: " + end.ToString();
            m2.TooltipMode = MarkerTooltipMode.Always;

            MainMap.AddMarker(m1);
            MainMap.AddMarker(m2);          
         }
      }

      private Random rand = new Random();
      public T RandomEnum<T>()
      {
         T[] values = (T[]) Enum.GetValues(typeof(T));
         return values[rand.Next(0, values.Length)];
      }

      // add marker on current position
      private void button4_Click(object sender, EventArgs e)
      {
         Marker m = new Marker(MainMap.CurrentPosition, RandomEnum<MarkerType>(), RandomEnum<MarkerColor>());

         Placemark p = null;
         if(checkBoxPlacemarkInfo.Checked)
         {
            p = GMaps.Instance.GetPlacemarkFromGeocoder(MainMap.CurrentPosition);
         }

         if(p != null)
         {
            m.Text = p.Address;
         }
         else
         {
            m.Text = MainMap.CurrentPosition.ToString();
         }

         if(m.Type == MarkerType.Custom)
         {
            m.CustomMarker = Properties.Resources.MapPointer;
            m.CustomMarkerAlign = CustomMarkerAlign.MiddleMiddle;

            // the same aligment can be that, because image is 40x40
            // so you can set marker 'center' anywhere on your image
            //
            //m.CustomMarkerAlign = CustomMarkerAlign.Manual;
            //m.CustomMarkerCenter = new System.Drawing.Point(20, 20);             
         }

         MainMap.AddMarker(m);
      }

      // clear routes
      private void button6_Click(object sender, EventArgs e)
      {
         MainMap.ClearAllRoutes();
      }

      // clear markers
      private void button5_Click(object sender, EventArgs e)
      {
         MainMap.ClearAllMarkers();         
      }

      // show current marker
      private void checkBoxCurrentMarker_CheckedChanged(object sender, EventArgs e)
      {
         MainMap.CurrentMarkerEnabled = checkBoxCurrentMarker.Checked;
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
         if(MainMap.ZoomAndCenterMarkers())
         {
            trackBar1.Value = MainMap.Zoom;
         }
      }

      // on shown
      private void MainForm_Shown(object sender, EventArgs e)
      {
         MainMap.ReloadMap();
      }
   }
}
