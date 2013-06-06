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
using System;
using System.Runtime.InteropServices;
using System.Text;

#if PocketPC
using OpenNETCF.ComponentModel;
using OpenNETCF.Threading;
using Thread=OpenNETCF.Threading.Thread2;
using System.Diagnostics;
#endif

namespace GMap.NET.GPS
{
   public delegate void LocationChangedEventHandler(object sender, GpsPosition args);
   public delegate void DeviceStateChangedEventHandler(object sender, GpsDeviceState args);

   /// <summary>
   /// Summary description for GPS.
   /// </summary>
   public class Gps
   {
      // handle to the gps device
      IntPtr gpsHandle = IntPtr.Zero;

      // handle to the native event that is signalled when the GPS
      // devices gets a new location
      IntPtr newLocationHandle = IntPtr.Zero;

      // handle to the native event that is signalled when the GPS
      // device state changes
      IntPtr deviceStateChangedHandle = IntPtr.Zero;

      // handle to the native event that we use to stop our event
      // thread
      IntPtr stopHandle = IntPtr.Zero;

      // holds our event thread instance
      Thread gpsEventThread = null;


      event LocationChangedEventHandler locationChanged;

      /// <summary>
      /// Event that is raised when the GPS locaction data changes
      /// </summary>
      public event LocationChangedEventHandler LocationChanged
      {
         add
         {
            locationChanged += value;

            // create our event thread only if the user decides to listen
            CreateGpsEventThread();
         }
         remove
         {
            locationChanged -= value;
         }
      }


      event DeviceStateChangedEventHandler deviceStateChanged;

      /// <summary>
      /// Event that is raised when the GPS device state changes
      /// </summary>
      public event DeviceStateChangedEventHandler DeviceStateChanged
      {
         add
         {
            deviceStateChanged += value;

            // create our event thread only if the user decides to listen
            CreateGpsEventThread();
         }
         remove
         {
            deviceStateChanged -= value;
         }
      }

      /// <summary>
      /// True: The GPS device has been opened. False: It has not been opened
      /// </summary>
      public bool Opened
      {
         get
         {
            return gpsHandle != IntPtr.Zero;
         }
      }

      ~Gps()
      {
         // make sure that the GPS was closed.
         Close();
      }

      /// <summary>
      /// Opens the GPS device and prepares to receive data from it.
      /// </summary>
      public void Open()
      {
         if(!Opened)
         {
            // create handles for GPS events
            newLocationHandle = Win32.CreateEvent(IntPtr.Zero, false, false, null);
            deviceStateChangedHandle = Win32.CreateEvent(IntPtr.Zero, false, false, null);
            stopHandle = Win32.CreateEvent(IntPtr.Zero, false, false, null);

            gpsHandle = GPSOpenDevice(newLocationHandle, deviceStateChangedHandle, null, 0);

            // if events were hooked up before the device was opened, we'll need
            // to create the gps event thread.
            if(locationChanged != null || deviceStateChanged != null)
            {
               CreateGpsEventThread();
            }
         }
      }

      /// <summary>
      /// Closes the gps device.
      /// </summary>
      public void Close()
      {
         if(gpsHandle != IntPtr.Zero)
         {
            GPSCloseDevice(gpsHandle);
            gpsHandle = IntPtr.Zero;
         }

         // Set our native stop event so we can exit our event thread.
         if(stopHandle != IntPtr.Zero)
         {
             Win32.EventModify(stopHandle, (int)Win32.EventFlags.SET);
         }

         // wait exit
         if(gpsEventThread != null && gpsEventThread.IsAlive)
         {
            //gpsEventThread.Join(4444);
         }
      }

      /// <summary>
      /// Get the position reported by the GPS receiver
      /// </summary>
      /// <returns>GpsPosition class with all the position details</returns>
      public GpsPosition GetPosition()
      {
         return GetPosition(TimeSpan.Zero);
      }

      /// <summary>
      /// Get the position reported by the GPS receiver that is no older than
      /// the maxAge passed in
      /// </summary>
      /// <param name="maxAge">Max age of the gps position data that you want back. 
      /// If there is no data within the required age, null is returned.  
      /// if maxAge == TimeSpan.Zero, then the age of the data is ignored</param>
      /// <returns>GpsPosition class with all the position details</returns>
      public GpsPosition GetPosition(TimeSpan maxAge)
      {
         GpsPosition gpsPosition = null;
         if(Opened)
         {
            // allocate the necessary memory on the native side.  We have a class (GpsPosition) that 
            // has the same memory layout as its native counterpart
            IntPtr ptr = Utils.LocalAlloc(Marshal.SizeOf(typeof(GpsPosition)));

            // fill in the required fields 
            gpsPosition = new GpsPosition();
            gpsPosition.dwVersion = 1;
            gpsPosition.dwSize = Marshal.SizeOf(typeof(GpsPosition));

            // Marshal our data to the native pointer we allocated.
            Marshal.StructureToPtr(gpsPosition, ptr, false);

            // call native method passing in our native buffer
            int result = GPSGetPosition(gpsHandle, ptr, 500000, 0);
            if(result == 0)
            {
               // native call succeeded, marshal native data to our managed data
               gpsPosition = (GpsPosition) Marshal.PtrToStructure(ptr, typeof(GpsPosition));

               if(maxAge != TimeSpan.Zero)
               {
                  // check to see if the data is recent enough.
                  if(!gpsPosition.Time.HasValue || DateTime.UtcNow - maxAge > gpsPosition.Time)
                  {
                     gpsPosition = null;
                  }
               }
            }

            // free our native memory
            Utils.LocalFree(ptr);
         }

         return gpsPosition;
      }

