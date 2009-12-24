using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using System.Runtime.InteropServices;
using System.IO;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.GPS;
using System.Diagnostics;
using System.Data.Common;
using System.Data.SQLite;
using GMap.NET.Internals;
using Microsoft.Win32;

namespace Demo.WindowsMobile
{
   public partial class MainForm : Form
   {
      PointLatLng start = new PointLatLng(54.6961334816182, 25.2985095977783);

      // marker
      GMapMarker currentMarker;
      GMapMarkerCross center;

      // layers
      GMapOverlay top;
      GMapOverlay objects;
      GMapOverlay routes;

      #region -- variables --
      string LogDb;
      SQLiteConnection cn;
      DbCommand cmd;
      DateTime LastFlush = DateTime.Now;
      TimeSpan FlushDelay = TimeSpan.FromSeconds(60);

      Gps gps = new Gps();
      GpsDeviceState device = null;

      int count = 0;
      int countReal = 0;
      double Total = 0;

      EventHandler updateDataHandler;
      TimeSpan delay = TimeSpan.FromSeconds(1);
      DateTime? TimeUTC;
      double? Lat = 0;
      double? Lng = 0;
      double Delta = 0;
      internal readonly List<Satellite> Satellites = new List<Satellite>();

      IntPtr gpsPowerHandle = IntPtr.Zero;

      GPS pageGps;
      #endregion

      public MainForm()
      {
         InitializeComponent();

         pageGps = new GPS(this);

         GMaps.Instance.Mode = AccessMode.CacheOnly;

         MainMap.MapType = MapType.ArcGIS_MapsLT_Map;
         MainMap.MaxZoom = 11;
         MainMap.MinZoom = 1;
         MainMap.Zoom = MainMap.MinZoom + 1;
         MainMap.CurrentPosition = start;

         MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
         MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
         MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);
         // add custom layers  
         {
            routes = new GMapOverlay(MainMap, "routes");
            MainMap.Overlays.Add(routes);

            objects = new GMapOverlay(MainMap, "objects");
            MainMap.Overlays.Add(objects);

            top = new GMapOverlay(MainMap, "top");
            MainMap.Overlays.Add(top);
         }

         // map center
         center = new GMapMarkerCross(MainMap.CurrentPosition);
         top.Markers.Add(center);
      }

      void MainMap_OnMapZoomChanged()
      {
         this.Text = "GMap.NET: " + (int) MainMap.Zoom;
      }

      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         center.Position = point;
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
         //trackBar1.Maximum = MainMap.MaxZoom;

