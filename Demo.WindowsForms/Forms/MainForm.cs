using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Demo.WindowsForms.CustomMarkers;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;

namespace Demo.WindowsForms
{
   public partial class MainForm : Form
   {
      // marker
      GMapMarker currentMarker;
      GMapMarker center;

      // polygons
      GMapPolygon polygon;

      // layers
      GMapOverlay top;
      internal GMapOverlay objects;
      internal GMapOverlay routes;
      internal GMapOverlay polygons;

      // etc
      readonly Random rnd = new Random();
      readonly DescendingComparer ComparerIpStatus = new DescendingComparer();
      GMapMarkerRect CurentRectMarker = null;
      string mobileGpsLog = string.Empty;
      bool isMouseDown = false;
      PointLatLng start;
      PointLatLng end;

      public MainForm()
      {
         InitializeComponent();

         if(!DesignMode)
         {
            // add your custom map db provider
            //GMap.NET.CacheProviders.MySQLPureImageCache ch = new GMap.NET.CacheProviders.MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //MainMap.Manager.ImageCacheSecond = ch;

            // set your proxy here if need
            //MainMap.Manager.Proxy = new WebProxy("10.2.0.100", 8080);
            //MainMap.Manager.Proxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeada");

            // set cache mode only if no internet avaible
            try
            {
               System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("www.google.com");
            }
            catch
            {
               MainMap.Manager.Mode = AccessMode.CacheOnly;
               MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET - Demo.WindowsForms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // config map 
            MainMap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
            MainMap.MapType = MapType.OpenStreetMap;
            MainMap.MinZoom = 1;
            MainMap.MaxZoom = 17;
            MainMap.Zoom = 3;

            // map events
            MainMap.OnCurrentPositionChanged += new CurrentPositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnMarkerClick += new MarkerClick(MainMap_OnMarkerClick);
            MainMap.OnMapZoomChanged += new MapZoomChanged(MainMap_OnMapZoomChanged);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseDown += new MouseEventHandler(MainMap_MouseDown);
            MainMap.MouseUp += new MouseEventHandler(MainMap_MouseUp);
            MainMap.OnMarkerEnter += new MarkerEnter(MainMap_OnMarkerEnter);
            MainMap.OnMarkerLeave += new MarkerLeave(MainMap_OnMarkerLeave);

            // get map type
            comboBoxMapType.DataSource = Enum.GetValues(typeof(MapType));
            comboBoxMapType.SelectedItem = MainMap.MapType;

            // acccess mode
            comboBoxMode.DataSource = Enum.GetValues(typeof(AccessMode));
            comboBoxMode.SelectedItem = GMaps.Instance.Mode;

            // get position
            textBoxLat.Text = MainMap.Position.Lat.ToString(CultureInfo.InvariantCulture);
            textBoxLng.Text = MainMap.Position.Lng.ToString(CultureInfo.InvariantCulture);

            // get cache modes
            checkBoxUseRouteCache.Checked = GMaps.Instance.UseRouteCache;
            checkBoxUseGeoCache.Checked = GMaps.Instance.UseGeocoderCache;

            MobileLogFrom.Value = DateTime.Today;
            MobileLogTo.Value = DateTime.Now;

            // get zoom  
            trackBar1.Minimum = MainMap.MinZoom;
            trackBar1.Maximum = MainMap.MaxZoom;

#if DEBUG
            checkBoxDebug.Checked = true;
#endif

            ToolStripManager.Renderer = new BSE.Windows.Forms.Office2007Renderer();

            // transport demo
            transport.DoWork += new DoWorkEventHandler(transport_DoWork);
            transport.ProgressChanged += new ProgressChangedEventHandler(transport_ProgressChanged);
            transport.WorkerSupportsCancellation = true;
            transport.WorkerReportsProgress = true;

            // Connections
            connectionsWorker.DoWork += new DoWorkEventHandler(connectionsWorker_DoWork);
            connectionsWorker.ProgressChanged += new ProgressChangedEventHandler(connectionsWorker_ProgressChanged);
            connectionsWorker.WorkerSupportsCancellation = true;
            connectionsWorker.WorkerReportsProgress = true;

            ipInfoSearchWorker.DoWork += new DoWorkEventHandler(ipInfoSearchWorker_DoWork);
            ipInfoSearchWorker.WorkerSupportsCancellation = true;

            iptracerWorker.DoWork += new DoWorkEventHandler(iptracerWorker_DoWork);
            iptracerWorker.WorkerSupportsCancellation = true;

            GridConnections.AutoGenerateColumns = false;
            IpCache.CacheLocation = MainMap.CacheLocation;

            // perf
            timerPerf.Tick += new EventHandler(timer_Tick);

            // add custom layers  
            {
               routes = new GMapOverlay(MainMap, "routes");
               MainMap.Overlays.Add(routes);

               polygons = new GMapOverlay(MainMap, "polygons");
               MainMap.Overlays.Add(polygons);

               objects = new GMapOverlay(MainMap, "objects");
               MainMap.Overlays.Add(objects);

               top = new GMapOverlay(MainMap, "top");
               MainMap.Overlays.Add(top);

               routes.Routes.CollectionChanged += new GMap.NET.ObjectModel.NotifyCollectionChangedEventHandler(Routes_CollectionChanged);
               objects.Markers.CollectionChanged += new GMap.NET.ObjectModel.NotifyCollectionChangedEventHandler(Markers_CollectionChanged);
            }

            // set current marker
            currentMarker = new GMapMarkerGoogleRed(MainMap.Position);
            currentMarker.IsHitTestVisible = false;
            top.Markers.Add(currentMarker);

            // map center
            center = new GMapMarkerCross(MainMap.Position);
            top.Markers.Add(center);

            //MainMap.VirtualSizeEnabled = true;
            //if(false)
            {
               // add my city location for demo
               GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
               {
                  PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, Vilnius", out status);
                  if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                  {
                     currentMarker.Position = pos.Value;

                     GMapMarker myCity = new GMapMarkerGoogleGreen(pos.Value);
                     myCity.ToolTipMode = MarkerTooltipMode.Always;
                     myCity.ToolTipText = "Welcome to Lithuania! ;}";
                     objects.Markers.Add(myCity);
                  }
               }

               // add some points in lithuania
               AddLocationLithuania("Kaunas");
               AddLocationLithuania("Klaipėda");
               AddLocationLithuania("Šiauliai");
               AddLocationLithuania("Panevėžys");

               RegeneratePolygon();
            }
         }
      }

      void Markers_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
      {
         textBoxMarkerCount.Text = objects.Markers.Count.ToString();
      }

      void Routes_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
      {
         textBoxrouteCount.Text = routes.Routes.Count.ToString();
      }

      #region -- performance test --

      double NextDouble(Random rng, double min, double max)
      {
         return min + (rng.NextDouble() * (max - min));
      }

      int tt = 0;
      void timer_Tick(object sender, EventArgs e)
      {
         var pos = new PointLatLng(NextDouble(rnd, MainMap.CurrentViewArea.Top, MainMap.CurrentViewArea.Bottom), NextDouble(rnd, MainMap.CurrentViewArea.Left, MainMap.CurrentViewArea.Right));
         GMapMarker m = new GMapMarkerGoogleGreen(pos);
         {
            m.ToolTipText = (tt++).ToString();
            m.ToolTipMode = MarkerTooltipMode.Always;
         }

         objects.Markers.Add(m);

         if(tt >= 333)
         {
            timerPerf.Stop();
            tt = 0;
         }
      }

      System.Windows.Forms.Timer timerPerf = new System.Windows.Forms.Timer();
      #endregion

      #region -- transport demo --
      BackgroundWorker transport = new BackgroundWorker();

      readonly List<VehicleData> trolleybus = new List<VehicleData>();
      readonly Dictionary<int, GMapMarker> trolleybusMarkers = new Dictionary<int, GMapMarker>();

      readonly List<VehicleData> bus = new List<VehicleData>();
      readonly Dictionary<int, GMapMarker> busMarkers = new Dictionary<int, GMapMarker>();

      bool firstLoadTrasport = true;
      GMapMarker currentTransport;

      void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         // stops immediate marker/route/polygon invalidations;
         // call Refresh to perform single refresh and reset invalidation state
         MainMap.HoldInvalidation = true;

         lock(trolleybus)
         {
            foreach(VehicleData d in trolleybus)
            {
               GMapMarker marker;

               if(!trolleybusMarkers.TryGetValue(d.Id, out marker))
               {
                  marker = new GMapMarkerGoogleRed(new PointLatLng(d.Lat, d.Lng));
                  marker.Tag = d.Id;
                  marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

                  trolleybusMarkers[d.Id] = marker;
                  objects.Markers.Add(marker);
               }
               else
               {
                  marker.Position = new PointLatLng(d.Lat, d.Lng);
                  (marker as GMapMarkerGoogleRed).Bearing = (float?) d.Bearing;
               }
               marker.ToolTipText = "Trolley " + d.Line + (d.Bearing.HasValue ? ", bearing: " + d.Bearing.Value.ToString() : string.Empty) + ", " + d.Time;

               if(currentTransport != null && currentTransport == marker)
               {
                  MainMap.Position = marker.Position;
                  if(d.Bearing.HasValue)
                  {
                     MainMap.Bearing = (float) d.Bearing.Value;
                  }
               }
            }
         }

         lock(bus)
         {
            foreach(VehicleData d in bus)
            {
               GMapMarker marker;

               if(!busMarkers.TryGetValue(d.Id, out marker))
               {
                  marker = new GMapMarkerGoogleGreen(new PointLatLng(d.Lat, d.Lng));
                  marker.Tag = d.Id;
                  marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

                  busMarkers[d.Id] = marker;
                  objects.Markers.Add(marker);
               }
               else
               {
                  marker.Position = new PointLatLng(d.Lat, d.Lng);
                  (marker as GMapMarkerGoogleGreen).Bearing = (float?) d.Bearing;
               }
               marker.ToolTipText = "Bus " + d.Line + (d.Bearing.HasValue ? ", bearing: " + d.Bearing.Value.ToString() : string.Empty) + ", " + d.Time;

               if(currentTransport != null && currentTransport == marker)
               {
                  MainMap.Position = marker.Position;
                  if(d.Bearing.HasValue)
                  {
                     MainMap.Bearing = (float) d.Bearing.Value;
                  }
               }
            }
         }

         if(firstLoadTrasport)
         {
            MainMap.ZoomAndCenterMarkers("objects");
            firstLoadTrasport = false;
         }
         MainMap.Refresh();
      }

