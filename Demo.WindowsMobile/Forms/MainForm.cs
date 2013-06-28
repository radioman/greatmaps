using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.GPS;
using GMap.NET.Internals;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Microsoft.Win32;
using Microsoft.WindowsCE.Forms;
using GMap.NET.MapProviders;
using System.Net;
using System.Threading;

namespace Demo.WindowsMobile
{
   public partial class MainForm : Form
   {
      PointLatLng start = new PointLatLng(54.6961334816182, 25.2985095977783);
      PointLatLng destinationPoint = new PointLatLng(54.6961334816182, 25.2985095977783);

      // marker
      GMarkerCross gpsPos;

      // layers
      GMapOverlay top;
      internal GMapOverlay objects;

      #region -- variables --
      string LogDb;
      SQLiteConnection cn;
      DbCommand cmd;
      DateTime LastFlush = DateTime.Now;
      TimeSpan FlushDelay = TimeSpan.FromSeconds(60);

      readonly Gps gps = new Gps();
      GpsDeviceState device = null;

      int count = 0;
      int countReal = 0;
      double totalDistance = 0;

      EventHandler updateDataHandler;
      TimeSpan delay = TimeSpan.FromSeconds(1);
      DateTime? TimeUTC = null;
      double? Lat = null;
      double? Lng = null;
      double lastDelta = 0;
      internal readonly List<Satellite> Satellites = new List<Satellite>();

      IntPtr gpsPowerHandle = IntPtr.Zero;

      GPS pageGps;
      Transport pageTransport;
      Search pageSearch;

      readonly HookKeys hook = new HookKeys();

      readonly AutoResetEvent gpsPositionWait = new AutoResetEvent(false);
      #endregion

      public MainForm()
      {
          InitializeComponent();

          pageGps = new GPS(this);
          pageTransport = new Transport(this);
          pageSearch = new Search(this);
          menuItemGPSenabled.Checked = false;
          gpsPos = new GMarkerCross(MainMap.Position);

#if DEBUG
          Debug.AutoFlush = true;
          Debug.WriteLine("go!");

          MainMap.Manager.Mode = AccessMode.ServerAndCache;
          menuItemServerAndCache.Checked = true;
          menuItemEnableGrid.Checked = true;
          MainMap.ShowTileGridLines = true;          
#else
         MainMap.Manager.Mode = AccessMode.CacheOnly;
         menuItemCacheOnly.Checked = true;
#endif
          MainMap.MapProvider = GMapProviders.LithuaniaMap;
          MainMap.MaxZoom = 11;
          MainMap.MinZoom = 1;
          MainMap.Zoom = MainMap.MinZoom + 1;
          MainMap.Position = start;

          MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
          MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);
          MainMap.OnPositionChanged += new PositionChanged(MainMap_OnPositionChanged);

          // add custom layers  
          {
              objects = new GMapOverlay("objects");
              MainMap.Overlays.Add(objects);

              top = new GMapOverlay("top");
              MainMap.Overlays.Add(top);
          }

          // gps pos
          gpsPos = new GMarkerCross(MainMap.Position);
          gpsPos.IsVisible = false;
          top.Markers.Add(gpsPos);

#if DEBUG
          // transparent marker test
          GMapMarkerTransparentGoogleGreen goo = new GMapMarkerTransparentGoogleGreen(MainMap.Position);
          goo.ToolTipMode = MarkerTooltipMode.Always;
          goo.ToolTipText = "Welcome to Lithuania! ;}";
          objects.Markers.Add(goo);
#endif

          // hook for volume up/down zooming
          hook.HookEvent += new HookKeys.HookEventHandler(hook_HookEvent);

          // test performance
          if (PerfTestEnabled)
          {
              timer.Interval = 111;
              timer.Tick += new EventHandler(timer_Tick);
              timer.Enabled = true;
          }         
      }

      void MainMap_OnPositionChanged(PointLatLng point)
      {
         if(menuItemshowDestination.Checked && !menuItemGPSenabled.Checked)
         {
            UpdateDestinationRoute(true, menuItemSnapToGps.Checked ? MainMap.Position : gpsPos.Position); 
         }
      }

      readonly IntPtr volumeUp = new IntPtr(257);

