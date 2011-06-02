
namespace GMap.NET.MapProviders
{
   using System;
   using GMap.NET.Projections;

   public abstract class NearMapProviderBase : GMapProvider
   {
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

      readonly MercatorProjection projection = new MercatorProjection();
      public override PureProjection Projection
      {
         get
         {
            return projection;
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
   /// NearMap provider
   /// </summary>
   public class NearMapProvider : NearMapProviderBase
   {
      public static readonly NearMapProvider Instance;

      NearMapProvider()
      {
      }

      static NearMapProvider()
      {
         Instance = new NearMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("E33803DF-22CB-4FFA-B8E3-15383ED9969D");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "NearMap";
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
         // http://web1.nearmap.com/maps/hl=en&x=18681&y=10415&z=15&nml=Map_&nmg=1&s=kY8lZssipLIJ7c5

         return string.Format(UrlFormat, GetServerNum(pos, 3), pos.X, pos.Y, zoom);
      }

      static readonly string UrlFormat = "http://web{0}.nearmap.com/maps/hl=en&x={1}&y={2}&z={3}&nml=Map_&nmg=1";
   }
}