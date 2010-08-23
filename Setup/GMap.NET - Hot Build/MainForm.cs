using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GMap.NET___Hot_Build
{
   public partial class MainForm : Form
   {
      Demo.WindowsForms.Dummy d = new Demo.WindowsForms.Dummy();
      Demo.WindowsPresentation.Dummy dd = new Demo.WindowsPresentation.Dummy();

      public MainForm()
      {
         InitializeComponent();

         Text = "GMap.NET - Great Maps for Windows Forms & Presentation - Hot Build - 9dee5422854d";
      }

      private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         try
         {
            Process.Start(new ProcessStartInfo("http://greatmaps.codeplex.com/"));
         }
         catch
         {
         }
      }

      private void pictureBox1_Click(object sender, EventArgs e)
      {
         try
         {
            Process.Start(new ProcessStartInfo("http://greatmaps.codeplex.com/"));
         }
         catch
         {
         }
      }

      static readonly Font on = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      static readonly Font off = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));

      private void button1_MouseEnter(object sender, EventArgs e)
      {
         Button b = sender as Button;
         b.Font = on;
         b.ForeColor = Color.Green;
      }

      private void button1_MouseLeave(object sender, EventArgs e)
      {
         Button b = sender as Button;
         b.Font = off;
         b.ForeColor = Color.Navy;
      }

      private void buttonForms_Click(object sender, EventArgs e)
      {
         try
         {
            Process.Start(new ProcessStartInfo("Demo.WindowsForms.exe"));
            this.Close();
         }
         catch(Exception ex)
         {
            MessageBox.Show(ex.Message, "GMap.NET - Demo.WindowsForms.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void buttonPresentation_Click(object sender, EventArgs e)
      {
         try
         {
            Process.Start(new ProcessStartInfo("Demo.WindowsPresentation.exe"));
            this.Close();
         }
         catch(Exception ex)
         {
            MessageBox.Show(ex.Message, "GMap.NET - Demo.WindowsPresentation.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }
   }
}
