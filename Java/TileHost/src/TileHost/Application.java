/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package TileHost;

/**
 *
 * @author radioman
 */
public class Application
{  
    /**
   * main method creates a new HttpServer instance for each
   * request and starts it running in a separate thread.
   */
  public static void main(String[] args)
  {
    boolean debug = false;
    
    if (args.length == 2)
    {
        TileHost.PORT = Integer.parseInt(args[0]);
        TileHost.Db = args[1]; 
        
        System.out.println("\nDatabase: " + TileHost.Db);
    }
    else
    {
        TileHost.verbose=true;
        debug = true;       
    }
    
    try
    {
        Thread t = new Thread(new TileHost());
        t.setDaemon(!debug);
        t.start();        
        
        if(!debug)
        {
            System.console().readLine();
        }
    }
    catch (Exception e)
    {
      System.err.println("Server error: " + e);
    }
  }
}

