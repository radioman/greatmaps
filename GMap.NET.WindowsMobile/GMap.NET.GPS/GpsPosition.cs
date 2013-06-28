//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//
// Use of this sample source code is subject to the terms of the Microsoft
// license agreement under which you licensed this sample source code. If
// you did not accept the terms of the license agreement, you are not
// authorized to use this sample source code. For the terms of the license,
// please see the license agreement between you and Microsoft or, if applicable,
// see the LICENSE.RTF on your install media or the root of your tools installation.
// THE SAMPLE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace GMap.NET.GPS
{
   #region Internal Native Structures
   [StructLayout(LayoutKind.Sequential)]
   internal struct SystemTime
   {
      internal short year;
      internal short month;
      internal short dayOfWeek;
      internal short day;
      internal short hour;
      internal short minute;
      internal short second;
      internal short millisecond;
   }

   [StructLayout(LayoutKind.Sequential)]
   internal struct SatelliteArray
   {
      int a, b, c, d, e, f, g, h, i, j, k, l;

      public int Count
      {
         get
         {
            return 12;
         }
      }

      public int this[int value]
      {
         get
         {
            if(value == 0)
               return a;
            else if(value == 1)
               return b;
            else if(value == 2)
               return c;
            else if(value == 3)
               return d;
            else if(value == 4)
               return e;
            else if(value == 5)
               return f;
            else if(value == 6)
               return g;
            else if(value == 7)
               return h;
            else if(value == 8)
               return i;
            else if(value == 9)
               return j;
            else if(value == 10)
               return k;
            else if(value == 11)
               return l;
            else
               throw new ArgumentOutOfRangeException("value must be 0 - 11");
         }
      }
   }
   #endregion

   public enum FixQuality : int
   {
      Unknown=0,
      Gps,
      DGps
   }
   public enum FixType : int
   {
      Unknown=0,
      XyD,
      XyzD
   }

   public enum FixSelection : int
   {
      Unknown=0,
      Auto,
      Manual
   }

   public struct Satellite
   {
      public Satellite(int id, int elevation, int azimuth, int signalStrength, bool inUse)
      {
         this.id = id;
         this.elevation = elevation;
         this.azimuth = azimuth;
         this.signalStrength = signalStrength;
         this.inUse = inUse;
      }

      bool inUse;
      public bool InSolution
      {
         get
         {
            return inUse;
         }
         set
         {
            inUse = value;
         }
      }

      int id;
      /// <summary>
      /// Id of the satellite
      /// </summary>
      public int Id
      {
         get
         {
            return id;
         }
         set
         {
            id = value;
         }
      }


      int elevation;
      /// <summary>
      /// Elevation of the satellite
      /// </summary>
      public int Elevation
      {
         get
         {
            return elevation;
         }
         set
         {
            elevation = value;
         }
      }


      int azimuth;
      /// <summary>
      /// Azimuth of the satellite
      /// </summary>
      public int Azimuth
      {
         get
         {
            return azimuth;
         }
         set
         {
            azimuth = value;
         }
      }


      int signalStrength;
      /// <summary>
      /// SignalStrenth of the satellite
      /// </summary>
      public int SignalStrength
      {
         get
         {
            return signalStrength;
         }
         set
         {
            signalStrength = value;
         }
      }

   }

   [StructLayout(LayoutKind.Sequential)]
   public class GpsPosition
   {
      internal GpsPosition()
      {
      }

      internal static int GPS_VALID_UTC_TIME = 0x00000001;
      internal static int GPS_VALID_LATITUDE = 0x00000002;
      internal static int GPS_VALID_LONGITUDE = 0x00000004;
      internal static int GPS_VALID_SPEED = 0x00000008;
      internal static int GPS_VALID_HEADING = 0x00000010;
      internal static int GPS_VALID_MAGNETIC_VARIATION = 0x00000020;
      internal static int GPS_VALID_ALTITUDE_WRT_SEA_LEVEL = 0x00000040;
      internal static int GPS_VALID_ALTITUDE_WRT_ELLIPSOID = 0x00000080;
      internal static int GPS_VALID_POSITION_DILUTION_OF_PRECISION = 0x00000100;
      internal static int GPS_VALID_HORIZONTAL_DILUTION_OF_PRECISION = 0x00000200;
      internal static int GPS_VALID_VERTICAL_DILUTION_OF_PRECISION = 0x00000400;
      internal static int GPS_VALID_SATELLITE_COUNT = 0x00000800;
      internal static int GPS_VALID_SATELLITES_USED_PRNS = 0x00001000;
      internal static int GPS_VALID_SATELLITES_IN_VIEW = 0x00002000;
      internal static int GPS_VALID_SATELLITES_IN_VIEW_PRNS = 0x00004000;
      internal static int GPS_VALID_SATELLITES_IN_VIEW_ELEVATION = 0x00008000;
      internal static int GPS_VALID_SATELLITES_IN_VIEW_AZIMUTH = 0x00010000;
      internal static int GPS_VALID_SATELLITES_IN_VIEW_SIGNAL_TO_NOISE_RATIO = 0x00020000;

      internal int dwVersion = 1;             // Current version of GPSID client is using.
      internal int dwSize = 0;                // sizeof(_GPS_POSITION)

      // Not all fields in the structure below are guaranteed to be valid.  
      // Which fields are valid depend on GPS device being used, how stale the API allows
      // the data to be, and current signal.
      // Valid fields are specified in dwValidFields, based on GPS_VALID_XXX flags.
      internal int dwValidFields = 0;

      // Additional information about this location structure (GPS_DATA_FLAGS_XXX)
      internal int dwFlags = 0;

      //** Time related
      internal SystemTime stUTCTime = new SystemTime(); 	//  UTC according to GPS clock.

      //** Position + heading related
      internal double dblLatitude = 0.0;            // Degrees latitude.  North is positive
      internal double dblLongitude = 0.0;           // Degrees longitude.  East is positive
      internal float flSpeed = 0.0f;                // Speed in knots
      internal float flHeading = 0.0f;              // Degrees heading (course made good).  True North=0
      internal double dblMagneticVariation = 0.0;   // Magnetic variation.  East is positive
      internal float flAltitudeWRTSeaLevel = 0.0f;  // Altitute with regards to sea level, in meters
      internal float flAltitudeWRTEllipsoid = 0.0f; // Altitude with regards to ellipsoid, in meters

      // Where did we get fix from?
      internal FixQuality fixQuality = FixQuality.Unknown;

      // Is this 2d or 3d fix?
      internal FixType fixType = FixType.Unknown;

      // Auto or manual selection between 2d or 3d mode
      internal FixSelection selectionType = FixSelection.Unknown;

      // Position Dilution Of Precision
      internal float flPositionDilutionOfPrecision = 0.0f;

      // Horizontal Dilution Of Precision
      internal float flHorizontalDilutionOfPrecision = 0.0f;

      // Vertical Dilution Of Precision
      internal float flVerticalDilutionOfPrecision = 0.0f;

      // Number of satellites used in solution
      internal int dwSatelliteCount = 0;

      // PRN numbers of satellites used in the solution
      internal SatelliteArray rgdwSatellitesUsedPRNs = new SatelliteArray();

      // Number of satellites in view.  From 0-GPS_MAX_SATELLITES
      internal int dwSatellitesInView = 0;

      // PRN numbers of satellites in view
      internal SatelliteArray rgdwSatellitesInViewPRNs = new SatelliteArray();

      // Elevation of each satellite in view
      internal SatelliteArray rgdwSatellitesInViewElevation = new SatelliteArray();

      // Azimuth of each satellite in view
      internal SatelliteArray rgdwSatellitesInViewAzimuth = new SatelliteArray();

      // Signal to noise ratio of each satellite in view
      internal SatelliteArray rgdwSatellitesInViewSignalToNoiseRatio = new SatelliteArray();

      // ...

      public FixQuality FixQuality
      {
         get
         {
            return fixQuality;
         }
      }

      public FixType FixType
      {
         get
         {
            return fixType;
         }
      }

      public FixSelection FixSelection
      {
         get
         {
            return selectionType;
         }
      }

      /// <summary>
      /// UTC according to GPS clock.
      /// </summary>
      public DateTime? Time
      {
         get
         {
            return ((dwValidFields & GPS_VALID_UTC_TIME) != 0) ? ((DateTime?) new DateTime(stUTCTime.year, stUTCTime.month, stUTCTime.day, stUTCTime.hour, stUTCTime.minute, stUTCTime.second, stUTCTime.millisecond, DateTimeKind.Utc)) : null;
         }
      }

      /// <summary>
      /// Satellites in view
      /// </summary>
      /// <returns>Array of Satellites</returns>
      public IEnumerable<Satellite> GetSatellitesInView()
      {
         // if valid view
         if((dwValidFields & GPS_VALID_SATELLITES_IN_VIEW) != 0 && dwSatellitesInView != 0)
         {
            for(int index = 0; index < dwSatellitesInView; index++)
            {
               Satellite ret = new Satellite(rgdwSatellitesInViewPRNs[index], rgdwSatellitesInViewElevation[index], rgdwSatellitesInViewAzimuth[index], rgdwSatellitesInViewSignalToNoiseRatio[index], false);

               // if valid solution
               if((dwValidFields & GPS_VALID_SATELLITES_USED_PRNS) != 0)
               {
                  for(int index2 = 0; index2 < dwSatelliteCount; index2++)
                  {
                     if(rgdwSatellitesUsedPRNs[index2] == ret.Id)
                     {
                        ret.InSolution = true;
                        break;
                     }
                  }
               }

               yield return ret;
            }
         }
      }

      /// <summary>
      /// Number of satellites used in solution
      /// </summary>
      public int? SatelliteCount
      {
         get
         {
            return (((dwValidFields & GPS_VALID_SATELLITE_COUNT) != 0) ? (int?) dwSatelliteCount : null);
         }
      }

      /// <summary>
      /// Number of satellites in view.  
      /// </summary>
      public int? SatellitesInViewCount
      {
         get
         {
            return ((dwValidFields & GPS_VALID_SATELLITES_IN_VIEW) != 0) ? (int?) dwSatellitesInView : null;
         }
      }

      /// <summary>
      /// Speed in km/h
      /// </summary>
      public float? Speed
      {
         get
         {
            return ((dwValidFields & GPS_VALID_SPEED) != 0) ? (float?) (flSpeed*1.852f) : null;
         }
      }

      /// <summary>
      /// Altitude with regards to ellipsoid, in meters
      /// </summary>
      public float? EllipsoidAltitude
      {
         get
         {
            return ((dwValidFields & GPS_VALID_ALTITUDE_WRT_ELLIPSOID) != 0) ? (float?) flAltitudeWRTEllipsoid : null;
         }
      }

      /// <summary>
      /// Altitute with regards to sea level, in meters
      /// </summary>
      public float? SeaLevelAltitude
      {
         get
         {
            return ((dwValidFields & GPS_VALID_ALTITUDE_WRT_SEA_LEVEL) != 0) ? (float?) flAltitudeWRTSeaLevel : null;
         }
      }

      /// <summary>
      /// Latitude in decimal degrees.  North is positive
      /// </summary>
      public double? Latitude
      {
         get
         {
            return ((dwValidFields & GPS_VALID_LATITUDE) != 0) ? (float?) dblLatitude : null;
         }
      }

      /// <summary>
      /// Longitude in decimal degrees.  East is positive
      /// </summary>
      public double? Longitude
      {
         get
         {
            return ((dwValidFields & GPS_VALID_LONGITUDE) != 0) ? (float?) dblLongitude : null;
         }
      }

      /// <summary>
      /// Degrees heading (course made good).  True North=0
      /// </summary>
      public float? Heading
      {
         get
         {
            return ((dwValidFields & GPS_VALID_HEADING) != 0) ? (float?) flHeading : null;
         }
      }

      /// <summary>
      /// Position Dilution Of Precision
      /// </summary>
      public float? PositionDilutionOfPrecision
      {
         get
         {
            return ((dwValidFields & GPS_VALID_POSITION_DILUTION_OF_PRECISION) != 0) ? (float?) flPositionDilutionOfPrecision : null;
         }
      }

      /// <summary>
      /// Horizontal Dilution Of Precision
      /// </summary>
      public float? HorizontalDilutionOfPrecision
      {
         get
         {
            return ((dwValidFields & GPS_VALID_HORIZONTAL_DILUTION_OF_PRECISION) != 0) ? (float?) flHorizontalDilutionOfPrecision : null;
         }
      }

      /// <summary>
      /// Vertical Dilution Of Precision
      /// </summary>
      public float? VerticalDilutionOfPrecision
      {
         get
         {
            return ((dwValidFields & GPS_VALID_VERTICAL_DILUTION_OF_PRECISION) != 0) ? (float?) flVerticalDilutionOfPrecision : null;
         }
      }
   }
}
