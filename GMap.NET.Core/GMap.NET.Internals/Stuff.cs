
namespace GMap.NET.Internals
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.IO;
   using System.Reflection;
   using System.Threading;
   using System.Diagnostics;

   /// <summary>
   /// etc functions...
   /// </summary>
   internal class Stuff
   {
      public static string EnumToString(Enum value)
      {
         FieldInfo fi = value.GetType().GetField(value.ToString());
         DescriptionAttribute[] attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(
               typeof(DescriptionAttribute), false);

         return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
      }

      [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint="SetCursorPos")]
      [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
      public static extern bool SetCursorPos(int X, int Y);

      static Random random = new System.Random();

      public static void Shuffle<T>(ref List<T> deck)
      {
         int N = deck.Count;

         for(int i = 0; i < N; ++i)
         {
            int r = i + (int) (random.Next(N - i));
            T t = deck[r];
            deck[r] = deck[i];
            deck[i] = t;
         }
      }

      public static MemoryStream CopyStream(Stream inputStream, bool SeekOriginBegin)
      {
         const int readSize = 4 * 1024;
         byte[] buffer = new byte[readSize];
         MemoryStream ms = new MemoryStream();
         {
            int count = inputStream.Read(buffer, 0, readSize);
            while(count > 0)
            {
               ms.Write(buffer, 0, count);
               count = inputStream.Read(buffer, 0, readSize);
            }
         }
         buffer = null;
         if(SeekOriginBegin)
         {
            inputStream.Seek(0, SeekOrigin.Begin);
         }
         ms.Seek(0, SeekOrigin.Begin);
         return ms;
      }
   }

   public sealed class oGReaderWriterLock
   {

      public void AcquireReaderLock()
      {
         Monitor.Enter(this);

         //Debug.WriteLine("AcquireReaderLock(" + numReads + "): " +  + Thread.CurrentThread.ManagedThreadId);
      }

      public void ReleaseReaderLock()
      {
         Monitor.Exit(this);

         //Debug.WriteLine("ReleaseReaderLock: " + Thread.CurrentThread.ManagedThreadId);
      }

      public void AcquireWriterLock()
      {
         Monitor.Enter(this);
         

         //Debug.WriteLine("AcquireWriterLock: " + Thread.CurrentThread.ManagedThreadId);
      }

      public void ReleaseWriterLock()
      {
         Monitor.Exit(this);
         

         // Debug.WriteLine("ReleaseWriterLock: " + Thread.CurrentThread.ManagedThreadId);
      }
   }

   public sealed class GReaderWriterLock
   {
      int busy = 0;
      int numReads = 0;

      public void AcquireReaderLock()
      {
         Thread.BeginCriticalRegion();

         while(Interlocked.CompareExchange(ref busy, 1, 0) != 0)
         {
            Thread.Sleep(1);
         }

         Interlocked.Increment(ref numReads);
         Thread.VolatileWrite(ref busy, 0);

         //Debug.WriteLine("AcquireReaderLock(" + numReads + "): " +  + Thread.CurrentThread.ManagedThreadId);
      }

      public void ReleaseReaderLock()
      {
         Interlocked.Decrement(ref numReads); 
         Thread.EndCriticalRegion();

         //Debug.WriteLine("ReleaseReaderLock: " + Thread.CurrentThread.ManagedThreadId);
      }

      public void AcquireWriterLock()
      {
         Thread.BeginCriticalRegion();

         while(Interlocked.CompareExchange(ref busy, 1, 0) != 0)
         {
            Thread.Sleep(1);
         }
         while(Thread.VolatileRead(ref numReads) != 0)
         {
            Thread.Sleep(1);
         }

         //Debug.WriteLine("AcquireWriterLock: " + Thread.CurrentThread.ManagedThreadId);
      }

      public void ReleaseWriterLock()
      {
         Thread.VolatileWrite(ref busy, 0); 
         Thread.EndCriticalRegion();

        // Debug.WriteLine("ReleaseWriterLock: " + Thread.CurrentThread.ManagedThreadId);
      }
   }
}
