using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using GMap.NET;
using System.Data.Common;  

#if !MONO
#if SQLite
using System.Data.SQLite;    
#endif
#else
   using SQLiteConnection=Mono.Data.SqliteClient.SqliteConnection;
   using SQLiteTransaction=Mono.Data.SqliteClient.SqliteTransaction;
   using SQLiteCommand=Mono.Data.SqliteClient.SqliteCommand;
   using SQLiteDataReader=Mono.Data.SqliteClient.SqliteDataReader;
   using SQLiteParameter=Mono.Data.SqliteClient.SqliteParameter;
#endif

namespace Demo.WindowsForms
{
   public struct VehicleData
   {
      public int Id;
      public double Lat;
      public double Lng;
      public string Line;
      public string LastStop;
      public string TrackType;
      public string AreaName;
      public string StreetName;
      public string Time;
      public double? Bearing;
   }

   public enum TransportType
   {
      Bus,
      TrolleyBus,
   }

   public class Stuff
   {
      /// <summary>
      /// gets routes from gpsd log file
      /// </summary>
      /// <param name="gpsdLogFile"></param>
      /// <param name="start">start time(UTC) of route, null to read from very start</param>
      /// <param name="end">end time(UTC) of route, null to read to the very end</param>
      /// <param name="maxPositionDilutionOfPrecision">max value of PositionDilutionOfPrecision, null to get all</param>
      /// <returns></returns>
      public static IEnumerable<List<GpsLog>> GetRoutesFromMobileLog(string gpsdLogFile, DateTime? start, DateTime? end, double? maxPositionDilutionOfPrecision)
      {
#if SQLite
         using(SQLiteConnection cn = new SQLiteConnection())
         {
#if !MONO
            cn.ConnectionString = string.Format("Data Source=\"{0}\";FailIfMissing=True;", gpsdLogFile);
#else
            cn.ConnectionString = string.Format("Version=3,URI=file://{0},FailIfMissing=True", gpsdLogFile);
#endif

            cn.Open();
            {
               using(DbCommand cmd = cn.CreateCommand())
               {
                  cmd.CommandText = "SELECT * FROM GPS ";
                  int initLenght = cmd.CommandText.Length;

                  if(start.HasValue)
                  {
                     cmd.CommandText += "WHERE TimeUTC >= @t1 ";
                     SQLiteParameter lookupValue = new SQLiteParameter("@t1", start);
                     cmd.Parameters.Add(lookupValue);
                  }

                  if(end.HasValue)
                  {
                     if(cmd.CommandText.Length <= initLenght)
                     {
                        cmd.CommandText += "WHERE ";
                     }
                     else
                     {
                        cmd.CommandText += "AND ";
                     }

                     cmd.CommandText += "TimeUTC <= @t2 ";
                     SQLiteParameter lookupValue = new SQLiteParameter("@t2", end);
                     cmd.Parameters.Add(lookupValue);
                  }

                  if(maxPositionDilutionOfPrecision.HasValue)
                  {
                     if(cmd.CommandText.Length <= initLenght)
                     {
                        cmd.CommandText += "WHERE ";
                     }
                     else
                     {
                        cmd.CommandText += "AND ";
                     }

                     cmd.CommandText += "PositionDilutionOfPrecision <= @p3 ";
                     SQLiteParameter lookupValue = new SQLiteParameter("@p3", maxPositionDilutionOfPrecision);
                     cmd.Parameters.Add(lookupValue);
                  }

                  using(DbDataReader rd = cmd.ExecuteReader())
                  {
                     List<GpsLog> points = new List<GpsLog>();
                     while(rd.Read())
                     {
                        GpsLog log = new GpsLog();
                        {
                           log.TimeUTC = (DateTime)rd["TimeUTC"];
                           log.SessionCounter = (long)rd["SessionCounter"];
                           log.Delta = rd["Delta"] as double?;
                           log.Speed = rd["Speed"] as double?;
                           log.SeaLevelAltitude = rd["SeaLevelAltitude"] as double?;
                           log.EllipsoidAltitude = rd["EllipsoidAltitude"] as double?;
                           log.SatellitesInView = rd["SatellitesInView"] as System.Byte?;
                           log.SatelliteCount = rd["SatelliteCount"] as System.Byte?;
                           log.Position = new PointLatLng((double)rd["Lat"], (double)rd["Lng"]);
                           log.PositionDilutionOfPrecision = rd["PositionDilutionOfPrecision"] as double?;
                           log.HorizontalDilutionOfPrecision = rd["HorizontalDilutionOfPrecision"] as double?;
                           log.VerticalDilutionOfPrecision = rd["VerticalDilutionOfPrecision"] as double?;
                           log.FixQuality = (FixQuality)((byte)rd["FixQuality"]);
                           log.FixType = (FixType)((byte)rd["FixType"]);
                           log.FixSelection = (FixSelection)((byte)rd["FixSelection"]);
                        }

                        if(log.SessionCounter == 0 && points.Count > 0)
                        {
                           List<GpsLog> ret = new List<GpsLog>(points);
                           points.Clear();
                           {
                              yield return ret;
                           }
                        }

                        points.Add(log);
                     }

                     if(points.Count > 0)
                     {
                        List<GpsLog> ret = new List<GpsLog>(points);
                        points.Clear();
                        {
                           yield return ret;
                        }
                     }

                     points.Clear();
                     points = null;

                     rd.Close();
                  }
               }
            }
            cn.Close();
         }
#else
         return null;
#endif
      }

