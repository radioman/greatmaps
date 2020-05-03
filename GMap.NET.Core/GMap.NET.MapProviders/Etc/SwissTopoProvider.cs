namespace GMap.NET.MapProviders
{
    using System;
    using GMap.NET.Projections;

    public class SwissTopoProvider : GMapProvider
    {
        private readonly Guid _id = new Guid("0F1F1EC5-B297-4B5B-8EB4-27AA403D1860");
        private readonly string _name = "SwissTopo";
        private readonly Random _randomGen;

        public override Guid Id
        {
            get { return _id; }
        }

        public static readonly SwissTopoProvider Instance;

        SwissTopoProvider()
        {
            // Terms of use: https://api3.geo.admin.ch/api/terms_of_use.html

            MaxZoom = null;
            _randomGen = new Random();
        }

        private GMapProvider[] _overlays;

        string MakeTileImageUrl(GPoint pos, int zoom)
        {
            int serverMaxDigits = 10; // from wmts[0-9].geo.admin.ch 
            var serverDigit = _randomGen.Next() % serverMaxDigits;
            var layerName = "ch.swisstopo.pixelkarte-farbe";
            var tileMatrixSet = "2056";
            var time = "current";

            // <Scheme>://<ServerName>/<ProtocoleVersion>/<LayerName>/<Stylename>/<Time>/<TileMatrixSet>/<TileSetId=Zoom>/<TileRow>/<TileCol>.<FormatExtension>
            var formattedUrl = $"https://wmts{serverDigit}.geo.admin.ch/1.0.0/{layerName}/default/{time}/{tileMatrixSet}/{zoom}/{pos.X}/{pos.Y}.jpeg";

            return formattedUrl;
        }

        static SwissTopoProvider()
        {
            Instance = new SwissTopoProvider();
        }

        #region GMapProvider Members

        public override string Name => _name;
        public override PureProjection Projection => SwissTopoProjection.Instance;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null) _overlays = new GMapProvider[] {this};
                return _overlays;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom);

            return GetTileImageUsingHttp(url);
        }

        #endregion
    }
}