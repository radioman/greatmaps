namespace CloudsDemo
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
			if (disposing && (components != null))
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
           this.mapControl = new CloudsDemo.MapControl();
           this.SuspendLayout();
           // 
           // mapControl
           // 
           this.mapControl.CanDragMap = true;
           this.mapControl.Dock = System.Windows.Forms.DockStyle.Fill;
           this.mapControl.GrayScaleMode = false;
           this.mapControl.LevelsKeepInMemmory = 5;
           this.mapControl.Location = new System.Drawing.Point(0, 0);
           this.mapControl.Margin = new System.Windows.Forms.Padding(5);
           this.mapControl.MarkersEnabled = true;
           this.mapControl.MaxZoom = 17;
           this.mapControl.MinZoom = 2;
           this.mapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
           this.mapControl.Name = "mapControl";
           this.mapControl.PolygonsEnabled = true;
           this.mapControl.RetryLoadTile = 0;
           this.mapControl.RoutesEnabled = true;
           this.mapControl.ShowTileGridLines = false;
           this.mapControl.Size = new System.Drawing.Size(876, 665);
           this.mapControl.TabIndex = 0;
           this.mapControl.Zoom = 0D;
           // 
           // MainForm
           // 
           this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
           this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
           this.ClientSize = new System.Drawing.Size(876, 665);
           this.Controls.Add(this.mapControl);
           this.Margin = new System.Windows.Forms.Padding(4);
           this.Name = "MainForm";
           this.Text = "Clouds Demo - shows how to render image marker";
           this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
           this.ResumeLayout(false);

		}

		#endregion

		private MapControl mapControl;
	}
}

