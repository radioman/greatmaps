/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package TileHost;

import java.net.ServerSocket;
import java.util.Date;

/**
 *
 * @author radioman
 */
public class Application
{ 
  static int PORT = 8844; //default port
  
    /**
   * main method creates a new HttpServer instance for each
   * request and starts it running in a separate thread.
   */
  public static void main(String[] args)
  {
    if (args.length == 2)
    {
        PORT = Integer.parseInt(args[0]);
        TileServer.Db = args[1]; 
        
        System.out.println("\nDatabase: " + TileServer.Db);
    }
    else
    {
        return;
    }
    
    try
    {
      //String workingDir = System.getProperty("user.dir");
      //System.out.println("Current working directory : " + workingDir);
           
      Class.forName("org.sqlite.JDBC"); // https://bitbucket.org/xerial/sqlite-jdbc
    
      ServerSocket serverConnect = new ServerSocket(PORT);
      System.out.println("\nListening for connections on port " + PORT + "...\n");
      
      while (true) //listen until user halts execution
      {
        TileServer server = new TileServer(serverConnect.accept());
        if (TileServer.verbose)
        {
          System.out.println("Connection opened. (" + new Date() + ")");
        }

        Thread threadRunner = new Thread(server);
        threadRunner.start();
      }
    }
    catch (Exception e)
    {
      System.err.println("Server error: " + e);
    }
  }
}

