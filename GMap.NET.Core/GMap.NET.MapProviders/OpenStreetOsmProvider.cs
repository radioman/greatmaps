
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   /// <summary>
   /// OpenStreetOsm provider
   /// </summary>
   public class OpenStreetOsmProvider : OpenStreetMapProviderBase
   {
      public static readonly OpenStreetOsmProvider Instance;

      OpenStreetOsmProvider()
      {
      }

      static OpenStreetOsmProvider()
      {
         Instance = new OpenStreetOsmProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("07EF8CBC-A91D-4B2F-8B2D-70DBE384EF18");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "OpenStreetOsm";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      GMapProvider[] overlays;
      public override GMapProvider[] Overlays
      {
         get
         {
            if(overlays == null)
            {
               overlays = new GMapProvider[] { this };
            }
            return overlays;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         string url = MakeTileImageUrl(pos, zoom, string.Empty);

         return GetTileImageUsingHttp(url);
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         char letter = ServerLetters[GMapProvider.GetServerNum(pos, 3)];
         return string.Format(UrlFormat, letter, zoom, pos.X, pos.Y);
      }

      static readonly string UrlFormat = "http://{0}.tah.openstreetmap.org/Tiles/tile/{1}/{2}/{3}.png";
   }
}