      bool hook_HookEvent(HookEventArgs e, KeyBoardInfo keyBoardInfo)
      {
         if(keyBoardInfo.vkCode == 117)
         {
            if(e.wParam == volumeUp)
            {
               MainMap.Zoom = (int) (MainMap.Zoom) + 1;
            }
         }
         else if(keyBoardInfo.vkCode == 118)
         {
            if(e.wParam == volumeUp)
            {
               MainMap.Zoom = (int) (MainMap.Zoom) - 1;
            }
         }
         return true;
      }

      #region -- performance test--

      bool PerfTestEnabled = false;

      double NextDouble(Random rng, double min, double max)
      {
         return min + (rng.NextDouble() * (max - min));
      }

      Random r = new Random();

      int tt = 0;
      void timer_Tick(object sender, EventArgs e)
      {
         var pos = new PointLatLng(NextDouble(r, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom), NextDouble(r, MainMap.ViewArea.Left, MainMap.ViewArea.Right));
         GMapMarker m = new GMapMarkerGoogleGreen(pos);
         {
            m.ToolTipText = (tt++).ToString();
            m.ToolTipMode = MarkerTooltipMode.Always;
            m.Offset = new System.Drawing.Point(-m.Size.Width, -m.Size.Height);
         }

         objects.Markers.Add(m);

         if(tt >= 44)
         {
            timer.Enabled = false;
            tt = 0;
         }
      }

      System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
      #endregion

      public void ZoomToFitMarkers()
      {
         if(objects.Markers.Count > 0)
         {
            RectLatLng? m = MainMap.GetRectOfAllMarkers(null);
            if(m.HasValue)
            {
               MainMap.SetZoomToFitRect(m.Value);
            }
         }
      }

      void MainMap_OnMapZoomChanged()
      {
         this.Text = "GMap.NET: " + (int) MainMap.Zoom;
      }

