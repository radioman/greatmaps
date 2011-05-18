
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   /// <summary>
   /// OpenCycleMap provider
   /// </summary>
   public class OpenCycleMapProvider : GMapProvider
   {
      public static readonly OpenCycleMapProvider Instance;

      OpenCycleMapProvider()
      {
      }

      static OpenCycleMapProvider()
      {
         Instance = new OpenCycleMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("D7E1826E-EE1E-4441-9F15-7C2DE0FE0B0A");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "OpenCycleMap";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      public override PureProjection Projection
      {
         get
         {
            return OpenStreetMapProviderBase.Projection;
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
         char letter = OpenStreetMapProviderBase.ServerLetters[GMapProvider.GetServerNum(pos, 3)];
         return string.Format(UrlFormat, letter, zoom, pos.X, pos.Y);
      }

      static readonly string UrlFormat = "http://{0}.tile.opencyclemap.org/cycle/{1}/{2}/{3}.png";
   }
}
