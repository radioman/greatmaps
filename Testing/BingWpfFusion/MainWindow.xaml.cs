using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.Maps.MapControl.WPF;

namespace BingMapsWpfUsingCache
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
            // or
            //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
            //GMapProvider.IsSocksProxy = true;

            GMaps.Instance.EnableTileHost(8844);

            Closing += new CancelEventHandler(MainWindow_Closing);

            // The pushpin to add to the map.
            Pushpin pin = new Pushpin();
            {
                pin.Location = map.Center;

                pin.ToolTip = new Label()
                {
                    Content = "GMap.NET fusion power! ;}"
                };
            }
            map.Children.Add(pin);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GMaps.Instance.DisableTileHost();
            GMaps.Instance.CancelTileCaching();
        }
    }

    public class CustomTileSource : TileSource
    {
        readonly string UrlFormat = "http://localhost:8844/{0}/{1}/{2}/{3}";
        readonly int DbId = GMapProviders.OpenStreetMap.DbId;

        // keep in mind that bing only supports mercator based maps

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            return new Uri(string.Format(UrlFormat, DbId, zoomLevel, x, y));
        }
    }

    public class CustomTileLayer : MapTileLayer
    {
        public CustomTileLayer()
        {
            TileSource = new CustomTileSource();
        }
    }
}
