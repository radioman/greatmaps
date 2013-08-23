using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using DotSpatial.Projections;
using GMap.NET;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using System.Threading;
using GMap.NET.WindowsForms;
using GMap.NET.CacheProviders;
using System.Drawing;

namespace ConsoleApplication
{
    class test
    {
        //const int batchSize = 3;
        const int logSize = 8;//1024 * 8;
        readonly PointLatLng[] gpsLog = new PointLatLng[logSize];
        int logCounter = 0;
        bool logFull = false;

        public IEnumerable<PointLatLng> GpsLogView()
        {
            int i = 0;
            int skip = 0;

            PointLatLng last = PointLatLng.Empty;

            foreach (var l in GpsLog())
            {
                if (i++ <= 4)
                {
                    last = l;
                    yield return l;
                }
                else
                {                    
                    if(MercatorProjection.Instance.GetDistance(l, last) > 0.1)
                    {
                        last = l;
                        yield return l;
                    }
                }
            }
        }

        public IEnumerable<PointLatLng> GpsLog()
        {
            for (int i = logCounter - 1; i >= 0; i--)
            {
                yield return gpsLog[i];
            }

            if (logFull)
            {
                for (int i = logSize - 1, start = logCounter; i >= start; i--)
                {
                    yield return gpsLog[i];
                }
            }
        }

        public void AddToLogCurrentInfo(PointLatLng data)
        {
            gpsLog[logCounter++] = data;
            if (logCounter == logSize)
            {
                logCounter = 0;
                logFull = true;
            }
        }

        public void Go()
        {
            for (int i = 0; i < 160; i++)
            {
                AddToLogCurrentInfo(new PointLatLng(i, 0));

                Debug.WriteLine("i: " + i);
                foreach (var l in GpsLog())
                {
                    Debug.Write(l.Lat + " ");
                }
                Debug.WriteLine("");
                Debug.WriteLine("-----------");
            }
        }
    }

    class Program
    {
        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Debug.WriteLine("LoadedAssembly.FullName: " + args.LoadedAssembly.FullName);
        }

