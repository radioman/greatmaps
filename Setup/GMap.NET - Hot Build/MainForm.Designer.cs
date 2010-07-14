namespace GMap.NET___Hot_Build
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
         this.buttonForms = new System.Windows.Forms.Button();
         this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
         this.linkLabel1 = new System.Windows.Forms.LinkLabel();
         this.buttonPresentation = new System.Windows.Forms.Button();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.tableLayoutPanel1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
         this.SuspendLayout();
         // 
         // buttonForms
         // 
         this.buttonForms.Dock = System.Windows.Forms.DockStyle.Fill;
         this.buttonForms.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
         this.buttonForms.ForeColor = System.Drawing.Color.Navy;
         this.buttonForms.Location = new System.Drawing.Point(4, 4);
         this.buttonForms.Margin = new System.Windows.Forms.Padding(4);
         this.buttonForms.Name = "buttonForms";
         this.buttonForms.Size = new System.Drawing.Size(376, 129);
         this.buttonForms.TabIndex = 1;
         this.buttonForms.Text = "Demo.WindowsForms";
         this.buttonForms.UseVisualStyleBackColor = true;
         this.buttonForms.Click += new System.EventHandler(this.buttonForms_Click);
         this.buttonForms.MouseEnter += new System.EventHandler(this.button1_MouseEnter);
         this.buttonForms.MouseLeave += new System.EventHandler(this.button1_MouseLeave);
         // 
         // tableLayoutPanel1
         // 
         this.tableLayoutPanel1.ColumnCount = 3;
         this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
         this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 148F));
         this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
         this.tableLayoutPanel1.Controls.Add(this.buttonForms, 0, 0);
         this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 3);
         this.tableLayoutPanel1.Controls.Add(this.buttonPresentation, 2, 0);
         this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 1, 0);
         this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 6);
         this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
         this.tableLayoutPanel1.Name = "tableLayoutPanel1";
         this.tableLayoutPanel1.RowCount = 5;
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 108F));
         this.tableLayoutPanel1.Size = new System.Drawing.Size(917, 190);
         this.tableLayoutPanel1.TabIndex = 2;
         // 
         // linkLabel1
         // 
         this.linkLabel1.AutoSize = true;
         this.tableLayoutPanel1.SetColumnSpan(this.linkLabel1, 3);
         this.linkLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
         this.linkLabel1.Location = new System.Drawing.Point(4, 151);
         this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 14, 4, 0);
         this.linkLabel1.Name = "linkLabel1";
         this.linkLabel1.Size = new System.Drawing.Size(909, 25);
         this.linkLabel1.TabIndex = 3;
         this.linkLabel1.TabStop = true;
         this.linkLabel1.Text = "http://greatmaps.codeplex.com/";
         this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         this.linkLabel1.VisitedLinkColor = System.Drawing.Color.Blue;
         this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
         // 
         // buttonPresentation
         // 
         this.buttonPresentation.Dock = System.Windows.Forms.DockStyle.Fill;
         this.buttonPresentation.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
         this.buttonPresentation.ForeColor = System.Drawing.Color.Navy;
         this.buttonPresentation.Location = new System.Drawing.Point(536, 4);
         this.buttonPresentation.Margin = new System.Windows.Forms.Padding(4);
         this.buttonPresentation.Name = "buttonPresentation";
         this.buttonPresentation.Size = new System.Drawing.Size(377, 129);
         this.buttonPresentation.TabIndex = 6;
         this.buttonPresentation.Text = "Demo.WindowsPresentation";
         this.buttonPresentation.UseVisualStyleBackColor = true;
         this.buttonPresentation.Click += new System.EventHandler(this.buttonPresentation_Click);
         this.buttonPresentation.MouseEnter += new System.EventHandler(this.button1_MouseEnter);
         this.buttonPresentation.MouseLeave += new System.EventHandler(this.button1_MouseLeave);
         // 
         // pictureBox1
         // 
         this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
         this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pictureBox1.Image = global::GMap.NET___Hot_Build.Properties.Resources.logo99;
         this.pictureBox1.Location = new System.Drawing.Point(388, 4);
         this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(140, 129);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
         this.pictureBox1.TabIndex = 4;
         this.pictureBox1.TabStop = false;
         this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
         // 
         // MainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.AliceBlue;
         this.ClientSize = new System.Drawing.Size(931, 202);
         this.Controls.Add(this.tableLayoutPanel1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Margin = new System.Windows.Forms.Padding(4);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "MainForm";
         this.Opacity = 0.88D;
         this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "GMap.NET - Great Maps for Windows Forms & Presentation - Hot Build";
         this.TopMost = true;
         this.tableLayoutPanel1.ResumeLayout(false);
         this.tableLayoutPanel1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Button buttonForms;
      private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
      private System.Windows.Forms.LinkLabel linkLabel1;
      private System.Windows.Forms.Button buttonPresentation;
      private System.Windows.Forms.PictureBox pictureBox1;
   }
}