      /// <summary>
      /// gets realtime data from public transport in city vilnius of lithuania
      /// </summary>
      /// <param name="type">type of transport</param>
      /// <param name="line">linenum or null to get all</param>
      /// <param name="ret"></param>
      public static void GetVilniusTransportData(TransportType type, string line, List<VehicleData> ret)
      {
         ret.Clear();

         string url = "http://www.troleibusai.lt/puslapiai/services/vehiclestate.php?type=";

         switch(type)
         {
            case TransportType.Bus:
            {
               url += "bus";
            }
            break;

            case TransportType.TrolleyBus:
            {
               url += "trolley";
            }
            break;
         }

         if(!string.IsNullOrEmpty(line))
         {
            url += "&line=" + line;
         }

#if !PocketPC
         url += "&app=GMap.NET.Desktop";
#else
         url += "&app=GMap.NET.WindowsMobile";
#endif

         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
         {
#if !PocketPC
            request.Proxy = WebRequest.DefaultWebProxy;
#else
            request.Proxy = GlobalProxySelection.GetEmptyWebProxy();
#endif
         }

         request.Timeout = 30 * 1000;
         request.ReadWriteTimeout = request.Timeout;
         request.Accept = "*/*";
         request.KeepAlive = false;

         string xml = string.Empty;

         using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
         {
            using(Stream responseStream = response.GetResponseStream())
            {
               using(StreamReader read = new StreamReader(responseStream))
               {
                  xml = read.ReadToEnd();
               }
            }
         }

         XmlDocument doc = new XmlDocument();
         {
            doc.LoadXml(xml);

            XmlNodeList devices = doc.GetElementsByTagName("Device");
            foreach(XmlNode dev in devices)
            {
               VehicleData d = new VehicleData();
               d.Id = int.Parse(dev.Attributes["ID"].InnerText);

               foreach(XmlElement elem in dev.ChildNodes)
               {
                  // Debug.WriteLine(d.Id + "->" + elem.Name + ": " + elem.InnerText);

                  switch(elem.Name)
                  {
                     case "Lat":
                     {
                        d.Lat = double.Parse(elem.InnerText, CultureInfo.InvariantCulture);
                     }
                     break;

                     case "Lng":
                     {
                        d.Lng = double.Parse(elem.InnerText, CultureInfo.InvariantCulture);
                     }
                     break;

                     case "Bearing":
                     {
                        if(!string.IsNullOrEmpty(elem.InnerText))
                        {
                           d.Bearing = double.Parse(elem.InnerText, CultureInfo.InvariantCulture);
                        }
                     }
                     break;

                     case "LineNum":
                     {
                        d.Line = elem.InnerText;
                     }
                     break;

                     case "AreaName":
                     {
                        d.AreaName = elem.InnerText;
                     }
                     break;

                     case "StreetName":
                     {
                        d.StreetName = elem.InnerText;
                     }
                     break;

                     case "TrackType":
                     {
                        d.TrackType = elem.InnerText;
                     }
                     break;

                     case "LastStop":
                     {
                        d.LastStop = elem.InnerText;
                     }
                     break;

                     case "Time":
                     {
                        d.Time = elem.InnerText;
                     }
                     break;
                  }
               }
               ret.Add(d);
            }
         }
         doc = null;
      }
   }
}
