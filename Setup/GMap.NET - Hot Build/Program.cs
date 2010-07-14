using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace GMap.NET___Hot_Build
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main()
      {
         if(CheckUpdate())
         {
            TryUpdate();
         }

         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         Application.Run(new MainForm());
      }

      public static void TryUpdate()
      {
         try
         {
            if(ApplicationDeployment.IsNetworkDeployed)
            {
               Debug.WriteLine("TryUpdate: " + DateTime.Now);

               if(ApplicationDeployment.CurrentDeployment.Update())
               {
                  try
                  {
                     System.Windows.Forms.Application.Restart();
                  }
                  catch
                  {
                  }
                  Thread.Sleep(444);
                  {
                     Environment.Exit(1);
                  }
               }
            }
         }
         catch
         {
         }
      }

      public static UpdateCheckInfo UpdateInfo = null;

      public static bool CheckUpdate()
      {
         try
         {
            if(ApplicationDeployment.IsNetworkDeployed)
            {
               var upInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();
               if(upInfo != null)
               {
                  UpdateInfo = upInfo;
                  return upInfo.UpdateAvailable;
               }
            }
         }
         catch
         {
         }
         return false;
      }
   }
}
