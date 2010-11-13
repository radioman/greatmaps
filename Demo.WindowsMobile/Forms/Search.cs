using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Demo.WindowsMobile
{
   public partial class Search : UserControl
   {
      MainForm Main;

      public Search(MainForm main)
      {
         InitializeComponent();
         Main = main;
      }

      private void button1_Click(object sender, EventArgs e)
      {
         try
         {
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
            {
               PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder(textAddress.Text, out status);
               if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
               {
                  GMapMarker address = new GMapMarkerTransparentGoogleGreen(pos.Value);
                  address.ToolTipMode = MarkerTooltipMode.Always;
                  address.ToolTipText = textAddress.Text;
                  Main.objects.Markers.Add(address);

                  Main.MainMap.Position = address.Position;
                  Main.menuItemGotoMap_Click(null, null);
               }
               else
               {
                  labelstatus.Text = status.ToString();
               }
            }
         }
         catch(Exception ex)
         {
            labelstatus.Text = ex.ToString();
         }
      }
   }
}
