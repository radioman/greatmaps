using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace Demo.WindowsForms
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main()
      {
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         Application.Run(new MainForm());
      }
   }

   struct IpInfo
   {
      public string Ip;
      public int Port;
      public TcpState State;

      public string CountryName;
      public string RegionName;
      public string City;
      public double Latitude;
      public double Longitude;

      public DateTime Time;       
   }
}
