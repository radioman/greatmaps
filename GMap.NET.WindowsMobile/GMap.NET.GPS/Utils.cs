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
using System.Text;
using System.Diagnostics;
using System.Threading;

#endregion

namespace GMap.NET.GPS
{
   /// <summary>
   /// Summary description for Utils.
   /// </summary>
   public class Utils
   {
      public Utils()
      {
      }

      public static IntPtr LocalAlloc(int byteCount)
      {
         IntPtr ptr = Win32.LocalAlloc(Win32.LMEM_ZEROINIT, byteCount);
         if(ptr == IntPtr.Zero)
         {
            throw new OutOfMemoryException();
         }

         return ptr;
      }

      public static void LocalFree(IntPtr hMem)
      {
         IntPtr ptr = Win32.LocalFree(hMem);
         if(ptr != IntPtr.Zero)
         {
            throw new ArgumentException();
         }
      }

      public static void UpdateTime(DateTime gpsTime)
      {
          Win32.SYSTEMTIME s = new Win32.SYSTEMTIME();
          s.Year = (short)gpsTime.Year;
          s.Month = (short)gpsTime.Month;
          s.DayOfWeek = (short)gpsTime.DayOfWeek;
          s.Day = (short)gpsTime.Day;
          s.Hour = (short)gpsTime.Hour;
          s.Minute = (short)gpsTime.Minute;
          s.Second = (short)gpsTime.Second;
          s.Milliseconds = (short)gpsTime.Millisecond;

          bool t = Win32.SetSystemTime(ref s);
          Debug.WriteLine("SetSystemTime: " + t);
      }
   }

   public class Win32
   {
      public const int LMEM_ZEROINIT = 0x40;
      [DllImport("coredll.dll", EntryPoint="#33", SetLastError=true)]
      public static extern IntPtr LocalAlloc(int flags, int byteCount);

      [DllImport("coredll.dll", EntryPoint="#36", SetLastError=true)]
      public static extern IntPtr LocalFree(IntPtr hMem);

      public const int waitFailed = -1;
      [DllImport("coredll.dll")]
      public static extern int WaitForMultipleObjects(int nCount, IntPtr lpHandles, int fWaitAll, int dwMilliseconds);

      [DllImport("coredll.dll", SetLastError = true)]
      public static extern Int32 WaitForSingleObject(IntPtr Handle, Int32 Wait);

      [DllImport("coredll.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
      public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, [In, MarshalAs(UnmanagedType.Bool)] bool bManualReset, [In, MarshalAs(UnmanagedType.Bool)] bool bIntialState, [In, MarshalAs(UnmanagedType.BStr)] string lpName);

      [DllImport("coredll.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool CloseHandle(IntPtr hObject);

      [DllImport("coredll.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool EventModify(IntPtr hEvent, [In, MarshalAs(UnmanagedType.U4)] int dEvent);

      public enum EventFlags
      {
          PULSE = 1,
          RESET = 2,
          SET = 3
      }

      public const UInt32 INFINITE = 0xFFFFFFFF;
      public const UInt32 WAIT_ABANDONED = 0x00000080;
      public const UInt32 WAIT_OBJECT_0 = 0x00000000;
      public const UInt32 WAIT_TIMEOUT = 0x00000102;

      public struct SYSTEMTIME
      {
          public short Year;
          public short Month;
          public short DayOfWeek;
          public short Day;
          public short Hour;
          public short Minute;
          public short Second;
          public short Milliseconds;

          public SYSTEMTIME(DateTime dt)
          {
              Year = (short)dt.Year;
              Month = (short)dt.Month;
              DayOfWeek = (short)dt.DayOfWeek;
              Day = (short)dt.Day;
              Hour = (short)dt.Hour;
              Minute = (short)dt.Minute;
              Second = (short)dt.Second;
              Milliseconds = (short)dt.Millisecond;
          }

          public static SYSTEMTIME FromDateTime(DateTime dt)
          {
              return new SYSTEMTIME
              {
                  Year = (short)dt.Year,
                  Month = (short)dt.Month,
                  DayOfWeek = (short)dt.DayOfWeek,
                  Day = (short)dt.Day,
                  Hour = (short)dt.Hour,
                  Minute = (short)dt.Minute,
                  Second = (short)dt.Second,
                  Milliseconds = (short)dt.Millisecond
              };
          }

          public DateTime ToDateTime()
          {
              if ((((Year == 0) && (Month == 0)) && ((Day == 0) && (Hour == 0))) && ((Minute == 0) && (Second == 0)))
                  return DateTime.MinValue;

              return new DateTime(Year, Month, Day, Hour, Minute, Second, Milliseconds);
          }
      }

      [DllImport("coredll.dll")]
      public static extern bool SetSystemTime(ref SYSTEMTIME time);

      [DllImport("coredll.dll")]
      public static extern void GetLocalTime(out SYSTEMTIME lpSystemTime);

      public enum CNT_TYPE : uint
      {
          CNT_EVENT = 1,
          CNT_TIME = 2,
          CNT_PERIOD = 3,
          CNT_CLASSICTIME = 4
      } 

      public class CE_NOTIFICATION_TRIGGER
      {
          public UInt32 Size = 52;
          public UInt32 Type = 0;
          public UInt32 Event = 0;

          [MarshalAs(UnmanagedType.LPWStr)]
          public string pAppName = string.Empty;

          [MarshalAs(UnmanagedType.LPWStr)]
          public string pArgs;

          public SYSTEMTIME StartTime;
          public SYSTEMTIME EndTime;
      }

      public class CE_USER_NOTIFICATION
      {
          public UInt32 ActionFlags;

          [MarshalAs(UnmanagedType.LPWStr)]
          public string pDialogTitle;

          [MarshalAs(UnmanagedType.LPWStr)]
          public string DialogText;

          [MarshalAs(UnmanagedType.LPWStr)]
          public string Sound;

          public UInt32 MaxSound;
          public UInt32 Reserved;
      }

      [DllImport("coredll.dll")]
      public static extern IntPtr CeSetUserNotificationEx(IntPtr notification, CE_NOTIFICATION_TRIGGER notificationTrigger, CE_USER_NOTIFICATION userNotification);

      [DllImport("coredll.dll", EntryPoint = "CeClearUserNotification", SetLastError = true)]
      public static extern bool CeClearUserNotification(int hNotification);
   }
}
