
namespace GMap.NET.Internals
{
   using System.Collections.Generic;
   using System.IO;
   using System.Text;
   using System;
   using System.Diagnostics;
   using GMap.NET.CacheProviders;

   /// <summary>
   /// cache system for tiles, geocoding, etc...
   /// </summary>
   internal class Cache : Singleton<Cache>
   {
      string cache;
      string routeCache;
      string geoCache;
      string placemarkCache;

      /// <summary>
      /// abstract image cache
      /// </summary>
      public PureImageCache ImageCache;

      /// <summary>
      /// second level abstract image cache
      /// </summary>
      public PureImageCache ImageCacheSecond;

      /// <summary>
      /// local cache location
      /// </summary>
      public string CacheLocation
      {
         get
         {
            return cache;
         }
         set
         {
            cache = value;
            routeCache = cache + "RouteCache" + Path.DirectorySeparatorChar;
            geoCache = cache + "GeocoderCache" + Path.DirectorySeparatorChar;
            placemarkCache = cache + "PlacemarkCache" + Path.DirectorySeparatorChar;

#if SQLiteEnabled
            if(ImageCache is SQLitePureImageCache)
            {
               (ImageCache as SQLitePureImageCache).CacheLocation = value;
            }
#else
            if(ImageCache is MsSQLCePureImageCache)
            {
               (ImageCache as MsSQLCePureImageCache).CacheLocation = value;
            }
#endif
         }
      }

      public Cache()
      {
         #region singleton check
         if(Instance != null)
         {
            throw (new System.Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
         }
         #endregion

#if SQLiteEnabled
         ImageCache = new SQLitePureImageCache();
#else
         // you can use $ms stuff if you like too ;}
         ImageCache = new MsSQLCePureImageCache();
#endif

         CacheLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "GMap.NET" + Path.DirectorySeparatorChar;
      }

      #region -- etc cache --
      public void CacheGeocoder(string urlEnd, string content)
      {
         try
         {
            // precrete dir
            if(!Directory.Exists(geoCache))
            {
               Directory.CreateDirectory(geoCache);
            }

            StringBuilder file = new StringBuilder(geoCache);
            file.AppendFormat("{0}.geo", urlEnd);

            File.WriteAllText(file.ToString(), content, Encoding.UTF8);
         }
         catch
         {
         }
      }

      public string GetGeocoderFromCache(string urlEnd)
      {
         string ret = null;

         try
         {
            StringBuilder file = new StringBuilder(geoCache);
            file.AppendFormat("{0}.geo", urlEnd);

            if(File.Exists(file.ToString()))
            {
               ret = File.ReadAllText(file.ToString(), Encoding.UTF8);
            }
         }
         catch
         {
            ret = null;
         }

         return ret;
      }

      public void CachePlacemark(string urlEnd, string content)
      {
         try
         {
            // precrete dir
            if(!Directory.Exists(placemarkCache))
            {
               Directory.CreateDirectory(placemarkCache);
            }

            StringBuilder file = new StringBuilder(placemarkCache);
            file.AppendFormat("{0}.plc", urlEnd);

            File.WriteAllText(file.ToString(), content, Encoding.UTF8);
         }
         catch
         {
         }
      }

      public string GetPlacemarkFromCache(string urlEnd)
      {
         string ret = null;

         try
         {
            StringBuilder file = new StringBuilder(placemarkCache);
            file.AppendFormat("{0}.plc", urlEnd);

            if(File.Exists(file.ToString()))
            {
               ret = File.ReadAllText(file.ToString(), Encoding.UTF8);
            }
         }
         catch
         {
            ret = null;
         }

         return ret;
      }

      public void CacheRoute(string urlEnd, string content)
      {
         try
         {
            // precrete dir
            if(!Directory.Exists(routeCache))
            {
               Directory.CreateDirectory(routeCache);
            }

            StringBuilder file = new StringBuilder(routeCache);
            file.AppendFormat("{0}.dragdir", urlEnd);

            File.WriteAllText(file.ToString(), content, Encoding.UTF8);
         }
         catch
         {
         }
      }

      public string GetRouteFromCache(string urlEnd)
      {
         string ret = null;

         try
         {
            StringBuilder file = new StringBuilder(routeCache);
            file.AppendFormat("{0}.dragdir", urlEnd);

            if(File.Exists(file.ToString()))
            {
               ret = File.ReadAllText(file.ToString(), Encoding.UTF8);
            }
         }
         catch
         {
            ret = null;
         }

         return ret;
      }
      #endregion
   }
}
