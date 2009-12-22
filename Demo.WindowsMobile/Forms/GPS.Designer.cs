namespace Demo.WindowsMobile
{
   partial class GPS
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
         this.status = new System.Windows.Forms.Label();
         this.splitter1 = new System.Windows.Forms.Splitter();
         this.panelSignals = new System.Windows.Forms.Panel();
         this.SuspendLayout();
         // 
         // status
         // 
         this.status.BackColor = System.Drawing.Color.Navy;
         this.status.Dock = System.Windows.Forms.DockStyle.Top;
         this.status.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
         this.status.ForeColor = System.Drawing.Color.Lime;
         this.status.Location = new System.Drawing.Point(0, 0);
         this.status.Name = "status";
         this.status.Size = new System.Drawing.Size(391, 417);
         this.status.Text = "Loading...";
         // 
         // splitter1
         // 
         this.splitter1.BackColor = System.Drawing.Color.Black;
         this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
         this.splitter1.Location = new System.Drawing.Point(0, 417);
         this.splitter1.MinExtra = 0;
         this.splitter1.MinSize = 0;
         this.splitter1.Name = "splitter1";
         this.splitter1.Size = new System.Drawing.Size(391, 11);
         // 
         // panelSignals
         // 
         this.panelSignals.BackColor = System.Drawing.Color.DarkBlue;
         this.panelSignals.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panelSignals.Location = new System.Drawing.Point(0, 428);
         this.panelSignals.Name = "panelSignals";
         this.panelSignals.Size = new System.Drawing.Size(391, 103);
         this.panelSignals.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSignals_Paint);
         // 
         // GPS
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
         this.Controls.Add(this.panelSignals);
         this.Controls.Add(this.splitter1);
         this.Controls.Add(this.status);
         this.Name = "GPS";
         this.Size = new System.Drawing.Size(391, 531);
         this.Resize += new System.EventHandler(this.GPS_Resize);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Splitter splitter1;
      internal System.Windows.Forms.Label status;
      internal System.Windows.Forms.Panel panelSignals;
   }
}
