
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// LithuaniaOrtoFotoNewMap, from 2010 data, provider
   /// </summary>
   public class LithuaniaOrtoFotoNewMapProvider : LithuaniaMapProviderBase
   {
      public static readonly LithuaniaOrtoFotoNewMapProvider Instance;

      LithuaniaOrtoFotoNewMapProvider()
      {
      }

      static LithuaniaOrtoFotoNewMapProvider()
      {
         Instance = new LithuaniaOrtoFotoNewMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("C37A148E-0A7D-4123-BE4E-D0D3603BE46B");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "LithuaniaOrtoFotoMap 2010";
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
         return string.Format(UrlFormat, zoom, pos.Y, pos.X);
      }

      static readonly string UrlFormat = "http://dc1.maps.lt/cache/mapslt_ortofoto_2010/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg";
   }
}