        static void Main(string[] args)
        {
            //var t = new test();
            //t.Go();

            //return;

            try
            {
                int c = 0;

                int type = GMapProviders.LithuaniaTOP50Map.DbId;

                //GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.MaxValue, type);

                var import = Directory.GetFiles(@"T:\tiles\Layer_NewLayer\", "*.jpg", SearchOption.AllDirectories).Where(p => p.Contains("Layer_") && !p.Contains("black")).ToList();

                int total = import.Count;
                
                foreach (var i in import)
                {
                    //using (Bitmap pic = new Bitmap(i))
                    //{
                    //    for (int ii = 0; ii < pic.Width; ii++)
                    //    {
                    //        for (int j = 0; j < pic.Height; j++)
                    //        {
                    //            if (pic.GetPixel(ii, j) == Color.Black)
                    //            {

                    //            }
                    //        }
                    //    }
                    //}                 

                    var qk = Path.GetFileNameWithoutExtension(i);

                    int x = 0;
                    int y = 0;
                    int z = 0;
                    GMapProviders.BingMap.QuadKeyToTileXY(qk, out x, out y, out z);

                    Debug.WriteLine(c++ + " of " + total + ", x: " + x + ", y: " + y + ", z: " + z);

                    if (!GMaps.Instance.PrimaryCache.PutImageToCache(File.ReadAllBytes(i), type, new GPoint(x, y), z))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("import: " + ex);
            }

            if(false)
            {
                var dirs = Directory.GetDirectories(@"D:\Temp\tmap\TOP50LKS");
                foreach (var dir in dirs)
                {
                    var jpg = Directory.GetFiles(dir, "*.jpg");

                    string files = Path.GetFileName(dir).Replace(" ", string.Empty) + " ";

                    if (File.Exists(@"D:\Temp\tmap\tmp\" + files.Replace(" ", string.Empty) + ".png"))
                    {
                        Debug.WriteLine("SKIP: " + dir);
                        continue;
                    }

                    foreach (var j in jpg)
                    {
                        files += "\"" + j + "\" ";
                    }

                    string ice = @"D:\Projektai\Test\ice\ice2\ICE\bin\x64\Debug\ICE.exe";
                    //string ice = @"C:\Program Files\Microsoft Research\Image Composite Editor\ICE.exe";

                    Debug.WriteLine("process: " + dir);

                    Process.Start(ice, files).WaitForExit();

                    //break;
                }
            }

            return;

            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);

            GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
            GMapProvider.IsSocksProxy = true;

            //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;

            //Debug.WriteLine("DbId: " + GMapProviders.YahooMap.DbId);

            if (false)
            {
                GMaps.Instance.EnableTileHost(8844);

                Console.ReadLine();

                GMaps.Instance.DisableTileHost();
                GMaps.Instance.CancelTileCaching();

                return;
            }

            if (false)
            {
                GeoCoderStatusCode status;
                var pp1 = GMapProviders.GoogleMap.GetPoint("Lithuania, Seda", out status);

                return;
            }

            if (false)
            {
                GeoCoderStatusCode status;
                var pp1 = GMapProviders.GoogleMap.GetPoint("Lithuania, vilnius, Antakalnio g. 67-35", out status);

                List<Placemark> plc = null;
                //var st = GMapProviders.GoogleMap.GetPlacemarks(new PointLatLng(54.6961334816182, 25.2985095977782), out plc);
                var st = GMapProviders.GoogleMap.GetPlacemarks(pp1.Value, out plc);

                if (st == GeoCoderStatusCode.G_GEO_SUCCESS && plc != null)
                {
                    foreach (var pl in plc)
                    {
                        if (!string.IsNullOrEmpty(pl.PostalCodeNumber))
                        {
                            Debug.WriteLine("Accuracy: " + pl.Accuracy + ", " + pl.Address + ", PostalCodeNumber: " + pl.PostalCodeNumber);
                        }
                    }
                }
                return;
            }

            if (false)
            {
                var p1 = new PointLatLng(54.6961334816182, 25.2985095977782);
                var p2 = new PointLatLng(54.7061334816182, 25.3085095977783);

                //GMaps.Instance.ImportFromGMDB(@"C:\Users\m.dambrauskas\AppData\Local\GMap.NET\TileDBv5\en\Data - Copy.gmdb");

                //var route = GMapProviders.OpenStreetMap.GetRoute(p1, p2, false, false, 10);
                //var route = GMapProviders.CloudMadeMap.GetRoute(p1, p2, false, false, 10);

                //Debug.WriteLine(GMapProviders.BingHybridMap.Name + ":" + GMapProviders.BingHybridMap.DbId);

                GeoCoderStatusCode status;
                var pp1 = GMapProviders.GoogleMap.GetPoint("Lithuania,Vilnius", out status);
                var pp2 = GMapProviders.GoogleMap.GetPoint("Lithuania,Kaunas", out status);
                if (pp1.HasValue && pp2.HasValue)
                {
                    GDirections s;
                    //var x = GMapProviders.GoogleMap.GetDirections(out s, "Lithuania,Vilnius", "Lithuania,Kaunas", false, false, false, true);
                    //if(x == DirectionsStatusCode.OK)
                    var x = GMapProviders.GoogleMap.GetDirections(out s, pp1.Value, pp2.Value, false, false, false, false, true);
                    {
                        Debug.WriteLine(s.Summary + ", " + s.Copyrights);
                        Debug.WriteLine(s.StartAddress + " -> " + s.EndAddress);
                        Debug.WriteLine(s.Distance);
                        Debug.WriteLine(s.Duration);
                        foreach (var step in s.Steps)
                        {
                            Debug.WriteLine(step);
                        }
                    }
                }
            }

            if (false)
            {
                var p = PlateCarreeProjectionDarbAe.Instance;

                var l = new PointLatLng(29.4052130085331, 41.522866508209);

                Debug.WriteLine("121 * 256: " + 121 * 256 + "px : Y");
                Debug.WriteLine("144 * 256: " + 144 * 256 + "px : X");

                Debug.WriteLine("l: " + l);

                var px = p.FromLatLngToPixel(l, 0);

                Debug.WriteLine("FromLatLngToPixel: " + px);

                var ll = p.FromPixelToLatLng(px, 0);
                Debug.WriteLine("FromPixelToLatLng: " + ll);


                var tl = p.FromPixelToTileXY(px);

                Debug.WriteLine("FromPixelToTileXY: " + tl);
            }

            /*
               0/1 = 2
               1/2 = 1,5
               2/3 = 2
               3/4 = 2
               4/5 = 2,5
               5/6 = 2
               6/7 = 2
               7/8 = 2
               8/9 = 2,5
               9/10 = 2
               10/11 = 2,5
               11/12 = 2 
            */

            if (false)
            {
                var p = LKS94Projection.Instance;
                //var p = PlateCarreeProjection.Instance;

                var pos = new PointLatLng(54.6961334816182, 25.2985095977783);

                {
                    var zoom = 4;
                    var px = p.FromPixelToTileXY(p.FromLatLngToPixel(pos, zoom));
                    Exception ex = null;
                    var img = GMaps.Instance.GetImageFrom(GMapProviders.LithuaniaMap, px, zoom, out ex);
                    File.WriteAllBytes(zoom + "z-" + px + ".png", img.Data.ToArray());
                }

                for (int i = 0; i < 12; i++)
                {
                    double scale = p.GetGroundResolution(i, pos.Lat);
                    double scale2 = p.GetGroundResolution(i + 1, pos.Lat);

                    var s = scale / scale2;

                    Debug.WriteLine(i + "/" + (i + 1) + " = " + s);
                }
            }

#if DEBUG
            if (false)
            {
                //GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, GMapProviders.GoogleMap.DbId);

                GMaps.Instance.Mode = AccessMode.CacheOnly;

                using (Core c = new Core())
                {
                    //c.compensationOffset = new GPoint(200, 200);

                    c.minZoom = 1;
                    c.maxZoom = 25;
                    c.Zoom = 16;
                    c.Provider = GMapProviders.OpenStreetMap;
                    c.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
                    c.OnMapSizeChanged(400, 400);

                    c.OnMapOpen();

                    Debug.WriteLine("Position: " + c.Position);
                    Debug.WriteLine("renderOffset: " + c.renderOffset);
                    Debug.WriteLine("compensationOffset: " + c.compensationOffset);

                    var l = c.FromLatLngToLocal(c.Position);
                    Debug.WriteLine("local: " + l);

                    var g = c.FromLocalToLatLng(l.X, l.Y);
                    Debug.WriteLine("geo: " + g);

                    //c.ReloadMap();

                    Console.ReadLine();

                    c.OnMapClose();
                }
            }

            if (false)
            {
                int i = 0;

                //while(true)
                {
                    Console.WriteLine(i + " start");
                    Debug.WriteLine(i + " start");

                    //using(Core c = new Core())
                    Core c = new Core();
                    {
                        var f = c.OnMapOpen();

                        Console.WriteLine("wait");
                        Console.ReadLine();

                        //c.OnMapClose();
                    }
                    c = null;

                    Debug.WriteLine("end");
                    Console.WriteLine("end");

                    Console.ReadLine();

                    GC.Collect();

                    //if(i++ > 10)
                    //{
                    //   GC.Collect();
                    //   i = 0;
                    //}
                }
            }
#endif

            if (false)
            {
                //-34,8859309407532, Lng=-58,359375
                PointLatLng p1 = new PointLatLng(-34.608, -58.348);
                PointLatLng p2 = new PointLatLng(-34.608, -58.348);

                //Sets up a array to contain the x and y coordinates
                double[] xy = new double[4] { p1.Lng, p1.Lat, p2.Lng, p2.Lat };

                //An array for the z coordinate
                double[] z = new double[1];
                z[0] = 1;

                ProjectionInfo pStart = KnownCoordinateSystems.Geographic.World.WGS1984;

                //ProjectionInfo pEnd = new ProjectionInfo("+proj=tmerc +lat_0=0 +lon_0=15 +k=0.9996 +x_0=4200000 +y_0=-1300000 +ellps=WGS84 +datum=WGS84 +to_meter=0.03125 +no_defs");
                ProjectionInfo pEnd = new ProjectionInfo("+proj=tmerc +lat_0=-34.629269 +lon_0=-58.4633 +k=0.9999980000000001 +x_0=100000 +y_0=100000 +ellps=intl +units=m +no_defs");

                Reproject.ReprojectPoints(xy, z, pStart, pEnd, 0, 2);

                Debug.WriteLine(" true1: " + (int)xy[0] + "; " + (int)xy[1]);

                //var prj = new MapyCZProjection();
                //{
                //   var p2 = prj.WGSToPP(y, x);

                //   Debug.WriteLine("false1: " + p2[0] + "; " + p2[1]);

                //   var p3 = prj.PPToWGS(p2[0], p2[1]);

                //   Reproject.ReprojectPoints(xy, z, pEnd, pStart, 0, 1);

                //   Debug.WriteLine("");
                //   Debug.WriteLine(" true2: " + xy[0] + "; " + xy[1]);
                //   Debug.WriteLine("false2: " + p3[1] + "; " + p3[0]);
                //}
                // 134400000],PARAMETER["false_northing",-41600000
            }

            // stop caching immediately
            GMaps.Instance.CancelTileCaching();

            //Console.ReadLine();
        }
    }
}
