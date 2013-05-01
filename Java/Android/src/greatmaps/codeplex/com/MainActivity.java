package greatmaps.codeplex.com;

import java.io.File;

import android.app.Activity;
import android.os.Bundle;
import android.os.Environment;
import android.view.Menu;
import android.webkit.WebView;
import android.webkit.WebViewClient;

public class MainActivity extends Activity
{

    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // launch TileHost
        // ///////////////////
        WebView webView = (WebView) findViewById(R.id.webview);
        webView.getSettings().setJavaScriptEnabled(true);

        webView.setWebViewClient(new WebViewClient()
        {
            @Override public boolean shouldOverrideUrlLoading(WebView view, String url)
            {
                view.loadUrl(url);
                return true;
            }
        });

        try
        {
            File sdcard = new File(Environment.getExternalStorageDirectory() + File.separator + "GreatMaps" + File.separator);
            TileHost.Db = sdcard.getAbsolutePath() + File.separator + "Data.gmdb";

            Thread t = new Thread(new TileHost());
            t.setDaemon(true);
            t.start();

            File www = new File(sdcard.getAbsolutePath() + File.separator + "gmap.html");
            webView.loadUrl("file:///" + www.getAbsolutePath());
        }
        catch (Exception e)
        {
            System.out.println("TileHost error: " + e);
        }
        // ///////////////////
    }

    @Override public boolean onCreateOptionsMenu(Menu menu)
    {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

}
