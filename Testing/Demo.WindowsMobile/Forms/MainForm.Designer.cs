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
         this.MainMap = new System.Windows.Forms.GMap();
         this.SuspendLayout();
         // 
         // gMap1
         // 
         this.MainMap.Dock = System.Windows.Forms.DockStyle.Fill;
         this.MainMap.Location = new System.Drawing.Point(0, 0);
         this.MainMap.Name = "gMap1";
         this.MainMap.Size = new System.Drawing.Size(240, 268);
         this.MainMap.TabIndex = 0;
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
         this.AutoScroll = true;
         this.ClientSize = new System.Drawing.Size(240, 268);
         this.Controls.Add(this.MainMap);
         this.Menu = this.mainMenu1;
         this.Name = "Form1";
         this.Text = "GMap.NET for Windows Mobile";
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.GMap MainMap;
   }
}

