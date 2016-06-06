
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// CzechSatelliteMap provider, http://www.mapy.cz/
   /// </summary>
   public class CzechSatelliteMapProvider : CzechMapProviderBase
   {
      public static readonly CzechSatelliteMapProvider Instance;

      CzechSatelliteMapProvider()
      {
      }

      static CzechSatelliteMapProvider()
      {
         Instance = new CzechSatelliteMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("7846D655-5F9C-4042-8652-60B6BF629C3C");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "CzechSatelliteMap";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         string url = MakeTileImageUrl(pos, zoom, LanguageStr);

         return GetTileImageUsingHttp(url);
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         // http://m3.mapserver.mapy.cz/ophoto-m/14-8802-5528

         return string.Format(UrlFormat, GetServerNum(pos, 3) + 1, zoom, pos.X, pos.Y);
      }

      static readonly string UrlFormat = "http://m{0}.mapserver.mapy.cz/ophoto-m/{1}-{2}-{3}";
   }
}