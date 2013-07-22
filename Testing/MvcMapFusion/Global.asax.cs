using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using System.Threading;

namespace MvcMapFusion
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                } // Parameter defaults
            );

        }

        static BackgroundWorker worker;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            BackgroundWorker w = (BackgroundWorker)base.Application.Get("BackgroundWorker");
            if (w != null && worker == null)
            {
                worker = w;
            }

            if (worker == null)
            {
                worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.WorkerReportsProgress = false;
                worker.WorkerSupportsCancellation = true;
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

                base.Application.Set("BackgroundWorker", worker);                
            }           

            worker.RunWorkerAsync(); 
        }

        void Application_End()
        {
            BackgroundWorker w = (BackgroundWorker)base.Application.Get("BackgroundWorker");
            if (w != null)
            {
                w.CancelAsync();               
            }
        }

        static void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("worker_RunWorkerCompleted");            
        }

        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("worker_DoWork");
            try
            {
                //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
                // or
                //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
                //GMapProvider.IsSocksProxy = true;

                GMaps.Instance.EnableTileHost(8844);

                BackgroundWorker w = sender as BackgroundWorker;
                while (!w.CancellationPending)
                {
                    Thread.Sleep(1111);
                }

                GMaps.Instance.DisableTileHost();
                GMaps.Instance.CancelTileCaching();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("worker_DoWork: " + ex);
            }
        }
    }
}