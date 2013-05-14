using System;
using System.Windows.Controls;
using Microsoft.Maps.MapControl;

namespace SilverlightMapFusion
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }

    public class CustomTileSource : TileSource
    {
        readonly string UrlFormat = "http://localhost:8844/{0}/{1}/{2}/{3}";
        readonly int DbId = 1492776782; //GMapProviders.OpenStreetMap.DbId;

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
            this.TileSources.Add(new CustomTileSource());
        }
    }
}