      void transport_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!transport.CancellationPending)
         {
            try
            {
               lock(trolleybus)
               {
                  MainMap.Manager.GetVilniusTransportData(GMap.NET.TransportType.TrolleyBus, string.Empty, trolleybus);
               }

               lock(bus)
               {
                  MainMap.Manager.GetVilniusTransportData(GMap.NET.TransportType.Bus, string.Empty, bus);
               }

               transport.ReportProgress(100);
            }
            catch(Exception ex)
            {
               Debug.WriteLine("transport_DoWork: " + ex.ToString());
            }
            Thread.Sleep(3333);
         }
         trolleybusMarkers.Clear();
         busMarkers.Clear();
      }

      #endregion

      #region -- tcp/ip connections demo --
      BackgroundWorker connectionsWorker = new BackgroundWorker();
      BackgroundWorker ipInfoSearchWorker = new BackgroundWorker();
      BackgroundWorker iptracerWorker = new BackgroundWorker();

      readonly Dictionary<string, IpInfo> TcpState = new Dictionary<string, IpInfo>();
      readonly Dictionary<string, IpInfo> TcpTracePoints = new Dictionary<string, IpInfo>();
      readonly Dictionary<string, List<PingReply>> TraceRoutes = new Dictionary<string, List<PingReply>>();

      readonly List<string> TcpStateNeedLocationInfo = new List<string>();
      readonly Queue<string> TcpStateNeedtraceInfo = new Queue<string>();

      volatile bool TryTraceConnection = false;
      GMapMarker lastTcpmarker;
      readonly SQLiteIpCache IpCache = new SQLiteIpCache();

      readonly Dictionary<string, GMapMarker> tcpConnections = new Dictionary<string, GMapMarker>();
      readonly Dictionary<string, GMapRoute> tcpRoutes = new Dictionary<string, GMapRoute>();

      readonly List<IpStatus> CountryStatusView = new List<IpStatus>();
      readonly SortedDictionary<string, int> CountryStatus = new SortedDictionary<string, int>();

      readonly List<string> SelectedCountries = new List<string>();
      readonly Dictionary<int, Process> ProcessList = new Dictionary<int, Process>();

      void ipInfoSearchWorker_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!ipInfoSearchWorker.CancellationPending)
         {
            try
            {
               string iplist = string.Empty;

               lock(TcpStateNeedLocationInfo)
               {
                  //int count = 0;
                  foreach(var info in TcpStateNeedLocationInfo)
                  {
                     if(iplist.Length > 0)
                     {
                        iplist += ",";
                     }
                     iplist += info;

                     //if(count++ >= 1)
                     {
                        break;
                     }
                  }
               }

               // fill location info
               if(!string.IsNullOrEmpty(iplist))
               {
                  List<IpInfo> ips = GetIpHostInfo(iplist);
                  foreach(var i in ips)
                  {
                     lock(TcpState)
                     {
                        IpInfo info;
                        if(TcpState.TryGetValue(i.Ip, out info))
                        {
                           info.CountryName = i.CountryName;
                           info.RegionName = i.RegionName;
                           info.City = i.City;
                           info.Latitude = i.Latitude;
                           info.Longitude = i.Longitude;
                           info.TracePoint = false;

                           if(info.CountryName != "Reserved")
                           {
                              info.Ip = i.Ip;

                              // add host for tracing
                              lock(TcpStateNeedtraceInfo)
                              {
                                 if(!TcpStateNeedtraceInfo.Contains(i.Ip))
                                 {
                                    TcpStateNeedtraceInfo.Enqueue(i.Ip);
                                 }
                              }
                           }

                           lock(TcpStateNeedLocationInfo)
                           {
                              TcpStateNeedLocationInfo.Remove(i.Ip);

                              Debug.WriteLine("TcpStateNeedLocationInfo: " + TcpStateNeedLocationInfo.Count + " left...");
                           }
                        }
                     }
                  }
                  ips.Clear();
               }
               else
               {
                  break;
               }
            }
            catch(Exception ex)
            {
               Debug.WriteLine("ipInfoSearchWorker_DoWork: " + ex.ToString());
            }
            Thread.Sleep(1111);
         }
         Debug.WriteLine("ipInfoSearchWorker_DoWork: QUIT");
      }

      void iptracerWorker_DoWork(object sender, DoWorkEventArgs e)
      {
         while(!iptracerWorker.CancellationPending)
         {
            try
            {
               string Ip = string.Empty;
               int count = 0;
               lock(TcpStateNeedtraceInfo)
               {
                  count = TcpStateNeedtraceInfo.Count;
                  if(count > 0)
                  {
                     Ip = TcpStateNeedtraceInfo.Dequeue();
                  }
               }

               if(!string.IsNullOrEmpty(Ip))
               {
                  string tracertIps = string.Empty;

                  List<PingReply> tracert;

                  bool contains = false;
                  lock(TraceRoutes)
                  {
                     contains = TraceRoutes.TryGetValue(Ip, out tracert);
                  }

                  if(!contains)
                  {
                     Debug.WriteLine("GetTraceRoute: " + Ip + ", left " + count);

                     tracert = TraceRoute.GetTraceRoute(Ip);
                     if(tracert != null)
                     {
                        if(tracert[tracert.Count - 1].Status == IPStatus.Success)
                        {
                           foreach(var t in tracert)
                           {
                              if(!t.ToString().StartsWith("192.168.") && !t.ToString().StartsWith("127.0."))
                              {
                                 if(tracertIps.Length > 0)
                                 {
                                    tracertIps += ",";
                                 }
                                 tracertIps += t.Address.ToString();
                              }
                           }

                           if(!string.IsNullOrEmpty(tracertIps))
                           {
                              List<IpInfo> tinfo = GetIpHostInfo(tracertIps);
                              if(tinfo.Count > 0)
                              {
                                 for(int i = 0; i < tinfo.Count; i++)
                                 {
                                    IpInfo ti = tinfo[i];
                                    ti.TracePoint = true;

                                    if(ti.CountryName != "Reserved")
                                    {
                                       lock(TcpTracePoints)
                                       {
                                          TcpTracePoints[ti.Ip] = ti;
                                       }
                                    }
                                 }
                                 tinfo.Clear();

                                 lock(TraceRoutes)
                                 {
                                    TraceRoutes[Ip] = tracert;
                                 }
                              }
                           }
                        }
                        else
                        {
                           // move failed, eque itself again
                           lock(TcpStateNeedtraceInfo)
                           {
                              TcpStateNeedtraceInfo.Enqueue(Ip);
                           }
                        }
                     }
                  }
               }
               else
               {
                  break;
               }
            }
            catch(Exception ex)
            {
               Debug.WriteLine("iptracerWorker_DoWork: " + ex.ToString());
            }
            Thread.Sleep(3333);
         }
         Debug.WriteLine("iptracerWorker_DoWork: QUIT");
      }

      void connectionsWorker_DoWork(object sender, DoWorkEventArgs e)
      {
         IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

         while(!connectionsWorker.CancellationPending)
         {
            try
            {
               #region -- xml --
               // http://ipinfodb.com/ip_location_api.php

               // http://ipinfodb.com/ip_query2.php?ip=74.125.45.100,206.190.60.37&timezone=false

               //<?xml version="1.0" encoding="UTF-8"?>
               //<Locations>
               //  <Location id="0">
               //    <Ip>74.125.45.100</Ip>
               //    <Status>OK</Status>
               //    <CountryCode>US</CountryCode>
               //    <CountryName>United States</CountryName>
               //    <RegionCode>06</RegionCode>
               //    <RegionName>California</RegionName>
               //    <City>Mountain View</City>
               //    <ZipPostalCode>94043</ZipPostalCode>
               //    <Latitude>37.4192</Latitude>
               //    <Longitude>-122.057</Longitude>
               //  </Location> 
               #endregion

               lock(TcpState)
               {
                  //TcpConnectionInformation[] tcpInfoList = properties.GetActiveTcpConnections();
                  //foreach(TcpConnectionInformation i in tcpInfoList)

                  CountryStatus.Clear();
                  ManagedIpHelper.UpdateExtendedTcpTable(false);

                  foreach(TcpRow i in ManagedIpHelper.TcpRows)
                  {
                     #region -- update TcpState --
                     string Ip = i.RemoteEndPoint.Address.ToString();

                     // exclude local network
                     if(!Ip.StartsWith("192.168.") && !Ip.StartsWith("127.0."))
                     {
                        IpInfo info;
                        if(!TcpState.TryGetValue(Ip, out info))
                        {
                           info = new IpInfo();
                           TcpState[Ip] = info;

                           // request location info
                           lock(TcpStateNeedLocationInfo)
                           {
                              if(!TcpStateNeedLocationInfo.Contains(Ip))
                              {
                                 TcpStateNeedLocationInfo.Add(Ip);

                                 if(!ipInfoSearchWorker.IsBusy)
                                 {
                                    ipInfoSearchWorker.RunWorkerAsync();
                                 }
                              }
                           }
                        }

                        info.State = i.State;
                        info.Port = i.RemoteEndPoint.Port;
                        info.StatusTime = DateTime.Now;

                        try
                        {
                           Process p;
                           if(!ProcessList.TryGetValue(i.ProcessId, out p))
                           {
                              p = Process.GetProcessById(i.ProcessId);
                              ProcessList[i.ProcessId] = p;
                           }
                           info.ProcessName = p.ProcessName;
                        }
                        catch
                        {
                        }

                        if(!string.IsNullOrEmpty(info.CountryName))
                        {
                           if(!CountryStatus.ContainsKey(info.CountryName))
                           {
                              CountryStatus[info.CountryName] = 1;
                           }
                           else
                           {
                              CountryStatus[info.CountryName]++;
                           }
                        }
                     }
                     #endregion
                  }
               }

               // launch tracer if needed
               if(TryTraceConnection)
               {
                  if(!iptracerWorker.IsBusy)
                  {
                     lock(TcpStateNeedtraceInfo)
                     {
                        if(TcpStateNeedtraceInfo.Count > 0)
                        {
                           iptracerWorker.RunWorkerAsync();
                        }
                     }
                  }
               }

               connectionsWorker.ReportProgress(100);
            }
            catch(Exception ex)
            {
               Debug.WriteLine("connectionsWorker_DoWork: " + ex.ToString());
            }
            Thread.Sleep(3333);
         }
         tcpConnections.Clear();
      }

      void connectionsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         try
         {
            // stops immediate marker/route/polygon invalidations;
            // call Refresh to perform single refresh and reset invalidation state
            MainMap.HoldInvalidation = true;

            SelectedCountries.Clear();
            Int32 SelectedCountriesCount = GridConnections.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if(SelectedCountriesCount > 0)
            {
               for(int i = 0; i < SelectedCountriesCount; i++)
               {
                  string country = GridConnections.SelectedRows[i].Cells[0].Value as string;
                  SelectedCountries.Add(country);
               }
            }

            ComparerIpStatus.SortOnlyCountryName = !(SelectedCountriesCount == 0);

            lock(TcpState)
            {
               bool snap = true;
               foreach(var tcp in TcpState)
               {
                  GMapMarker marker;

                  if(!tcpConnections.TryGetValue(tcp.Key, out marker))
                  {
                     if(!string.IsNullOrEmpty(tcp.Value.Ip))
                     {
                        marker = new GMapMarkerGoogleGreen(new PointLatLng(tcp.Value.Latitude, tcp.Value.Longitude));
                        marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                        marker.Tag = tcp.Value.CountryName;

                        tcpConnections[tcp.Key] = marker;
                        {
                           if(!(SelectedCountriesCount > 0 && !SelectedCountries.Contains(tcp.Value.CountryName)))
                           {
                              objects.Markers.Add(marker);

                              UpdateMarkerTcpIpToolTip(marker, tcp.Value, "(" + objects.Markers.Count + ") ");

                              if(snap)
                              {
                                 if(checkBoxTcpIpSnap.Checked && !MainMap.IsDragging)
                                 {
                                    MainMap.Position = marker.Position;
                                 }
                                 snap = false;

                                 if(lastTcpmarker != null)
                                 {
                                    marker.ToolTipMode = MarkerTooltipMode.Always;
                                    lastTcpmarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                                 }

                                 lastTcpmarker = marker;
                              }
                           }
                        }
                     }
                  }
                  else
                  {
                     if((DateTime.Now - tcp.Value.StatusTime > TimeSpan.FromSeconds(8)) || (SelectedCountriesCount > 0 && !SelectedCountries.Contains(tcp.Value.CountryName)))
                     {
                        objects.Markers.Remove(marker);

                        GMapRoute route;
                        if(tcpRoutes.TryGetValue(tcp.Key, out route))
                        {
                           routes.Routes.Remove(route);
                        }

                        lock(TcpStateNeedLocationInfo)
                        {
                           bool r = TcpStateNeedLocationInfo.Remove(tcp.Key);
                           if(r)
                           {
                              Debug.WriteLine("TcpStateNeedLocationInfo: removed " + tcp.Key + " " + r);
                           }
                        }
                     }
                     else
                     {
                        marker.Position = new PointLatLng(tcp.Value.Latitude, tcp.Value.Longitude);
                        if(!objects.Markers.Contains(marker))
                        {
                           objects.Markers.Add(marker);
                        }
                        UpdateMarkerTcpIpToolTip(marker, tcp.Value, string.Empty);

                        if(TryTraceConnection)
                        {
                           // routes
                           GMapRoute route;
                           if(!this.tcpRoutes.TryGetValue(tcp.Key, out route))
                           {
                              lock(TraceRoutes)
                              {
                                 List<PingReply> tr;
                                 if(TraceRoutes.TryGetValue(tcp.Key, out tr))
                                 {
                                    if(tr != null)
                                    {
                                       List<PointLatLng> points = new List<PointLatLng>();
                                       foreach(var add in tr)
                                       {
                                          IpInfo info;

                                          lock(TcpTracePoints)
                                          {
                                             if(TcpTracePoints.TryGetValue(add.Address.ToString(), out info))
                                             {
                                                if(!string.IsNullOrEmpty(info.Ip))
                                                {
                                                   points.Add(new PointLatLng(info.Latitude, info.Longitude));
                                                }
                                             }
                                          }
                                       }

                                       if(points.Count > 0)
                                       {
                                          route = new GMapRoute(points, tcp.Value.CountryName);

                                          route.Stroke = new Pen(GetRandomColor());
                                          route.Stroke.Width = 4;
                                          route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                                          route.Stroke.StartCap = LineCap.NoAnchor;
                                          route.Stroke.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                                          route.Stroke.LineJoin = LineJoin.Round;

                                          routes.Routes.Add(route);
                                          tcpRoutes[tcp.Key] = route;
                                       }
                                    }
                                 }
                              }
                           }
                           else
                           {
                              if(!routes.Routes.Contains(route))
                              {
                                 routes.Routes.Add(route);
                              }
                           }
                        }
                     }
                  }
               }

               // update grid data
               if(panelMenu.Expand && xPanderPanelLive.Expand)
               {
                  bool empty = CountryStatusView.Count == 0;

                  if(!ComparerIpStatus.SortOnlyCountryName)
                  {
                     CountryStatusView.Clear();
                  }

                  foreach(var c in CountryStatus)
                  {
                     IpStatus s = new IpStatus();
                     {
                        s.CountryName = c.Key;
                        s.ConnectionsCount = c.Value;
                     }

                     if(ComparerIpStatus.SortOnlyCountryName)
                     {
                        int idx = CountryStatusView.FindIndex(p => p.CountryName == c.Key);
                        if(idx >= 0)
                        {
                           CountryStatusView[idx] = s;
                        }
                     }
                     else
                     {
                        CountryStatusView.Add(s);
                     }
                  }

                  CountryStatusView.Sort(ComparerIpStatus);

                  GridConnections.RowCount = CountryStatusView.Count;
                  GridConnections.Refresh();

                  if(empty)
                  {
                     GridConnections.ClearSelection();
                  }
               }
            }

            MainMap.Refresh();
         }
         catch(Exception ex)
         {
            Debug.WriteLine(ex.ToString());
         }
      }

      void GridConnections_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
      {
         if(e.RowIndex >= CountryStatusView.Count)
            return;

         IpStatus val = CountryStatusView[e.RowIndex];

         switch(GridConnections.Columns[e.ColumnIndex].Name)
         {
            case "CountryName":
            e.Value = val.CountryName;
            break;

            case "ConnectionsCount":
            e.Value = val.ConnectionsCount;
            break;
         }
      }

      Color GetRandomColor()
      {
         byte r = Convert.ToByte(rnd.Next(0, 111));
         byte g = Convert.ToByte(rnd.Next(0, 111));
         byte b = Convert.ToByte(rnd.Next(0, 111));

         return Color.FromArgb(144, r, g, b);
      }

      void UpdateMarkerTcpIpToolTip(GMapMarker marker, IpInfo tcp, string info)
      {
         marker.ToolTipText = tcp.State.ToString();

         if(!string.IsNullOrEmpty(tcp.ProcessName))
         {
            marker.ToolTipText += " by " + tcp.ProcessName;
         }

         if(!string.IsNullOrEmpty(tcp.CountryName))
         {
            marker.ToolTipText += "\n" + tcp.CountryName;
         }

         if(!string.IsNullOrEmpty(tcp.City))
         {
            marker.ToolTipText += ", " + tcp.City;
         }
         else
         {
            if(!string.IsNullOrEmpty(tcp.RegionName))
            {
               marker.ToolTipText += ", " + tcp.RegionName;
            }
         }

         marker.ToolTipText += "\n" + tcp.Ip + ":" + tcp.Port + "\n" + info;
      }

      List<IpInfo> GetIpHostInfo(string iplist)
      {
         List<IpInfo> ret = new List<IpInfo>();
         bool retry = false;

         string iplistNew = string.Empty;

         string[] ips = iplist.Split(',');
         foreach(var ip in ips)
         {
            IpInfo val = IpCache.GetDataFromCache(ip);
            if(val != null)
            {
               ret.Add(val);
            }
            else
            {
               if(iplistNew.Length > 0)
               {
                  iplistNew += ",";
               }
               iplistNew += ip;
            }
         }

         if(!string.IsNullOrEmpty(iplistNew))
         {
            Debug.WriteLine("GetIpHostInfo: " + iplist);

            string reqUrl = string.Format("http://api.ipinfodb.com/v2/ip_query.php?key=fbea1992ab11f7125064590a417a8461ccaf06728798c718dbd2809b31a7a5e0&ip={0}&timezone=false", iplistNew);

            while(true)
            {
               ret.Clear();
               try
               {
                  HttpWebRequest httpReq = HttpWebRequest.Create(reqUrl) as HttpWebRequest;
                  {
                     string result = string.Empty;
                     using(HttpWebResponse response = httpReq.GetResponse() as HttpWebResponse)
                     {
                        using(StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                        {
                           result = reader.ReadToEnd();
                        }
                        response.Close();
                     }

                     XmlDocument x = new XmlDocument();
                     x.LoadXml(result);

                     XmlNodeList nodes = x.SelectNodes("/Response");
                     foreach(XmlNode node in nodes)
                     {
                        string Ip = node.SelectSingleNode("Ip").InnerText;

                        IpInfo info = new IpInfo();
                        {
                           info.Ip = Ip;
                           info.CountryName = node.SelectSingleNode("CountryName").InnerText;
                           info.RegionName = node.SelectSingleNode("RegionName").InnerText;
                           info.City = node.SelectSingleNode("City").InnerText;
                           info.Latitude = double.Parse(node.SelectSingleNode("Latitude").InnerText, CultureInfo.InvariantCulture);
                           info.Longitude = double.Parse(node.SelectSingleNode("Longitude").InnerText, CultureInfo.InvariantCulture);
                           info.CacheTime = DateTime.Now;

                           ret.Add(info);
                        }

                        IpCache.PutDataToCache(Ip, info);
                     }
                  }

                  break;
               }
               catch(Exception ex)
               {
                  if(retry) // fail in retry too, full stop ;}
                  {
                     break;
                  }
                  retry = true;
                  reqUrl = string.Format("http://backup.ipinfodb.com/v2/ip_query.php?key=fbea1992ab11f7125064590a417a8461ccaf06728798c718dbd2809b31a7a5e0&ip={0}&timezone=false", iplist);
                  Debug.WriteLine("GetIpHostInfo: " + ex.Message + ", retry on backup server...");
               }
            }
         }
         return ret;
      }

      #endregion

      #region -- some functions --

      void RegeneratePolygon()
      {
         List<PointLatLng> polygonPoints = new List<PointLatLng>();

         foreach(GMapMarker m in objects.Markers)
         {
            if(m is GMapMarkerRect)
            {
               m.Tag = polygonPoints.Count;
               polygonPoints.Add(m.Position);
            }
         }

         if(polygon == null)
         {
            polygon = new GMapPolygon(polygonPoints, "polygon test");
            polygons.Polygons.Add(polygon);
         }
         else
         {
            polygon.Points.Clear();
            polygon.Points.AddRange(polygonPoints);

            if(polygons.Polygons.Count == 0)
            {
               polygons.Polygons.Add(polygon);
            }
            else
            {
               MainMap.UpdatePolygonLocalPosition(polygon);
            }
         }
      }

      // testing my mobile gps log
      void AddGpsMobileLogRoutes(string file)
      {
         try
         {
            DateTime? date = null;
            DateTime? dateEnd = null;

            if(MobileLogFrom.Checked)
            {
               date = MobileLogFrom.Value.ToUniversalTime();
            }

            if(MobileLogTo.Checked)
            {
               dateEnd = MobileLogTo.Value.ToUniversalTime();
            }

            var log = GMaps.Instance.GetRoutesFromMobileLog(file, date, dateEnd, 3.3);

            if(routes != null)
            {
               List<PointLatLng> track = new List<PointLatLng>();

               var sessions = new List<List<GpsLog>>(log);

               foreach(var session in sessions)
               {
                  track.Clear();

                  foreach(var point in session)
                  {
                     track.Add(point.Position);
                  }

                  GMapRoute gr = new GMapRoute(track, "");

                  routes.Routes.Add(gr);
               }

               sessions.Clear();
               sessions = null;

               track.Clear();
               track = null;
            }
         }
         catch(Exception ex)
         {
            Debug.WriteLine("AddGpsMobileLogRoutes: " + ex.ToString());
         }
      }

      /// <summary>
      /// adds marker using geocoder
      /// </summary>
      /// <param name="place"></param>
      void AddLocationLithuania(string place)
      {
         GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
         PointLatLng? pos = GMaps.Instance.GetLatLngFromGeocoder("Lithuania, " + place, out status);
         if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
         {
            GMapMarkerGoogleGreen m = new GMapMarkerGoogleGreen(pos.Value);
            m.ToolTip = new GMapRoundedToolTip(m);

            GMapMarkerRect mBorders = new GMapMarkerRect(pos.Value);
            {
               mBorders.InnerMarker = m;
               mBorders.ToolTipText = place;
               mBorders.ToolTipMode = MarkerTooltipMode.Always;
            }

            objects.Markers.Add(m);
            objects.Markers.Add(mBorders);
         }
      }

      #endregion

      #region -- map events --
      void MainMap_OnMarkerLeave(GMapMarker item)
      {
         if(item is GMapMarkerRect)
         {
            CurentRectMarker = null;

            GMapMarkerRect rc = item as GMapMarkerRect;
            rc.Pen.Color = Color.Blue;
            MainMap.Invalidate(false);

            Debug.WriteLine("OnMarkerLeave: " + item.Position);
         }
      }

      void MainMap_OnMarkerEnter(GMapMarker item)
      {
         if(item is GMapMarkerRect)
         {
            GMapMarkerRect rc = item as GMapMarkerRect;
            rc.Pen.Color = Color.Red;
            MainMap.Invalidate(false);

            CurentRectMarker = rc;

            Debug.WriteLine("OnMarkerEnter: " + item.Position);
         }
      }

      void MainMap_OnMapTypeChanged(MapType type)
      {
         comboBoxMapType.SelectedItem = MainMap.MapType;

         trackBar1.Minimum = MainMap.MinZoom;
         trackBar1.Maximum = MainMap.MaxZoom;

         if(routes.Routes.Count > 0)
         {
            MainMap.ZoomAndCenterRoutes(null);
         }

         if(radioButtonTransport.Checked)
         {
            MainMap.ZoomAndCenterMarkers("objects");
         }
      }

      void MainMap_MouseUp(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left)
         {
            isMouseDown = false;
         }
      }

      void MainMap_MouseDown(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left)
         {
            isMouseDown = true;

            if(currentMarker.IsVisible)
            {
               currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);

               var px = MainMap.Projection.FromLatLngToPixel(currentMarker.Position.Lat, currentMarker.Position.Lng, (int) MainMap.Zoom);
               var tile = MainMap.Projection.FromPixelToTileXY(px);

               Debug.WriteLine("MouseDown: " + currentMarker.LocalPosition + " | geo: " + currentMarker.Position + " | px: " + px + " | tile: " + tile);
            }
         }
      }

      // move current marker with left holding
      void MainMap_MouseMove(object sender, MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left && isMouseDown)
         {
            if(CurentRectMarker == null)
            {
               if(currentMarker.IsVisible)
               {
                  currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
               }
            }
            else // move rect marker
            {
               PointLatLng pnew = MainMap.FromLocalToLatLng(e.X, e.Y);

               int? pIndex = (int?) CurentRectMarker.Tag;
               if(pIndex.HasValue)
               {
                  if(pIndex < polygon.Points.Count)
                  {
                     polygon.Points[pIndex.Value] = pnew;
                     MainMap.UpdatePolygonLocalPosition(polygon);
                  }
               }

               if(currentMarker.IsVisible)
               {
                  currentMarker.Position = pnew;
               }
               CurentRectMarker.Position = pnew;

               if(CurentRectMarker.InnerMarker != null)
               {
                  CurentRectMarker.InnerMarker.Position = pnew;
               }
            }
         }
      }

      // MapZoomChanged
      void MainMap_OnMapZoomChanged()
      {
         trackBar1.Value = (int) (MainMap.Zoom);
         textBoxZoomCurrent.Text = MainMap.Zoom.ToString();
         center.Position = MainMap.Position;
      }

      // click on some marker
      void MainMap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
      {
         if(e.Button == System.Windows.Forms.MouseButtons.Left)
         {
            if(item is GMapMarkerRect)
            {
               Placemark pos = GMaps.Instance.GetPlacemarkFromGeocoder(item.Position);
               if(pos != null)
               {
                  GMapMarkerRect v = item as GMapMarkerRect;
                  {
                     v.ToolTipText = pos.Address;
                  }
                  MainMap.Invalidate(false);
               }
            }
            else
            {
               if(item.Tag != null)
               {
                  if(currentTransport != null)
                  {
                     currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                     currentTransport = null;
                  }
                  currentTransport = item;
                  currentTransport.ToolTipMode = MarkerTooltipMode.Always;
               }
            }
         }
      }

      // loader start loading tiles
      void MainMap_OnTileLoadStart()
      {
         MethodInvoker m = delegate()
         {
            panelMenu.Text = "Menu: loading tiles...";
         };
         try
         {
            BeginInvoke(m);
         }
         catch
         {
         }
      }

      // loader end loading tiles
      void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
      {
         MainMap.ElapsedMilliseconds = ElapsedMilliseconds;

         MethodInvoker m = delegate()
         {
            panelMenu.Text = "Menu, last load in " + MainMap.ElapsedMilliseconds + "ms";

            textBoxMemory.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.00}MB of {1:0.00}MB", MainMap.Manager.MemoryCacheSize, MainMap.Manager.MemoryCacheCapacity);
         };
         try
         {
            BeginInvoke(m);
         }
         catch
         {
         }
      }

      // current point changed
      void MainMap_OnCurrentPositionChanged(PointLatLng point)
      {
         center.Position = point;
         textBoxLatCurrent.Text = point.Lat.ToString(CultureInfo.InvariantCulture);
         textBoxLngCurrent.Text = point.Lng.ToString(CultureInfo.InvariantCulture);
      }

      // center markers on start
      private void MainForm_Load(object sender, EventArgs e)
      {
         if(objects.Markers.Count > 0)
         {
            MainMap.ZoomAndCenterMarkers(null);
            trackBar1.Value = (int) MainMap.Zoom;
         }
      }

      // ensure focus on map, trackbar can have it too
      private void MainMap_MouseEnter(object sender, EventArgs e)
      {
         MainMap.Focus();
      }
      #endregion

      #region -- menu panels expanders --
      private void xPanderPanel1_Click(object sender, EventArgs e)
      {
         xPanderPanelList1.Expand(xPanderPanelMain);
      }

      private void xPanderPanelCache_Click(object sender, EventArgs e)
      {
         xPanderPanelList1.Expand(xPanderPanelCache);
      }

      private void xPanderPanelLive_Click(object sender, EventArgs e)
      {
         xPanderPanelList1.Expand(xPanderPanelLive);
      }

      private void xPanderPanelInfo_Click(object sender, EventArgs e)
      {
         xPanderPanelList1.Expand(xPanderPanelInfo);
      }
      #endregion

      #region -- ui events --
      // change map type
      private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
      {
         MainMap.MapType = (MapType) comboBoxMapType.SelectedValue;
      }

      // change mdoe
      private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
      {
         GMaps.Instance.Mode = (AccessMode) comboBoxMode.SelectedValue;
         MainMap.ReloadMap();
      }

      // zoom
      private void trackBar1_ValueChanged(object sender, EventArgs e)
      {
         MainMap.Zoom = (trackBar1.Value);
      }

      // go to
      private void button8_Click(object sender, EventArgs e)
      {
         double lat = double.Parse(textBoxLat.Text, CultureInfo.InvariantCulture);
         double lng = double.Parse(textBoxLng.Text, CultureInfo.InvariantCulture);

         MainMap.Position = new PointLatLng(lat, lng);
      }

      // goto by geocoder
      private void textBoxGeo_KeyPress(object sender, KeyPressEventArgs e)
      {
         if((Keys) e.KeyChar == Keys.Enter)
         {
            GeoCoderStatusCode status = MainMap.SetCurrentPositionByKeywords(textBoxGeo.Text);
            if(status != GeoCoderStatusCode.G_GEO_SUCCESS)
            {
               MessageBox.Show("Google Maps Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
         }
      }

      // reload map
      private void button1_Click(object sender, EventArgs e)
      {
         MainMap.ReloadMap();
      }

      // cache config
      private void checkBoxUseCache_CheckedChanged(object sender, EventArgs e)
      {
         GMaps.Instance.UseRouteCache = checkBoxUseRouteCache.Checked;
         GMaps.Instance.UseGeocoderCache = checkBoxUseGeoCache.Checked;
         GMaps.Instance.UsePlacemarkCache = GMaps.Instance.UseGeocoderCache;
      }

      // clear cache
      private void button2_Click(object sender, EventArgs e)
      {
         if(MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
         {
            try
            {
               System.IO.Directory.Delete(MainMap.CacheLocation, true);
            }
            catch(Exception ex)
            {
               MessageBox.Show(ex.Message);
            }
         }
      }

      // add test route
      private void button3_Click(object sender, EventArgs e)
      {
         MapRoute route = GMaps.Instance.GetRouteBetweenPoints(start, end, false, (int) MainMap.Zoom);
         if(route != null)
         {
            // add route
            GMapRoute r = new GMapRoute(route.Points, route.Name);
            routes.Routes.Add(r);

            // add route start/end marks
            GMapMarker m1 = new GMapMarkerGoogleRed(start);
            m1.ToolTipText = "Start: " + route.Name;
            m1.ToolTipMode = MarkerTooltipMode.Always;

            GMapMarker m2 = new GMapMarkerGoogleGreen(end);
            m2.ToolTipText = "End: " + end.ToString();
            m2.ToolTipMode = MarkerTooltipMode.Always;

            objects.Markers.Add(m1);
            objects.Markers.Add(m2);

            MainMap.ZoomAndCenterRoute(r);

            // testing kml support
            //KmlType info = GMaps.Instance.GetRouteBetweenPointsKml(start, end, false);
            //if(info != null)
            //{

            //}
         }
      }

      // add marker on current position
      private void button4_Click(object sender, EventArgs e)
      {
         GMapMarkerGoogleGreen m = new GMapMarkerGoogleGreen(currentMarker.Position);
         GMapMarkerRect mBorders = new GMapMarkerRect(currentMarker.Position);
         {
            mBorders.InnerMarker = m;
            if(polygon != null)
            {
               mBorders.Tag = polygon.Points.Count;
            }
            mBorders.ToolTipMode = MarkerTooltipMode.Always;
         }

         Placemark p = null;
         if(checkBoxPlacemarkInfo.Checked)
         {
            p = GMaps.Instance.GetPlacemarkFromGeocoder(currentMarker.Position);
         }

         if(p != null)
         {
            mBorders.ToolTipText = p.Address;
         }
         else
         {
            mBorders.ToolTipText = currentMarker.Position.ToString();
         }

         objects.Markers.Add(m);
         objects.Markers.Add(mBorders);

         RegeneratePolygon();
      }

      // clear routes
      private void button6_Click(object sender, EventArgs e)
      {
         routes.Routes.Clear();
      }

      // clear polygons
      private void button15_Click(object sender, EventArgs e)
      {
         polygons.Polygons.Clear();
      }

      // clear markers
      private void button5_Click(object sender, EventArgs e)
      {
         objects.Markers.Clear();
      }

      // show current marker
      private void checkBoxCurrentMarker_CheckedChanged(object sender, EventArgs e)
      {
         currentMarker.IsVisible = checkBoxCurrentMarker.Checked;
      }

      // can drag
      private void checkBoxCanDrag_CheckedChanged(object sender, EventArgs e)
      {
         MainMap.CanDragMap = checkBoxCanDrag.Checked;
      }

      // set route start
      private void buttonSetStart_Click(object sender, EventArgs e)
      {
         start = currentMarker.Position;
      }

      // set route end
      private void buttonSetEnd_Click(object sender, EventArgs e)
      {
         end = currentMarker.Position;
      }

      // zoom to max for markers
      private void button7_Click(object sender, EventArgs e)
      {
         MainMap.ZoomAndCenterMarkers("objects");
      }

      // export map data
      private void button9_Click(object sender, EventArgs e)
      {
         MainMap.ShowExportDialog();
      }

      // import map data
      private void button10_Click(object sender, EventArgs e)
      {
         MainMap.ShowImportDialog();
      }

      // prefetch
      private void button11_Click(object sender, EventArgs e)
      {
         RectLatLng area = MainMap.SelectedArea;
         if(!area.IsEmpty)
         {
            for(int i = (int) MainMap.Zoom; i <= MainMap.MaxZoom; i++)
            {
               DialogResult res = MessageBox.Show("Ready ripp at Zoom = " + i + " ?", "GMap.NET", MessageBoxButtons.YesNoCancel);

               if(res == DialogResult.Yes)
               {
                  TilePrefetcher obj = new TilePrefetcher();
                  obj.ShowCompleteMessage = true;
                  obj.Start(area, MainMap.Projection, i, MainMap.MapType, 100);
               }
               else if(res == DialogResult.No)
               {
                  continue;
               }
               else if(res == DialogResult.Cancel)
               {
                  break;
               }
            }
         }
         else
         {
            MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
         }
      }

      // saves current map view 
      private void button12_Click(object sender, EventArgs e)
      {
         try
         {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
               sfd.Filter = "PNG (*.png)|*.png";
               sfd.FileName = "GMap.NET image";

               Image tmpImage = MainMap.ToImage();
               if(tmpImage != null)
               {
                  using(tmpImage)
                  {
                     if(sfd.ShowDialog() == DialogResult.OK)
                     {
                        tmpImage.Save(sfd.FileName);

                        MessageBox.Show("Image saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            MessageBox.Show("Image failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      // debug tile grid
      private void checkBoxDebug_CheckedChanged(object sender, EventArgs e)
      {
         MainMap.ShowTileGridLines = checkBoxDebug.Checked;
      }

      // launch static map maker
      private void button13_Click(object sender, EventArgs e)
      {
         StaticImage st = new StaticImage(this);
         st.Owner = this;
         st.Show();
      }

      // add gps log from mobile
      private void button14_Click(object sender, EventArgs e)
      {
         using(FileDialog dlg = new OpenFileDialog())
         {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gpsd";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: open gps log generated in your windows mobile";
            dlg.Filter = "GMap.NET gps log DB files (*.gpsd)|*.gpsd";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
               routes.Routes.Clear();

               mobileGpsLog = dlg.FileName;

               // test
               AddGpsMobileLogRoutes(dlg.FileName);

               if(routes.Routes.Count > 0)
               {
                  MainMap.ZoomAndCenterRoutes(null);
               }
            }
         }
      }

      // key-up events
      private void MainForm_KeyUp(object sender, KeyEventArgs e)
      {
         int offset = -22;

         if(e.KeyCode == Keys.Left)
         {
            MainMap.Offset(-offset, 0);
         }
         else if(e.KeyCode == Keys.Right)
         {
            MainMap.Offset(offset, 0);
         }
         else if(e.KeyCode == Keys.Up)
         {
            MainMap.Offset(0, -offset);
         }
         else if(e.KeyCode == Keys.Down)
         {
            MainMap.Offset(0, offset);
         }
         else if(e.KeyCode == Keys.Delete)
         {
            if(CurentRectMarker != null)
            {
               objects.Markers.Remove(CurentRectMarker);

               if(CurentRectMarker.InnerMarker != null)
               {
                  objects.Markers.Remove(CurentRectMarker.InnerMarker);
               }
               CurentRectMarker = null;

               RegeneratePolygon();
            }
         }
         else if(e.KeyCode == Keys.Escape)
         {
            MainMap.Bearing = 0;

            if(currentTransport != null && !MainMap.IsMouseOverMarker)
            {
               currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
               currentTransport = null;
            }
         }
      }

      // key-press events
      private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
      {
         if(MainMap.Focused)
         {
            if(e.KeyChar == '+')
            {
               MainMap.Zoom += 1;
            }
            else if(e.KeyChar == '-')
            {
               MainMap.Zoom -= 1;
            }
            else if(e.KeyChar == 'a')
            {
               MainMap.Bearing--;
            }
            else if(e.KeyChar == 'z')
            {
               MainMap.Bearing++;
            }
         }
      }

      // engage some live demo
      private void RealTimeChanged(object sender, EventArgs e)
      {
         objects.Markers.Clear();
         polygons.Polygons.Clear();
         routes.Routes.Clear();

         // start performance test
         if(radioButtonPerf.Checked)
         {
            timerPerf.Interval = 44;
            timerPerf.Start();
         }
         else
         {
            // stop performance test
            timerPerf.Stop();
         }

         // start realtime transport tracking demo
         if(radioButtonTransport.Checked)
         {
            if(!transport.IsBusy)
            {
               firstLoadTrasport = true;
               transport.RunWorkerAsync();
            }
         }
         else
         {
            if(transport.IsBusy)
            {
               transport.CancelAsync();
            }
         }

         // start live tcp/ip connections demo
         if(radioButtonTcpIp.Checked)
         {
            GridConnections.Visible = true;
            checkBoxTcpIpSnap.Visible = true;
            checkBoxTraceRoute.Visible = true;
            GridConnections.Refresh();

            if(!connectionsWorker.IsBusy)
            {
               if(MainMap.MapType != MapType.GoogleMap)
               {
                  MainMap.MapType = MapType.GoogleMap;
               }
               MainMap.Zoom = 5;

               connectionsWorker.RunWorkerAsync();
            }
         }
         else
         {
            CountryStatusView.Clear();
            GridConnections.Visible = false;
            checkBoxTcpIpSnap.Visible = false;
            checkBoxTraceRoute.Visible = false;

            if(connectionsWorker.IsBusy)
            {
               connectionsWorker.CancelAsync();
            }
         }
      }

      // export mobile gps log to gpx file
      private void buttonExportToGpx_Click(object sender, EventArgs e)
      {
         try
         {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
               sfd.Filter = "GPX (*.gpx)|*.gpx";
               sfd.FileName = "mobile gps log";

               DateTime? date = null;
               DateTime? dateEnd = null;

               if(MobileLogFrom.Checked)
               {
                  date = MobileLogFrom.Value.ToUniversalTime();

                  sfd.FileName += " from " + MobileLogFrom.Value.ToString("yyyy-MM-dd HH-mm");
               }

               if(MobileLogTo.Checked)
               {
                  dateEnd = MobileLogTo.Value.ToUniversalTime();

                  sfd.FileName += " to " + MobileLogTo.Value.ToString("yyyy-MM-dd HH-mm");
               }

               if(sfd.ShowDialog() == DialogResult.OK)
               {
                  var log = GMaps.Instance.GetRoutesFromMobileLog(mobileGpsLog, date, dateEnd, 3.3);
                  if(log != null)
                  {
                     if(MainMap.Manager.ExportGPX(log, sfd.FileName))
                     {
                        MessageBox.Show("GPX saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     }
                  }
               }
            }
         }
         catch(Exception ex)
         {
            MessageBox.Show("GPX failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      // load gpx file
      private void button16_Click(object sender, EventArgs e)
      {
         using(FileDialog dlg = new OpenFileDialog())
         {
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.DefaultExt = "gpx";
            dlg.ValidateNames = true;
            dlg.Title = "GMap.NET: open gpx log";
            dlg.Filter = "gpx files (*.gpx)|*.gpx";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
               try
               {
                  string gpx = File.ReadAllText(dlg.FileName);

                  gpxType r = GMaps.Instance.DeserializeGPX(gpx);
                  if(r != null)
                  {
                     if(r.trk.Length > 0)
                     {
                        foreach(var trk in r.trk)
                        {
                           List<PointLatLng> points = new List<PointLatLng>();

                           foreach(var seg in trk.trkseg)
                           {
                              foreach(var p in seg.trkpt)
                              {
                                 points.Add(new PointLatLng((double) p.lat, (double) p.lon));
                              }
                           }

                           GMapRoute rt = new GMapRoute(points, string.Empty);
                           {
                              rt.Stroke = new Pen(Color.FromArgb(144, Color.Red));
                              rt.Stroke.Width = 5;
                              rt.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                           }
                           routes.Routes.Add(rt);
                        }

                        MainMap.ZoomAndCenterRoutes(null);
                     }
                  }
               }
               catch(Exception ex)
               {
                  Debug.WriteLine("GPX import: " + ex.ToString());
                  MessageBox.Show("Error importing gpx: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               }
            }
         }
      }

      // enable/disable host tracing
      private void checkBoxTraceRoute_CheckedChanged(object sender, EventArgs e)
      {
         TryTraceConnection = checkBoxTraceRoute.Checked;
         if(!TryTraceConnection)
         {
            if(iptracerWorker.IsBusy)
            {
               iptracerWorker.CancelAsync();
            }
            routes.Routes.Clear();
         }
      }

      private void GridConnections_DoubleClick(object sender, EventArgs e)
      {
         GridConnections.ClearSelection();
      }

      #endregion
   }
}
