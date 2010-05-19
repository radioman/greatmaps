namespace Demo.WindowsForms
{
   partial class StaticImage
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
         this.button1 = new System.Windows.Forms.Button();
         this.progressBar1 = new System.Windows.Forms.ProgressBar();
         this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
         this.button2 = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.checkBoxWorldFile = new System.Windows.Forms.CheckBox();
         this.checkBoxRoutes = new System.Windows.Forms.CheckBox();
         ((System.ComponentModel.ISupportInitialize) (this.numericUpDown1)).BeginInit();
         this.SuspendLayout();
         // 
         // button1
         // 
         this.button1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button1.Location = new System.Drawing.Point(497, 29);
         this.button1.Margin = new System.Windows.Forms.Padding(2);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(84, 32);
         this.button1.TabIndex = 1;
         this.button1.Text = "Generate";
         this.button1.UseVisualStyleBackColor = true;
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // progressBar1
         // 
         this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.progressBar1.Location = new System.Drawing.Point(6, 6);
         this.progressBar1.Margin = new System.Windows.Forms.Padding(2);
         this.progressBar1.Name = "progressBar1";
         this.progressBar1.Size = new System.Drawing.Size(484, 93);
         this.progressBar1.TabIndex = 2;
         // 
         // numericUpDown1
         // 
         this.numericUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.numericUpDown1.Location = new System.Drawing.Point(535, 6);
         this.numericUpDown1.Margin = new System.Windows.Forms.Padding(2);
         this.numericUpDown1.Name = "numericUpDown1";
         this.numericUpDown1.Size = new System.Drawing.Size(47, 20);
         this.numericUpDown1.TabIndex = 3;
         // 
         // button2
         // 
         this.button2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.button2.Location = new System.Drawing.Point(497, 67);
         this.button2.Margin = new System.Windows.Forms.Padding(2);
         this.button2.Name = "button2";
         this.button2.Size = new System.Drawing.Size(84, 32);
         this.button2.TabIndex = 4;
         this.button2.Text = "Cancel";
         this.button2.UseVisualStyleBackColor = true;
         this.button2.Click += new System.EventHandler(this.button2_Click);
         // 
         // label1
         // 
         this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(494, 8);
         this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(37, 13);
         this.label1.TabIndex = 5;
         this.label1.Text = "Zoom:";
         // 
         // checkBoxWorldFile
         // 
         this.checkBoxWorldFile.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.checkBoxWorldFile.AutoSize = true;
         this.checkBoxWorldFile.Location = new System.Drawing.Point(497, 106);
         this.checkBoxWorldFile.Name = "checkBoxWorldFile";
         this.checkBoxWorldFile.Size = new System.Drawing.Size(96, 17);
         this.checkBoxWorldFile.TabIndex = 6;
         this.checkBoxWorldFile.Text = "make Worldfile";
         this.checkBoxWorldFile.UseVisualStyleBackColor = true;
         // 
         // checkBoxRoutes
         // 
         this.checkBoxRoutes.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.checkBoxRoutes.AutoSize = true;
         this.checkBoxRoutes.Location = new System.Drawing.Point(7, 106);
         this.checkBoxRoutes.Name = "checkBoxRoutes";
         this.checkBoxRoutes.Size = new System.Drawing.Size(147, 17);
         this.checkBoxRoutes.TabIndex = 7;
         this.checkBoxRoutes.Text = "Use area of routes in map";
         this.checkBoxRoutes.UseVisualStyleBackColor = true;
         // 
         // StaticImage
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(588, 130);
         this.Controls.Add(this.checkBoxRoutes);
         this.Controls.Add(this.checkBoxWorldFile);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.button2);
         this.Controls.Add(this.numericUpDown1);
         this.Controls.Add(this.progressBar1);
         this.Controls.Add(this.button1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.Margin = new System.Windows.Forms.Padding(2);
         this.MinimumSize = new System.Drawing.Size(16, 164);
         this.Name = "StaticImage";
         this.Padding = new System.Windows.Forms.Padding(4);
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Static Map maker";
         ((System.ComponentModel.ISupportInitialize) (this.numericUpDown1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.ProgressBar progressBar1;
      private System.Windows.Forms.NumericUpDown numericUpDown1;
      private System.Windows.Forms.Button button2;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.CheckBox checkBoxWorldFile;
      private System.Windows.Forms.CheckBox checkBoxRoutes;
   }
}