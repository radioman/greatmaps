
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// NearSatelliteMap provider
   /// </summary>
   public class NearSatelliteMapProvider : NearMapProviderBase
   {
      public static readonly NearSatelliteMapProvider Instance;

      NearSatelliteMapProvider()
      {
      }

      static NearSatelliteMapProvider()
      {
         Instance = new NearSatelliteMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("56D00148-05B7-408D-8F7A-8D7250FF8121");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "NearSatelliteMap";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         string url = MakeTileImageUrl(pos, zoom, Language);

         return GetTileImageUsingHttp(url);
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         // http://web2.nearmap.com/maps/hl=en&x=34&y=20&z=6&nml=Vert&s=2NYYKGF  

         return string.Format(UrlFormat, GetServerNum(pos, 3), pos.X, pos.Y, zoom);
      }

      static readonly string UrlFormat = "http://web{0}.nearmap.com/maps/hl=en&x={1}&y={2}&z={3}&nml=Vert";
   }
}