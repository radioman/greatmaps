/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package TileHost;

import java.io.BufferedOutputStream;
import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.io.Reader;
import java.io.Writer;
import java.net.Socket;
import java.util.StringTokenizer;
import java.util.Date;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.Statement;
import java.sql.DriverManager;

public class TileServer implements Runnable
{ 
  public static String Db = "C:\\Users\\mindmb\\AppData\\Local\\GMap.NET\\TileDBv5\\en\\Data.gmdb";
  public static boolean verbose=true;
  Socket connect;

  public TileServer(Socket connect)
  {
    this.connect = connect;
  }

  public void run()
  {
    BufferedReader in = null;
    PrintWriter out = null;
    BufferedOutputStream dataOut = null;
    String fileRequested = null;

    try
    {
      //get character input stream from client
      in = new BufferedReader(new InputStreamReader(connect.getInputStream()));
      
      //get character output stream to client (for headers)
      out = new PrintWriter(connect.getOutputStream());
      
      //get binary output stream to client (for requested data)
      dataOut = new BufferedOutputStream(connect.getOutputStream());

      //get first line of request from client
      String input = in.readLine();
      
      //create StringTokenizer to parse request
      StringTokenizer parse = new StringTokenizer(input);
      
      //parse out method
      String method = parse.nextToken().toUpperCase();
      
      //parse out file requested
      fileRequested = parse.nextToken().toLowerCase();

      //methods other than GET and HEAD are not implemented
      if (!method.equals("GET") && !method.equals("HEAD"))
      {
        if (verbose)
        {
          System.out.println("501 Not Implemented: " + method +
            " method.");
        }

        //send Not Implemented message to client
        out.println("HTTP/1.0 501 Not Implemented");
        out.println("Server: Java HTTP Server 1.0");
        out.println("Date: " + new Date());
        out.println("Content-Type: text/html");
        out.println(); //blank line between headers and content
        out.println("<HTML>");
        out.println("<HEAD><TITLE>Not Implemented</TITLE>" +
          "</HEAD>");
        out.println("<BODY>");
        out.println("<H2>501 Not Implemented: " + method +
          " method.</H2>");
        out.println("</BODY></HTML>");
        out.flush();

        return;
      }
      else
      {
        //System.out.println("fileRequested: " + fileRequested);
          
        byte[] allBytesInBlob = null;
        
        String [] params = fileRequested.split("/");

        if(params.length == 5)
        {
            String dbId = params[1];
            String zoom = params[2];
            String x = params[3];
            String y = params[4];           

            // create a database connection
            Connection conn = DriverManager.getConnection("jdbc:sqlite:" + Db);
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
        }
        
        if(allBytesInBlob != null)
        {
           out.println("HTTP/1.0 200 OK");
           out.println("Content-type: image");
           out.println("Content-length: " + allBytesInBlob.length);
           out.println(); //blank line between headers and content
           out.flush(); //flush character output stream buffer 
        
           dataOut.write(allBytesInBlob, 0, allBytesInBlob.length); 
           dataOut.flush();
        }
        else            
        {
           out.println("HTTP/1.0 204 No Content");
           out.println(); //blank line between headers and content
           out.flush(); //flush character output stream buffer
        }
      }
    }
    catch (Exception ioe)
    {
      System.err.println("Server Error: " + ioe);
    }
    finally
    {
      close(in); //close character input stream
      close(out); //close character output stream
      close(dataOut); //close binary output stream
      close(connect); //close socket connection

      if (verbose)
      {
        System.out.println("Connection closed.\n");
      }
    }
  }

  /**
   * close method closes the given stream.
   *
   * @param stream
   */
  public void close(Object stream)
  {
    if (stream == null)
      return;

    try
    {
      if (stream instanceof Reader)
      {
        ((Reader)stream).close();
      }
      else if (stream instanceof Writer)
      {
        ((Writer)stream).close();
      }
      else if (stream instanceof InputStream)
      {
        ((InputStream)stream).close();
      }
      else if (stream instanceof OutputStream)
      {
        ((OutputStream)stream).close();
      }
      else if (stream instanceof Socket)
      {
        ((Socket)stream).close();
      }
      else
      {
        System.err.println("Unable to close object: " + stream);
      }
    }
    catch (Exception e)
    {
      System.err.println("Error closing stream: " + e);
    }
  }
}
