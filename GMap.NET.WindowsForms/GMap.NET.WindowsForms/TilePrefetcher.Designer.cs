namespace GMap.NET
{
   partial class TilePrefetcher
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
         this.progressBar1 = new System.Windows.Forms.ProgressBar();
         this.label1 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // progressBar1
         // 
         this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.progressBar1.Location = new System.Drawing.Point(21, 42);
         this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
         this.progressBar1.Name = "progressBar1";
         this.progressBar1.Size = new System.Drawing.Size(508, 28);
         this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
         this.progressBar1.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(21, 16);
         this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(46, 17);
         this.label1.TabIndex = 1;
         this.label1.Text = "label1";
         // 
         // TilePrefetcher
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.AliceBlue;
         this.ClientSize = new System.Drawing.Size(550, 90);
         this.ControlBox = false;
         this.Controls.Add(this.label1);
         this.Controls.Add(this.progressBar1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.KeyPreview = true;
         this.Margin = new System.Windows.Forms.Padding(4);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "TilePrefetcher";
         this.Padding = new System.Windows.Forms.Padding(5);
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "GMap.NET - esc to cancel fetching";
         this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Prefetch_FormClosed);
         this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Prefetch_PreviewKeyDown);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ProgressBar progressBar1;
      private System.Windows.Forms.Label label1;
   }
}