
namespace GMap.NET.Internals
{
   using System;
   using System.Runtime.InteropServices;

   public class Native
   {
      static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr) (-1);

      // The CharSet must match the CharSet of the corresponding PInvoke signature
      [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
      struct WIN32_FIND_DATA
      {
         public int dwFileAttributes;
         public FILETIME ftCreationTime;
         public FILETIME ftLastAccessTime;
         public FILETIME ftLastWriteTime;
         public int nFileSizeHigh;
         public int nFileSizeLow;
         public int dwOID;
         [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
         public string cFileName;
         [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
         public string cAlternateFileName;
      }

      [StructLayout(LayoutKind.Sequential)]
      struct FILETIME
      {
         public int dwLowDateTime;
         public int dwHighDateTime;
      };

      [DllImport("note_prj", EntryPoint="FindFirstFlashCard")]
      extern static IntPtr FindFirstFlashCard(ref WIN32_FIND_DATA findData);

      [DllImport("note_prj", EntryPoint="FindNextFlashCard")]
      [return: MarshalAs(UnmanagedType.Bool)]
      extern static bool FindNextFlashCard(IntPtr hFlashCard, ref WIN32_FIND_DATA findData);

      [DllImport("coredll")]
      static extern bool FindClose(IntPtr hFindFile);

      public static string GetRemovableStorageDirectory()
      {
         string removableStorageDirectory = null;
         IntPtr handle = IntPtr.Zero;
         try
         {
            WIN32_FIND_DATA findData = new WIN32_FIND_DATA();

            handle = FindFirstFlashCard(ref findData);

            if(handle != INVALID_HANDLE_VALUE)
            {
               do
               {
                  if(!string.IsNullOrEmpty(findData.cFileName))
                  {
                     removableStorageDirectory = findData.cFileName;
                     break;
                  }
               }
               while(FindNextFlashCard(handle, ref findData));
            }
         }
         catch
         {
            removableStorageDirectory = null;
         }
         finally
         {
            if(handle != INVALID_HANDLE_VALUE)
            {
               FindClose(handle);
            }
         }
         return removableStorageDirectory;
      }

      [DllImport("coredll.dll")]
      public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

      public const int SW_MINIMIZED = 6;

      public const int PPN_UNATTENDEDMODE = 0x0003;
      public const int POWER_NAME = 0x00000001;
      public const int POWER_FORCE = 0x00001000;

      [DllImport("coredll.dll")]
      public static extern bool PowerPolicyNotify(int dwMessage, bool dwData);

      [DllImport("coredll.dll", SetLastError=true)]
      public static extern IntPtr SetPowerRequirement(string pvDevice, CedevicePowerStateState deviceState, uint deviceFlags, string pvSystemState, ulong stateFlags);

      [DllImport("coredll.dll", SetLastError=true)]
      public static extern int ReleasePowerRequirement(IntPtr hPowerReq);

      public enum CedevicePowerStateState : int
      {
         PwrDeviceUnspecified=-1,
         D0=0,
         D1,
         D2,
         D3,
         D4,
      }

      [DllImport("coredll")]
      public static extern void SystemIdleTimerReset();
   }
}