      void MainMap_OnMapTypeChanged(GMapProvider type)
      {
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

      private void menuItem14_Click(object sender, EventArgs e)
      {
         MainMap.MapProvider = GMapProviders.LithuaniaMap;
         MainMap.MaxZoom = MainMap.MapProvider.MaxZoom.Value;
      } 

      private void menuItem15_Click(object sender, EventArgs e)
      {
         MainMap.MapProvider = GMapProviders.LithuaniaOrtoFotoMap;
         MainMap.MaxZoom = MainMap.MapProvider.MaxZoom.Value;
      }

      private void menuItem12_Click(object sender, EventArgs e)
      {
         MainMap.MapProvider = GMapProviders.OpenStreetMap;
         MainMap.MaxZoom = 17;
      }

      private void menuItem4_Click_1(object sender, EventArgs e)
      {
         MainMap.MapProvider = GMapProviders.OpenCycleMap;
         MainMap.MaxZoom = 17;
      }

      private void menuItem9_Click(object sender, EventArgs e)
      {
         MainMap.MapProvider = GMapProviders.GoogleMap;
         MainMap.MaxZoom = 17;
      }

      private void menuItem10_Click(object sender, EventArgs e)
      {
         MainMap.MapProvider = GMapProviders.GoogleSatelliteMap;
         MainMap.MaxZoom = 17;
      }                     

      private void menuItem27_Click(object sender, EventArgs e)
      {
         MainMap.ReloadMap();
      }

      private void menuItemCacheOnly_Click(object sender, EventArgs e)
      {
         MainMap.Manager.Mode = AccessMode.CacheOnly;
         menuItemCacheOnly.Checked = true;
         menuItemServerAndCache.Checked = false;
         menuItemServerOnly.Checked = false;
      }

      private void menuItemServerAndCache_Click(object sender, EventArgs e)
      {
         MainMap.Manager.Mode = AccessMode.ServerAndCache;
         menuItemServerAndCache.Checked = true;
         menuItemCacheOnly.Checked = false;
         menuItemServerOnly.Checked = false;
      }

      private void menuItemServerOnly_Click(object sender, EventArgs e)
      {
         MainMap.Manager.Mode = AccessMode.ServerOnly;
         menuItemServerOnly.Checked = true;
         menuItemServerAndCache.Checked = false;
         menuItemCacheOnly.Checked = false;
      }

      void ResetGpsCounter()
      {
          count = 0;
          countReal = 0;
          totalDistance = 0;
          {
              TimeUTC = null;
              Lat = null;
              Lng = null;
              lastDelta = 0;
          }

          lock (Satellites)
          {
              Satellites.Clear();
              Satellites.TrimExcess();
          }
          gpsPositionWait.Reset();
      }

      private void menuItemGPSenabled_Click(object sender, EventArgs e)
      {
         menuItemGPSenabled.Checked = !menuItemGPSenabled.Checked;

         if(!menuItemGPSenabled.Checked)
         {
            if(gps.Opened)
            {
               gps.Close();
               SetOffGPSPower();
            }            

            if(Controls.Contains(pageGps))
            {
               pageGps.panelSignals.Invalidate();
            }

            gpsPos.Pen.Color = Color.Blue;
            menuItemGpsFix.Enabled = true;

            TryCommitData();            
         }
         else // start tracking
         {
            menuItemGpsFix.Enabled = false;

            gpsPos.Pen.Color = Color.Red;
            gpsPos.IsVisible = true;

            ResetGpsCounter();

            if(!gps.Opened)
            {               
               gps.Open();
               SetOnGPSPower();
            }
         }
      }

      public new void Hide()
      {
         Native.ShowWindow(this.Handle, Native.SW_MINIMIZED);
         timerKeeperOfLife.Enabled = false;
         IsVisible = false;

         hook.Stop();
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
               cmd.Parameters["@p3"].Value = lastDelta;
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

         string sd = Native.GetRemovableStorageDirectory();
         if(!string.IsNullOrEmpty(sd))
         {
            var fileName = sd + Path.DirectorySeparatorChar + "GMap.NET" + Path.DirectorySeparatorChar + "log.gpsd";
            {
               CheckLogDb(fileName);
            }
         }

         timerKeeperOfLife.Interval = ShortestTimeoutInterval() * 1000;
         timerKeeperOfLife.Enabled = true;

         if(menuItemGPSenabled.Checked)
         {
            if(!gps.Opened)
            {
               gps.Open();
               gpsPos.Pen.Color = Color.Red;
               gpsPos.IsVisible = true;

               SetOnGPSPower();
            }
         }
         else
         {
            gpsPos.Pen.Color = Color.Blue;
            gpsPos.IsVisible = false;
         }

         try
         {
            // Config AutoRotate
            {
               using(RegistryKey Config = Registry.CurrentUser.OpenSubKey(@"Software\HTC\HTCSENSOR\GSensor\ModuleName", true))
               {
                  if(Config != null)
                  {
                     string gmapnet = Config.GetValue("GMapNET", null) as string;
                     if(string.IsNullOrEmpty(gmapnet))
                     {
                        Config.SetValue("GMapNET", @"\Program files\gmap.net\GMap.NET.exe");
                     }
                  }
               }

               using(RegistryKey Config = Registry.CurrentUser.OpenSubKey(@"Software\HTC\HTCSENSOR\GSensor\WhiteList", true))
               {
                  if(Config != null)
                  {
                     string gmapnet = Config.GetValue("GMapNET", null) as string;
                     if(string.IsNullOrEmpty(gmapnet))
                     {
                        Config.SetValue("GMapNET", "#NETCF_AGL_BASE_");
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine(ex.ToString());
         }
      }

      void gps_LocationChanged(object sender, GpsPosition position)
      {
         try
         {
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
                     lastDelta = gps.GetDistance(position.Latitude.Value, position.Longitude.Value, Lat.Value, Lng.Value);
                     totalDistance += lastDelta;
                     Lat = position.Latitude;
                     Lng = position.Longitude;
                     TimeUTC = position.Time;

                     AddToLogCurrentInfo(position);

                     if (countReal == 4)
                     {
                         gpsPositionWait.Set();
                     }
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
         catch(Exception ex)
         {
            Debug.WriteLine("gps_LocationChanged: " + ex);
         }
      }

      void gps_DeviceStateChanged(object sender, GpsDeviceState args)
      {
         device = args;

         if(IsVisible)
         {
            Invoke(updateDataHandler);
         }
      }

      private void MainForm_Closed(object sender, EventArgs e)
      {
         hook.Stop();

         if(gps.Opened)
         {
            gps.Close();
            SetOffGPSPower();
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
      }

      void SetOnGPSPower()
      {
         // Keep the GPS and device alive
         bool power = Native.PowerPolicyNotify(Native.PPN_UNATTENDEDMODE, true);
         if(!power)
         {
            Debug.WriteLine("PowerPolicyNotify failed for PPN_UNATTENDEDMODE");
         }
         else
         {
            if(gpsPowerHandle == IntPtr.Zero)
            {
               gpsPowerHandle = Native.SetPowerRequirement("gps0:", Native.CedevicePowerStateState.D0, Native.POWER_NAME | Native.POWER_FORCE, null, 0);
               if(gpsPowerHandle == IntPtr.Zero)
               {
                  Debug.WriteLine("SetPowerRequirement failed for GPS");
               }
            }
         }
      }

      void SetOffGPSPower()
      {
         if(gpsPowerHandle != IntPtr.Zero)
         {
            Native.ReleasePowerRequirement(gpsPowerHandle);
            gpsPowerHandle = IntPtr.Zero;
         }
         Native.PowerPolicyNotify(Native.PPN_UNATTENDEDMODE, false);
      }

      ~MainForm()
      {
         MainForm_Closed(null, null);
      }

      void UpdateData(object sender, System.EventArgs args)
      {
         try
         {
            var lastData = sender as GpsPosition;

            // update signals
            if(Controls.Contains(pageGps))
            {
               pageGps.panelSignals.Invalidate();

               string str = Environment.NewLine;

               str += "GPS: " + device.DeviceState + ", Driver: " + device.ServiceState + ", " + count + " | " + countReal + "\n";

               if(lastData != null)
               {
                  if(lastData.Time.HasValue && lastData.Longitude.HasValue && lastData.Longitude.HasValue)
                  {
                     int deltaClock = ((int) (DateTime.UtcNow - lastData.Time.Value).TotalSeconds);

                     str += "Time(UTC): " + lastData.Time.Value.ToLongTimeString() + ", delay: " + deltaClock + "s \n";
                     str += "Delta: " + string.Format("{0:0.00}m, total: {1:0.00km}\n", lastDelta * 1000.0, totalDistance);
                     str += "Latitude: " + lastData.Latitude.Value + "\n";
                     str += "Longitude: " + lastData.Longitude.Value + "\n\n";

                     if(Math.Abs(deltaClock) > 5) // 5s
                     {
                        Utils.UpdateTime(lastData.Time.Value);
                     }
                  }
                  else
                  {
                     str += "Time(UTC): -" + "\n";
                     str += "Delta: - \n";
                     str += "Latitude: -" + "\n";
                     str += "Longitude: -" + "\n\n";
                  }

                  if(lastData.Speed.HasValue)
                  {
                     str += "Speed: " + string.Format("{0:0.0}km/h | {1:0.0}m/s, head: {2}\n", lastData.Speed, lastData.Speed / 3.6, (int) (lastData.Heading.HasValue ? lastData.Heading.Value : 0));
                  }
                  else
                  {
                     str += "Speed: -\n";
                  }

                  if(lastData.SeaLevelAltitude.HasValue)
                  {
                     str += "SeaLevelAltitude: " + string.Format("{0:0.00}m\n", lastData.SeaLevelAltitude);
                  }
                  else
                  {
                     str += "SeaLevelAltitude: -\n";
                  }

                  if(lastData.PositionDilutionOfPrecision.HasValue)
                  {
                     str += "PositionDilutionOfPrecision: " + string.Format("{0:0.00}\n", lastData.PositionDilutionOfPrecision);
                  }
                  else
                  {
                     str += "PositionDilutionOfPrecision: -\n";
                  }

                  if(lastData.HorizontalDilutionOfPrecision.HasValue)
                  {
                     str += "HorizontalDilutionOfPrecision: " + string.Format("{0:0.00}\n", lastData.HorizontalDilutionOfPrecision);
                  }
                  else
                  {
                     str += "HorizontalDilutionOfPrecision: -\n";
                  }

                  if(lastData.VerticalDilutionOfPrecision.HasValue)
                  {
                     str += "VerticalDilutionOfPrecision: " + string.Format("{0:0.00}\n", lastData.VerticalDilutionOfPrecision);
                  }
                  else
                  {
                     str += "VerticalDilutionOfPrecision: -\n";
                  }

                  if(lastData.SatellitesInViewCount.HasValue)
                  {
                     str += "SatellitesInView: " + lastData.SatellitesInViewCount + "\n";
                  }
                  else
                  {
                     str += "SatellitesInView: -" + "\n";
                  }

                  if(lastData.SatelliteCount.HasValue)
                  {
                     str += "SatelliteCount: " + lastData.SatelliteCount + "\n";
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
               if(lastData != null)
               {
                  if(lastData.Time.HasValue && lastData.Longitude.HasValue && lastData.Longitude.HasValue)
                  {
                     // center map
                     if(menuItemGPSenabled.Checked)
                     {
                        if(menuItemSnapToGps.Checked && !MainMap.IsDragging)
                        {
                           var newPos = new PointLatLng(lastData.Latitude.Value, lastData.Longitude.Value);
                           
                           if(menuItemshowDestination.Checked)
                           {
                              UpdateDestinationRoute(false, newPos);
                           }

                           MainMap.Position = newPos;
                        }
                        else
                        {
                           gpsPos.Position = new PointLatLng(lastData.Latitude.Value, lastData.Longitude.Value);

                           if(menuItemshowDestination.Checked)
                           {
                              UpdateDestinationRoute(true, gpsPos.Position);
                           }
                        }
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
               if(v > 0)
                  retVal = Math.Min(retVal, v);
            }
            if(oACTimeOut is int)
            {
               int v = (int) oACTimeOut;
               if(v > 0)
                  retVal = Math.Min(retVal, v);
            }
            if(oScreenPowerOff is int)
            {
               int v = (int) oScreenPowerOff;
               if(v > 0)
                  retVal = Math.Min(retVal, v);
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("ShortestTimeoutInterval: " + ex.ToString());
         }

         return retVal * 9 / 10;
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
         hook.Start();
      }

      private void menuItem32_Click(object sender, EventArgs e)
      {
         this.Hide();
      }

      internal void menuItemGotoMap_Click(object sender, EventArgs e)
      {
         menuItemGotoMap.Checked = true;
         menuItemGotoGps.Checked = false;
         menuItemGotoTransport.Checked = false;
         menuItemSearch.Checked = false;

         this.SuspendLayout();
         this.Controls.Clear();
         this.Controls.Add(this.MainMap);
         this.ResumeLayout(false);
      }

      private void menuItemGotoGps_Click(object sender, EventArgs e)
      {
         menuItemGotoGps.Checked = true;
         menuItemGotoTransport.Checked = false;
         menuItemGotoMap.Checked = false;
         menuItemSearch.Checked = false;

         this.SuspendLayout();
         this.Controls.Clear();
         this.pageGps.Dock = DockStyle.Fill;
         this.Controls.Add(pageGps);
         this.ResumeLayout(false);

         pageGps.panelSignals.Invalidate();
      }

      internal void menuItemGotoTransport_Click(object sender, EventArgs e)
      {
         menuItemGotoTransport.Checked = true;
         menuItemGotoMap.Checked = false;
         menuItemGotoGps.Checked = false;
         menuItemSearch.Checked = false;

         this.SuspendLayout();
         this.Controls.Clear();
         this.pageTransport.Dock = DockStyle.Fill;
         this.Controls.Add(pageTransport);
         this.ResumeLayout(false);
      }

      private void menuItemSearch_Click(object sender, EventArgs e)
      {
         menuItemSearch.Checked = true;
         menuItemGotoTransport.Checked = false;
         menuItemGotoMap.Checked = false;
         menuItemGotoGps.Checked = false;

         this.SuspendLayout();
         this.Controls.Clear();
         this.pageSearch.Dock = DockStyle.Fill;
         this.Controls.Add(pageSearch);
         this.ResumeLayout(false);
      }

      private void menuItem35_Click(object sender, EventArgs e)
      {
         ZoomToFitMarkers();
      }

      private void menuItem31_Click(object sender, EventArgs e)
      {
         MainMap.Zoom = MainMap.MinZoom;
      }

      private void menuItem33_Click(object sender, EventArgs e)
      {
         MainMap.Zoom = MainMap.MaxZoom;
      }

      // clear markers
      private void menuItem37_Click(object sender, EventArgs e)
      {
         objects.Markers.Clear();
      }      
     
      private void menuItemSnapToGps_Click(object sender, EventArgs e)
      {
         menuItemSnapToGps.Checked = !menuItemSnapToGps.Checked;
         if(!menuItemSnapToGps.Checked)
         {
            gpsPos.Position = MainMap.Position;
         }
      }

      private void menuItemSetDestination_Click(object sender, EventArgs e)
      {
         {
            destinationPoint = MainMap.Position;

            if(menuItemshowDestination.Checked)
            {
               UpdateDestinationRoute(true, menuItemSnapToGps.Checked ? MainMap.Position : gpsPos.Position);
            }
         }           
      }

      internal GMapRoute destinationRoute;

      private void menuItemshowDestination_Click(object sender, EventArgs e)
      {
         menuItemshowDestination.Checked = !menuItemshowDestination.Checked;

         if(menuItemshowDestination.Checked)
         {
            if(MainMap.panel == null)
            {
               MainMap.panel = this;
            }
            UpdateDestinationRoute(true, menuItemSnapToGps.Checked ? MainMap.Position : gpsPos.Position);
         }

         if(destinationRoute != null)
         {
            destinationRoute.IsVisible = menuItemshowDestination.Checked;
         }
      }

      internal string destinationRouteInfo = string.Empty;

      void UpdateDestinationRoute(bool forceUpdate, PointLatLng startPoint)
      {
         if(destinationRoute == null)
         {
            destinationRoute = new GMapRoute("destination route");

            destinationRoute.Stroke.Color = Color.Red;
            destinationRoute.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            destinationRoute.Stroke.Width = 1.0f;
 
            objects.Routes.Add(destinationRoute);
         }
         else
         {
            destinationRoute.Points.Clear();
         }

         destinationRoute.Points.Add(startPoint);
         destinationRoute.Points.Add(destinationPoint);

         var bearing = (int)GMapProviders.EmptyProvider.Projection.GetBearing(startPoint, destinationPoint);
         destinationRouteInfo = string.Format("destination: {0:0.00} km, bearing: {1:0}°", destinationRoute.Distance, bearing);

         if(forceUpdate)
         {
            MainMap.UpdateRouteLocalPosition(destinationRoute);
            MainMap.Invalidate();
         }
      }

      void GetGpsFix(object val) 
      {
          bool w = false;
          try
          {
              if (!gps.Opened)
              {
                  ResetGpsCounter();
                  gps.Open();

                  if (gps.Opened)
                  {
                      SetOnGPSPower();

                      // wait
                      w = gpsPositionWait.WaitOne(1000 * 33, false);

                      gps.Close();
                      SetOffGPSPower(); 
                      
                      if (w)
                      {
                          Debug.WriteLine("GetGpsFix: OK");
                      }
                      else
                      {
                          Debug.WriteLine("GetGpsFix: timeout");
                      }
                  }
              }
          }
          catch (Exception ex)
          {
              Debug.WriteLine("GetGpsFix: " + ex);
          }

          try
          {
              Invoke(new EventHandler(GetGpsFixEnd), w);
          }
          catch (Exception ex2)
          {
              Debug.WriteLine("GetGpsFix,2: " + ex2);
          }
      }

      void GetGpsFixEnd(object sender, EventArgs args)
      {
          bool result = (bool)sender;

          gpsPos.Pen.Color = Color.Blue;
          menuItemGpsFix.Enabled = true;
          menuItemGPSenabled.Enabled = true;

          if (!result && IsVisible)
          {
              MessageBox.Show("timeout...", "GPS fix");
          }
      }

      private void menuItemGpsFix_Click(object sender, EventArgs e)
      {
          menuItemGpsFix.Enabled = false;
          menuItemGPSenabled.Enabled = false;
          gpsPos.Pen.Color = Color.Red;
          gpsPos.IsVisible = true;
          ThreadPool.QueueUserWorkItem(new WaitCallback(GetGpsFix));
      }

      void WaitForEvent(object val)
      {
          for (int i = 0; i < 1; i++)
          {
              try
              {
                  string eventName = "GMapWakeUp0";
                  string eventStr = "\\\\.\\Notifications\\NamedEvents\\" + eventName;

                  Win32.SYSTEMTIME time;
                  Win32.GetLocalTime(out time);

                  Win32.CE_NOTIFICATION_TRIGGER t = new Win32.CE_NOTIFICATION_TRIGGER();
                  t.Type = (uint)Win32.CNT_TYPE.CNT_TIME;
                  t.pAppName = eventStr;
                  t.pArgs = null;
                  t.StartTime = Win32.SYSTEMTIME.FromDateTime(time.ToDateTime().AddSeconds(60));
                  t.Size = (uint)Marshal.SizeOf(t);

                  Win32.CE_USER_NOTIFICATION n = new Win32.CE_USER_NOTIFICATION();
                  n.DialogText = "test: " + t.StartTime.ToDateTime();
                  n.pDialogTitle = "event";
                  n.ActionFlags = 4;

                  Debug.WriteLine("event expected: " + t.StartTime.ToDateTime());

                  {
                      var p = Win32.CreateEvent(IntPtr.Zero, false, false, eventName);
                      if(p != IntPtr.Zero)
                      {                         
                          var un = Win32.CeSetUserNotificationEx(IntPtr.Zero, t, null);
                          if (un != IntPtr.Zero)
                          {
                              #region -- wait --

                              int r = Win32.WaitForSingleObject(p, 1000 * 80);
                              if (r == Win32.WAIT_TIMEOUT)
                              {
                                  Debug.WriteLine("event timeout: " + r + ", " + DateTime.Now);
                              }
                              else if (r == Win32.WAIT_OBJECT_0)
                              {
                                  Debug.WriteLine("event OK: " + r + ", " + DateTime.Now);

                                  Thread.Sleep(4444);

                                  //if (!gps.Opened)
                                  //{                                  
                                  //    ResetGpsCounter();
                                  //    gps.Open();

                                  //    if (gps.Opened)
                                  //    {
                                  //        SetOnGPSPower();

                                  //        // wait
                                  //        if (gpsPositionWait.WaitOne(1000 * 33, false))
                                  //        {
                                  //            Debug.WriteLine("gpsPositionWait: OK");
                                  //        }
                                  //        else
                                  //        {
                                  //            Debug.WriteLine("gpsPositionWait: timeout");
                                  //        }

                                  //        gps.Close();
                                  //        SetOffGPSPower();
                                  //    }
                                  //}                              
                              }
                              else
                              {
                                  Debug.WriteLine("event ?: " + r + ", " + DateTime.Now);
                              }                              

                              #endregion

                              Win32.CeClearUserNotification((int)un);
                              un = IntPtr.Zero;
                          }

                          Win32.CloseHandle(p);
                          p = IntPtr.Zero;
                      }
                  }
              }
              catch (Exception ex)
              {
                  Debug.WriteLine("WaitForEvent: " + ex);
              }
          }
          Debug.WriteLine("WaitForEvent: end");
      } 

      private void menuItemLog10min_Click(object sender, EventArgs e)
      {
          ThreadPool.QueueUserWorkItem(new WaitCallback(WaitForEvent));
      }

      private void menuItemLog30min_Click(object sender, EventArgs e)
      {

      }

      private void menuItemLog1h_Click(object sender, EventArgs e)
      {

      }      
   }

   public class Map : GMapControl
   {
      readonly Brush screenBrush = new SolidBrush(Color.Navy);
      readonly Font screenFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);

      public MainForm panel;     

      protected override void OnPaintOverlays(Graphics g)
      {
         base.OnPaintOverlays(g);

         if(panel != null && panel.menuItemshowDestination.Checked)
         {
             g.DrawString(panel.destinationRouteInfo, screenFont, screenBrush, screenFont.Size, screenFont.Size);
         }
      }
   }
}