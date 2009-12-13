
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.WindowsMobile.Samples.Location;

namespace GpsTest
{
   /// <summary>
   /// Summary description for Form1.
   /// </summary>
   public class Form1 : System.Windows.Forms.Form
   {
      private MenuItem exitMenuItem;
      private MainMenu mainMenu1;
      private MenuItem menuItem2;
      private MenuItem startGpsMenuItem;
      private MenuItem stopGpsMenuItem;
      private MenuItem menuItem1;
      private TabPage tabPage1;
      private Label status;
      private TabControl tabControlLog;
      private Panel panelSignals;
      private Splitter splitter1;

      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.mainMenu1 = new System.Windows.Forms.MainMenu();
         this.exitMenuItem = new System.Windows.Forms.MenuItem();
         this.menuItem2 = new System.Windows.Forms.MenuItem();
         this.startGpsMenuItem = new System.Windows.Forms.MenuItem();
         this.stopGpsMenuItem = new System.Windows.Forms.MenuItem();
         this.menuItem1 = new System.Windows.Forms.MenuItem();
         this.tabPage1 = new System.Windows.Forms.TabPage();
         this.status = new System.Windows.Forms.Label();
         this.tabControlLog = new System.Windows.Forms.TabControl();
         this.panelSignals = new System.Windows.Forms.Panel();
         this.splitter1 = new System.Windows.Forms.Splitter();
         this.tabPage1.SuspendLayout();
         this.tabControlLog.SuspendLayout();
         this.SuspendLayout();
         // 
         // mainMenu1
         // 
         this.mainMenu1.MenuItems.Add(this.exitMenuItem);
         this.mainMenu1.MenuItems.Add(this.menuItem2);
         // 
         // exitMenuItem
         // 
         this.exitMenuItem.Text = "Hide";
         this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
         // 
         // menuItem2
         // 
         this.menuItem2.MenuItems.Add(this.startGpsMenuItem);
         this.menuItem2.MenuItems.Add(this.stopGpsMenuItem);
         this.menuItem2.MenuItems.Add(this.menuItem1);
         this.menuItem2.Text = "Menu";
         // 
         // startGpsMenuItem
         // 
         this.startGpsMenuItem.Text = "Start GPS";
         this.startGpsMenuItem.Click += new System.EventHandler(this.startGpsMenuItem_Click);
         // 
         // stopGpsMenuItem
         // 
         this.stopGpsMenuItem.Enabled = false;
         this.stopGpsMenuItem.Text = "Stop GPS";
         this.stopGpsMenuItem.Click += new System.EventHandler(this.stopGpsMenuItem_Click);
         // 
         // menuItem1
         // 
         this.menuItem1.Text = "Exit";
         this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
         // 
         // tabPage1
         // 
         this.tabPage1.Controls.Add(this.status);
         this.tabPage1.Location = new System.Drawing.Point(0, 0);
         this.tabPage1.Name = "tabPage1";
         this.tabPage1.Size = new System.Drawing.Size(480, 637);
         this.tabPage1.Text = "gps";
         // 
         // status
         // 
         this.status.BackColor = System.Drawing.Color.Navy;
         this.status.Dock = System.Windows.Forms.DockStyle.Fill;
         this.status.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
         this.status.ForeColor = System.Drawing.Color.Lime;
         this.status.Location = new System.Drawing.Point(0, 0);
         this.status.Name = "status";
         this.status.Size = new System.Drawing.Size(480, 637);
         this.status.Text = "Loading...";
         // 
         // tabControlLog
         // 
         this.tabControlLog.Controls.Add(this.tabPage1);
         this.tabControlLog.Location = new System.Drawing.Point(0, 0);
         this.tabControlLog.Name = "tabControlLog";
         this.tabControlLog.SelectedIndex = 0;
         this.tabControlLog.Size = new System.Drawing.Size(480, 688);
         this.tabControlLog.TabIndex = 1;
         // 
         // panelSignals
         // 
         this.panelSignals.BackColor = System.Drawing.Color.DarkBlue;
         this.panelSignals.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panelSignals.Location = new System.Drawing.Point(0, 688);
         this.panelSignals.Name = "panelSignals";
         this.panelSignals.Size = new System.Drawing.Size(480, 8);
         this.panelSignals.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSignals_Paint);
         // 
         // splitter1
         // 
         this.splitter1.BackColor = System.Drawing.Color.Black;
         this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
         this.splitter1.Location = new System.Drawing.Point(0, 688);
         this.splitter1.MinExtra = 0;
         this.splitter1.MinSize = 0;
         this.splitter1.Name = "splitter1";
         this.splitter1.Size = new System.Drawing.Size(480, 11);
         // 
         // Form1
         // 
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
         this.ClientSize = new System.Drawing.Size(480, 696);
         this.Controls.Add(this.splitter1);
         this.Controls.Add(this.panelSignals);
         this.Controls.Add(this.tabControlLog);
         this.Location = new System.Drawing.Point(0, 52);
         this.Menu = this.mainMenu1;
         this.Name = "Form1";
         this.Text = "GPS test";
         this.Load += new System.EventHandler(this.Form1_Load);
         this.Closed += new System.EventHandler(this.Form1_Closed);
         this.tabPage1.ResumeLayout(false);
         this.tabControlLog.ResumeLayout(false);
         this.ResumeLayout(false);

      }
      #endregion

      public Form1()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose(bool disposing)
      {
         base.Dispose(disposing);
      }

      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      static void Main()
      {
         Application.Run(new Form1());
      }

      #region -- native imports --

      [DllImport("coredll.dll")]
      static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

      const int SW_MINIMIZED = 6;

      public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr) (-1);

      // The CharSet must match the CharSet of the corresponding PInvoke signature
      [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
      public struct WIN32_FIND_DATA
      {
         public int dwFileAttributes;
         public FILETIME ftCreationTime;
         public FILETIME ftLastAccessTime;
         public FILETIME ftLastWriteTime;
         public int nFileSizeHigh;
         public int nFileSizeLow;
         public int dwOID;
         [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
         public string cFileName;
         [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
         public string cAlternateFileName;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct FILETIME
      {
         public int dwLowDateTime;
         public int dwHighDateTime;
      };

      [DllImport("note_prj", EntryPoint="FindFirstFlashCard")]
      public extern static IntPtr FindFirstFlashCard(ref WIN32_FIND_DATA findData);

      [DllImport("note_prj", EntryPoint="FindNextFlashCard")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public extern static bool FindNextFlashCard(IntPtr hFlashCard, ref WIN32_FIND_DATA findData);

      [DllImport("coredll")]
      public static extern bool FindClose(IntPtr hFindFile);

      public const int PPN_UNATTENDEDMODE = 0x0003;
      public const int POWER_NAME = 0x00000001;
      public const int POWER_FORCE = 0x00001000;

      [DllImport("coredll.dll")]
      public static extern bool PowerPolicyNotify(int dwMessage, bool dwData);

      [DllImport("coredll.dll", SetLastError=true)]
      public static extern IntPtr SetPowerRequirement(string pvDevice, CedevicePowerStateState deviceState, uint deviceFlags, string pvSystemState, ulong stateFlags);

      [DllImport("coredll.dll", SetLastError=true)]
      public static extern int ReleasePowerRequirement(IntPtr hPowerReq);

      public enum CedevicePowerStateState : int
      {
         PwrDeviceUnspecified=-1,
         D0=0,
         D1,
         D2,
         D3,
         D4,
      }

      [DllImport("coredll")]
      public static extern void SystemIdleTimerReset();

      #endregion

      #region -- variables --
      DateTime LastMove = DateTime.Now;

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
      readonly List<Satellite> Satellites = new List<Satellite>();

      Pen penForSat = new Pen(Color.White, 3.0f);
      Brush brushForSatOk = new SolidBrush(Color.LimeGreen);
      Brush brushForSatNo = new SolidBrush(Color.Red);

      IntPtr gpsPowerHandle = IntPtr.Zero;
      #endregion

      #region -- functions --
      public new void Hide()
      {
         ShowWindow(this.Handle, SW_MINIMIZED);
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

      string GetRemovableStorageDirectory()
      {
         string removableStorageDirectory = null;

         WIN32_FIND_DATA findData = new WIN32_FIND_DATA();
         IntPtr handle = IntPtr.Zero;

         handle = FindFirstFlashCard(ref findData);

         if(handle != INVALID_HANDLE_VALUE)
         {
            do
            {
               if(!string.IsNullOrEmpty(findData.cFileName))
               {
                  removableStorageDirectory = findData.cFileName;
                  break;
               }
            }
            while(FindNextFlashCard(handle, ref findData));
            FindClose(handle);
         }

         return removableStorageDirectory;
      }
      #endregion

      void Form1_Load(object sender, System.EventArgs e)
      {
         updateDataHandler = new EventHandler(UpdateData);

         status.Text = "";

         status.Width = Screen.PrimaryScreen.WorkingArea.Width;
         status.Height = Screen.PrimaryScreen.WorkingArea.Height;

         gps.DeviceStateChanged += new DeviceStateChangedEventHandler(gps_DeviceStateChanged);
         gps.LocationChanged += new LocationChangedEventHandler(gps_LocationChanged);

         panelSignals.MouseMove += new MouseEventHandler(panelSignals_MouseMove);

         // Keep the GPS and device alive
         bool power = PowerPolicyNotify(PPN_UNATTENDEDMODE, true);
         if(!power)
         {
            Debug.WriteLine("PowerPolicyNotify failed for PPN_UNATTENDEDMODE");
         }
         else
         {
            gpsPowerHandle = SetPowerRequirement("gps0:", CedevicePowerStateState.D0, POWER_NAME | POWER_FORCE, null, 0);
            if(gpsPowerHandle == IntPtr.Zero)
            {
               Debug.WriteLine("SetPowerRequirement failed for GPS");
            }
         }
      }

      void UpdateData(object sender, System.EventArgs args)
      {
         try
         {
            // update signals
            panelSignals.Invalidate();
            {
               var data = sender as GpsPosition;
               string str = "\n";

               if(data != null)
               {
                  {
                     if(data.Time.HasValue && data.Longitude.HasValue && data.Longitude.HasValue)
                     {
                        str += "Time: " + data.Time.Value.ToLongDateString() + " " + data.Time.Value.ToLongTimeString() + "\n";
                        str += "Delay: " + ((int) (DateTime.UtcNow - data.Time.Value).TotalSeconds) + "s\n";
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
               }

               status.Text = str;
               this.Text = "GPS: " + count + "|" + countReal;
            }
         }
         catch(Exception ex)
         {
            status.Text = "\n" + ex.ToString();
         }
      }

      void Form1_Closed(object sender, System.EventArgs e)
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
            ReleasePowerRequirement(gpsPowerHandle);
            gpsPowerHandle = IntPtr.Zero;
         }
         PowerPolicyNotify(PPN_UNATTENDEDMODE, false);         
      }

      void panelSignals_MouseMove(object sender, MouseEventArgs e)
      {
         LastMove = DateTime.Now;
      }

      void gps_LocationChanged(object sender, LocationChangedEventArgs args)
      {
         try
         {
            var position = args.Position;
            if(position != null)
            {
               count++;

               Debug.WriteLine("LocationChanged: " + DateTime.Now.ToLongTimeString() + " -> " + count);

               if(position.Time.HasValue && position.Latitude.HasValue && position.Longitude.HasValue)
               {
                  Debug.WriteLine("Location: " + position.Latitude.Value + "|" + position.Longitude.Value);

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
               if((DateTime.Now - LastMove).TotalMinutes < 1)
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

      void panelSignals_Paint(object sender, PaintEventArgs e)
      {
         lock(Satellites)
         {
            if(Satellites.Count > 0)
            {
               int cc = Width / Satellites.Count;
               for(int i = 0; i < Satellites.Count; i++)
               {
                  int str = (int) ((panelSignals.Height * Satellites[i].SignalStrength)/100.0);

                  if(Satellites[i].InSolution)
                  {
                     e.Graphics.FillRectangle(brushForSatOk, new Rectangle(i*cc, panelSignals.Height - str, cc, str));
                  }
                  else
                  {
                     e.Graphics.FillRectangle(brushForSatNo, new Rectangle(i*cc, panelSignals.Height - str, cc, str));
                  }

                  e.Graphics.DrawRectangle(penForSat, new Rectangle(i*cc + (int) penForSat.Width/2, 0, cc - (int) penForSat.Width/2, panelSignals.Height));
               }
            }
         }
      }

      void stopGpsMenuItem_Click(object sender, EventArgs e)
      {
         if(gps.Opened)
         {
            gps.Close();
         }

         startGpsMenuItem.Enabled = true;
         stopGpsMenuItem.Enabled = false;
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
         tabControlLog.Height = 688;
         panelSignals.Invalidate();

         TryCommitData();
      }

      void startGpsMenuItem_Click(object sender, EventArgs e)
      {
         string sd = GetRemovableStorageDirectory();
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

         startGpsMenuItem.Enabled = false;
         stopGpsMenuItem.Enabled = true;
         tabControlLog.Height = 511;
      }

      void exitMenuItem_Click(object sender, EventArgs e)
      {
         Hide();
      }

      void menuItem1_Click(object sender, EventArgs e)
      {
         Close();
      }
   }
}
