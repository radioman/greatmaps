
namespace GMap.NET.Internals
{
#if PocketPC
   internal class FastReaderWriterLock
   {
      object rlock = new object();

      internal void AcquireReaderLock()
      {
          System.Threading.Monitor.Enter(rlock);
      }

      internal void ReleaseReaderLock()
      {
          System.Threading.Monitor.Exit(rlock);
      }

      internal void AcquireWriterLock()
      {
          System.Threading.Monitor.Enter(rlock);
      }

      internal void ReleaseWriterLock()
      {
          System.Threading.Monitor.Exit(rlock);
      }
 
      internal void Dispose()
      {
          rlock = null;
      }
   }
#endif
}
