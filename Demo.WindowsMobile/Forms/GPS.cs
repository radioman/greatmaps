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
      Font fSignal = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Regular);
      Brush bSignal = new SolidBrush(Color.Blue);
      StringFormat sformat = new StringFormat();

      public GPS(MainForm main)
      {
         InitializeComponent();
         Main = main;
         sformat.LineAlignment = StringAlignment.Far;
         sformat.Alignment = StringAlignment.Center;
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
                  int str = (int) (2.0 * (panelSignals.Height * Main.Satellites[i].SignalStrength)/100.0);

                  if(Main.Satellites[i].InSolution)
                  {
                     e.Graphics.FillRectangle(brushForSatOk, new Rectangle(i*cc, panelSignals.Height - str, cc, str));
                  }
                  else
                  {
                     e.Graphics.FillRectangle(brushForSatNo, new Rectangle(i*cc, panelSignals.Height - str, cc, str));
                  }

                  e.Graphics.DrawRectangle(penForSat, new Rectangle(i*cc + (int) penForSat.Width/2, 0, cc - (int) penForSat.Width/2, panelSignals.Height));

                  e.Graphics.DrawString(Main.Satellites[i].SignalStrength + "dB", fSignal, bSignal, new Rectangle(i*cc, 0, cc, (int)(panelSignals.Height-fSignal.Size/2)), sformat);
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
