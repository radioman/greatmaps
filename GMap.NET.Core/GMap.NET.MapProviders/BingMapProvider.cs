
namespace GMap.NET.MapProviders
{
   using System;
   using System.Text;
   using GMap.NET.Projections;
   using System.Diagnostics;
   using System.Net;
   using System.IO;
   using System.Text.RegularExpressions;
   using System.Threading;
   using GMap.NET.Internals;

   public abstract class BingMapProviderBase : GMapProvider
   {
      public BingMapProviderBase()
      {
         RefererUrl = "http://www.bing.com/maps/";
         Copyright = string.Format("©{0} Microsoft Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);
      }

      public string Version = "838";

      /// <summary>
      /// Bing Maps Customer Identification, more info here
      /// http://msdn.microsoft.com/en-us/library/bb924353.aspx
      /// </summary>
      public string ClientKey = null;

      /// <summary>
      /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
      /// </summary>
      /// <param name="tileX">Tile X coordinate.</param>
      /// <param name="tileY">Tile Y coordinate.</param>
      /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
      /// to 23 (highest detail).</param>
      /// <returns>A string containing the QuadKey.</returns>
      internal string TileXYToQuadKey(long tileX, long tileY, int levelOfDetail)
      {
         StringBuilder quadKey = new StringBuilder();
         for(int i = levelOfDetail; i > 0; i--)
         {
            char digit = '0';
            int mask = 1 << (i - 1);
            if((tileX & mask) != 0)
            {
               digit++;
            }
            if((tileY & mask) != 0)
            {
               digit++;
               digit++;
            }
            quadKey.Append(digit);
         }
         return quadKey.ToString();
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

      public bool TryCorrectVersion = true;
      static bool init = false;

      public override void OnInitialized()
      {
         if(!init && TryCorrectVersion)
         {
            string url = @"http://www.bing.com/maps";

            try
            {
               string html = Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromHours(8));

               if(string.IsNullOrEmpty(html))
               {
                  html = GetContentUsingHttp(url);
                  if(!string.IsNullOrEmpty(html))
                  {
                     Cache.Instance.SaveContent(url, CacheType.UrlCache, html);
                  }
               }

               if(!string.IsNullOrEmpty(html))
               {
                  #region -- match versions --
                  Regex reg = new Regex("http://ecn.t(\\d*).tiles.virtualearth.net/tiles/r(\\d*)[?*]g=(\\d*)", RegexOptions.IgnoreCase);
                  Match mat = reg.Match(html);
                  if(mat.Success)
                  {
                     GroupCollection gc = mat.Groups;
                     int count = gc.Count;
                     if(count > 2)
                     {
                        string ver = gc[3].Value;
                        string old = GMapProviders.BingMap.Version;
                        if(ver != old)
                        {
                           GMapProviders.BingMap.Version = ver;
                           GMapProviders.BingMapOld.Version = ver;
                           GMapProviders.BingSatelliteMap.Version = ver;
                           GMapProviders.BingHybridMap.Version = ver;

                           Debug.WriteLine("GMapProviders.BingMap.Version: " + ver + ", old: " + old + ", consider updating source");
                           if(Debugger.IsAttached)
                           {
                              Thread.Sleep(5555);
                           }
                        }
                        else
                        {
                           Debug.WriteLine("GMapProviders.BingMap.Version: " + ver + ", OK");
                        }
                     }
                  }
                  #endregion
               }

               init = true; // try it only once
            }
            catch(Exception ex)
            {
               Debug.WriteLine("TryCorrectBingVersions failed: " + ex.ToString());
            }
         }
      }
   }

   /// <summary>
   /// BingMapProvider provider
   /// </summary>
   public class BingMapProvider : BingMapProviderBase
   {
      public static readonly BingMapProvider Instance;

      BingMapProvider()
      {
      }

      static BingMapProvider()
      {
         Instance = new BingMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("D0CEB371-F10A-4E12-A2C1-DF617D6674A8");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "BingMap";
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
         string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
         return string.Format(UrlFormat, GetServerNum(pos, 4), key, Version, language, (!string.IsNullOrEmpty(ClientKey) ? "&key=" + ClientKey : string.Empty));
      }

      static readonly string UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.png?g={2}&mkt={3}&lbl=l1&stl=h&shading=hill&n=z{4}";
   }
}
