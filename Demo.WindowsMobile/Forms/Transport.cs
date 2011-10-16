using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Demo.WindowsForms;
using System.Linq;

namespace Demo.WindowsMobile
{
   public partial class Transport : UserControl
   {
      MainForm Main;
      int Count = 0;

      readonly List<VehicleData> Bus = new List<VehicleData>();
      readonly List<VehicleData> Trolley = new List<VehicleData>();

      readonly List<GMapMarkerTransparent> BusMarkers = new List<GMapMarkerTransparent>();
      readonly List<GMapMarkerTransparent> TrolleyMarkers = new List<GMapMarkerTransparent>();

      public Transport(MainForm main)
      {
         InitializeComponent();
         Main = main;
      }

      private void checkBoxRefresh_CheckStateChanged(object sender, EventArgs e)
      {
         timerRefresh.Enabled = checkBoxRefresh.Checked;
         labelstatus.Text = string.Empty;
         Count = 0;
      }

      private void checkBoxBus_CheckStateChanged(object sender, EventArgs e)
      {
         if(!checkBoxBus.Checked)
         {
            foreach(var b in BusMarkers)
            {
               Main.objects.Markers.Remove(b);
            }
         }
      }

      private void checkBoxTrolley_CheckStateChanged(object sender, EventArgs e)
      {
         if(!checkBoxTrolley.Checked)
         {
            foreach(var b in TrolleyMarkers)
            {
               Main.objects.Markers.Remove(b);
            }
         }
      }

      // update data
      private void timerRefresh_Tick(object sender, EventArgs e)
      {
         timerRefresh.Enabled = false;

         try
         {
            DateTime tstart = DateTime.Now;
            {
               if(checkBoxBus.Checked)
               {
                  foreach(var b in BusMarkers)
                  {
                     Main.objects.Markers.Remove(b);
                  }

                  Stuff.GetVilniusTransportData(TransportType.Bus, textBoxBus.Text, Bus);

                  foreach(var t in Bus)
                  {
                      if (textBoxBus.Text.Split(',').Contains(t.Line))
                      {
                          var r = new GMapMarkerTransparent(new PointLatLng(t.Lat, t.Lng));
                          {
                              r.ToolTipMode = MarkerTooltipMode.Always;
                              r.ToolTipText = "B " + t.Line + ", " + t.Time;
                              r.Data = t;
                          }
                          Main.objects.Markers.Add(r);
                          BusMarkers.Add(r);
                      }
                  }
               }

               if(checkBoxTrolley.Checked)
               {
                  foreach(var b in TrolleyMarkers)
                  {
                     Main.objects.Markers.Remove(b);
                  }

                  Stuff.GetVilniusTransportData(TransportType.TrolleyBus, textBoxTrolley.Text, Trolley);

                  foreach(var t in Trolley)
                  {
                      if (textBoxTrolley.Text.Split(',').Contains(t.Line))
                      {
                          var r = new GMapMarkerTransparent(new PointLatLng(t.Lat, t.Lng));
                          {
                              r.ToolTipMode = MarkerTooltipMode.Always;
                              r.ToolTipText = "T " + t.Line + ", " + t.Time;
                              r.Data = t;
                          }
                          Main.objects.Markers.Add(r);
                          TrolleyMarkers.Add(r);
                      }
                  }
               }
            }
            labelstatus.Text = ++Count + " -> " + DateTime.Now.ToLongTimeString() + ", request:  " + (DateTime.Now - tstart).TotalSeconds+ "s";
         }
         catch(Exception ex)
         {
            labelstatus.Text = ex.ToString();
         }
         timerRefresh.Enabled = checkBoxRefresh.Checked;
      }

      private void button1_Click(object sender, EventArgs e)
      {
         labelstatus.Text = "Connecting manualy once...";
         labelstatus.Invalidate();
         timerRefresh_Tick(null, null);
         Main.ZoomToFitMarkers();
         Main.menuItemGotoMap_Click(null, null);
      }
   }

   public class GMapMarkerTransparent : GMapMarkerTransparentGoogleGreen
   {
      public VehicleData Data;

      public GMapMarkerTransparent(PointLatLng p)
         : base(p)
      {

      }
   }
}
