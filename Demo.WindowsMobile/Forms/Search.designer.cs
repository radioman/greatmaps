namespace Demo.WindowsMobile
{
   partial class Search
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
         this.textAddress = new System.Windows.Forms.TextBox();
         this.labelstatus = new System.Windows.Forms.Label();
         this.button1 = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // textAddress
         // 
         this.textAddress.BackColor = System.Drawing.Color.Navy;
         this.textAddress.Dock = System.Windows.Forms.DockStyle.Top;
         this.textAddress.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Regular);
         this.textAddress.ForeColor = System.Drawing.Color.AliceBlue;
         this.textAddress.Location = new System.Drawing.Point(0, 0);
         this.textAddress.Multiline = true;
         this.textAddress.Name = "textAddress";
         this.textAddress.Size = new System.Drawing.Size(239, 199);
         this.textAddress.TabIndex = 4;
         this.textAddress.Text = "Lithuania, Vilnius";
         // 
         // labelstatus
         // 
         this.labelstatus.Dock = System.Windows.Forms.DockStyle.Fill;
         this.labelstatus.Location = new System.Drawing.Point(0, 199);
         this.labelstatus.Name = "labelstatus";
         this.labelstatus.Size = new System.Drawing.Size(239, 127);
         // 
         // button1
         // 
         this.button1.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.button1.BackColor = System.Drawing.Color.Blue;
         this.button1.ForeColor = System.Drawing.Color.White;
         this.button1.Location = new System.Drawing.Point(19, 242);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(197, 53);
         this.button1.TabIndex = 7;
         this.button1.Text = "Search Address";
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // Search
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
         this.Controls.Add(this.button1);
         this.Controls.Add(this.labelstatus);
         this.Controls.Add(this.textAddress);
         this.Name = "Search";
         this.Size = new System.Drawing.Size(239, 326);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TextBox textAddress;
      private System.Windows.Forms.Label labelstatus;
      private System.Windows.Forms.Button button1;

   }
}