         //if(routes.Routes.Count > 0)
         //{
         //   MainMap.ZoomAndCenterRoutes(null);
         //}
      }

      private void menuItem3_Click(object sender, EventArgs e)
      {
         this.Close();
      }

      // zoom in
      private void menuItem4_Click(object sender, EventArgs e)
      {
         MainMap.Zoom = (int) (MainMap.Zoom) + 1;
      }

      // zoom out
      private void menuItem5_Click(object sender, EventArgs e)
      {
         MainMap.Zoom = (int) (MainMap.Zoom) - 1;
      }

      private void menuItem15_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.ArcGIS_MapsLT_Map_Hybrid;
      }

      private void menuItem10_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.GoogleHybrid;
      }

      private void menuItem12_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.OpenStreetMap;
      }

      private void menuItem22_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.OpenStreetMapSurfer;
      }

      private void menuItem23_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.OpenStreetOsm;
      }

      private void menuItem9_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.GoogleMap;
      }

      private void menuItem16_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.BingMap;
      }

      private void menuItem18_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.BingHybrid;
      }

      private void menuItem20_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.YahooMap;
      }

      private void menuItem21_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.YahooHybrid;
      }

      private void menuItem14_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.ArcGIS_MapsLT_Map;
      }

      private void menuItem25_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.ArcGIS_Map;
      }

      private void menuItem26_Click(object sender, EventArgs e)
      {
         MainMap.MapType = MapType.ArcGIS_Satellite;
      }

      private void menuItem27_Click(object sender, EventArgs e)
      {
         MainMap.ReloadMap();
      }

      private void menuItemCacheOnly_Click(object sender, EventArgs e)
      {
         GMaps.Instance.Mode = AccessMode.CacheOnly;
         menuItemCacheOnly.Checked = true;
         menuItemServerAndCache.Checked = false;
         menuItemServerOnly.Checked = false;
      }

      private void menuItemServerAndCache_Click(object sender, EventArgs e)
      {
         GMaps.Instance.Mode = AccessMode.ServerAndCache;
         menuItemServerAndCache.Checked = true;
         menuItemCacheOnly.Checked = false;
         menuItemServerOnly.Checked = false;

      }

      private void menuItemServerOnly_Click(object sender, EventArgs e)
      {
         GMaps.Instance.Mode = AccessMode.ServerOnly;
         menuItemServerOnly.Checked = true;
         menuItemServerAndCache.Checked = false;
         menuItemCacheOnly.Checked = false;
      }

      private void menuItemGPSenabled_Click(object sender, EventArgs e)
      {
         menuItemGPSenabled.Checked = !menuItemGPSenabled.Checked;

         if(!menuItemGPSenabled.Checked)
         {
            if(gps.Opened)
            {
               gps.Close();
            }

            count = 0;
            countReal = 0;
            Total = 0;
            {
               TimeUTC = null;
               Lat = null;
               Lng = null;
               Delta = 0;
            }
            lock(Satellites)
            {
               Satellites.Clear();
               Satellites.TrimExcess();
            }

            if(Controls.Contains(pageGps))
            {
               pageGps.panelSignals.Invalidate();
            }

            TryCommitData();

            center.Pen.Color = Color.Red;
         }
         else // start tracking
         {
            center.Pen.Color = Color.Blue;

            if(!gps.Opened)
            {
               gps.Open();
            }
         }
      }

      public new void Hide()
      {
         Native.ShowWindow(this.Handle, Native.SW_MINIMIZED);
         timerKeeperOfLife.Enabled = false;
         IsVisible = false;
      }

      object visibleLock = new object();
      bool visible = true;
      public bool IsVisible
      {
         get
         {
            lock(visibleLock)
            {
               return visible;
            }
         }
         set
         {
            lock(visibleLock)
            {
               visible = value;
            }
         }
      }

      public bool AddToLogCurrentInfo(GpsPosition data)
      {
         if(string.IsNullOrEmpty(LogDb))
         {
            return false;
         }

         bool ret = true;
         try
         {
            {
               if(cmd.Transaction == null)
               {
                  cmd.Transaction = cn.BeginTransaction(IsolationLevel.Serializable);
                  Debug.WriteLine("BeginTransaction: " + DateTime.Now.ToLongTimeString());
               }

               cmd.Parameters["@p1"].Value = data.Time.Value;
               cmd.Parameters["@p2"].Value = countReal++;
               cmd.Parameters["@p3"].Value = Delta;
               cmd.Parameters["@p4"].Value = data.Speed;
               cmd.Parameters["@p5"].Value = data.SeaLevelAltitude;
               cmd.Parameters["@p6"].Value = data.EllipsoidAltitude;
               cmd.Parameters["@p7"].Value = (short?) data.SatellitesInViewCount;
               cmd.Parameters["@p8"].Value = (short?) data.SatelliteCount;
               cmd.Parameters["@p9"].Value = data.Latitude.Value;
               cmd.Parameters["@p10"].Value = data.Longitude.Value;
               cmd.Parameters["@p11"].Value = data.PositionDilutionOfPrecision;
               cmd.Parameters["@p12"].Value = data.HorizontalDilutionOfPrecision;
               cmd.Parameters["@p13"].Value = data.VerticalDilutionOfPrecision;
               cmd.Parameters["@p14"].Value = (byte) data.FixQuality;
               cmd.Parameters["@p15"].Value = (byte) data.FixType;
               cmd.Parameters["@p16"].Value = (byte) data.FixSelection;

               cmd.ExecuteNonQuery();
            }

            if(DateTime.Now - LastFlush >= FlushDelay)
            {
               TryCommitData();
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("AddToLog: " + ex.ToString());
            ret = false;
         }

         return ret;
      }

      void TryCommitData()
      {
         try
         {
            if(cmd.Transaction != null)
            {
               using(cmd.Transaction)
               {
                  cmd.Transaction.Commit();
                  LastFlush = DateTime.Now;
               }
               cmd.Transaction = null;
               Debug.WriteLine("TryCommitData: OK " + LastFlush.ToLongTimeString());
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("TryCommitData: " + ex.ToString());
         }
      }

      public bool CheckLogDb(string file)
      {
         bool ret = true;

         try
         {
            string dir = Path.GetDirectoryName(file);
            if(!Directory.Exists(dir))
            {
               Directory.CreateDirectory(dir);
            }

            cn = new SQLiteConnection();
            {
               cn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=False;", file);
               cn.Open();
               {
                  using(DbTransaction tr = cn.BeginTransaction())
                  {
                     try
                     {
                        using(DbCommand cmd = cn.CreateCommand())
                        {
                           cmd.Transaction = tr;
                           cmd.CommandText = @"CREATE TABLE IF NOT EXISTS GPS (id INTEGER NOT NULL PRIMARY KEY,
                                                TimeUTC DATETIME NOT NULL,
                                                SessionCounter INTEGER NOT NULL,
                                                Delta DOUBLE,
                                                Speed DOUBLE,
                                                SeaLevelAltitude DOUBLE,
                                                EllipsoidAltitude DOUBLE,
                                                SatellitesInView TINYINT,
                                                SatelliteCount TINYINT,
                                                Lat DOUBLE NOT NULL,
                                                Lng DOUBLE NOT NULL,
                                                PositionDilutionOfPrecision DOUBLE,
                                                HorizontalDilutionOfPrecision DOUBLE,
                                                VerticalDilutionOfPrecision DOUBLE,
                                                FixQuality TINYINT NOT NULL,
                                                FixType TINYINT NOT NULL,
                                                FixSelection TINYINT NOT NULL); 
                                               CREATE INDEX IF NOT EXISTS IndexOfGPS ON GPS (TimeUTC, PositionDilutionOfPrecision);";
                           cmd.ExecuteNonQuery();
                        }

                        this.cmd = cn.CreateCommand();
                        {
                           cmd.CommandText = @"INSERT INTO GPS
                                         (TimeUTC,
                                          SessionCounter,
                                          Delta,
                                          Speed,
                                          SeaLevelAltitude,
                                          EllipsoidAltitude,
                                          SatellitesInView,
                                          SatelliteCount,
                                          Lat,
                                          Lng,
                                          PositionDilutionOfPrecision,
                                          HorizontalDilutionOfPrecision,
                                          VerticalDilutionOfPrecision,
                                          FixQuality,
                                          FixType,
                                          FixSelection) VALUES(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16);";

                           cmd.Parameters.Add(new SQLiteParameter("@p1"));
                           cmd.Parameters.Add(new SQLiteParameter("@p2"));
                           cmd.Parameters.Add(new SQLiteParameter("@p3"));
                           cmd.Parameters.Add(new SQLiteParameter("@p4"));
                           cmd.Parameters.Add(new SQLiteParameter("@p5"));
                           cmd.Parameters.Add(new SQLiteParameter("@p6"));
                           cmd.Parameters.Add(new SQLiteParameter("@p7"));
                           cmd.Parameters.Add(new SQLiteParameter("@p8"));
                           cmd.Parameters.Add(new SQLiteParameter("@p9"));
                           cmd.Parameters.Add(new SQLiteParameter("@p10"));
                           cmd.Parameters.Add(new SQLiteParameter("@p11"));
                           cmd.Parameters.Add(new SQLiteParameter("@p12"));
                           cmd.Parameters.Add(new SQLiteParameter("@p13"));
                           cmd.Parameters.Add(new SQLiteParameter("@p14"));
                           cmd.Parameters.Add(new SQLiteParameter("@p15"));
                           cmd.Parameters.Add(new SQLiteParameter("@p16"));
                           cmd.Prepare();
                        }

                        tr.Commit();
                     }
                     catch
                     {
                        tr.Rollback();
                        ret = false;
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            if(cn != null)
            {
               cn.Dispose();
               cn = null;
            }
            if(cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
            Debug.WriteLine("CreateEmptyDB: " + ex.ToString());
            ret = false;
         }

         if(ret)
         {
            LogDb = file;
         }
         else
         {
            LogDb = null;
         }

         return ret;
      }

      private void MainForm_Load(object sender, EventArgs e)
      {
         updateDataHandler = new EventHandler(UpdateData);

         gps.DeviceStateChanged += new DeviceStateChangedEventHandler(gps_DeviceStateChanged);
         gps.LocationChanged += new LocationChangedEventHandler(gps_LocationChanged);

         // Keep the GPS and device alive
         bool power = Native.PowerPolicyNotify(Native.PPN_UNATTENDEDMODE, true);
         if(!power)
         {
            Debug.WriteLine("PowerPolicyNotify failed for PPN_UNATTENDEDMODE");
         }
         else
         {
            gpsPowerHandle = Native.SetPowerRequirement("gps0:", Native.CedevicePowerStateState.D0, Native.POWER_NAME | Native.POWER_FORCE, null, 0);
            if(gpsPowerHandle == IntPtr.Zero)
            {
               Debug.WriteLine("SetPowerRequirement failed for GPS");
            }
         }

         string sd = Native.GetRemovableStorageDirectory();
         if(!string.IsNullOrEmpty(sd))
         {
            var fileName = sd + Path.DirectorySeparatorChar +  "GMap.NET" + Path.DirectorySeparatorChar + "log.gpsd";
            {
               CheckLogDb(fileName);
            }
         }

         if(!gps.Opened)
         {
            gps.Open();
         }

         center.Pen.Color = Color.Blue;

         timerKeeperOfLife.Interval = ShortestTimeoutInterval() * 1000;
         timerKeeperOfLife.Enabled = true;
      }

      void gps_LocationChanged(object sender, LocationChangedEventArgs args)
      {
         try
         {
            var position = args.Position;
            if(position != null)
            {
               count++;

               //Debug.WriteLine("LocationChanged: " + DateTime.Now.ToLongTimeString() + " -> " + count);

               if(position.Time.HasValue && position.Latitude.HasValue && position.Longitude.HasValue)
               {
                  //Debug.WriteLine("Location: " + position.Latitude.Value + "|" + position.Longitude.Value);

                  // first time
                  if(!TimeUTC.HasValue)
                  {
                     TimeUTC = position.Time;
                     Lat = position.Latitude;
                     Lng = position.Longitude;
                  }

                  if(TimeUTC.HasValue && position.Time - TimeUTC.Value >= delay)
                  {
                     Delta = gps.GetDistance(position.Latitude.Value, position.Longitude.Value, Lat.Value, Lng.Value);
                     Total += Delta;
                     Lat = position.Latitude;
                     Lng = position.Longitude;
                     TimeUTC = position.Time;

                     AddToLogCurrentInfo(position);
                  }
               }
               else
               {
                  Lat = position.Latitude;
                  Lng = position.Longitude;
                  TimeUTC = position.Time;
               }

               // do not update if user is idling
               if(IsVisible)
               {
                  lock(Satellites)
                  {
                     Satellites.Clear();
                     Satellites.AddRange(position.GetSatellitesInView());
                     Satellites.TrimExcess();
                  }

                  Invoke(updateDataHandler, position);
               }
            }
         }
         catch
         {
         }
      }

      void gps_DeviceStateChanged(object sender, DeviceStateChangedEventArgs args)
      {
         device = args.DeviceState;
         Invoke(updateDataHandler);
      }

      private void MainForm_Closed(object sender, EventArgs e)
      {
         if(gps.Opened)
         {
            gps.Close();
         }

         if(cn != null)
         {
            TryCommitData();

            if(cn.State == ConnectionState.Open)
            {
               cn.Close();
            }
            cn.Dispose();
            cn = null;
         }
         if(cmd != null)
         {
            cmd.Dispose();
            cmd = null;
         }

         if(gpsPowerHandle == IntPtr.Zero)
         {
            Native.ReleasePowerRequirement(gpsPowerHandle);
            gpsPowerHandle = IntPtr.Zero;
         }
         Native.PowerPolicyNotify(Native.PPN_UNATTENDEDMODE, false);
      }

      void UpdateData(object sender, System.EventArgs args)
      {
         try
         {
            var data = sender as GpsPosition;

            // update signals
            if(Controls.Contains(pageGps))
            {
               pageGps.panelSignals.Invalidate();

               string str = "\n";

               if(data != null)
               {
                  if(data.Time.HasValue && data.Longitude.HasValue && data.Longitude.HasValue)
                  {
                     str += "Time: " + data.Time.Value.ToLongDateString() + " " + data.Time.Value.ToLongTimeString() + "\n";
                     str += "Delay: " + ((int) (DateTime.UtcNow - data.Time.Value).TotalSeconds) + "s, Count:  " + count + "|" + countReal + "\n";
                     str += "Delta: " + string.Format("{0:0.00}m, total: {1:0.00m}\n", Delta*1000.0, Total*1000.0);
                     str += "Latitude: " + data.Latitude.Value + "\n";
                     str += "Longitude: " + data.Longitude.Value + "\n\n";
                  }
                  else
                  {
                     str += "Time: -" + "\n";
                     str += "Delay: -" + "\n";
                     str += "Delta: - \n";
                     str += "Latitude: -" + "\n";
                     str += "Longitude: -" + "\n\n";
                  }

                  if(data.Speed.HasValue)
                  {
                     str += "Speed: " + string.Format("{0:0.00} km/h\n", data.Speed);
                  }
                  else
                  {
                     str += "Speed: -\n";
                  }

                  if(data.SeaLevelAltitude.HasValue)
                  {
                     str += "SeaLevelAltitude: " + string.Format("{0:0.00}m\n", data.SeaLevelAltitude);
                  }
                  else
                  {
                     str += "SeaLevelAltitude: -\n";
                  }

                  if(data.PositionDilutionOfPrecision.HasValue)
                  {
                     str += "PositionDilutionOfPrecision: " + string.Format("{0:0.00}\n", data.PositionDilutionOfPrecision);
                  }
                  else
                  {
                     str += "PositionDilutionOfPrecision: -\n";
                  }

                  if(data.HorizontalDilutionOfPrecision.HasValue)
                  {
                     str += "HorizontalDilutionOfPrecision: " + string.Format("{0:0.00}\n", data.HorizontalDilutionOfPrecision);
                  }
                  else
                  {
                     str += "HorizontalDilutionOfPrecision: -\n";
                  }

                  if(data.VerticalDilutionOfPrecision.HasValue)
                  {
                     str += "VerticalDilutionOfPrecision: " + string.Format("{0:0.00}\n", data.VerticalDilutionOfPrecision);
                  }
                  else
                  {
                     str += "VerticalDilutionOfPrecision: -\n";
                  }

                  if(data.SatellitesInViewCount.HasValue)
                  {
                     str += "SatellitesInView: " + data.SatellitesInViewCount + "\n";
                  }
                  else
                  {
                     str += "SatellitesInView: -" + "\n";
                  }

                  if(data.SatelliteCount.HasValue)
                  {
                     str += "SatelliteCount: " + data.SatelliteCount + "\n";
                  }
                  else
                  {
                     str += "SatelliteCount: -" + "\n";
                  }
               }
               pageGps.status.Text = str;
            }
            else if(Controls.Contains(MainMap))
            {
               if(data != null)
               {
                  if(data.Time.HasValue && data.Longitude.HasValue && data.Longitude.HasValue)
                  {
                     // center map
                     if(menuItemGPSenabled.Checked)
                     {
                        MainMap.CurrentPosition = new PointLatLng(data.Latitude.Value, data.Longitude.Value);
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            pageGps.status.Text = "\n" + ex.ToString();
         }
      }

      int ShortestTimeoutInterval()
      {
         int retVal = 1000;

         try
         {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Control\Power");
            object oBatteryTimeout = key.GetValue("BattPowerOff");
            object oACTimeOut = key.GetValue("ExtPowerOff");
            object oScreenPowerOff = key.GetValue("ScreenPowerOff");
            key.Close();

            if(oBatteryTimeout is int)
            {
               int v = (int) oBatteryTimeout;
               if(v>0)
                  retVal = Math.Min(retVal, v);
            }
            if(oACTimeOut is int)
            {
               int v = (int) oACTimeOut;
               if(v>0)
                  retVal = Math.Min(retVal, v);
            }
            if(oScreenPowerOff is int)
            {
               int v = (int) oScreenPowerOff;
               if(v>0)
                  retVal = Math.Min(retVal, v);
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("ShortestTimeoutInterval: " + ex.ToString());
         }

         return retVal*9/10;
      }

      private void menuItemEnableGrid_Click(object sender, EventArgs e)
      {
         menuItemEnableGrid.Checked = !menuItemEnableGrid.Checked;
         MainMap.ShowTileGridLines = menuItemEnableGrid.Checked;
      }

      private void timerKeeperOfLife_Tick(object sender, EventArgs e)
      {
         Native.SystemIdleTimerReset();
      }

      private void menuItemDisableAutoSleep_Click(object sender, EventArgs e)
      {
         menuItemDisableAutoSleep.Checked = !menuItemDisableAutoSleep.Checked;
         timerKeeperOfLife.Enabled = menuItemDisableAutoSleep.Checked;
      }

      private void MainForm_Activated(object sender, EventArgs e)
      {
         timerKeeperOfLife.Enabled = menuItemDisableAutoSleep.Checked;
         IsVisible = true;
      }

      private void menuItem32_Click(object sender, EventArgs e)
      {
         this.Hide();
      }

      private void menuItemGotoMap_Click(object sender, EventArgs e)
      {
         menuItemGotoMap.Checked = true;
         menuItemGotoGps.Checked = false;

         this.SuspendLayout();
         this.Controls.Clear();
         this.Controls.Add(this.MainMap);
         this.ResumeLayout(false);
      }

      private void menuItemGotoGps_Click(object sender, EventArgs e)
      {
         menuItemGotoMap.Checked = false;
         menuItemGotoGps.Checked = true;

         this.SuspendLayout();
         this.Controls.Clear();
         this.pageGps.Dock = DockStyle.Fill;
         this.Controls.Add(pageGps);
         this.ResumeLayout(false);

         pageGps.panelSignals.Invalidate();
      }

      private void menuItem31_Click(object sender, EventArgs e)
      {
         MainMap.Zoom = MainMap.MinZoom;
      }

      private void menuItem33_Click(object sender, EventArgs e)
      {
         MainMap.Zoom = MainMap.MaxZoom;
      }
   }
}