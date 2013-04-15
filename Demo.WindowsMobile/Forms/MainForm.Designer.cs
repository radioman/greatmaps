namespace Demo.WindowsMobile
{
   partial class MainForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;
      private System.Windows.Forms.MainMenu mainMenu1;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if(disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.mainMenu1 = new System.Windows.Forms.MainMenu();
         this.menuItem1 = new System.Windows.Forms.MenuItem();
         this.menuItemGotoMap = new System.Windows.Forms.MenuItem();
         this.menuItemGotoGps = new System.Windows.Forms.MenuItem();
         this.menuItemSearch = new System.Windows.Forms.MenuItem();
         this.menuItemGotoTransport = new System.Windows.Forms.MenuItem();
         this.menuItem34 = new System.Windows.Forms.MenuItem();
         this.menuItem32 = new System.Windows.Forms.MenuItem();
         this.menuItem2 = new System.Windows.Forms.MenuItem();
         this.menuItem29 = new System.Windows.Forms.MenuItem();
         this.menuItemGPSenabled = new System.Windows.Forms.MenuItem();
         this.menuItem8 = new System.Windows.Forms.MenuItem();
         this.menuItem12 = new System.Windows.Forms.MenuItem();
         this.menuItem13 = new System.Windows.Forms.MenuItem();
         this.menuItem9 = new System.Windows.Forms.MenuItem();
         this.menuItem10 = new System.Windows.Forms.MenuItem();
         this.menuItem11 = new System.Windows.Forms.MenuItem();
         this.menuItem14 = new System.Windows.Forms.MenuItem();
         this.menuItem15 = new System.Windows.Forms.MenuItem();
         this.menuItem19 = new System.Windows.Forms.MenuItem();
         this.menuItem7 = new System.Windows.Forms.MenuItem();
         this.menuItemCacheOnly = new System.Windows.Forms.MenuItem();
         this.menuItemServerAndCache = new System.Windows.Forms.MenuItem();
         this.menuItemServerOnly = new System.Windows.Forms.MenuItem();
         this.menuItem30 = new System.Windows.Forms.MenuItem();
         this.menuItemshowDestination = new System.Windows.Forms.MenuItem();
         this.menuItemSnapToGps = new System.Windows.Forms.MenuItem();
         this.menuItemDisableAutoSleep = new System.Windows.Forms.MenuItem();
         this.menuItemEnableGrid = new System.Windows.Forms.MenuItem();
         this.menuItem6 = new System.Windows.Forms.MenuItem();
         this.menuItem36 = new System.Windows.Forms.MenuItem();
         this.menuItem37 = new System.Windows.Forms.MenuItem();
         this.menuItem35 = new System.Windows.Forms.MenuItem();
         this.menuItemSetDestination = new System.Windows.Forms.MenuItem();
         this.menuItem38 = new System.Windows.Forms.MenuItem();
         this.menuItem41 = new System.Windows.Forms.MenuItem();
         this.menuItem42 = new System.Windows.Forms.MenuItem();
         this.menuItem43 = new System.Windows.Forms.MenuItem();
         this.menuItem40 = new System.Windows.Forms.MenuItem();
         this.menuItem39 = new System.Windows.Forms.MenuItem();
         this.menuItem27 = new System.Windows.Forms.MenuItem();
         this.menuItem28 = new System.Windows.Forms.MenuItem();
         this.menuItem3 = new System.Windows.Forms.MenuItem();
         this.timerKeeperOfLife = new System.Windows.Forms.Timer();
         this.MainMap = new Demo.WindowsMobile.Map();
         this.menuItem4 = new System.Windows.Forms.MenuItem();
         this.SuspendLayout();
         // 
         // mainMenu1
         // 
         this.mainMenu1.MenuItems.Add(this.menuItem1);
         this.mainMenu1.MenuItems.Add(this.menuItem2);
         // 
         // menuItem1
         // 
         this.menuItem1.MenuItems.Add(this.menuItemGotoMap);
         this.menuItem1.MenuItems.Add(this.menuItemGotoGps);
         this.menuItem1.MenuItems.Add(this.menuItemSearch);
         this.menuItem1.MenuItems.Add(this.menuItemGotoTransport);
         this.menuItem1.MenuItems.Add(this.menuItem34);
         this.menuItem1.MenuItems.Add(this.menuItem32);
         this.menuItem1.Text = "Page";
         // 
         // menuItemGotoMap
         // 
         this.menuItemGotoMap.Checked = true;
         this.menuItemGotoMap.Text = "Map";
         this.menuItemGotoMap.Click += new System.EventHandler(this.menuItemGotoMap_Click);
         // 
         // menuItemGotoGps
         // 
         this.menuItemGotoGps.Text = "GPS";
         this.menuItemGotoGps.Click += new System.EventHandler(this.menuItemGotoGps_Click);
         // 
         // menuItemSearch
         // 
         this.menuItemSearch.Text = "Search";
         this.menuItemSearch.Click += new System.EventHandler(this.menuItemSearch_Click);
         // 
         // menuItemGotoTransport
         // 
         this.menuItemGotoTransport.Text = "Vilnius Transport";
         this.menuItemGotoTransport.Click += new System.EventHandler(this.menuItemGotoTransport_Click);
         // 
         // menuItem34
         // 
         this.menuItem34.Text = "-";
         // 
         // menuItem32
         // 
         this.menuItem32.Text = "Hide";
         this.menuItem32.Click += new System.EventHandler(this.menuItem32_Click);
         // 
         // menuItem2
         // 
         this.menuItem2.MenuItems.Add(this.menuItem29);
         this.menuItem2.MenuItems.Add(this.menuItem8);
         this.menuItem2.MenuItems.Add(this.menuItem7);
         this.menuItem2.MenuItems.Add(this.menuItem30);
         this.menuItem2.MenuItems.Add(this.menuItem6);
         this.menuItem2.MenuItems.Add(this.menuItem36);
         this.menuItem2.MenuItems.Add(this.menuItem38);
         this.menuItem2.MenuItems.Add(this.menuItem27);
         this.menuItem2.MenuItems.Add(this.menuItem28);
         this.menuItem2.MenuItems.Add(this.menuItem3);
         this.menuItem2.Text = "Menu";
         // 
         // menuItem29
         // 
         this.menuItem29.MenuItems.Add(this.menuItemGPSenabled);
         this.menuItem29.Text = "GPS";
         // 
         // menuItemGPSenabled
         // 
         this.menuItemGPSenabled.Checked = true;
         this.menuItemGPSenabled.Text = "Enabled";
         this.menuItemGPSenabled.Click += new System.EventHandler(this.menuItemGPSenabled_Click);
         // 
         // menuItem8
         // 
         this.menuItem8.MenuItems.Add(this.menuItem14);
         this.menuItem8.MenuItems.Add(this.menuItem15);
         this.menuItem8.MenuItems.Add(this.menuItem11);
         this.menuItem8.MenuItems.Add(this.menuItem4);
         this.menuItem8.MenuItems.Add(this.menuItem12);
         this.menuItem8.MenuItems.Add(this.menuItem13);
         this.menuItem8.MenuItems.Add(this.menuItem9);
         this.menuItem8.MenuItems.Add(this.menuItem10);
         this.menuItem8.MenuItems.Add(this.menuItem19);
         this.menuItem8.Text = "Map";
         // 
         // menuItem12
         // 
         this.menuItem12.Text = "OpenStreet";
         this.menuItem12.Click += new System.EventHandler(this.menuItem12_Click);
         // 
         // menuItem13
         // 
         this.menuItem13.Text = "-";
         // 
         // menuItem9
         // 
         this.menuItem9.Text = "Google";
         this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
         // 
         // menuItem10
         // 
         this.menuItem10.Text = "Google Sattelite";
         this.menuItem10.Click += new System.EventHandler(this.menuItem10_Click);
         // 
         // menuItem11
         // 
         this.menuItem11.Text = "-";
         // 
         // menuItem14
         // 
         this.menuItem14.Text = "Lithuania";
         this.menuItem14.Click += new System.EventHandler(this.menuItem14_Click);
         // 
         // menuItem15
         // 
         this.menuItem15.Text = "Lithuania OrtoFoto";
         this.menuItem15.Click += new System.EventHandler(this.menuItem15_Click);
         // 
         // menuItem19
         // 
         this.menuItem19.Text = "-";
         // 
         // menuItem7
         // 
         this.menuItem7.MenuItems.Add(this.menuItemCacheOnly);
         this.menuItem7.MenuItems.Add(this.menuItemServerAndCache);
         this.menuItem7.MenuItems.Add(this.menuItemServerOnly);
         this.menuItem7.Text = "Mode";
         // 
         // menuItemCacheOnly
         // 
         this.menuItemCacheOnly.Text = "CacheOnly";
         this.menuItemCacheOnly.Click += new System.EventHandler(this.menuItemCacheOnly_Click);
         // 
         // menuItemServerAndCache
         // 
         this.menuItemServerAndCache.Text = "ServerAndCache";
         this.menuItemServerAndCache.Click += new System.EventHandler(this.menuItemServerAndCache_Click);
         // 
         // menuItemServerOnly
         // 
         this.menuItemServerOnly.Text = "ServerOnly";
         this.menuItemServerOnly.Click += new System.EventHandler(this.menuItemServerOnly_Click);
         // 
         // menuItem30
         // 
         this.menuItem30.MenuItems.Add(this.menuItemshowDestination);
         this.menuItem30.MenuItems.Add(this.menuItemSnapToGps);
         this.menuItem30.MenuItems.Add(this.menuItemDisableAutoSleep);
         this.menuItem30.MenuItems.Add(this.menuItemEnableGrid);
         this.menuItem30.Text = "Options";
         // 
         // menuItemshowDestination
         // 
         this.menuItemshowDestination.Text = "Show Destination";
         this.menuItemshowDestination.Click += new System.EventHandler(this.menuItemshowDestination_Click);
         // 
         // menuItemSnapToGps
         // 
         this.menuItemSnapToGps.Checked = true;
         this.menuItemSnapToGps.Text = "Snap To GPS";
         this.menuItemSnapToGps.Click += new System.EventHandler(this.menuItemSnapToGps_Click);
         // 
         // menuItemDisableAutoSleep
         // 
         this.menuItemDisableAutoSleep.Checked = true;
         this.menuItemDisableAutoSleep.Text = "Disable Auto Sleep";
         this.menuItemDisableAutoSleep.Click += new System.EventHandler(this.menuItemDisableAutoSleep_Click);
         // 
         // menuItemEnableGrid
         // 
         this.menuItemEnableGrid.Text = "Enable Grid";
         this.menuItemEnableGrid.Click += new System.EventHandler(this.menuItemEnableGrid_Click);
         // 
         // menuItem6
         // 
         this.menuItem6.Text = "-";
         // 
         // menuItem36
         // 
         this.menuItem36.MenuItems.Add(this.menuItem37);
         this.menuItem36.MenuItems.Add(this.menuItem35);
         this.menuItem36.MenuItems.Add(this.menuItemSetDestination);
         this.menuItem36.Text = "Markers";
         // 
         // menuItem37
         // 
         this.menuItem37.Text = "Clear";
         this.menuItem37.Click += new System.EventHandler(this.menuItem37_Click);
         // 
         // menuItem35
         // 
         this.menuItem35.Text = "Fit to All";
         this.menuItem35.Click += new System.EventHandler(this.menuItem35_Click);
         // 
         // menuItemSetDestination
         // 
         this.menuItemSetDestination.Text = "Set Destination";
         this.menuItemSetDestination.Click += new System.EventHandler(this.menuItemSetDestination_Click);
         // 
         // menuItem38
         // 
         this.menuItem38.MenuItems.Add(this.menuItem41);
         this.menuItem38.MenuItems.Add(this.menuItem42);
         this.menuItem38.MenuItems.Add(this.menuItem43);
         this.menuItem38.MenuItems.Add(this.menuItem40);
         this.menuItem38.MenuItems.Add(this.menuItem39);
         this.menuItem38.Text = "Zoom";
         // 
         // menuItem41
         // 
         this.menuItem41.Text = "In";
         this.menuItem41.Click += new System.EventHandler(this.menuItem4_Click);
         // 
         // menuItem42
         // 
         this.menuItem42.Text = "Out";
         this.menuItem42.Click += new System.EventHandler(this.menuItem5_Click);
         // 
         // menuItem43
         // 
         this.menuItem43.Text = "-";
         // 
         // menuItem40
         // 
         this.menuItem40.Text = "Max";
         this.menuItem40.Click += new System.EventHandler(this.menuItem33_Click);
         // 
         // menuItem39
         // 
         this.menuItem39.Text = "Min";
         this.menuItem39.Click += new System.EventHandler(this.menuItem31_Click);
         // 
         // menuItem27
         // 
         this.menuItem27.Text = "Reload";
         this.menuItem27.Click += new System.EventHandler(this.menuItem27_Click);
         // 
         // menuItem28
         // 
         this.menuItem28.Text = "-";
         // 
         // menuItem3
         // 
         this.menuItem3.Text = "Exit";
         this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
         // 
         // timerKeeperOfLife
         // 
         this.timerKeeperOfLife.Tick += new System.EventHandler(this.timerKeeperOfLife_Tick);
         // 
         // MainMap
         // 
         this.MainMap.BackColor = System.Drawing.Color.White;
         this.MainMap.Dock = System.Windows.Forms.DockStyle.Fill;
         this.MainMap.Location = new System.Drawing.Point(0, 0);
         this.MainMap.Name = "MainMap";
         this.MainMap.Size = new System.Drawing.Size(480, 696);
         this.MainMap.TabIndex = 0;
         // 
         // menuItem4
         // 
         this.menuItem4.Text = "OpenCycle";
         this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click_1);
         // 
         // MainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
         this.AutoScroll = true;
         this.ClientSize = new System.Drawing.Size(480, 696);
         this.Controls.Add(this.MainMap);
         this.KeyPreview = true;
         this.Location = new System.Drawing.Point(0, 52);
         this.Menu = this.mainMenu1;
         this.Name = "MainForm";
         this.Text = "GMap.NET";
         this.Load += new System.EventHandler(this.MainForm_Load);
         this.Closed += new System.EventHandler(this.MainForm_Closed);
         this.Activated += new System.EventHandler(this.MainForm_Activated);
         this.ResumeLayout(false);

      }

      #endregion

      internal Map MainMap;
      private System.Windows.Forms.MenuItem menuItem1;
      private System.Windows.Forms.MenuItem menuItem2;
      private System.Windows.Forms.MenuItem menuItem3;
      private System.Windows.Forms.MenuItem menuItem6;
      private System.Windows.Forms.MenuItem menuItem8;
      private System.Windows.Forms.MenuItem menuItem12;
      private System.Windows.Forms.MenuItem menuItem13;
      private System.Windows.Forms.MenuItem menuItem9;
      private System.Windows.Forms.MenuItem menuItem10;
      private System.Windows.Forms.MenuItem menuItem11;
      private System.Windows.Forms.MenuItem menuItem14;
      private System.Windows.Forms.MenuItem menuItem15;
      private System.Windows.Forms.MenuItem menuItem19;
      private System.Windows.Forms.MenuItem menuItem27;
      private System.Windows.Forms.MenuItem menuItem28;
      private System.Windows.Forms.MenuItem menuItem7;
      private System.Windows.Forms.MenuItem menuItemServerAndCache;
      private System.Windows.Forms.MenuItem menuItemCacheOnly;
      private System.Windows.Forms.MenuItem menuItem29;
      private System.Windows.Forms.MenuItem menuItemGPSenabled;
      private System.Windows.Forms.MenuItem menuItem30;
      private System.Windows.Forms.MenuItem menuItemEnableGrid;
      private System.Windows.Forms.Timer timerKeeperOfLife;
      private System.Windows.Forms.MenuItem menuItemDisableAutoSleep;
      private System.Windows.Forms.MenuItem menuItemGotoMap;
      private System.Windows.Forms.MenuItem menuItemGotoGps;
      private System.Windows.Forms.MenuItem menuItem34;
      private System.Windows.Forms.MenuItem menuItem32;
      private System.Windows.Forms.MenuItem menuItemServerOnly;
      private System.Windows.Forms.MenuItem menuItemGotoTransport;
      private System.Windows.Forms.MenuItem menuItem36;
      private System.Windows.Forms.MenuItem menuItem37;
      private System.Windows.Forms.MenuItem menuItem35;
      private System.Windows.Forms.MenuItem menuItemSearch;
      private System.Windows.Forms.MenuItem menuItem38;
      private System.Windows.Forms.MenuItem menuItem41;
      private System.Windows.Forms.MenuItem menuItem42;
      private System.Windows.Forms.MenuItem menuItem43;
      private System.Windows.Forms.MenuItem menuItem40;
      private System.Windows.Forms.MenuItem menuItem39;
      private System.Windows.Forms.MenuItem menuItemSnapToGps;
      internal System.Windows.Forms.MenuItem menuItemshowDestination;
      private System.Windows.Forms.MenuItem menuItemSetDestination;
      private System.Windows.Forms.MenuItem menuItem4;
   }
}

