
namespace Demo.WindowsMobile
{
   using System;
   using System.Runtime.CompilerServices;
   using System.Runtime.InteropServices;

   public class HookKeys
   {
      private static int hHook;
      private HookProc hookDeleg;
      private const int WH_KEYBOARD_LL = 20;

      public event HookEventHandler HookEvent;

      [DllImport("coredll.dll")]
      private static extern int CallNextHookEx(HookProc hhk, int nCode, IntPtr wParam, IntPtr lParam);
      [DllImport("coredll.dll")]
      private static extern int GetCurrentThreadId();
      [DllImport("coredll.dll")]
      private static extern IntPtr GetModuleHandle(string mod);
      private int HookProcedure(int code, IntPtr wParam, IntPtr lParam)
      {
         KBDLLHOOKSTRUCT kbdllhookstruct = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
         if(code >= 0)
         {
            HookEventArgs hookArgs = new HookEventArgs();
            hookArgs.Code = code;
            hookArgs.wParam = wParam;
            hookArgs.lParam = lParam;
            KeyBoardInfo keyBoardInfo = new KeyBoardInfo();
            keyBoardInfo.vkCode = kbdllhookstruct.vkCode;
            keyBoardInfo.scanCode = kbdllhookstruct.scanCode;
            if(this.OnHookEvent(hookArgs, keyBoardInfo))
            {
               return 1;
            }
         }
         return CallNextHookEx(this.hookDeleg, code, wParam, lParam);
      }

      public bool isRunning()
      {
         if(hHook == 0)
         {
            return false;
         }
         return true;
      }

      protected virtual bool OnHookEvent(HookEventArgs hookArgs, KeyBoardInfo keyBoardInfo)
      {
         return ((this.HookEvent != null) && this.HookEvent(hookArgs, keyBoardInfo));
      }

      [DllImport("coredll.dll")]
      private static extern int SetWindowsHookEx(int type, HookProc hookProc, IntPtr hInstance, int m);
      public void Start()
      {
         if(isRunning())
         {
            this.Stop();
         }
         this.hookDeleg = new HookProc(this.HookProcedure);
         hHook = SetWindowsHookEx(20, this.hookDeleg, GetModuleHandle(null), 0);
         if(hHook == 0)
         {
            throw new SystemException("Failed acquiring of the hook.");
         }
      }

      public void Stop()
      {
         if(isRunning())
         {
            UnhookWindowsHookEx(hHook);
            hHook = 0;
         }
      }

      [DllImport("coredll.dll", SetLastError=true)]
      private static extern int UnhookWindowsHookEx(int idHook);

      public delegate bool HookEventHandler(HookEventArgs e, KeyBoardInfo keyBoardInfo);

      public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

      [StructLayout(LayoutKind.Sequential)]
      private struct KBDLLHOOKSTRUCT
      {
         public int vkCode;
         public int scanCode;
         public int flags;
         public int time;
         public IntPtr dwExtraInfo;
      }
   }

   public class HookEventArgs : EventArgs
   {
      public int Code;
      public IntPtr lParam;
      public IntPtr wParam;
   }

   public class KeyBoardInfo
   {
      public int flags;
      public int scanCode;
      public int time;
      public int vkCode;
   }
}

