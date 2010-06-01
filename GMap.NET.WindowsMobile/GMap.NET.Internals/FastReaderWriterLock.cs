
namespace System.Threading
{
   using System;

#if PocketPC
   internal class FastReaderWriterLock
   {
      object rlock = new object();
      internal void AcquireReaderLock()
      {
         Monitor.Enter(rlock);
      }

      internal void ReleaseReaderLock()
      {
         Monitor.Exit(rlock);
      }

      internal void AcquireWriterLock()
      {
         Monitor.Enter(rlock);
      }

      internal void ReleaseWriterLock()
      {
         Monitor.Exit(rlock);
      }
   }
#endif
}
