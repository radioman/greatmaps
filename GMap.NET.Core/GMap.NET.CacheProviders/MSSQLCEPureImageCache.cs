
namespace GMap.NET.CacheProviders
{
#if !SQLiteEnabled
   using System;
   using System.Data;
   using System.Diagnostics;
   using System.IO;
   using SqlCommand=System.Data.SqlServerCe.SqlCeCommand;
   using SqlConnection=System.Data.SqlServerCe.SqlCeConnection;

   /// <summary>
   /// image cache for ms sql server
   /// </summary>
   internal class MsSQLCePureImageCache : PureImageCache, IDisposable
   {
      string cache;
      string gtileCache;

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
            gtileCache = cache + "TileDBv3" + Path.DirectorySeparatorChar + GMaps.Instance.Language + Path.DirectorySeparatorChar;
         }
      }

      SqlCommand cmdInsert;
      SqlCommand cmdFetch;
      SqlConnection cnGet;
      SqlConnection cnSet;

      public MsSQLCePureImageCache()
      {
         CacheLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "GMap.NET" + Path.DirectorySeparatorChar;
      }

      bool initialized = false;

      /// <summary>
      /// is cache initialized
      /// </summary>
      public bool Initialized
      {
         get
         {
            lock(this)
            {
               return initialized;
            }
         }
         private set
         {
            lock(this)
            {
               initialized = value;
            }
         }
      }

      /// <summary>
      /// inits connection to server
      /// </summary>
      /// <returns></returns>
      public bool Initialize()
      {
         lock(this)
         {
            if(!Initialized)
            {
   #region prepare mssql & cache table
               try
               {
                  // precrete dir
                  if(!Directory.Exists(gtileCache))
                  {
                     Directory.CreateDirectory(gtileCache);
                  }

                  string connectionString = string.Format("Data Source={0}Data.sdf", gtileCache);

                  if(!File.Exists(gtileCache + "Data.sdf"))
                  {
                     using(System.Data.SqlServerCe.SqlCeEngine engine = new System.Data.SqlServerCe.SqlCeEngine(connectionString))
                     {
                        engine.CreateDatabase();                         
                     }

                     try
                        {
                           using(SqlConnection c = new SqlConnection(connectionString))
                           {
                              c.Open();

                              using(SqlCommand cmd = new SqlCommand(
                                 "CREATE TABLE [GMapNETcache] ( \n"
                  + "   [Type] [int]   NOT NULL, \n"
                  + "   [Zoom] [int]   NOT NULL, \n"
                  + "   [X]    [int]   NOT NULL, \n"
                  + "   [Y]    [int]   NOT NULL, \n"
                  + "   [Tile] [image] NOT NULL, \n"
                  + "   CONSTRAINT [PK_GMapNETcache] PRIMARY KEY (Type, Zoom, X, Y) \n"
                  + ")", c))
                              {
                                 cmd.ExecuteNonQuery();
                              }
                           }
                        }
                        catch(Exception ex)
                        {
                           try
                           {
                              File.Delete(gtileCache + "Data.sdf");
                           }
                           catch
                           {
                           }

                           throw ex;
                        }
                  }

                  // different connections so the multi-thread inserts and selects don't collide on open readers.
                  this.cnGet = new SqlConnection(connectionString);
                  this.cnGet.Open();
                  this.cnSet = new SqlConnection(connectionString);
                  this.cnSet.Open();

                  this.cmdFetch = new SqlCommand("SELECT [Tile] FROM [GMapNETcache] WITH (NOLOCK) WHERE [X]=@x AND [Y]=@y AND [Zoom]=@zoom AND [Type]=@type", cnGet);
                  this.cmdFetch.Parameters.Add("@x", System.Data.SqlDbType.Int);
                  this.cmdFetch.Parameters.Add("@y", System.Data.SqlDbType.Int);
                  this.cmdFetch.Parameters.Add("@zoom", System.Data.SqlDbType.Int);
                  this.cmdFetch.Parameters.Add("@type", System.Data.SqlDbType.Int);
                  this.cmdFetch.Prepare();

                  this.cmdInsert = new SqlCommand("INSERT INTO [GMapNETcache] ( [X], [Y], [Zoom], [Type], [Tile] ) VALUES ( @x, @y, @zoom, @type, @tile )", cnSet);
                  this.cmdInsert.Parameters.Add("@x", System.Data.SqlDbType.Int);
                  this.cmdInsert.Parameters.Add("@y", System.Data.SqlDbType.Int);
                  this.cmdInsert.Parameters.Add("@zoom", System.Data.SqlDbType.Int);
                  this.cmdInsert.Parameters.Add("@type", System.Data.SqlDbType.Int);
                  this.cmdInsert.Parameters.Add("@tile", System.Data.SqlDbType.Image); //, calcmaximgsize);
                  //can't prepare insert because of the IMAGE field having a variable size.  Could set it to some 'maximum' size?

                  Initialized = true;
               }
               catch(Exception ex)
               {
                  this.initialized = false;
                  Debug.WriteLine(ex.Message);
               }
               #endregion
            }
            return Initialized;
         }
      }

   #region IDisposable Members
      public void Dispose()
      {
         lock(cmdInsert)
         {
            if(cmdInsert != null)
            {
               cmdInsert.Dispose();
               cmdInsert = null;
            }

            if(cnSet != null)
            {
               cnSet.Dispose();
               cnSet = null;
            }
         }

         lock(cmdFetch)
         {
            if(cmdFetch != null)
            {
               cmdFetch.Dispose();
               cmdFetch = null;
            }

            if(cnGet != null)
            {
               cnGet.Dispose();
               cnGet = null;
            }
         }
         Initialized = false;
      }
      #endregion

   #region PureImageCache Members
      public bool PutImageToCache(MemoryStream tile, MapType type, Point pos, int zoom)
      {
         bool ret = true;
         {
            if(Initialize())
            {
               try
               {
                  lock(cmdInsert)
                  {
                     //cnSet.Ping();

                     if(cnSet.State != ConnectionState.Open)
                     {
                        cnSet.Open();
                     }

                     cmdInsert.Parameters["@x"].Value = pos.X;
                     cmdInsert.Parameters["@y"].Value = pos.Y;
                     cmdInsert.Parameters["@zoom"].Value = zoom;
                     cmdInsert.Parameters["@type"].Value = (int) type;
                     cmdInsert.Parameters["@tile"].Value = tile.GetBuffer();
                     cmdInsert.ExecuteNonQuery();
                  }
               }
               catch(Exception ex)
               {
                  Debug.WriteLine(ex.ToString());
                  ret = false;
                  Dispose();
               }
            }
         }
         return ret;
      }

      public PureImage GetImageFromCache(MapType type, Point pos, int zoom)
      {
         PureImage ret = null;
         {
            if(Initialize())
            {
               try
               {
                  object odata = null;
                  lock(cmdFetch)
                  {
                     //cnGet.Ping();

                     if(cnGet.State != ConnectionState.Open)
                     {
                        cnGet.Open();
                     }

                     cmdFetch.Parameters["@x"].Value = pos.X;
                     cmdFetch.Parameters["@y"].Value = pos.Y;
                     cmdFetch.Parameters["@zoom"].Value = zoom;
                     cmdFetch.Parameters["@type"].Value = (int) type;
                     odata = cmdFetch.ExecuteScalar();
                  }

                  if(odata != null && odata != DBNull.Value)
                  {
                     byte[] tile = (byte[]) odata;
                     if(tile != null && tile.Length > 0)
                     {
                        if(GMaps.Instance.ImageProxy != null)
                        {
                           MemoryStream stm = new MemoryStream(tile);
                           ret = GMaps.Instance.ImageProxy.FromStream(stm);
                           ret.Data = stm;
                        }
                     }
                     tile = null;
                  }
               }
               catch(Exception ex)
               {
                  Debug.WriteLine(ex.ToString());
                  ret = null;
                  Dispose();
               }
            }
         }
         return ret;
      }
      #endregion
   }
#endif
}
