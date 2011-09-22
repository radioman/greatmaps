
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   public abstract class CloudMadeMapProviderBase : GMapProvider
   {
      public readonly string ServerLetters = "abc";
      public readonly string DoubleResolutionString = "@2x";

      public bool DoubleResolution = true;

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
   /// CloudMadeMap demo provider, http://maps.cloudmade.com/
   /// </summary>
   public class CloudMadeMapProvider : CloudMadeMapProviderBase
   {
      public static readonly CloudMadeMapProvider Instance;

      readonly string Key = "5937c2bd907f4f4a92d8980a7c666ac0"; // demo key of CloudMade
      readonly int StyleID = 45363; // grab your style here http://maps.cloudmade.com/?styleId=45363

      CloudMadeMapProvider()
      {
      }

      static CloudMadeMapProvider()
      {
         Instance = new CloudMadeMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("00403A36-725F-4BC4-934F-BFC1C164D003");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "CloudMade, Demo";
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
         return string.Format(UrlFormat, ServerLetters[GetServerNum(pos, 3)], Key, StyleID, (DoubleResolution ? DoubleResolutionString : string.Empty), zoom, pos.X, pos.Y);
      }

      static readonly string UrlFormat = "http://{0}.tile.cloudmade.com/{1}/{2}{3}/256/{4}/{5}/{6}.png";
   }
}