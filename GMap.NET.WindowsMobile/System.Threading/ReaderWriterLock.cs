
namespace System.Threading
{
   using System;

#if PocketPC
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
   }
#endif
}
