using System.Drawing;
using System.IO;
using System.Text;

namespace GMapNET.Internals
{
   internal class Cache : Singleton<Cache>
   {
      string cache;
      string gtileCache;
      string routeCache;
      string geoCache;
      string placemarkCache;

      public string CacheLocation
      {
         get
         {
            return cache;
         }
         set
         {
            cache = value;
            gtileCache = cache + "TileCache" + Path.DirectorySeparatorChar;
            routeCache = cache + "RouteCache" + Path.DirectorySeparatorChar;
            geoCache = cache + "GeocoderCache" + Path.DirectorySeparatorChar;
            placemarkCache = cache + "PlacemarkCache" + Path.DirectorySeparatorChar;
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

         CacheLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Google" + Path.DirectorySeparatorChar + "GMap.NET" + Path.DirectorySeparatorChar;
      }

      public void CacheImage(PureImage tile, GMapType type, Point pos, int zoom, string language)
      {
         try
         {
            using(tile)
            {
               StringBuilder dir = new StringBuilder(gtileCache);
               dir.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", zoom, Path.DirectorySeparatorChar, language, Path.DirectorySeparatorChar, pos.X, Path.DirectorySeparatorChar, pos.Y, Path.DirectorySeparatorChar);

               string d = dir.ToString();

               // precrete dir
               if(!Directory.Exists(d))
               {
                  Directory.CreateDirectory(d);
               }

               // save
               {
                  dir.AppendFormat("{0}.png", type.ToString());

                  using(FileStream s = File.Open(dir.ToString(), FileMode.Create, FileAccess.Write, FileShare.None))
                  {
                     if(Purity.Instance.ImageProxy != null)
                     {
                        Purity.Instance.ImageProxy.Save(s, tile);
                     }

                     s.Flush();
                     s.Close();
                  }

                  // remove crap
                  FileInfo f = new FileInfo(dir.ToString());
                  if(f.Length == 0)
                  {
                     File.Delete(dir.ToString());
                  }
               }
            }
         }
         catch
         {
         }
      }

      public PureImage GetImageFromCache(GMapType type, Point pos, int zoom, string language)
      {
         PureImage ret = null;
         try
         {
            StringBuilder dir = new StringBuilder(gtileCache);
            dir.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", zoom, Path.DirectorySeparatorChar, language, Path.DirectorySeparatorChar, pos.X, Path.DirectorySeparatorChar, pos.Y, Path.DirectorySeparatorChar);
            {
               dir.AppendFormat("{0}.png", type.ToString());
            }

            if(File.Exists(dir.ToString()))
            {
               FileInfo f = new FileInfo(dir.ToString());
               if(f.Length == 0)
               {
                  f.Delete();
               }
               else
               {
                  using(FileStream s = f.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                  {
                     if(Purity.Instance.ImageProxy != null)
                     {
                        ret = Purity.Instance.ImageProxy.FromStream(s);
                     }

                     s.Close();
                  }
               }
            }
         }
         catch
         {
            ret = null;
         }

         return ret;
      }

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
   }
}
