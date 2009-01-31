using System.Drawing;
using System.IO;
using System.Text;
using System.Data.SQLite;
using System.Data.Common;

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
            gtileCache = cache + "TileDBv2" + Path.DirectorySeparatorChar;
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

         CacheLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "GMap.NET" + Path.DirectorySeparatorChar;
      }

      public void CacheImageDB(byte[] tile, GMapType type, Point pos, int zoom, string language)
      {
         try
         {
            StringBuilder dir = new StringBuilder(gtileCache);
            dir.AppendFormat("{0}{1}", language, Path.DirectorySeparatorChar);

            string d = dir.ToString();

            // precrete dir
            if(!Directory.Exists(d))
            {
               Directory.CreateDirectory(d);
            }

            // save
            {
               dir.AppendFormat("{0}.db3", zoom);

               string db = dir.ToString();
               if(!File.Exists(db))
               {
                  using(SQLiteConnection cn = new SQLiteConnection())
                  {
                     cn.ConnectionString = string.Format("Data Source=\"{0}\";Pooling=False;FailIfMissing=False;", db);
                     cn.Open();
                     if(cn.State == System.Data.ConnectionState.Open)
                     {
                        using(SQLiteTransaction tr = cn.BeginTransaction())
                        {
                           try
                           {
                              using(SQLiteCommand cmd = new SQLiteCommand(cn))
                              {
                                 cmd.CommandText = Properties.Resources.CreateTileDb;
                                 cmd.ExecuteNonQuery();
                              }
                              tr.Commit();
                           }
                           catch
                           {
                              tr.Rollback();
                           }
                        }
                        cn.Close();
                     }
                  }
               }

               using(SQLiteConnection cn = new SQLiteConnection())
               {
                  cn.ConnectionString = string.Format("Data Source=\"{0}\"; Pooling=False;", db);
                  cn.Open();
                  if(cn.State == System.Data.ConnectionState.Open)
                  {
                     using(SQLiteTransaction tr = cn.BeginTransaction())
                     {
                        try
                        {
                           using(SQLiteCommand cmd = new SQLiteCommand(cn))
                           {
                              cmd.CommandText = "INSERT INTO Tiles(X, Y, Type) VALUES(@p1, @p2, @p3)";
                              cmd.Parameters.AddWithValue("@p1", pos.X);
                              cmd.Parameters.AddWithValue("@p2", pos.Y);
                              cmd.Parameters.AddWithValue("@p3", (int) type);                              

                              cmd.ExecuteNonQuery();
                           }

                           using(SQLiteCommand cmd = new SQLiteCommand(cn))
                           {
                              cmd.CommandText = "INSERT INTO TilesData(id, Tile) VALUES((SELECT last_insert_rowid()), @p1)";
                              cmd.Parameters.AddWithValue("@p1", tile);

                              cmd.ExecuteNonQuery();
                           }

                           tr.Commit();
                        }
                        catch
                        {
                           tr.Rollback();
                        }
                     }
                  }
               }
            }
            tile = null;
         }
         catch(System.Exception ex)
         {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
         }
      }

      public PureImage GetImageFromCacheDB(GMapType type, Point pos, int zoom, string language)
      {
         PureImage ret = null;
         try
         {
            StringBuilder dir = new StringBuilder(gtileCache);
            dir.AppendFormat("{0}{1}", language, Path.DirectorySeparatorChar);

            // get
            {
               dir.AppendFormat("{0}.db3", zoom);

               string db = dir.ToString();
               if(File.Exists(db))
               {
                  using(SQLiteConnection cn = new SQLiteConnection())
                  {
                     cn.ConnectionString = string.Format("Data Source=\"{0}\"; Pooling=False;", db);
                     cn.Open();
                     if(cn.State == System.Data.ConnectionState.Open)
                     {
                        using(SQLiteCommand com = new SQLiteCommand(cn))
                        {
                           com.CommandText = string.Format("SELECT Tile FROM TilesData WHERE id = (SELECT id FROM Tiles WHERE X={0} AND Y={1} AND Type={2});", pos.X, pos.Y, (int) type);

                           using(SQLiteDataReader rd = com.ExecuteReader())
                           {
                              if(rd.Read())
                              {
                                 long length = rd.GetBytes(0, 0, null, 0, 0);
                                 byte[] tile = new byte[length];
                                 rd.GetBytes(0, 0, tile, 0, tile.Length);

                                 using(MemoryStream stm = new MemoryStream(tile))
                                 {
                                    if(Purity.Instance.ImageProxy != null)
                                    {
                                       ret = Purity.Instance.ImageProxy.FromStream(stm);
                                    }
                                 }
                                 tile = null;
                              }
                           }
                        }
                     }
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

      #region -- old tile file system --
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
      #endregion

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
