using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Street_WpfApplication
{
   public class PanoramaViewer : Viewport3D
   {
      public double FieldOfView
      {
         get
         {
            return (double) GetValue(FieldOfViewProperty);
         }
         set
         {
            SetValue(FieldOfViewProperty, value);
         }
      }

      // Using a DependencyProperty as the backing store for FieldOfView.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FieldOfViewProperty =
            DependencyProperty.Register("FieldOfView", typeof(double), typeof(PanoramaViewer), new PropertyMetadata(
                  (double) 0, new PropertyChangedCallback(OnFieldOfViewChanged)));

      internal static void OnFieldOfViewChanged(Object sender, DependencyPropertyChangedEventArgs e)
      {
         PanoramaViewer Viewer = ((PanoramaViewer) sender);
         PerspectiveCamera Camera = Viewer.Camera as PerspectiveCamera;
         Camera.FieldOfView = Viewer.FieldOfView;
      }

      public ImageSource PanoramaImage
      {
         get
         {
            return (ImageSource) GetValue(PanoramaImageProperty);
         }
         set
         {
            SetValue(PanoramaImageProperty, value);
         }
      }

      // Using a DependencyProperty as the backing store for PanoramaImage.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty PanoramaImageProperty =
            DependencyProperty.Register("PanoramaImage", typeof(ImageSource), typeof(PanoramaViewer), new PropertyMetadata(
                  null, new PropertyChangedCallback(OnPanoramaImageChanged)));

      internal static void OnPanoramaImageChanged(Object sender, DependencyPropertyChangedEventArgs e)
      {
         PanoramaViewer Viewer = ((PanoramaViewer) sender);
         ImageBrush PanoramaBrush = new ImageBrush(Viewer.PanoramaImage);
         Viewer.PanoramaGeometry.BackMaterial = new DiffuseMaterial(PanoramaBrush);
      }

      #region Rotation

      static private readonly Vector3D AxisX = new Vector3D(1, 0, 0);
      static private readonly Vector3D AxisY = new Vector3D(0, 1, 0);
      static private readonly Vector3D AxisZ = new Vector3D(0, 0, 1);

      public static readonly DependencyProperty RotationXProperty =
            DependencyProperty.Register("RotationX", typeof(double), typeof(PanoramaViewer), new UIPropertyMetadata(0.0, (d, args) => ((PanoramaViewer) d).UpdateRotation()));
      public static readonly DependencyProperty RotationYProperty =
            DependencyProperty.Register("RotationY", typeof(double), typeof(PanoramaViewer), new UIPropertyMetadata(0.0, (d, args) => ((PanoramaViewer) d).UpdateRotation()));
      public static readonly DependencyProperty RotationZProperty =
            DependencyProperty.Register("RotationZ", typeof(double), typeof(PanoramaViewer), new UIPropertyMetadata(0.0, (d, args) => ((PanoramaViewer) d).UpdateRotation()));


      public double RotationX
      {
         get
         {
            return (double) GetValue(RotationXProperty);
         }
         set
         {
            SetValue(RotationXProperty, value);
         }
      }

      public double RotationY
      {
         get
         {
            return (double) GetValue(RotationYProperty);
         }
         set
         {
            SetValue(RotationYProperty, value);
         }
      }

      public double RotationZ
      {
         get
         {
            return (double) GetValue(RotationZProperty);
         }
         set
         {
            SetValue(RotationZProperty, value);
         }
      }

      private void UpdateRotation()
      {
         Quaternion qx = new Quaternion(AxisX, RotationX);
         Quaternion qy = new Quaternion(AxisY, RotationY);
         Quaternion qz = new Quaternion(AxisZ, RotationZ);
         PanoramaRotation.Quaternion = qx * qy * qz;
      }

      QuaternionRotation3D PanoramaRotation
      {
         get;
         set;
      }
      #endregion

      public PanoramaViewer()
      {
         InitializeViewer();
      }

      public ModelVisual3D PanoramaObject
      {
         get;
         set;
      }
      public GeometryModel3D PanoramaGeometry
      {
         get;
         set;
      }

      public void InitializeViewer()
      {
         ///////////////////////////////////////////
         // Camera Initialize
         ///////////////////////////////////////////
         PerspectiveCamera PanoramaCamera = new PerspectiveCamera();
         PanoramaCamera.Position = new Point3D(0, -0.0, 0);
         PanoramaCamera.UpDirection = new Vector3D(0, 1, 0);
         PanoramaCamera.LookDirection = new Vector3D(0, 0, 1);
         PanoramaCamera.FieldOfView = 80;
         this.Camera = PanoramaCamera;

         FieldOfView = 80;

         ///////////////////////////////////////////
         // Light Initialize
         ///////////////////////////////////////////
         ModelVisual3D LightModel = new ModelVisual3D();
         LightModel.Content = new DirectionalLight(Colors.White, new Vector3D(0, 0, 1));
         this.Children.Add(LightModel);

         ///////////////////////////////////////////
         // Panorama Object Initialize
         ///////////////////////////////////////////

         PanoramaObject = new ModelVisual3D();
         PanoramaGeometry = new GeometryModel3D();
         PanoramaGeometry.Geometry = CreateGeometry();
         PanoramaObject.Content = PanoramaGeometry;

         RotateTransform3D RotateTransform = new RotateTransform3D();

         double x = 1.0;
         ScaleTransform3D ScaleTransform = new ScaleTransform3D()
         {
            ScaleX = x*1,
            ScaleY = x*1.65,
            ScaleZ = x*1
         };

         Transform3DGroup Group = new Transform3DGroup();
         PanoramaRotation = new QuaternionRotation3D();
         Group.Children.Add(ScaleTransform);
         Group.Children.Add(RotateTransform);

         RotateTransform.Rotation = PanoramaRotation;
         PanoramaObject.Transform = Group;

         this.Children.Add(PanoramaObject);
      }

      private Geometry3D CreateGeometry()
      {
         int tDiv = 64;
         int yDiv = 64;
         double maxTheta = (360.0 / 180.0) * Math.PI;
         double minY = -1.0;
         double maxY = 1.0;

         double dt = maxTheta / tDiv;
         double dy = (maxY - minY) / yDiv;

         MeshGeometry3D mesh = new MeshGeometry3D();

         for(int yi = 0; yi <= yDiv; yi++)
         {
            double y = minY + yi * dy;

            for(int ti = 0; ti <= tDiv; ti++)
            {
               double t = ti * dt;

               mesh.Positions.Add(GetPosition(t, y));
               mesh.Normals.Add(GetNormal(t, y));
               mesh.TextureCoordinates.Add(GetTextureCoordinate(t, y));
            }
         }

         for(int yi = 0; yi < yDiv; yi++)
         {
            for(int ti = 0; ti < tDiv; ti++)
            {
               int x0 = ti;
               int x1 = (ti + 1);
               int y0 = yi * (tDiv + 1);
               int y1 = (yi + 1) * (tDiv + 1);

               mesh.TriangleIndices.Add(x0 + y0);
               mesh.TriangleIndices.Add(x0 + y1);
               mesh.TriangleIndices.Add(x1 + y0);

               mesh.TriangleIndices.Add(x1 + y0);
               mesh.TriangleIndices.Add(x0 + y1);
               mesh.TriangleIndices.Add(x1 + y1);
            }
         }

         mesh.Freeze();
         return mesh;
      }

      internal Point3D GetPosition(double t, double y)
      {
         double r = Math.Sqrt(1 - y * y);
         double x = r * Math.Cos(t);
         double z = r * Math.Sin(t);

         return new Point3D(x, y, z);
      }

      private Vector3D GetNormal(double t, double y)
      {
         return (Vector3D) GetPosition(t, y);
      }

      private Point GetTextureCoordinate(double t, double y)
      {
         Matrix TYtoUV = new Matrix();
         TYtoUV.Scale(1 / (2 * Math.PI), -0.5);

         Point p = new Point(t, y);
         p = p * TYtoUV;

         return p;
      }
   }
}
