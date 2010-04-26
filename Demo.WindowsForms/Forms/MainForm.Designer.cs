namespace Demo.WindowsForms
{
   partial class MainForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

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
         this.label7 = new System.Windows.Forms.Label();
         this.comboBoxMapType = new System.Windows.Forms.ComboBox();
         this.groupMap = new System.Windows.Forms.GroupBox();
         this.MainMap = new Demo.WindowsForms.Map();
         this.groupBox3 = new System.Windows.Forms.GroupBox();
         this.label6 = new System.Windows.Forms.Label();
         this.textBoxGeo = new System.Windows.Forms.TextBox();
         this.button1 = new System.Windows.Forms.Button();
         this.button8 = new System.Windows.Forms.Button();
         this.label2 = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.textBoxLng = new System.Windows.Forms.TextBox();
         this.textBoxLat = new System.Windows.Forms.TextBox();
         this.button5 = new System.Windows.Forms.Button();
         this.button4 = new System.Windows.Forms.Button();
         this.trackBar1 = new System.Windows.Forms.TrackBar();
         this.groupBox2 = new System.Windows.Forms.GroupBox();
         this.groupBox5 = new System.Windows.Forms.GroupBox();
         this.button13 = new System.Windows.Forms.Button();
         this.checkBoxDebug = new System.Windows.Forms.CheckBox();
         this.button12 = new System.Windows.Forms.Button();
         this.label8 = new System.Windows.Forms.Label();
         this.comboBoxMode = new System.Windows.Forms.ComboBox();
         this.checkBoxCanDrag = new System.Windows.Forms.CheckBox();
         this.checkBoxCurrentMarker = new System.Windows.Forms.CheckBox();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.button11 = new System.Windows.Forms.Button();
         this.button10 = new System.Windows.Forms.Button();
         this.button9 = new System.Windows.Forms.Button();
         this.checkBoxUseGeoCache = new System.Windows.Forms.CheckBox();
         this.checkBoxUseRouteCache = new System.Windows.Forms.CheckBox();
         this.button2 = new System.Windows.Forms.Button();
         this.button3 = new System.Windows.Forms.Button();
         this.groupBox7 = new System.Windows.Forms.GroupBox();
         this.label3 = new System.Windows.Forms.Label();
         this.button15 = new System.Windows.Forms.Button();
         this.dateTimePickerMobileLog = new System.Windows.Forms.DateTimePicker();
         this.button14 = new System.Windows.Forms.Button();
         this.buttonSetEnd = new System.Windows.Forms.Button();
         this.buttonSetStart = new System.Windows.Forms.Button();
         this.button6 = new System.Windows.Forms.Button();
         this.groupBox8 = new System.Windows.Forms.GroupBox();
         this.button7 = new System.Windows.Forms.Button();
         this.checkBoxPlacemarkInfo = new System.Windows.Forms.CheckBox();
         this.statusStrip1 = new System.Windows.Forms.StatusStrip();
         this.toolStripStatusLabelMemoryCache = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabelCurrentPosition = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStripStatusLabelLoading = new System.Windows.Forms.ToolStripStatusLabel();
         this.progressBar1 = new System.Windows.Forms.ToolStripProgressBar();
         this.groupBox6 = new System.Windows.Forms.GroupBox();
         this.radioButtonNone = new System.Windows.Forms.RadioButton();
         this.radioButtonPerf = new System.Windows.Forms.RadioButton();
         this.radioButtonTransport = new System.Windows.Forms.RadioButton();
         this.groupMap.SuspendLayout();
         this.groupBox3.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize) (this.trackBar1)).BeginInit();
         this.groupBox2.SuspendLayout();
         this.groupBox5.SuspendLayout();
         this.groupBox1.SuspendLayout();
         this.groupBox7.SuspendLayout();
         this.groupBox8.SuspendLayout();
         this.statusStrip1.SuspendLayout();
         this.groupBox6.SuspendLayout();
         this.SuspendLayout();
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(132, 22);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(27, 13);
         this.label7.TabIndex = 31;
         this.label7.Text = "type";
         // 
         // comboBoxMapType
         // 
         this.comboBoxMapType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxMapType.FormattingEnabled = true;
         this.comboBoxMapType.Location = new System.Drawing.Point(6, 19);
         this.comboBoxMapType.Name = "comboBoxMapType";
         this.comboBoxMapType.Size = new System.Drawing.Size(125, 21);
         this.comboBoxMapType.TabIndex = 9;
         this.comboBoxMapType.DropDownClosed += new System.EventHandler(this.comboBoxMapType_DropDownClosed);
         // 
         // groupMap
         // 
         this.groupMap.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.groupMap.Controls.Add(this.MainMap);
         this.groupMap.Location = new System.Drawing.Point(12, 7);
         this.groupMap.Name = "groupMap";
         this.groupMap.Size = new System.Drawing.Size(597, 674);
         this.groupMap.TabIndex = 27;
         this.groupMap.TabStop = false;
         this.groupMap.Text = "gmap";
         // 
         // MainMap
         // 
         this.MainMap.CanDragMap = true;
         this.MainMap.Dock = System.Windows.Forms.DockStyle.Fill;
         this.MainMap.GrayScaleMode = false;
         this.MainMap.Location = new System.Drawing.Point(3, 16);
         this.MainMap.MapType = GMap.NET.MapType.ArcGIS_MapsLT_Map;
         this.MainMap.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
         this.MainMap.MarkersEnabled = true;
         this.MainMap.MaxZoom = 2;
         this.MainMap.MinZoom = 2;
         this.MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
         this.MainMap.Name = "MainMap";
         this.MainMap.PolygonsEnabled = true;
         this.MainMap.RoutesEnabled = true;
         this.MainMap.ShowTileGridLines = false;
         this.MainMap.Size = new System.Drawing.Size(591, 655);
         this.MainMap.TabIndex = 0;
         this.MainMap.Zoom = 0;
         this.MainMap.MouseEnter += new System.EventHandler(this.MainMap_MouseEnter);
         // 
         // groupBox3
         // 
         this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox3.Controls.Add(this.label6);
         this.groupBox3.Controls.Add(this.textBoxGeo);
         this.groupBox3.Controls.Add(this.button1);
         this.groupBox3.Controls.Add(this.button8);
         this.groupBox3.Controls.Add(this.label2);
         this.groupBox3.Controls.Add(this.label1);
         this.groupBox3.Controls.Add(this.textBoxLng);
         this.groupBox3.Controls.Add(this.textBoxLat);
         this.groupBox3.Location = new System.Drawing.Point(669, 7);
         this.groupBox3.Name = "groupBox3";
         this.groupBox3.Size = new System.Drawing.Size(169, 128);
         this.groupBox3.TabIndex = 28;
         this.groupBox3.TabStop = false;
         this.groupBox3.Text = "coordinates";
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(132, 74);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(28, 13);
         this.label6.TabIndex = 11;
         this.label6.Text = "goto";
         // 
         // textBoxGeo
         // 
         this.textBoxGeo.Location = new System.Drawing.Point(6, 71);
         this.textBoxGeo.Name = "textBoxGeo";
         this.textBoxGeo.Size = new System.Drawing.Size(125, 20);
         this.textBoxGeo.TabIndex = 10;
         this.textBoxGeo.Text = "lietuva vilnius";
         this.textBoxGeo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxGeo_KeyPress);
         // 
         // button1
         // 
         this.button1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.button1.Location = new System.Drawing.Point(85, 98);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(74, 24);
         this.button1.TabIndex = 9;
         this.button1.Text = "Reload";
         this.button1.UseVisualStyleBackColor = true;
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // button8
         // 
         this.button8.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.button8.Location = new System.Drawing.Point(6, 98);
         this.button8.Name = "button8";
         this.button8.Size = new System.Drawing.Size(75, 24);
         this.button8.TabIndex = 8;
         this.button8.Text = "GoTo !";
         this.button8.UseVisualStyleBackColor = true;
         this.button8.Click += new System.EventHandler(this.button8_Click);
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(132, 48);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(21, 13);
         this.label2.TabIndex = 3;
         this.label2.Text = "lng";
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(132, 22);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(18, 13);
         this.label1.TabIndex = 2;
         this.label1.Text = "lat";
         // 
         // textBoxLng
         // 
         this.textBoxLng.Location = new System.Drawing.Point(6, 45);
         this.textBoxLng.Name = "textBoxLng";
         this.textBoxLng.Size = new System.Drawing.Size(125, 20);
         this.textBoxLng.TabIndex = 1;
         this.textBoxLng.Text = "25.2985095977783";
         // 
         // textBoxLat
         // 
         this.textBoxLat.Location = new System.Drawing.Point(6, 19);
         this.textBoxLat.Name = "textBoxLat";
         this.textBoxLat.Size = new System.Drawing.Size(125, 20);
         this.textBoxLat.TabIndex = 0;
         this.textBoxLat.Text = "54.6961334816182";
         // 
         // button5
         // 
         this.button5.Location = new System.Drawing.Point(94, 45);
         this.button5.Name = "button5";
         this.button5.Size = new System.Drawing.Size(63, 24);
         this.button5.TabIndex = 13;
         this.button5.Text = "Clear All";
         this.button5.UseVisualStyleBackColor = true;
         this.button5.Click += new System.EventHandler(this.button5_Click);
         // 
         // button4
         // 
         this.button4.Location = new System.Drawing.Point(6, 15);
         this.button4.Name = "button4";
         this.button4.Size = new System.Drawing.Size(82, 24);
         this.button4.TabIndex = 12;
         this.button4.Text = "Add Marker";
         this.button4.UseVisualStyleBackColor = true;
         this.button4.Click += new System.EventHandler(this.button4_Click);
         // 
         // trackBar1
         // 
         this.trackBar1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.trackBar1.LargeChange = 1;
         this.trackBar1.Location = new System.Drawing.Point(3, 16);
         this.trackBar1.Maximum = 17;
         this.trackBar1.Minimum = 1;
         this.trackBar1.Name = "trackBar1";
         this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
         this.trackBar1.Size = new System.Drawing.Size(42, 655);
         this.trackBar1.TabIndex = 29;
         this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
         this.trackBar1.Value = 12;
         this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
         // 
         // groupBox2
         // 
         this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox2.Controls.Add(this.trackBar1);
         this.groupBox2.Location = new System.Drawing.Point(615, 7);
         this.groupBox2.Name = "groupBox2";
         this.groupBox2.Size = new System.Drawing.Size(48, 674);
         this.groupBox2.TabIndex = 30;
         this.groupBox2.TabStop = false;
         this.groupBox2.Text = "zoom";
         // 
         // groupBox5
         // 
         this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox5.Controls.Add(this.button13);
         this.groupBox5.Controls.Add(this.checkBoxDebug);
         this.groupBox5.Controls.Add(this.button12);
         this.groupBox5.Controls.Add(this.label8);
         this.groupBox5.Controls.Add(this.comboBoxMode);
         this.groupBox5.Controls.Add(this.checkBoxCanDrag);
         this.groupBox5.Controls.Add(this.checkBoxCurrentMarker);
         this.groupBox5.Controls.Add(this.label7);
         this.groupBox5.Controls.Add(this.comboBoxMapType);
         this.groupBox5.Location = new System.Drawing.Point(669, 141);
         this.groupBox5.Name = "groupBox5";
         this.groupBox5.Size = new System.Drawing.Size(169, 146);
         this.groupBox5.TabIndex = 31;
         this.groupBox5.TabStop = false;
         this.groupBox5.Text = "gmap";
         // 
         // button13
         // 
         this.button13.Location = new System.Drawing.Point(8, 117);
         this.button13.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
         this.button13.Name = "button13";
         this.button13.Size = new System.Drawing.Size(73, 24);
         this.button13.TabIndex = 41;
         this.button13.Text = "Get Static Map";
         this.button13.UseVisualStyleBackColor = true;
         this.button13.Click += new System.EventHandler(this.button13_Click);
         // 
         // checkBoxDebug
         // 
         this.checkBoxDebug.AutoSize = true;
         this.checkBoxDebug.Location = new System.Drawing.Point(104, 74);
         this.checkBoxDebug.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
         this.checkBoxDebug.Name = "checkBoxDebug";
         this.checkBoxDebug.Size = new System.Drawing.Size(45, 17);
         this.checkBoxDebug.TabIndex = 40;
         this.checkBoxDebug.Text = "Grid";
         this.checkBoxDebug.UseVisualStyleBackColor = true;
         this.checkBoxDebug.CheckedChanged += new System.EventHandler(this.checkBoxDebug_CheckedChanged);
         // 
         // button12
         // 
         this.button12.Location = new System.Drawing.Point(85, 117);
         this.button12.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
         this.button12.Name = "button12";
         this.button12.Size = new System.Drawing.Size(73, 24);
         this.button12.TabIndex = 39;
         this.button12.Text = "Save View";
         this.button12.UseVisualStyleBackColor = true;
         this.button12.Click += new System.EventHandler(this.button12_Click);
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(132, 49);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(33, 13);
         this.label8.TabIndex = 38;
         this.label8.Text = "mode";
         // 
         // comboBoxMode
         // 
         this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxMode.FormattingEnabled = true;
         this.comboBoxMode.Location = new System.Drawing.Point(6, 46);
         this.comboBoxMode.Name = "comboBoxMode";
         this.comboBoxMode.Size = new System.Drawing.Size(125, 21);
         this.comboBoxMode.TabIndex = 37;
         this.comboBoxMode.DropDownClosed += new System.EventHandler(this.comboBoxMode_DropDownClosed);
         // 
         // checkBoxCanDrag
         // 
         this.checkBoxCanDrag.AutoSize = true;
         this.checkBoxCanDrag.Checked = true;
         this.checkBoxCanDrag.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxCanDrag.Location = new System.Drawing.Point(6, 97);
         this.checkBoxCanDrag.Name = "checkBoxCanDrag";
         this.checkBoxCanDrag.Size = new System.Drawing.Size(95, 17);
         this.checkBoxCanDrag.TabIndex = 36;
         this.checkBoxCanDrag.Text = "Can Drag Map";
         this.checkBoxCanDrag.UseVisualStyleBackColor = true;
         this.checkBoxCanDrag.CheckedChanged += new System.EventHandler(this.checkBoxCanDrag_CheckedChanged);
         // 
         // checkBoxCurrentMarker
         // 
         this.checkBoxCurrentMarker.AutoSize = true;
         this.checkBoxCurrentMarker.Checked = true;
         this.checkBoxCurrentMarker.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxCurrentMarker.Location = new System.Drawing.Point(6, 74);
         this.checkBoxCurrentMarker.Name = "checkBoxCurrentMarker";
         this.checkBoxCurrentMarker.Size = new System.Drawing.Size(96, 17);
         this.checkBoxCurrentMarker.TabIndex = 35;
         this.checkBoxCurrentMarker.Text = "Current Marker";
         this.checkBoxCurrentMarker.UseVisualStyleBackColor = true;
         this.checkBoxCurrentMarker.CheckedChanged += new System.EventHandler(this.checkBoxCurrentMarker_CheckedChanged);
         // 
         // groupBox1
         // 
         this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox1.Controls.Add(this.button11);
         this.groupBox1.Controls.Add(this.button10);
         this.groupBox1.Controls.Add(this.button9);
         this.groupBox1.Controls.Add(this.checkBoxUseGeoCache);
         this.groupBox1.Controls.Add(this.checkBoxUseRouteCache);
         this.groupBox1.Controls.Add(this.button2);
         this.groupBox1.Location = new System.Drawing.Point(669, 588);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(169, 93);
         this.groupBox1.TabIndex = 32;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "cache";
         // 
         // button11
         // 
         this.button11.Location = new System.Drawing.Point(87, 65);
         this.button11.Name = "button11";
         this.button11.Size = new System.Drawing.Size(71, 20);
         this.button11.TabIndex = 38;
         this.button11.Text = "Prefetch";
         this.button11.UseVisualStyleBackColor = true;
         this.button11.Click += new System.EventHandler(this.button11_Click);
         // 
         // button10
         // 
         this.button10.Location = new System.Drawing.Point(87, 39);
         this.button10.Name = "button10";
         this.button10.Size = new System.Drawing.Size(71, 20);
         this.button10.TabIndex = 5;
         this.button10.Text = "Import";
         this.button10.UseVisualStyleBackColor = true;
         this.button10.Click += new System.EventHandler(this.button10_Click);
         // 
         // button9
         // 
         this.button9.Location = new System.Drawing.Point(8, 39);
         this.button9.Name = "button9";
         this.button9.Size = new System.Drawing.Size(74, 20);
         this.button9.TabIndex = 4;
         this.button9.Text = "Export";
         this.button9.UseVisualStyleBackColor = true;
         this.button9.Click += new System.EventHandler(this.button9_Click);
         // 
         // checkBoxUseGeoCache
         // 
         this.checkBoxUseGeoCache.AutoSize = true;
         this.checkBoxUseGeoCache.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBoxUseGeoCache.Checked = true;
         this.checkBoxUseGeoCache.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxUseGeoCache.Location = new System.Drawing.Point(86, 19);
         this.checkBoxUseGeoCache.Name = "checkBoxUseGeoCache";
         this.checkBoxUseGeoCache.Size = new System.Drawing.Size(76, 17);
         this.checkBoxUseGeoCache.TabIndex = 3;
         this.checkBoxUseGeoCache.Text = "geocoding";
         this.checkBoxUseGeoCache.UseVisualStyleBackColor = true;
         this.checkBoxUseGeoCache.CheckedChanged += new System.EventHandler(this.checkBoxUseCache_CheckedChanged);
         // 
         // checkBoxUseRouteCache
         // 
         this.checkBoxUseRouteCache.AutoSize = true;
         this.checkBoxUseRouteCache.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBoxUseRouteCache.Checked = true;
         this.checkBoxUseRouteCache.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxUseRouteCache.Location = new System.Drawing.Point(6, 19);
         this.checkBoxUseRouteCache.Name = "checkBoxUseRouteCache";
         this.checkBoxUseRouteCache.Size = new System.Drawing.Size(58, 17);
         this.checkBoxUseRouteCache.TabIndex = 2;
         this.checkBoxUseRouteCache.Text = "routing";
         this.checkBoxUseRouteCache.UseVisualStyleBackColor = true;
         this.checkBoxUseRouteCache.CheckedChanged += new System.EventHandler(this.checkBoxUseCache_CheckedChanged);
         // 
         // button2
         // 
         this.button2.Location = new System.Drawing.Point(8, 65);
         this.button2.Name = "button2";
         this.button2.Size = new System.Drawing.Size(74, 20);
         this.button2.TabIndex = 0;
         this.button2.Text = "Clear All";
         this.button2.UseVisualStyleBackColor = true;
         this.button2.Click += new System.EventHandler(this.button2_Click);
         // 
         // button3
         // 
         this.button3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button3.Location = new System.Drawing.Point(9, 49);
         this.button3.Name = "button3";
         this.button3.Size = new System.Drawing.Size(73, 24);
         this.button3.TabIndex = 33;
         this.button3.Text = "Add Route";
         this.button3.UseVisualStyleBackColor = true;
         this.button3.Click += new System.EventHandler(this.button3_Click);
         // 
         // groupBox7
         // 
         this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox7.Controls.Add(this.label3);
         this.groupBox7.Controls.Add(this.button15);
         this.groupBox7.Controls.Add(this.dateTimePickerMobileLog);
         this.groupBox7.Controls.Add(this.button14);
         this.groupBox7.Controls.Add(this.buttonSetEnd);
         this.groupBox7.Controls.Add(this.buttonSetStart);
         this.groupBox7.Controls.Add(this.button6);
         this.groupBox7.Controls.Add(this.button3);
         this.groupBox7.Location = new System.Drawing.Point(669, 294);
         this.groupBox7.Name = "groupBox7";
         this.groupBox7.Size = new System.Drawing.Size(169, 138);
         this.groupBox7.TabIndex = 35;
         this.groupBox7.TabStop = false;
         this.groupBox7.Text = "routing";
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(6, 111);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(34, 13);
         this.label3.TabIndex = 46;
         this.label3.Text = "Clear:";
         // 
         // button15
         // 
         this.button15.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button15.Location = new System.Drawing.Point(100, 106);
         this.button15.Name = "button15";
         this.button15.Size = new System.Drawing.Size(58, 24);
         this.button15.TabIndex = 45;
         this.button15.Text = "Polygons";
         this.button15.UseVisualStyleBackColor = true;
         this.button15.Click += new System.EventHandler(this.button15_Click);
         // 
         // dateTimePickerMobileLog
         // 
         this.dateTimePickerMobileLog.Location = new System.Drawing.Point(7, 82);
         this.dateTimePickerMobileLog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
         this.dateTimePickerMobileLog.Name = "dateTimePickerMobileLog";
         this.dateTimePickerMobileLog.ShowCheckBox = true;
         this.dateTimePickerMobileLog.Size = new System.Drawing.Size(152, 20);
         this.dateTimePickerMobileLog.TabIndex = 44;
         // 
         // button14
         // 
         this.button14.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button14.Location = new System.Drawing.Point(85, 49);
         this.button14.Name = "button14";
         this.button14.Size = new System.Drawing.Size(74, 24);
         this.button14.TabIndex = 43;
         this.button14.Text = "Mobile log...";
         this.button14.UseVisualStyleBackColor = true;
         this.button14.Click += new System.EventHandler(this.button14_Click);
         // 
         // buttonSetEnd
         // 
         this.buttonSetEnd.Location = new System.Drawing.Point(85, 19);
         this.buttonSetEnd.Name = "buttonSetEnd";
         this.buttonSetEnd.Size = new System.Drawing.Size(74, 24);
         this.buttonSetEnd.TabIndex = 42;
         this.buttonSetEnd.Text = "set End";
         this.buttonSetEnd.UseVisualStyleBackColor = true;
         this.buttonSetEnd.Click += new System.EventHandler(this.buttonSetEnd_Click);
         // 
         // buttonSetStart
         // 
         this.buttonSetStart.Location = new System.Drawing.Point(9, 19);
         this.buttonSetStart.Name = "buttonSetStart";
         this.buttonSetStart.Size = new System.Drawing.Size(72, 24);
         this.buttonSetStart.TabIndex = 41;
         this.buttonSetStart.Text = "set Start";
         this.buttonSetStart.UseVisualStyleBackColor = true;
         this.buttonSetStart.Click += new System.EventHandler(this.buttonSetStart_Click);
         // 
         // button6
         // 
         this.button6.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button6.Location = new System.Drawing.Point(42, 106);
         this.button6.Name = "button6";
         this.button6.Size = new System.Drawing.Size(55, 24);
         this.button6.TabIndex = 34;
         this.button6.Text = "Routes";
         this.button6.UseVisualStyleBackColor = true;
         this.button6.Click += new System.EventHandler(this.button6_Click);
         // 
         // groupBox8
         // 
         this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox8.Controls.Add(this.button7);
         this.groupBox8.Controls.Add(this.checkBoxPlacemarkInfo);
         this.groupBox8.Controls.Add(this.button5);
         this.groupBox8.Controls.Add(this.button4);
         this.groupBox8.Location = new System.Drawing.Point(669, 439);
         this.groupBox8.Name = "groupBox8";
         this.groupBox8.Size = new System.Drawing.Size(169, 74);
         this.groupBox8.TabIndex = 37;
         this.groupBox8.TabStop = false;
         this.groupBox8.Text = "markers";
         // 
         // button7
         // 
         this.button7.Location = new System.Drawing.Point(6, 45);
         this.button7.Name = "button7";
         this.button7.Size = new System.Drawing.Size(82, 24);
         this.button7.TabIndex = 15;
         this.button7.Text = "Zoom Center";
         this.button7.UseVisualStyleBackColor = true;
         this.button7.Click += new System.EventHandler(this.button7_Click);
         // 
         // checkBoxPlacemarkInfo
         // 
         this.checkBoxPlacemarkInfo.AutoSize = true;
         this.checkBoxPlacemarkInfo.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
         this.checkBoxPlacemarkInfo.Checked = true;
         this.checkBoxPlacemarkInfo.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxPlacemarkInfo.Location = new System.Drawing.Point(94, 19);
         this.checkBoxPlacemarkInfo.Name = "checkBoxPlacemarkInfo";
         this.checkBoxPlacemarkInfo.Size = new System.Drawing.Size(72, 17);
         this.checkBoxPlacemarkInfo.TabIndex = 14;
         this.checkBoxPlacemarkInfo.Text = "place info";
         this.checkBoxPlacemarkInfo.UseVisualStyleBackColor = true;
         // 
         // statusStrip1
         // 
         this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelMemoryCache,
            this.toolStripStatusLabelCurrentPosition,
            this.toolStripStatusLabelLoading,
            this.progressBar1});
         this.statusStrip1.Location = new System.Drawing.Point(0, 691);
         this.statusStrip1.Name = "statusStrip1";
         this.statusStrip1.Size = new System.Drawing.Size(849, 22);
         this.statusStrip1.TabIndex = 38;
         this.statusStrip1.Text = "statusStrip1";
         // 
         // toolStripStatusLabelMemoryCache
         // 
         this.toolStripStatusLabelMemoryCache.Name = "toolStripStatusLabelMemoryCache";
         this.toolStripStatusLabelMemoryCache.Size = new System.Drawing.Size(85, 17);
         this.toolStripStatusLabelMemoryCache.Text = "MemoryCache";
         // 
         // toolStripStatusLabelCurrentPosition
         // 
         this.toolStripStatusLabelCurrentPosition.Name = "toolStripStatusLabelCurrentPosition";
         this.toolStripStatusLabelCurrentPosition.Size = new System.Drawing.Size(90, 17);
         this.toolStripStatusLabelCurrentPosition.Text = "CurrentPosition";
         // 
         // toolStripStatusLabelLoading
         // 
         this.toolStripStatusLabelLoading.Name = "toolStripStatusLabelLoading";
         this.toolStripStatusLabelLoading.Size = new System.Drawing.Size(53, 17);
         this.toolStripStatusLabelLoading.Text = "Loading:";
         // 
         // progressBar1
         // 
         this.progressBar1.Name = "progressBar1";
         this.progressBar1.Size = new System.Drawing.Size(100, 16);
         this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
         // 
         // groupBox6
         // 
         this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox6.Controls.Add(this.radioButtonNone);
         this.groupBox6.Controls.Add(this.radioButtonPerf);
         this.groupBox6.Controls.Add(this.radioButtonTransport);
         this.groupBox6.Location = new System.Drawing.Point(669, 519);
         this.groupBox6.Name = "groupBox6";
         this.groupBox6.Size = new System.Drawing.Size(169, 67);
         this.groupBox6.TabIndex = 39;
         this.groupBox6.TabStop = false;
         this.groupBox6.Text = "RealTime";
         // 
         // radioButtonNone
         // 
         this.radioButtonNone.AutoSize = true;
         this.radioButtonNone.Checked = true;
         this.radioButtonNone.Location = new System.Drawing.Point(112, 19);
         this.radioButtonNone.Name = "radioButtonNone";
         this.radioButtonNone.Size = new System.Drawing.Size(51, 17);
         this.radioButtonNone.TabIndex = 2;
         this.radioButtonNone.TabStop = true;
         this.radioButtonNone.Text = "None";
         this.radioButtonNone.UseVisualStyleBackColor = true;
         this.radioButtonNone.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
         // 
         // radioButtonPerf
         // 
         this.radioButtonPerf.AutoSize = true;
         this.radioButtonPerf.Location = new System.Drawing.Point(6, 42);
         this.radioButtonPerf.Name = "radioButtonPerf";
         this.radioButtonPerf.Size = new System.Drawing.Size(105, 17);
         this.radioButtonPerf.TabIndex = 1;
         this.radioButtonPerf.Text = "Performance test";
         this.radioButtonPerf.UseVisualStyleBackColor = true;
         this.radioButtonPerf.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
         // 
         // radioButtonTransport
         // 
         this.radioButtonTransport.AutoSize = true;
         this.radioButtonTransport.Location = new System.Drawing.Point(6, 19);
         this.radioButtonTransport.Name = "radioButtonTransport";
         this.radioButtonTransport.Size = new System.Drawing.Size(99, 17);
         this.radioButtonTransport.TabIndex = 0;
         this.radioButtonTransport.Text = "Transport demo";
         this.radioButtonTransport.UseVisualStyleBackColor = true;
         this.radioButtonTransport.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
         // 
         // MainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.AliceBlue;
         this.ClientSize = new System.Drawing.Size(849, 713);
         this.Controls.Add(this.groupBox6);
         this.Controls.Add(this.statusStrip1);
         this.Controls.Add(this.groupBox8);
         this.Controls.Add(this.groupBox7);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.groupBox5);
         this.Controls.Add(this.groupBox3);
         this.Controls.Add(this.groupBox2);
         this.Controls.Add(this.groupMap);
         this.KeyPreview = true;
         this.MinimumSize = new System.Drawing.Size(554, 110);
         this.Name = "MainForm";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "GMap.NET - Great Maps for Windows Forms";
         this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
         this.Load += new System.EventHandler(this.MainForm_Load);
         this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
         this.groupMap.ResumeLayout(false);
         this.groupBox3.ResumeLayout(false);
         this.groupBox3.PerformLayout();
         ((System.ComponentModel.ISupportInitialize) (this.trackBar1)).EndInit();
         this.groupBox2.ResumeLayout(false);
         this.groupBox2.PerformLayout();
         this.groupBox5.ResumeLayout(false);
         this.groupBox5.PerformLayout();
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.groupBox7.ResumeLayout(false);
         this.groupBox7.PerformLayout();
         this.groupBox8.ResumeLayout(false);
         this.groupBox8.PerformLayout();
         this.statusStrip1.ResumeLayout(false);
         this.statusStrip1.PerformLayout();
         this.groupBox6.ResumeLayout(false);
         this.groupBox6.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.GroupBox groupMap;
      private System.Windows.Forms.GroupBox groupBox3;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox textBoxLng;
      private System.Windows.Forms.TextBox textBoxLat;
      private System.Windows.Forms.Button button8;
      private Map MainMap;
      private System.Windows.Forms.ComboBox comboBoxMapType;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.TrackBar trackBar1;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.GroupBox groupBox5;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Button button2;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.TextBox textBoxGeo;
      private System.Windows.Forms.Button button3;
      private System.Windows.Forms.Button button4;
      private System.Windows.Forms.Button button5;
      private System.Windows.Forms.GroupBox groupBox7;
      private System.Windows.Forms.Button button6;
      private System.Windows.Forms.CheckBox checkBoxUseGeoCache;
      private System.Windows.Forms.CheckBox checkBoxUseRouteCache;
      private System.Windows.Forms.GroupBox groupBox8;
      private System.Windows.Forms.CheckBox checkBoxCurrentMarker;
      private System.Windows.Forms.CheckBox checkBoxCanDrag;
      private System.Windows.Forms.Button buttonSetEnd;
      private System.Windows.Forms.Button buttonSetStart;
      private System.Windows.Forms.CheckBox checkBoxPlacemarkInfo;
      private System.Windows.Forms.Button button7;
      private System.Windows.Forms.Button button9;
      private System.Windows.Forms.Button button10;
      private System.Windows.Forms.Button button11;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.ComboBox comboBoxMode;
      private System.Windows.Forms.Button button12;
      private System.Windows.Forms.CheckBox checkBoxDebug;
      private System.Windows.Forms.Button button13;
      private System.Windows.Forms.Button button14;
      private System.Windows.Forms.DateTimePicker dateTimePickerMobileLog;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Button button15;
      private System.Windows.Forms.StatusStrip statusStrip1;
      private System.Windows.Forms.ToolStripProgressBar progressBar1;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCurrentPosition;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLoading;
      private System.Windows.Forms.GroupBox groupBox6;
      private System.Windows.Forms.RadioButton radioButtonNone;
      private System.Windows.Forms.RadioButton radioButtonPerf;
      private System.Windows.Forms.RadioButton radioButtonTransport;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelMemoryCache;
   }
}

