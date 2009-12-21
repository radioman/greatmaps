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

#endregion

namespace GMap.NET.GPS
{
   /// <summary>
   /// class that represents a gps coordinate in degrees, minutes, and seconds.  
   /// </summary>
   public class DegreesMinutesSeconds
   {

      bool isPositive;
      /// <summary>
      /// Returns true if the degrees, minutes and seconds refer to a positive value,
      /// false otherwise.
      /// </summary>
      public bool IsPositive
      {
         get
         {
            return isPositive;
         }
      }

      uint degrees;
      /// <summary>
      /// The degrees unit of the coordinate
      /// </summary>
      public uint Degrees
      {
         get
         {
            return degrees;
         }
      }

      uint minutes;
      /// <summary>
      /// The minutes unit of the coordinate
      /// </summary>
      public uint Minutes
      {
         get
         {
            return minutes;
         }
      }

      double seconds;
      /// <summary>
      /// The seconds unit of the coordinate
      /// </summary>
      public double Seconds
      {
         get
         {
            return seconds;
         }
      }

      /// <summary>
      /// Constructs a new instance of DegreesMinutesSeconds converting 
      /// from decimal degrees
      /// </summary>
      /// <param name="decimalDegrees">Initial value as decimal degrees</param>
      public DegreesMinutesSeconds(double decimalDegrees)
      {
         isPositive = (decimalDegrees > 0);

         degrees = (uint) Math.Abs(decimalDegrees);

         double doubleMinutes = (Math.Abs(decimalDegrees) - Math.Abs((double) degrees)) * 60.0;
         minutes = (uint) doubleMinutes;

         seconds = (doubleMinutes - (double) minutes) * 60.0;
      }

      /// <summary>
      /// Constructs a new instance of DegreesMinutesSeconds
      /// </summary>
      /// <param name="isPositive">True if the coordinates are positive coordinate, false if they
      /// are negative coordinates.</param>
      /// <param name="degrees">Degrees unit of the coordinate</param>
      /// <param name="minutes">Minutes unit of the coordinate</param>
      /// <param name="seconds">Seconds unit of the coordinate. This should be a positive value.</param>
      public DegreesMinutesSeconds(bool isPositive, uint degrees, uint minutes, double seconds)
      {
         this.isPositive = isPositive;
         this.degrees = degrees;
         this.minutes = minutes;
         this.seconds = seconds;
      }

      /// <summary>
      /// Converts the decimal, minutes, seconds coordinate to 
      /// decimal degrees
      /// </summary>
      /// <returns></returns>
      public double ToDecimalDegrees()
      {
         double val = (double) degrees + ((double) minutes / 60.0) + ((double) seconds / 3600.0);
         val = isPositive ? val : val * -1;
         return val;
      }

      /// <summary>
      /// Converts the instance to a string in format: D M' S"
      /// </summary>
      /// <returns>string representation of degrees, minutes, seconds</returns>
      public override string ToString()
      {
         return degrees + "d " + minutes + "' " + seconds + "\"";
      }
   }
}
