using System;
using System.IO;

namespace GMapNET
{
   internal abstract class PureImageProxy
   {
      abstract public PureImage FromStream(Stream stream);
      abstract public bool Save(Stream stream, PureImage image);
   }

   internal abstract class PureImage : ICloneable, IDisposable
   {
      abstract public IntPtr GetHbitmap();

      #region ICloneable Members

      abstract public object Clone();

      #endregion

      #region IDisposable Members

      abstract public void Dispose();

      #endregion
   }
}
