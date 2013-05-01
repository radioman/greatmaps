package greatmaps.codeplex.com;

import java.sql.SQLException;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

public class GmDb
{
    static void Init() throws ClassNotFoundException
    {
        // ...
    }

    public static byte[] GetTile(String x, String y, String zoom, String dbId) throws SQLException
    {
        byte[] allBytesInBlob = null;

        SQLiteDatabase db = SQLiteDatabase.openDatabase(TileHost.Db, null, SQLiteDatabase.OPEN_READONLY);
        if (db.isOpen())
        {
            try
            {
                Cursor c = db.rawQuery(String.format("select Tile from TilesData where id = (select id from Tiles where X = %s and Y = %s and Zoom = %s and Type = %s)", x, y, zoom, dbId), null);

                try
                {
                    if (c.moveToFirst())
                    {
                        allBytesInBlob = c.getBlob(0);
                    }
                }
                finally
                {
                    try
                    {
                        c.close();
                    }
                    catch (Exception ignore)
                    {
                    }
                }
            }
            finally
            {
                try
                {
                    db.close();
                }
                catch (Exception ignore)
                {
                }
            }
        }
        return allBytesInBlob;
    }
}
