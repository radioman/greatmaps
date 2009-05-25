using System;
using System.IO;

namespace GMapNET
{
   public abstract class PureImageProxy
   {
      abstract public PureImage FromStream(Stream stream);
      abstract public bool Save(Stream stream, PureImage image);
   }

   public abstract class PureImage : ICloneable, IDisposable
   {
      #region ICloneable Members

      abstract public object Clone();

      #endregion

      #region IDisposable Members

      abstract public void Dispose();

      #endregion
   }
}
