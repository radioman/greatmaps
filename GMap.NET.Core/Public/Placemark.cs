using System.Globalization;

namespace GMapNET
{
   public class Placemark
   {
      string address;
      public string Address
      {
         get
         {
            return address;
         }
         internal set
         {
            address = value;
         }
      }

      private int accuracy;
      public int Accuracy
      {
         get
         {
            return accuracy;
         }
         internal set
         {
            accuracy = value;
         }
      }

      // parsed values from address
      public string ThoroughfareName;
      public string LocalityName;
      public string PostalCodeNumber;
      public string CountryName;

      public Placemark(string address)
      {
         this.address = address;
      }

      /// <summary>
      /// parse address
      /// </summary>
      /// <param name="address"></param>
      /// <returns></returns>
      protected virtual bool ParseAddress()
      {
         // usa format
         //200,8,\"701 Constitution Ave NW, Washington, DC 20004, USA\"

         // eu format           
         //200,8,"10-80 Didlaukio gatve, Vilnius LT-08013, Lietuva"

         bool ret = false;

         // ...
         //plc.CountryName = address.Substring(...

         return ret;
      }
   }
}
