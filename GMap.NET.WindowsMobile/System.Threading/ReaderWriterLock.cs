
namespace System.Threading
{
   using System;

#if PocketPC

   /// <summary>
   /// Condition Variable (CV) class.
   /// </summary>
   //public class CV
   //{
   //   private readonly OpenNETCF.Threading.Monitor2 syncLock = new OpenNETCF.Threading.Monitor2(); // Internal lock.
   //   private readonly OpenNETCF.Threading.Monitor2 m;  // The lock associated with this CV.

   //   public CV(OpenNETCF.Threading.Monitor2 m)
   //   {
   //      lock(syncLock)
   //      {
   //         this.m = m;
   //      }
   //   }

   //   public void Wait()
   //   {
   //      bool enter = false;
   //      try
   //      {
   //         lock(syncLock)
   //         {
   //            Monitor.Exit(m);
   //            enter = true;
   //            syncLock.Wait(); // Monitor.Wait(syncLock);
   //         }
   //      }
   //      finally
   //      {
   //         if(enter)
   //         {
   //            Monitor.Enter(m);
   //         }
   //      }
   //   }

   //   public void Pulse()
   //   {
   //      lock(syncLock)
   //      {
   //         syncLock.Pulse(); // Monitor.Pulse(syncLock);
   //      }
   //   }

   //   public void PulseAll()
   //   {
   //      lock(syncLock)
   //      {
   //         syncLock.PulseAll(); // Monitor.PulseAll(syncLock);
   //      }
   //   }
   //}

   internal class ReaderWriterLock
   {
      object rlock = new object();
      internal void AcquireReaderLock(int p)
      {
         Monitor.Enter(rlock);
      }

      internal void ReleaseReaderLock()
      {
         Monitor.Exit(rlock);
      }

      internal void AcquireWriterLock(int p)
      {
         Monitor.Enter(rlock);
      }

      internal void ReleaseWriterLock()
      {
         Monitor.Exit(rlock);
      }

      //private readonly OpenNETCF.Threading.Monitor2 syncRoot = new OpenNETCF.Threading.Monitor2();    // Internal lock.
      //private int i = 0;                                  // 0 or greater means readers can pass; -1 is active writer.
      //private int readWaiters = 0;                        // Readers waiting for writer to exit.
      //private int writeWaiters = 0;                       // Writers waiting for writer lock.
      //private CV wQ;                                      // Condition variable.

      //public ReaderWriterLock()
      //{
      //   wQ = new CV(syncRoot);
      //}
      ///// <summary>
      ///// Gets a value indicating if a reader lock is held.
      ///// </summary>
      //public bool IsReaderLockHeld
      //{
      //   get
      //   {
      //      lock(syncRoot)
      //      {
      //         if(i > 0)
      //         {
      //            return true;
      //         }
      //         return false;
      //      }
      //   }
      //}
      ///// <summary>
      ///// Gets a value indicating if the writer lock is held.
      ///// </summary>
      //public bool IsWriterLockHeld
      //{
      //   get
      //   {
      //      lock(syncRoot)
      //      {
      //         if(i < 0)
      //         {
      //            return true;
      //         }
      //         return false;
      //      }
      //   }
      //}
      ///// <summary>
      ///// Aquires the writer lock.
      ///// </summary>
      //public void AcquireWriterLock(int i)
      //{
      //   lock(syncRoot)
      //   {
      //      writeWaiters++;
      //      while(i != 0)
      //      {
      //         wQ.Wait();      // Wait until existing writer frees the lock.
      //      }
      //      writeWaiters--;
      //      i = -1;             // Thread has writer lock.
      //   }
      //}
      ///// <summary>
      ///// Aquires a reader lock.
      ///// </summary>
      //public void AcquireReaderLock(int i)
      //{
      //   lock(syncRoot)
      //   {
      //      readWaiters++;

      //      // Defer to a writer (one time only) if one is waiting to prevent writer starvation.
      //      if(writeWaiters > 0)
      //      {
      //         wQ.Pulse();
      //         syncRoot.Wait(); //Monitor.Wait(syncRoot); 
      //      }

      //      while(i < 0)
      //      {
      //         syncRoot.Wait(); //Monitor.Wait(syncRoot);
      //      }

      //      readWaiters--;
      //      i++;
      //   }
      //}
      ///// <summary>
      ///// Releases the writer lock.
      ///// </summary>
      //public void ReleaseWriterLock()
      //{
      //   bool doPulse = false;

      //   lock(syncRoot)
      //   {
      //      i = 0;

      //      // Decide if we pulse a writer or readers.
      //      if(readWaiters > 0)
      //      {
      //         // If multiple readers waiting, pulse them all.
      //         syncRoot.PulseAll();
      //      }
      //      else
      //      {
      //         doPulse = true;
      //      }
      //   }

      //   if(doPulse)
      //   {
      //      wQ.Pulse();                     // Pulse one writer if one waiting.
      //   }
      //}
      ///// <summary>
      ///// Releases a reader lock.
      ///// </summary>
      //public void ReleaseReaderLock()
      //{
      //   bool doPulse = false;

      //   lock(syncRoot)
      //   {
      //      i--;
      //      if(i == 0)
      //      {
      //         doPulse = true;
      //      }
      //   }

      //   if(doPulse)
      //   {
      //      wQ.Pulse();                     // Pulse one writer if one waiting.
      //   }
      //}
   }
#endif
}
