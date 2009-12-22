using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Demo.WindowsMobile
{
   public partial class GPS : UserControl
   {
      MainForm Main;

      Pen penForSat = new Pen(Color.White, 3.0f);
      Brush brushForSatOk = new SolidBrush(Color.LimeGreen);
      Brush brushForSatNo = new SolidBrush(Color.Red);

      public GPS(MainForm main)
      {
         InitializeComponent();
         Main = main;
      }

      private void panelSignals_Paint(object sender, PaintEventArgs e)
      {
         lock(Main.Satellites)
         {
            if(Main.Satellites.Count > 0)
            {
               int cc = Width / Main.Satellites.Count;
               for(int i = 0; i < Main.Satellites.Count; i++)
               {
                  int str = (int) ((panelSignals.Height * Main.Satellites[i].SignalStrength)/100.0);

                  if(Main.Satellites[i].InSolution)
                  {
                     e.Graphics.FillRectangle(brushForSatOk, new Rectangle(i*cc, panelSignals.Height - str, cc, str));
                  }
                  else
                  {
                     e.Graphics.FillRectangle(brushForSatNo, new Rectangle(i*cc, panelSignals.Height - str, cc, str));
                  }

                  e.Graphics.DrawRectangle(penForSat, new Rectangle(i*cc + (int) penForSat.Width/2, 0, cc - (int) penForSat.Width/2, panelSignals.Height));
               }
            }
         }
      }

      private void GPS_Resize(object sender, EventArgs e)
      {
         if(Parent != null)
         {
            status.Height = Parent.Height - 44*5;
         }
      }
   }
}
