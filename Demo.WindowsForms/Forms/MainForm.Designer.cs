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
         this.groupBox4 = new System.Windows.Forms.GroupBox();
         this.MainMap = new System.Windows.Forms.GMap();
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
         this.checkBoxCanDrag = new System.Windows.Forms.CheckBox();
         this.checkBoxCurrentMarker = new System.Windows.Forms.CheckBox();
         this.label3 = new System.Windows.Forms.Label();
         this.comboBoxRenderType = new System.Windows.Forms.ComboBox();
         this.label13 = new System.Windows.Forms.Label();
         this.label14 = new System.Windows.Forms.Label();
         this.textBoxCurrLng = new System.Windows.Forms.TextBox();
         this.textBoxCurrLat = new System.Windows.Forms.TextBox();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.button9 = new System.Windows.Forms.Button();
         this.checkBoxUseGeoCache = new System.Windows.Forms.CheckBox();
         this.checkBoxUseRouteCache = new System.Windows.Forms.CheckBox();
         this.button2 = new System.Windows.Forms.Button();
         this.checkBoxUseTileCache = new System.Windows.Forms.CheckBox();
         this.button3 = new System.Windows.Forms.Button();
         this.groupBox6 = new System.Windows.Forms.GroupBox();
         this.groupBox7 = new System.Windows.Forms.GroupBox();
         this.buttonSetEnd = new System.Windows.Forms.Button();
         this.buttonSetStart = new System.Windows.Forms.Button();
         this.label5 = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.button6 = new System.Windows.Forms.Button();
         this.groupBoxLoading = new System.Windows.Forms.GroupBox();
         this.progressBar3 = new System.Windows.Forms.ProgressBar();
         this.progressBar2 = new System.Windows.Forms.ProgressBar();
         this.progressBar1 = new System.Windows.Forms.ProgressBar();
         this.groupBox8 = new System.Windows.Forms.GroupBox();
         this.button7 = new System.Windows.Forms.Button();
         this.checkBoxPlacemarkInfo = new System.Windows.Forms.CheckBox();
         this.button10 = new System.Windows.Forms.Button();
         this.groupBox4.SuspendLayout();
         this.groupBox3.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize) (this.trackBar1)).BeginInit();
         this.groupBox2.SuspendLayout();
         this.groupBox5.SuspendLayout();
         this.groupBox1.SuspendLayout();
         this.groupBox6.SuspendLayout();
         this.groupBox7.SuspendLayout();
         this.groupBoxLoading.SuspendLayout();
         this.groupBox8.SuspendLayout();
         this.SuspendLayout();
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(132, 49);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(27, 13);
         this.label7.TabIndex = 31;
         this.label7.Text = "type";
         // 
         // comboBoxMapType
         // 
         this.comboBoxMapType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxMapType.FormattingEnabled = true;
         this.comboBoxMapType.Location = new System.Drawing.Point(6, 46);
         this.comboBoxMapType.Name = "comboBoxMapType";
         this.comboBoxMapType.Size = new System.Drawing.Size(125, 21);
         this.comboBoxMapType.TabIndex = 9;
         this.comboBoxMapType.DropDownClosed += new System.EventHandler(this.comboBoxMapType_DropDownClosed);
         // 
         // groupBox4
         // 
         this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox4.Controls.Add(this.MainMap);
         this.groupBox4.Location = new System.Drawing.Point(12, 7);
         this.groupBox4.Name = "groupBox4";
         this.groupBox4.Size = new System.Drawing.Size(695, 627);
         this.groupBox4.TabIndex = 27;
         this.groupBox4.TabStop = false;
         this.groupBox4.Text = "gmap";
         // 
         // MainMap
         // 
         this.MainMap.BackColor = System.Drawing.SystemColors.AppWorkspace;
         this.MainMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.MainMap.CanDragMap = true;
         this.MainMap.CurrentMarkerEnabled = true;
         this.MainMap.CurrentMarkerStyle = GMapNET.CurrentMarkerType.GMap;
         this.MainMap.Dock = System.Windows.Forms.DockStyle.Fill;
         this.MainMap.ForeColor = System.Drawing.SystemColors.ControlText;
         this.MainMap.Location = new System.Drawing.Point(3, 16);
         this.MainMap.MapType = GMapNET.GMapType.GoogleMap;
         this.MainMap.MarkersEnabled = true;
         this.MainMap.Name = "MainMap";
         this.MainMap.RenderMode = GMapNET.RenderMode.GDI_PLUS;
         this.MainMap.RoutesEnabled = true;
         this.MainMap.Size = new System.Drawing.Size(689, 608);
         this.MainMap.TabIndex = 0;
         this.MainMap.TooltipFont = new System.Drawing.Font("Microsoft Sans Serif", 11F);
         this.MainMap.TooltipTextPadding = new System.Drawing.Size(10, 10);
         this.MainMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainMap_MouseDown);
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
         this.groupBox3.Location = new System.Drawing.Point(767, 7);
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
         this.button5.Enabled = false;
         this.button5.Location = new System.Drawing.Point(94, 45);
         this.button5.Name = "button5";
         this.button5.Size = new System.Drawing.Size(65, 24);
         this.button5.TabIndex = 13;
         this.button5.Text = "Clear All";
         this.button5.UseVisualStyleBackColor = true;
         this.button5.Click += new System.EventHandler(this.button5_Click);
         // 
         // button4
         // 
         this.button4.Enabled = false;
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
         this.trackBar1.Size = new System.Drawing.Size(42, 608);
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
         this.groupBox2.Location = new System.Drawing.Point(713, 7);
         this.groupBox2.Name = "groupBox2";
         this.groupBox2.Size = new System.Drawing.Size(48, 627);
         this.groupBox2.TabIndex = 30;
         this.groupBox2.TabStop = false;
         this.groupBox2.Text = "zoom";
         // 
         // groupBox5
         // 
         this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox5.Controls.Add(this.checkBoxCanDrag);
         this.groupBox5.Controls.Add(this.checkBoxCurrentMarker);
         this.groupBox5.Controls.Add(this.label3);
         this.groupBox5.Controls.Add(this.comboBoxRenderType);
         this.groupBox5.Controls.Add(this.label7);
         this.groupBox5.Controls.Add(this.comboBoxMapType);
         this.groupBox5.Location = new System.Drawing.Point(767, 141);
         this.groupBox5.Name = "groupBox5";
         this.groupBox5.Size = new System.Drawing.Size(169, 116);
         this.groupBox5.TabIndex = 31;
         this.groupBox5.TabStop = false;
         this.groupBox5.Text = "gmap";
         // 
         // checkBoxCanDrag
         // 
         this.checkBoxCanDrag.AutoSize = true;
         this.checkBoxCanDrag.Checked = true;
         this.checkBoxCanDrag.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxCanDrag.Location = new System.Drawing.Point(6, 96);
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
         this.checkBoxCurrentMarker.Location = new System.Drawing.Point(6, 73);
         this.checkBoxCurrentMarker.Name = "checkBoxCurrentMarker";
         this.checkBoxCurrentMarker.Size = new System.Drawing.Size(126, 17);
         this.checkBoxCurrentMarker.TabIndex = 35;
         this.checkBoxCurrentMarker.Text = "Show Current Marker";
         this.checkBoxCurrentMarker.UseVisualStyleBackColor = true;
         this.checkBoxCurrentMarker.CheckedChanged += new System.EventHandler(this.checkBoxCurrentMarker_CheckedChanged);
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(132, 22);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(30, 13);
         this.label3.TabIndex = 34;
         this.label3.Text = "paint";
         // 
         // comboBoxRenderType
         // 
         this.comboBoxRenderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxRenderType.FormattingEnabled = true;
         this.comboBoxRenderType.Location = new System.Drawing.Point(6, 19);
         this.comboBoxRenderType.Name = "comboBoxRenderType";
         this.comboBoxRenderType.Size = new System.Drawing.Size(125, 21);
         this.comboBoxRenderType.TabIndex = 33;
         this.comboBoxRenderType.DropDownClosed += new System.EventHandler(this.comboBoxRenderType_DropDownClosed);
         // 
         // label13
         // 
         this.label13.AutoSize = true;
         this.label13.Location = new System.Drawing.Point(132, 48);
         this.label13.Name = "label13";
         this.label13.Size = new System.Drawing.Size(21, 13);
         this.label13.TabIndex = 11;
         this.label13.Text = "lng";
         // 
         // label14
         // 
         this.label14.AutoSize = true;
         this.label14.Location = new System.Drawing.Point(132, 22);
         this.label14.Name = "label14";
         this.label14.Size = new System.Drawing.Size(18, 13);
         this.label14.TabIndex = 10;
         this.label14.Text = "lat";
         // 
         // textBoxCurrLng
         // 
         this.textBoxCurrLng.Location = new System.Drawing.Point(6, 45);
         this.textBoxCurrLng.Name = "textBoxCurrLng";
         this.textBoxCurrLng.ReadOnly = true;
         this.textBoxCurrLng.Size = new System.Drawing.Size(125, 20);
         this.textBoxCurrLng.TabIndex = 9;
         // 
         // textBoxCurrLat
         // 
         this.textBoxCurrLat.Location = new System.Drawing.Point(6, 19);
         this.textBoxCurrLat.Name = "textBoxCurrLat";
         this.textBoxCurrLat.ReadOnly = true;
         this.textBoxCurrLat.Size = new System.Drawing.Size(125, 20);
         this.textBoxCurrLat.TabIndex = 8;
         // 
         // groupBox1
         // 
         this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox1.Controls.Add(this.button10);
         this.groupBox1.Controls.Add(this.button9);
         this.groupBox1.Controls.Add(this.checkBoxUseGeoCache);
         this.groupBox1.Controls.Add(this.checkBoxUseRouteCache);
         this.groupBox1.Controls.Add(this.button2);
         this.groupBox1.Controls.Add(this.checkBoxUseTileCache);
         this.groupBox1.Location = new System.Drawing.Point(767, 515);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(169, 65);
         this.groupBox1.TabIndex = 32;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "cache";
         // 
         // button9
         // 
         this.button9.Location = new System.Drawing.Point(63, 39);
         this.button9.Name = "button9";
         this.button9.Size = new System.Drawing.Size(47, 20);
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
         this.checkBoxUseGeoCache.Location = new System.Drawing.Point(122, 16);
         this.checkBoxUseGeoCache.Name = "checkBoxUseGeoCache";
         this.checkBoxUseGeoCache.Size = new System.Drawing.Size(38, 17);
         this.checkBoxUseGeoCache.TabIndex = 3;
         this.checkBoxUseGeoCache.Text = "gp";
         this.checkBoxUseGeoCache.UseVisualStyleBackColor = true;
         this.checkBoxUseGeoCache.CheckedChanged += new System.EventHandler(this.checkBoxUseCache_CheckedChanged);
         // 
         // checkBoxUseRouteCache
         // 
         this.checkBoxUseRouteCache.AutoSize = true;
         this.checkBoxUseRouteCache.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBoxUseRouteCache.Checked = true;
         this.checkBoxUseRouteCache.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxUseRouteCache.Location = new System.Drawing.Point(58, 16);
         this.checkBoxUseRouteCache.Name = "checkBoxUseRouteCache";
         this.checkBoxUseRouteCache.Size = new System.Drawing.Size(58, 17);
         this.checkBoxUseRouteCache.TabIndex = 2;
         this.checkBoxUseRouteCache.Text = "routing";
         this.checkBoxUseRouteCache.UseVisualStyleBackColor = true;
         this.checkBoxUseRouteCache.CheckedChanged += new System.EventHandler(this.checkBoxUseCache_CheckedChanged);
         // 
         // button2
         // 
         this.button2.Location = new System.Drawing.Point(6, 39);
         this.button2.Name = "button2";
         this.button2.Size = new System.Drawing.Size(53, 20);
         this.button2.TabIndex = 0;
         this.button2.Text = "Clear All";
         this.button2.UseVisualStyleBackColor = true;
         this.button2.Click += new System.EventHandler(this.button2_Click);
         // 
         // checkBoxUseTileCache
         // 
         this.checkBoxUseTileCache.AutoSize = true;
         this.checkBoxUseTileCache.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBoxUseTileCache.Checked = true;
         this.checkBoxUseTileCache.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxUseTileCache.Location = new System.Drawing.Point(6, 16);
         this.checkBoxUseTileCache.Name = "checkBoxUseTileCache";
         this.checkBoxUseTileCache.Size = new System.Drawing.Size(46, 17);
         this.checkBoxUseTileCache.TabIndex = 1;
         this.checkBoxUseTileCache.Text = "map";
         this.checkBoxUseTileCache.UseVisualStyleBackColor = true;
         this.checkBoxUseTileCache.CheckedChanged += new System.EventHandler(this.checkBoxUseCache_CheckedChanged);
         // 
         // button3
         // 
         this.button3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button3.Enabled = false;
         this.button3.Location = new System.Drawing.Point(6, 54);
         this.button3.Name = "button3";
         this.button3.Size = new System.Drawing.Size(75, 24);
         this.button3.TabIndex = 33;
         this.button3.Text = "Add Route";
         this.button3.UseVisualStyleBackColor = true;
         this.button3.Click += new System.EventHandler(this.button3_Click);
         // 
         // groupBox6
         // 
         this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox6.Controls.Add(this.textBoxCurrLat);
         this.groupBox6.Controls.Add(this.textBoxCurrLng);
         this.groupBox6.Controls.Add(this.label14);
         this.groupBox6.Controls.Add(this.label13);
         this.groupBox6.Location = new System.Drawing.Point(767, 436);
         this.groupBox6.Name = "groupBox6";
         this.groupBox6.Size = new System.Drawing.Size(169, 73);
         this.groupBox6.TabIndex = 34;
         this.groupBox6.TabStop = false;
         this.groupBox6.Text = "current location";
         // 
         // groupBox7
         // 
         this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox7.Controls.Add(this.buttonSetEnd);
         this.groupBox7.Controls.Add(this.buttonSetStart);
         this.groupBox7.Controls.Add(this.label5);
         this.groupBox7.Controls.Add(this.label4);
         this.groupBox7.Controls.Add(this.button6);
         this.groupBox7.Controls.Add(this.button3);
         this.groupBox7.Location = new System.Drawing.Point(767, 263);
         this.groupBox7.Name = "groupBox7";
         this.groupBox7.Size = new System.Drawing.Size(169, 84);
         this.groupBox7.TabIndex = 35;
         this.groupBox7.TabStop = false;
         this.groupBox7.Text = "routing";
         // 
         // buttonSetEnd
         // 
         this.buttonSetEnd.Enabled = false;
         this.buttonSetEnd.Location = new System.Drawing.Point(113, 19);
         this.buttonSetEnd.Name = "buttonSetEnd";
         this.buttonSetEnd.Size = new System.Drawing.Size(47, 27);
         this.buttonSetEnd.TabIndex = 42;
         this.buttonSetEnd.Text = "set";
         this.buttonSetEnd.UseVisualStyleBackColor = true;
         this.buttonSetEnd.Click += new System.EventHandler(this.buttonSetEnd_Click);
         // 
         // buttonSetStart
         // 
         this.buttonSetStart.Enabled = false;
         this.buttonSetStart.Location = new System.Drawing.Point(34, 19);
         this.buttonSetStart.Name = "buttonSetStart";
         this.buttonSetStart.Size = new System.Drawing.Size(47, 27);
         this.buttonSetStart.TabIndex = 41;
         this.buttonSetStart.Text = "set";
         this.buttonSetStart.UseVisualStyleBackColor = true;
         this.buttonSetStart.Click += new System.EventHandler(this.buttonSetStart_Click);
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(91, 26);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(25, 13);
         this.label5.TabIndex = 38;
         this.label5.Text = "end";
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(10, 26);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(27, 13);
         this.label4.TabIndex = 36;
         this.label4.Text = "start";
         // 
         // button6
         // 
         this.button6.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button6.Enabled = false;
         this.button6.Location = new System.Drawing.Point(85, 54);
         this.button6.Name = "button6";
         this.button6.Size = new System.Drawing.Size(75, 24);
         this.button6.TabIndex = 34;
         this.button6.Text = "Clear All";
         this.button6.UseVisualStyleBackColor = true;
         this.button6.Click += new System.EventHandler(this.button6_Click);
         // 
         // groupBoxLoading
         // 
         this.groupBoxLoading.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBoxLoading.Controls.Add(this.progressBar3);
         this.groupBoxLoading.Controls.Add(this.progressBar2);
         this.groupBoxLoading.Controls.Add(this.progressBar1);
         this.groupBoxLoading.Location = new System.Drawing.Point(767, 586);
         this.groupBoxLoading.Name = "groupBoxLoading";
         this.groupBoxLoading.Size = new System.Drawing.Size(169, 48);
         this.groupBoxLoading.TabIndex = 36;
         this.groupBoxLoading.TabStop = false;
         this.groupBoxLoading.Text = "loading";
         // 
         // progressBar3
         // 
         this.progressBar3.Location = new System.Drawing.Point(110, 19);
         this.progressBar3.Name = "progressBar3";
         this.progressBar3.Size = new System.Drawing.Size(53, 20);
         this.progressBar3.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
         this.progressBar3.TabIndex = 2;
         this.progressBar3.Visible = false;
         // 
         // progressBar2
         // 
         this.progressBar2.Location = new System.Drawing.Point(59, 19);
         this.progressBar2.Name = "progressBar2";
         this.progressBar2.Size = new System.Drawing.Size(53, 20);
         this.progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
         this.progressBar2.TabIndex = 1;
         this.progressBar2.Visible = false;
         // 
         // progressBar1
         // 
         this.progressBar1.Location = new System.Drawing.Point(6, 19);
         this.progressBar1.Name = "progressBar1";
         this.progressBar1.Size = new System.Drawing.Size(53, 20);
         this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
         this.progressBar1.TabIndex = 0;
         this.progressBar1.Visible = false;
         // 
         // groupBox8
         // 
         this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox8.Controls.Add(this.button7);
         this.groupBox8.Controls.Add(this.checkBoxPlacemarkInfo);
         this.groupBox8.Controls.Add(this.button5);
         this.groupBox8.Controls.Add(this.button4);
         this.groupBox8.Location = new System.Drawing.Point(767, 353);
         this.groupBox8.Name = "groupBox8";
         this.groupBox8.Size = new System.Drawing.Size(169, 74);
         this.groupBox8.TabIndex = 37;
         this.groupBox8.TabStop = false;
         this.groupBox8.Text = "markers";
         // 
         // button7
         // 
         this.button7.Enabled = false;
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
         this.checkBoxPlacemarkInfo.Enabled = false;
         this.checkBoxPlacemarkInfo.Location = new System.Drawing.Point(94, 19);
         this.checkBoxPlacemarkInfo.Name = "checkBoxPlacemarkInfo";
         this.checkBoxPlacemarkInfo.Size = new System.Drawing.Size(72, 17);
         this.checkBoxPlacemarkInfo.TabIndex = 14;
         this.checkBoxPlacemarkInfo.Text = "place info";
         this.checkBoxPlacemarkInfo.UseVisualStyleBackColor = true;
         // 
         // button10
         // 
         this.button10.Location = new System.Drawing.Point(116, 39);
         this.button10.Name = "button10";
         this.button10.Size = new System.Drawing.Size(47, 20);
         this.button10.TabIndex = 5;
         this.button10.Text = "Import";
         this.button10.UseVisualStyleBackColor = true;
         this.button10.Click += new System.EventHandler(this.button10_Click);
         // 
         // MainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.AliceBlue;
         this.ClientSize = new System.Drawing.Size(950, 646);
         this.Controls.Add(this.groupBox8);
         this.Controls.Add(this.groupBoxLoading);
         this.Controls.Add(this.groupBox7);
         this.Controls.Add(this.groupBox6);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.groupBox5);
         this.Controls.Add(this.groupBox3);
         this.Controls.Add(this.groupBox2);
         this.Controls.Add(this.groupBox4);
         this.MinimumSize = new System.Drawing.Size(522, 677);
         this.Name = "MainForm";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "GMap.NET - Great Maps for Windows Forms";
         this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
         this.Shown += new System.EventHandler(this.MainForm_Shown);
         this.groupBox4.ResumeLayout(false);
         this.groupBox3.ResumeLayout(false);
         this.groupBox3.PerformLayout();
         ((System.ComponentModel.ISupportInitialize) (this.trackBar1)).EndInit();
         this.groupBox2.ResumeLayout(false);
         this.groupBox2.PerformLayout();
         this.groupBox5.ResumeLayout(false);
         this.groupBox5.PerformLayout();
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.groupBox6.ResumeLayout(false);
         this.groupBox6.PerformLayout();
         this.groupBox7.ResumeLayout(false);
         this.groupBox7.PerformLayout();
         this.groupBoxLoading.ResumeLayout(false);
         this.groupBox8.ResumeLayout(false);
         this.groupBox8.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.GroupBox groupBox4;
      private System.Windows.Forms.GroupBox groupBox3;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox textBoxLng;
      private System.Windows.Forms.TextBox textBoxLat;
      private System.Windows.Forms.Button button8;
      private System.Windows.Forms.GMap MainMap;
      private System.Windows.Forms.ComboBox comboBoxMapType;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.TrackBar trackBar1;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.GroupBox groupBox5;
      private System.Windows.Forms.Label label13;
      private System.Windows.Forms.Label label14;
      private System.Windows.Forms.TextBox textBoxCurrLng;
      private System.Windows.Forms.TextBox textBoxCurrLat;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.ComboBox comboBoxRenderType;
      private System.Windows.Forms.CheckBox checkBoxUseTileCache;
      private System.Windows.Forms.Button button2;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.TextBox textBoxGeo;
      private System.Windows.Forms.Button button3;
      private System.Windows.Forms.Button button4;
      private System.Windows.Forms.Button button5;
      private System.Windows.Forms.GroupBox groupBox6;
      private System.Windows.Forms.GroupBox groupBox7;
      private System.Windows.Forms.Button button6;
      private System.Windows.Forms.GroupBox groupBoxLoading;
      private System.Windows.Forms.ProgressBar progressBar1;
      private System.Windows.Forms.ProgressBar progressBar3;
      private System.Windows.Forms.ProgressBar progressBar2;
      private System.Windows.Forms.CheckBox checkBoxUseGeoCache;
      private System.Windows.Forms.CheckBox checkBoxUseRouteCache;
      private System.Windows.Forms.GroupBox groupBox8;
      private System.Windows.Forms.CheckBox checkBoxCurrentMarker;
      private System.Windows.Forms.CheckBox checkBoxCanDrag;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Button buttonSetEnd;
      private System.Windows.Forms.Button buttonSetStart;
      private System.Windows.Forms.CheckBox checkBoxPlacemarkInfo;
      private System.Windows.Forms.Button button7;
      private System.Windows.Forms.Button button9;
      private System.Windows.Forms.Button button10;
   }
}