      /// <summary>
      /// Queries the device state.
      /// </summary>
      /// <returns>Device state information</returns>
      public GpsDeviceState GetDeviceState()
      {
         GpsDeviceState device = null;

         // allocate a buffer on the native side.  Since the
         IntPtr pGpsDevice = Utils.LocalAlloc(GpsDeviceState.GpsDeviceStructureSize);

         // GPS_DEVICE structure has arrays of characters, it's easier to just
         // write directly into memory rather than create a managed structure with
         // the same layout.
         Marshal.WriteInt32(pGpsDevice, 1);	// write out GPS version of 1
         Marshal.WriteInt32(pGpsDevice, 4, GpsDeviceState.GpsDeviceStructureSize);	// write out dwSize of structure

         int result = GPSGetDeviceState(pGpsDevice);

         if(result == 0)
         {
            // instantiate the GpsDeviceState class passing in the native pointer
            device = new GpsDeviceState(pGpsDevice);
         }

         // free our native memory
         Utils.LocalFree(pGpsDevice);

         return device;
      }

      /// <summary>
      /// Creates our event thread that will receive native events
      /// </summary>
      private void CreateGpsEventThread()
      {
         // we only want to create the thread if we don't have one created already 
         // and we have opened the gps device
         if(gpsEventThread == null && gpsHandle != IntPtr.Zero)
         {
            // Create and start thread to listen for GPS events
            gpsEventThread = new Thread(new System.Threading.ThreadStart(WaitForGpsEvents));
            gpsEventThread.IsBackground = false;
            gpsEventThread.Name = "GMap.NET GpsEvents";
            gpsEventThread.Start();
         }
      }

      /// <summary>
      /// Method used to listen for native events from the GPS. 
      /// </summary>
      private void WaitForGpsEvents()
      {
         //lock (this)
         {
            bool listening = true;
            // allocate 3 handles worth of memory to pass to WaitForMultipleObjects
            IntPtr handles = Utils.LocalAlloc(12);

            // write the three handles we are listening for.
            Marshal.WriteInt32(handles, 0, stopHandle.ToInt32());
            Marshal.WriteInt32(handles, 4, deviceStateChangedHandle.ToInt32());
            Marshal.WriteInt32(handles, 8, newLocationHandle.ToInt32());

            while(listening)
            {
                int obj = Win32.WaitForMultipleObjects(3, handles, 0, -1);
                if (obj != Win32.waitFailed)
               {
                  switch(obj)
                  {
                     case 0:
                     // we've been signalled to stop
                     listening = false;
                     break;

                     case 1:
                     // device state has changed
                     if(deviceStateChanged != null)
                     {
                        deviceStateChanged(this, GetDeviceState());
                     }
                     break;

                     case 2:
                     // location has changed
                     if(locationChanged != null)
                     {
                        locationChanged(this, GetPosition());
                     }
                     break;
                  }
               }
            }

            // free the memory we allocated for the native handles
            Utils.LocalFree(handles);

            if(newLocationHandle != IntPtr.Zero)
            {
                Win32.CloseHandle(newLocationHandle);
               newLocationHandle = IntPtr.Zero;
            }

            if(deviceStateChangedHandle != IntPtr.Zero)
            {
                Win32.CloseHandle(deviceStateChangedHandle);
               deviceStateChangedHandle = IntPtr.Zero;
            }

            if(stopHandle != IntPtr.Zero)
            {
               Win32.CloseHandle(stopHandle);
               stopHandle = IntPtr.Zero;
            }

            // clear our gpsEventThread so that we can recreate this thread again
            // if the events are hooked up again.
            gpsEventThread = null;

            Debug.WriteLine("gps device stopped...");
         }
      }

      public double GetDistance(double p1Lat, double p1Lng, double p2Lat, double p2Lng)
      {
         double dLat1InRad = p1Lat * (Math.PI / 180);
         double dLong1InRad = p1Lng * (Math.PI / 180);
         double dLat2InRad = p2Lat * (Math.PI / 180);
         double dLong2InRad = p2Lng * (Math.PI / 180);
         double dLongitude = dLong2InRad - dLong1InRad;
         double dLatitude = dLat2InRad - dLat1InRad;
         double a = Math.Pow(Math.Sin(dLatitude / 2), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
         double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
         double dDistance = EarthRadiusKm * c;
         return dDistance;
      }

      /// <summary>
      /// Radius of the Earth
      /// </summary>
      public double EarthRadiusKm = 6378.137; // WGS-84

      #region PInvokes to gpsapi.dll
      [DllImport("gpsapi.dll")]
      static extern IntPtr GPSOpenDevice(IntPtr hNewLocationData, IntPtr hDeviceStateChange, string szDeviceName, int dwFlags);

      [DllImport("gpsapi.dll")]
      static extern int GPSCloseDevice(IntPtr hGPSDevice);

      [DllImport("gpsapi.dll")]
      static extern int GPSGetPosition(IntPtr hGPSDevice, IntPtr pGPSPosition, int dwMaximumAge, int dwFlags);

      [DllImport("gpsapi.dll")]
      static extern int GPSGetDeviceState(IntPtr pGPSDevice);
      #endregion
   }
}
