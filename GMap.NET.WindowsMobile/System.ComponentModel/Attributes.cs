
namespace System.ComponentModel
{
   using System;

   class CategoryAttribute : Attribute
   {
      string category;
      public string Category
      {
         get
         {
            return category;
         }
         set
         {
            category = value;
         }
      }

      public CategoryAttribute(string category)
      {
         this.Category = category;
      }
   }

   class DescriptionAttribute : Attribute
   {
      string description;
      public string Description
      {
         get
         {
            return description;
         }
         set
         {
            description = value;
         }
      }

      public DescriptionAttribute(string description)
      {
         this.Description = description;
      }
   }

   class BrowsableAttribute : Attribute
   {
      bool browsable;

      public bool Browsable
      {
         get
         {
            return browsable;
         }
         set
         {
            browsable = value;
         }
      }

      public BrowsableAttribute(bool browsable)
      {
         this.Browsable = browsable;
      }
   }

   public enum DesignerSerializationVisibility
   {
      Hidden=0,
      Visible=1,
      Content=2,
   }

   public class DesignerSerializationVisibilityAttribute : Attribute
   {
      DesignerSerializationVisibility visibility;
      public DesignerSerializationVisibility Visibility
      {
         get
         {
            return visibility;
         }
         set
         {
            visibility = value;
         }
      }

      public DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility visibility)
      {
         this.Visibility = visibility;
      }
   }
}
