package TileHost;

import java.net.ServerSocket;
import java.util.Date;

public class TileHost implements Runnable
{
    public static int PORT = 8844; // default port
    public static boolean verbose=false;
    public static String Db = "C:\\Users\\radioman\\AppData\\Local\\GMap.NET\\TileDBv5\\en\\Data.gmdb";
    
    @Override
    public void run()
    {
        ServerSocket serverConnect = null;
        try
        {
            GmDb.Init();
            
            serverConnect = new ServerSocket(PORT);
            System.out.println("\nListening for connections on port " + PORT + "...\n");

            while (true) // listen until user halts execution
            {
                TileServer server = new TileServer(serverConnect.accept());
                if (verbose)
                {
                    System.out.println("Connection opened. (" + new Date() + ")");
                }

                Thread t = new Thread(server);
                t.setDaemon(true);
                t.start();
            }
        }
        catch (Exception e)
        {
            System.err.println("Server error: " + e);
        }
        finally
        {
            if (serverConnect != null)
            {
                try
                {
                    serverConnect.close();
                }
                catch (Exception ignore)
                {
                }
            }
        }
    }
}
