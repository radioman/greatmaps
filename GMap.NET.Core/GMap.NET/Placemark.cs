
namespace GMap.NET
{
   /// <summary>
   /// represents place info
   /// </summary>
   public class Placemark
   {
      string address;

      /// <summary>
      /// the address
      /// </summary>
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

      /// <summary>
      /// the accuracy of address
      /// </summary>
      public int Accuracy;

      // parsed values from address
      public string HouseNo;
      public string ThoroughfareName;
      public string DistrictName;
      public string LocalityName;
      public string PostalCodeNumber;
      public string CountryName;
      public string CountryNameCode;
      public string AdministrativeAreaName;
      public string SubAdministrativeAreaName;

      public Placemark(string address)
      {
         this.address = address;
      }
   }
}
