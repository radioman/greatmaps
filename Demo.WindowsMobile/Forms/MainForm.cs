using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GMapNET;

namespace Demo.WindowsMobile
{
   public partial class MainForm : Form
   {
      public MainForm()
      {
         InitializeComponent();
         {
            // config gmaps
            GMaps.Instance.Language = "lt";
            GMaps.Instance.UseTileCache = true;
            GMaps.Instance.UseRouteCache = true;
            GMaps.Instance.UseGeocoderCache = true;
            GMaps.Instance.UsePlacemarkCache = true;

            // set your proxy here if need
            //GMaps.Instance.Proxy = new WebProxy("10.2.0.100", 8080);
            //GMaps.Instance.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeadam");

            // config map             
            MainMap.MapType = GMapType.OpenStreetMap;
            MainMap.Zoom = 12;
            MainMap.CurrentMarkerEnabled = true;
            MainMap.CurrentMarkerStyle = CurrentMarkerType.GMap;
            MainMap.CurrentPosition = new PointLatLng(54.6961334816182, 25.2985095977783);
         }
      }

      protected override void OnLoad(EventArgs e)
      {
         MainMap.ReloadMap();
         base.OnLoad(e);
      }
   }
}