using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using GMapNET.Internals;

namespace GMapNET
{
   /// <summary>
   /// image cache for ms sql server
   /// </summary>
   public class MSSQLPureImageCache : PureImageCache
   {
      public string ConnectionString;

      #region PureImageCache Members

      public bool PutImageToCache(PureImage tile, MapType type, Point pos, int zoom)
      {
         bool ret = true;
         try
         {
            // save
            {
               using(SqlConnection cn = new SqlConnection())
               {
                  cn.ConnectionString = ConnectionString;
                  cn.Open();
                  if(cn.State == System.Data.ConnectionState.Open)
                  {
                     using(MemoryStream m = new MemoryStream())
                     {
                        ret = Purity.Instance.ImageProxy.Save(m, tile);
                        if(ret)
                        {
                           using(SqlTransaction tr = cn.BeginTransaction())
                           {
                              try
                              {
                                 long lasId = 0;
                                 using(SqlCommand cmd = new SqlCommand("INSERT INTO Tiles(X, Y, Zoom, Type) VALUES(@p1, @p2, @p3, @p4); select scope_identity()", cn))
                                 {
                                    cmd.Transaction = tr;

                                    cmd.Parameters.AddWithValue("@p1", pos.X);
                                    cmd.Parameters.AddWithValue("@p2", pos.Y);
                                    cmd.Parameters.AddWithValue("@p3", zoom);
                                    cmd.Parameters.AddWithValue("@p4", (int) type);

                                    object x = cmd.ExecuteScalar();
                                    lasId = long.Parse(x.ToString());
                                 }

                                 using(SqlCommand cmd = new SqlCommand(string.Format("INSERT INTO TilesData(id, Tile) VALUES({0}, @p1)", lasId), cn))
                                 {
                                    cmd.Transaction = tr;

                                    cmd.Parameters.AddWithValue("@p1", m.GetBuffer());

                                    cmd.ExecuteNonQuery();
                                 }
                                 tr.Commit();
                              }
                              catch(Exception ex)
                              {
                                 tr.Rollback();
                                 ret = false;
                                 Debug.WriteLine(ex.ToString());
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine(ex.ToString());
            ret = false;
         }
         return ret;
      }

      public PureImage GetImageFromCache(MapType type, Point pos, int zoom)
      {
         PureImage ret = null;
         try
         {
            // get
            {
               {
                  using(SqlConnection cn = new SqlConnection())
                  {
                     cn.ConnectionString = ConnectionString;
                     cn.Open();
                     if(cn.State == System.Data.ConnectionState.Open)
                     {
                        using(SqlCommand com = new SqlCommand(string.Format("SELECT Tile FROM TilesData WHERE id = (SELECT id FROM Tiles WHERE X={0} AND Y={1} AND Zoom={2} AND Type={3})", pos.X, pos.Y, zoom, (int) type), cn))
                        {
                           using(SqlDataReader rd = com.ExecuteReader())
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
         catch(Exception ex)
         {
            Debug.WriteLine(ex.ToString());
            ret = null;
         }

         return ret;
      }

      #endregion
   }

   #region -- ms sql table scripts --
   /*
   USE [Test]
   GO

   SET ANSI_NULLS ON
   GO

   SET QUOTED_IDENTIFIER ON
   GO

   CREATE TABLE [dbo].[Tiles](
	   [Id] [bigint] IDENTITY(1,1) NOT NULL,
	   [X] [int] NOT NULL,
	   [Y] [int] NOT NULL,
	   [Zoom] [int] NOT NULL,
	   [Type] [int] NOT NULL,
    CONSTRAINT [PK_Tiles] PRIMARY KEY CLUSTERED 
   (
	   [Id] ASC
   )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
   ) ON [PRIMARY]

   GO
   */

   /*
   USE [Test]
   GO

   SET ANSI_NULLS ON
   GO

   SET QUOTED_IDENTIFIER ON
   GO

   CREATE TABLE [dbo].[TilesData](
	   [Id] [bigint] NOT NULL,
	   [Tile] [image] NOT NULL,
    CONSTRAINT [PK_TilesData] PRIMARY KEY CLUSTERED 
   (
	   [Id] ASC
   )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
   ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

   GO 
   */ 

   // also add indexes to boost data performance :}

   #endregion
}
