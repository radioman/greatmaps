
namespace GMap.NET.Internals
{
   using System;
   using System.Threading;
#if !MONO
   using System.Runtime.InteropServices;
#endif

   /// <summary>
   /// custom ReaderWriterLock
   /// in Vista and later uses integrated Slim Reader/Writer (SRW) Lock
   /// http://msdn.microsoft.com/en-us/library/aa904937(VS.85).aspx
   /// http://msdn.microsoft.com/en-us/magazine/cc163405.aspx#S2
   /// </summary>
   public sealed class FastReaderWriterLock
   {
#if !MONO
      private static class NativeMethods
      {
         // Methods
         [DllImport("Kernel32", ExactSpelling=true)]
         internal static extern void AcquireSRWLockExclusive(ref IntPtr srw);
         [DllImport("Kernel32", ExactSpelling=true)]
         internal static extern void AcquireSRWLockShared(ref IntPtr srw);
         [DllImport("Kernel32", ExactSpelling=true)]
         internal static extern void InitializeSRWLock(out IntPtr srw);
         [DllImport("Kernel32", ExactSpelling=true)]
         internal static extern void ReleaseSRWLockExclusive(ref IntPtr srw);
         [DllImport("Kernel32", ExactSpelling=true)]
         internal static extern void ReleaseSRWLockShared(ref IntPtr srw);
      }

      IntPtr LockSRW;

      public FastReaderWriterLock()
      {
         if(VistaOrLater)
         {
            NativeMethods.InitializeSRWLock(out this.LockSRW);
         }
      }
#endif

      static readonly bool VistaOrLater = Stuff.IsRunningOnVistaOrLater();
      Int32 busy = 0;
      Int32 readCount = 0;

      public void AcquireReaderLock()
      {
         if(VistaOrLater)
         {
            NativeMethods.AcquireSRWLockShared(ref LockSRW);
         }
         else
         {
            Thread.BeginCriticalRegion();

            while(Interlocked.CompareExchange(ref busy, 1, 0) != 0)
            {
               Thread.Sleep(1);
            }

            Interlocked.Increment(ref readCount);

            // somehow this fix deadlock on heavy reads
            Thread.Sleep(0);
            Thread.Sleep(0);
            Thread.Sleep(0);
            Thread.Sleep(0);
            Thread.Sleep(0);
            Thread.Sleep(0);
            Thread.Sleep(0);

            Interlocked.Exchange(ref busy, 0);
         }
      }

      public void ReleaseReaderLock()
      {
         if(VistaOrLater)
         {
            NativeMethods.ReleaseSRWLockShared(ref LockSRW);
         }
         else
         {
            Interlocked.Decrement(ref readCount);
            Thread.EndCriticalRegion();
         }
      }

      public void AcquireWriterLock()
      {
         if(VistaOrLater)
         {
            NativeMethods.AcquireSRWLockExclusive(ref LockSRW);
         }
         else
         {
            Thread.BeginCriticalRegion();

            while(Interlocked.CompareExchange(ref busy, 1, 0) != 0)
            {
               Thread.Sleep(1);
            }

            while(Interlocked.CompareExchange(ref readCount, 0, 0) != 0)
            {
               Thread.Sleep(1);
            }
         }
      }

      public void ReleaseWriterLock()
      {
         if(VistaOrLater)
         {
            NativeMethods.ReleaseSRWLockExclusive(ref LockSRW);
         }
         else
         {
            Interlocked.Exchange(ref busy, 0);
            Thread.EndCriticalRegion();
         }
      }
   }
}
