using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Net;
using System.Diagnostics;

namespace SilverlightMapFusion.Web
{
    public partial class MainForm : System.Web.UI.Page
    {
        static MainForm()
        {
            try
            {
                //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
                // or
                //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
                //GMapProvider.IsSocksProxy = true;

                GMaps.Instance.EnableTileHost(8844);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainForm: " + ex);
                throw;
            }

            //GMaps.Instance.DisableTileHost();
            //GMaps.Instance.CancelTileCaching();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
    }
}