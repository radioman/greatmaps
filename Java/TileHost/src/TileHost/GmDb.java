/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package TileHost;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;

public class GmDb
{
   static void Init() throws ClassNotFoundException
   {
      Class.forName("org.sqlite.JDBC"); // https://bitbucket.org/xerial/sqlite-jdbc        
   }
   
   public static byte [] GetTile(String x, String y, String zoom, String dbId) throws SQLException 
   {
        byte [] allBytesInBlob = null;
       
        // create a database connection
        Connection conn = DriverManager.getConnection("jdbc:sqlite:" + TileHost.Db);
        try
        {
            Statement stmt = conn.createStatement();
            try 
            {                
                ResultSet rs = stmt.executeQuery(String.format("select Tile from TilesData where id = (select id from Tiles where X = %s and Y = %s and Zoom = %s and Type = %s)", x, y, zoom, dbId));
                if(rs != null)
                {
                    try
                    {
                        if(rs.next())
                        {                
                            allBytesInBlob = rs.getBytes(1);
                        }
                    } 
                    finally
                    {
                        try { rs.close(); } catch (Exception ignore) {}
                    }
                }
            } 
            finally
            {
                try { stmt.close(); } catch (Exception ignore) {}
            }           
        } 
        finally
        {
            try { conn.close(); } catch (Exception ignore) {}
        } 
        return allBytesInBlob;
   }
}
