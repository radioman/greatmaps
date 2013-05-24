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

        public MvcApplication()
        {
            try
            {
                //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
                // or
                GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
                GMapProvider.IsSocksProxy = true;

                GMaps.Instance.EnableTileHost(8844);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Application_Start: " + ex);
                throw;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            GMaps.Instance.DisableTileHost();
            GMaps.Instance.CancelTileCaching();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);            
        }
    }
}