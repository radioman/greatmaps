
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   public abstract class YahooMapProviderBase : GMapProvider
   {
      public YahooMapProviderBase()
      {
         RefererUrl = "http://maps.yahoo.com/";
         Copyright = string.Format("© Yahoo! Inc. - Map data & Imagery ©{0} NAVTEQ", DateTime.Today.Year);
      }

      #region GMapProvider Members
      public override Guid Id
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public override string Name
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public override PureProjection Projection
      {
         get
         {
            return MercatorProjection.Instance;
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
         throw new NotImplementedException();
      }
      #endregion
   }

   /// <summary>
   /// YahooMap provider
   /// </summary>
   public class YahooMapProvider : YahooMapProviderBase
   {
      public static readonly YahooMapProvider Instance;

      YahooMapProvider()
      {
      }

      static YahooMapProvider()
      {
         Instance = new YahooMapProvider();
      }

      public string Version = "4.3";

      #region GMapProvider Members

      readonly Guid id = new Guid("65DB032C-6869-49B0-A7FC-3AE41A26AF4D");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "YahooMap";
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
         // http://maps1.yimg.com/hx/tl?b=1&v=4.3&.intl=en&x=12&y=7&z=7&r=1

         return string.Format(UrlFormat, ((GetServerNum(pos, 2)) + 1), Version, language, pos.X, (((1 << zoom) >> 1) - 1 - pos.Y), (zoom + 1));
      }

      static readonly string UrlFormat = "http://maps{0}.yimg.com/hx/tl?v={1}&.intl={2}&x={3}&y={4}&z={5}&r=1";
   }
}