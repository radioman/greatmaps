namespace Demo.WindowsMobile
{
   partial class Transport
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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.textBoxTrolley = new System.Windows.Forms.TextBox();
         this.checkBoxTrolley = new System.Windows.Forms.CheckBox();
         this.checkBoxBus = new System.Windows.Forms.CheckBox();
         this.textBoxBus = new System.Windows.Forms.TextBox();
         this.checkBoxRefresh = new System.Windows.Forms.CheckBox();
         this.timerRefresh = new System.Windows.Forms.Timer();
         this.labelstatus = new System.Windows.Forms.Label();
         this.button1 = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // textBoxTrolley
         // 
         this.textBoxTrolley.BackColor = System.Drawing.Color.Navy;
         this.textBoxTrolley.Dock = System.Windows.Forms.DockStyle.Top;
         this.textBoxTrolley.Font = new System.Drawing.Font("Tahoma", 26F, System.Drawing.FontStyle.Regular);
         this.textBoxTrolley.ForeColor = System.Drawing.Color.AliceBlue;
         this.textBoxTrolley.Location = new System.Drawing.Point(0, 48);
         this.textBoxTrolley.Name = "textBoxTrolley";
         this.textBoxTrolley.Size = new System.Drawing.Size(310, 59);
         this.textBoxTrolley.TabIndex = 0;
         this.textBoxTrolley.Text = "13,17";
         // 
         // checkBoxTrolley
         // 
         this.checkBoxTrolley.Checked = true;
         this.checkBoxTrolley.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxTrolley.Dock = System.Windows.Forms.DockStyle.Top;
         this.checkBoxTrolley.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular);
         this.checkBoxTrolley.Location = new System.Drawing.Point(0, 0);
         this.checkBoxTrolley.Name = "checkBoxTrolley";
         this.checkBoxTrolley.Size = new System.Drawing.Size(310, 48);
         this.checkBoxTrolley.TabIndex = 2;
         this.checkBoxTrolley.Text = "Trolley";
         this.checkBoxTrolley.CheckStateChanged += new System.EventHandler(this.checkBoxTrolley_CheckStateChanged);
         // 
         // checkBoxBus
         // 
         this.checkBoxBus.Dock = System.Windows.Forms.DockStyle.Top;
         this.checkBoxBus.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular);
         this.checkBoxBus.Location = new System.Drawing.Point(0, 107);
         this.checkBoxBus.Name = "checkBoxBus";
         this.checkBoxBus.Size = new System.Drawing.Size(310, 48);
         this.checkBoxBus.TabIndex = 5;
         this.checkBoxBus.Text = "Bus";
         this.checkBoxBus.CheckStateChanged += new System.EventHandler(this.checkBoxBus_CheckStateChanged);
         // 
         // textBoxBus
         // 
         this.textBoxBus.BackColor = System.Drawing.Color.Navy;
         this.textBoxBus.Dock = System.Windows.Forms.DockStyle.Top;
         this.textBoxBus.Font = new System.Drawing.Font("Tahoma", 26F, System.Drawing.FontStyle.Regular);
         this.textBoxBus.ForeColor = System.Drawing.Color.AliceBlue;
         this.textBoxBus.Location = new System.Drawing.Point(0, 155);
         this.textBoxBus.Name = "textBoxBus";
         this.textBoxBus.Size = new System.Drawing.Size(310, 59);
         this.textBoxBus.TabIndex = 4;
         this.textBoxBus.Text = "50";
         // 
         // checkBoxRefresh
         // 
         this.checkBoxRefresh.BackColor = System.Drawing.Color.Navy;
         this.checkBoxRefresh.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.checkBoxRefresh.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular);
         this.checkBoxRefresh.ForeColor = System.Drawing.Color.Lime;
         this.checkBoxRefresh.Location = new System.Drawing.Point(0, 388);
         this.checkBoxRefresh.Name = "checkBoxRefresh";
         this.checkBoxRefresh.Size = new System.Drawing.Size(310, 59);
         this.checkBoxRefresh.TabIndex = 6;
         this.checkBoxRefresh.Text = "AutoRefresh 15s";
         this.checkBoxRefresh.CheckStateChanged += new System.EventHandler(this.checkBoxRefresh_CheckStateChanged);
         // 
         // timerRefresh
         // 
         this.timerRefresh.Interval = 15000;
         this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
         // 
         // labelstatus
         // 
         this.labelstatus.Dock = System.Windows.Forms.DockStyle.Fill;
         this.labelstatus.Location = new System.Drawing.Point(0, 214);
         this.labelstatus.Name = "labelstatus";
         this.labelstatus.Size = new System.Drawing.Size(310, 174);
         // 
         // button1
         // 
         this.button1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.button1.BackColor = System.Drawing.Color.Blue;
         this.button1.ForeColor = System.Drawing.Color.White;
         this.button1.Location = new System.Drawing.Point(237, 391);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(70, 53);
         this.button1.TabIndex = 7;
         this.button1.Text = "Manual";
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // Transport
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
         this.Controls.Add(this.button1);
         this.Controls.Add(this.labelstatus);
         this.Controls.Add(this.textBoxBus);
         this.Controls.Add(this.checkBoxBus);
         this.Controls.Add(this.textBoxTrolley);
         this.Controls.Add(this.checkBoxTrolley);
         this.Controls.Add(this.checkBoxRefresh);
         this.Name = "Transport";
         this.Size = new System.Drawing.Size(310, 447);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TextBox textBoxTrolley;
      private System.Windows.Forms.CheckBox checkBoxTrolley;
      private System.Windows.Forms.CheckBox checkBoxBus;
      private System.Windows.Forms.TextBox textBoxBus;
      private System.Windows.Forms.CheckBox checkBoxRefresh;
      private System.Windows.Forms.Timer timerRefresh;
      private System.Windows.Forms.Label labelstatus;
      private System.Windows.Forms.Button button1;

   }
}
