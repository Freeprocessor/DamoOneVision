using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using DamoOneVision.Camera;
using Spinnaker;

namespace DamoOneVision
{
	/// <summary>
	/// _3DView.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class _3DView : Window
	{

		private byte[] pixels;
		private Viewport3D viewport;

		private System.Windows.Point lastMousePosition;
		private bool isLeftButtonDown = false;
		private bool isRightButtonDown = false;

		// 변환을 적용할 Transform3DGroup과 각종 Transform
		private Transform3DGroup transformGroup;
		private RotateTransform3D rotateXTransform;
		private RotateTransform3D rotateYTransform;
		private AxisAngleRotation3D xRotation;
		private AxisAngleRotation3D yRotation;
		private TranslateTransform3D translateTransform;

		// 카메라 정보
		private PerspectiveCamera camera;
		private double cameraDistance;

		private System.Windows.Point initialMouseDownPosition;
		private double initialXAngle = 0;
		private double initialYAngle = 0;


		public _3DView( byte[ ] pixels )
		{
			InitializeComponent();

			this.pixels = pixels;
			this.Loaded += ViewWindow_Loaded;
		}

		private void ViewWindow_Loaded( object sender, RoutedEventArgs e )
		{
			LoadAndDisplay3DColumns( pixels );
		}

		private void LoadAndDisplay3DColumns( byte[] pixels )
		{
			int width=0;// = (int)MILContext.Width;
			int height=0;// = (int)MILContext.Height;
			int bytesPerPixel = 2; // Gray16 => 16bit = 2bytes
			int stride = width * bytesPerPixel;

			MeshGeometry3D mesh = new MeshGeometry3D();

			// Gray16 범위: 0~65535
			// 스케일을 적절히 조정. 예: 최대 높이를 50으로
			double zScaleMaxHeight = 1000.0;

			for (int py = 0; py < height; py++)
			{
				for (int px = 0; px < width; px++)
				{
					// **변경사항: 픽셀 추출 (2바이트)**
					int baseIndex = py * stride + px * 2;
					ushort pixelValue = (ushort)(pixels[baseIndex] | (pixels[baseIndex + 1] << 8));

					double normalizedValue = pixelValue / 65535.0;
					double h = normalizedValue * zScaleMaxHeight;

					int baseVertexIndex = mesh.Positions.Count;

					// 하단면 (z=0)
					mesh.Positions.Add( new Point3D( px, py, 0 ) );   // V0
					mesh.Positions.Add( new Point3D( px + 1, py, 0 ) );   // V1
					mesh.Positions.Add( new Point3D( px + 1, py + 1, 0 ) ); // V2
					mesh.Positions.Add( new Point3D( px, py + 1, 0 ) ); // V3

					// 상단면 (z=h)
					mesh.Positions.Add( new Point3D( px, py, h ) );   // V4
					mesh.Positions.Add( new Point3D( px + 1, py, h ) );   // V5
					mesh.Positions.Add( new Point3D( px + 1, py + 1, h ) ); // V6
					mesh.Positions.Add( new Point3D( px, py + 1, h ) ); // V7

					// 인덱스 추가 (면별 2삼각형)
					// 아래면
					mesh.TriangleIndices.Add( baseVertexIndex + 0 );
					mesh.TriangleIndices.Add( baseVertexIndex + 1 );
					mesh.TriangleIndices.Add( baseVertexIndex + 2 );
					mesh.TriangleIndices.Add( baseVertexIndex + 0 );
					mesh.TriangleIndices.Add( baseVertexIndex + 2 );
					mesh.TriangleIndices.Add( baseVertexIndex + 3 );

					// 위면
					mesh.TriangleIndices.Add( baseVertexIndex + 4 );
					mesh.TriangleIndices.Add( baseVertexIndex + 6 );
					mesh.TriangleIndices.Add( baseVertexIndex + 5 );
					mesh.TriangleIndices.Add( baseVertexIndex + 4 );
					mesh.TriangleIndices.Add( baseVertexIndex + 7 );
					mesh.TriangleIndices.Add( baseVertexIndex + 6 );

					// 앞면
					mesh.TriangleIndices.Add( baseVertexIndex + 0 );
					mesh.TriangleIndices.Add( baseVertexIndex + 5 );
					mesh.TriangleIndices.Add( baseVertexIndex + 1 );
					mesh.TriangleIndices.Add( baseVertexIndex + 0 );
					mesh.TriangleIndices.Add( baseVertexIndex + 4 );
					mesh.TriangleIndices.Add( baseVertexIndex + 5 );

					// 오른쪽면
					mesh.TriangleIndices.Add( baseVertexIndex + 1 );
					mesh.TriangleIndices.Add( baseVertexIndex + 6 );
					mesh.TriangleIndices.Add( baseVertexIndex + 2 );
					mesh.TriangleIndices.Add( baseVertexIndex + 1 );
					mesh.TriangleIndices.Add( baseVertexIndex + 5 );
					mesh.TriangleIndices.Add( baseVertexIndex + 6 );

					// 뒷면
					mesh.TriangleIndices.Add( baseVertexIndex + 2 );
					mesh.TriangleIndices.Add( baseVertexIndex + 7 );
					mesh.TriangleIndices.Add( baseVertexIndex + 3 );
					mesh.TriangleIndices.Add( baseVertexIndex + 2 );
					mesh.TriangleIndices.Add( baseVertexIndex + 6 );
					mesh.TriangleIndices.Add( baseVertexIndex + 7 );

					// 왼쪽면
					mesh.TriangleIndices.Add( baseVertexIndex + 3 );
					mesh.TriangleIndices.Add( baseVertexIndex + 4 );
					mesh.TriangleIndices.Add( baseVertexIndex + 0 );
					mesh.TriangleIndices.Add( baseVertexIndex + 3 );
					mesh.TriangleIndices.Add( baseVertexIndex + 7 );
					mesh.TriangleIndices.Add( baseVertexIndex + 4 );
				}
			}

			DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
			GeometryModel3D geometry = new GeometryModel3D(mesh, material);

			// 변환 그룹 (마우스 회전/이동용)
			transformGroup = new Transform3DGroup();
			xRotation = new AxisAngleRotation3D( new Vector3D( 1, 0, 0 ), 0 );
			yRotation = new AxisAngleRotation3D( new Vector3D( 0, 1, 0 ), 0 );
			rotateXTransform = new RotateTransform3D( xRotation );
			rotateYTransform = new RotateTransform3D( yRotation );
			translateTransform = new TranslateTransform3D( 0, 0, 0 );

			transformGroup.Children.Add( rotateXTransform );
			transformGroup.Children.Add( rotateYTransform );
			transformGroup.Children.Add( translateTransform );
			geometry.Transform = transformGroup;

			Model3DGroup modelGroup = new Model3DGroup();
			modelGroup.Children.Add( new AmbientLight( Colors.DarkGray ) );
			modelGroup.Children.Add( new DirectionalLight( Colors.White, new Vector3D( -1, -1, -1 ) ) );
			modelGroup.Children.Add( geometry );

			double maxDim = Math.Max(width, height);
			cameraDistance = maxDim * 2;
			camera = new PerspectiveCamera( new Point3D( width / 2.0, height / 2.0, -cameraDistance ),
										   new Vector3D( 0, 0, 1 ),
										   new Vector3D( 0, 1, 0 ),
										   60 );

			viewport = new Viewport3D();
			viewport.Camera = camera;

			ModelVisual3D modelVisual = new ModelVisual3D();
			modelVisual.Content = modelGroup;
			viewport.Children.Add( modelVisual );

			// 마우스 이벤트 핸들러 (이전 코드와 동일)
			viewport.MouseDown += Viewport_MouseDown;
			viewport.MouseUp += Viewport_MouseUp;
			viewport.MouseMove += Viewport_MouseMove;
			viewport.MouseWheel += Viewport_MouseWheel;

			MainGrid.Children.Clear();
			MainGrid.Children.Add( viewport );
		}

		private void Viewport_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
			lastMousePosition = e.GetPosition( viewport );

			// 마우스 위치 기록
			initialMouseDownPosition = lastMousePosition;

			if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
			{
				isLeftButtonDown = true;
				// 현재 회전각을 저장: 마우스 누른 시점의 회전 상태를 기준점으로 삼는다.
				initialXAngle = xRotation.Angle;
				initialYAngle = yRotation.Angle;
			}

			if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
				isRightButtonDown = true;

			viewport.CaptureMouse();
		}

		private void Viewport_MouseUp( object sender, MouseButtonEventArgs e )
		{
			if (e.LeftButton == MouseButtonState.Released)
				isLeftButtonDown = false;
			if (e.RightButton == MouseButtonState.Released)
				isRightButtonDown = false;

			if (!isLeftButtonDown && !isRightButtonDown)
				viewport.ReleaseMouseCapture();
		}

		private void Viewport_MouseMove( object sender, System.Windows.Input.MouseEventArgs e )
		{
			if (!isLeftButtonDown && !isRightButtonDown)
				return;

			System.Windows.Point currentPos = e.GetPosition(viewport);
			Vector delta = currentPos - initialMouseDownPosition;
			// 여기서 delta는 마우스를 처음 누른 위치 대비 현재 위치의 차이

			lastMousePosition = currentPos;

			if (isLeftButtonDown)
			{
				double rotationSpeed = 0.5;
				// 초기 각도에서 delta를 반영
				yRotation.Angle = initialYAngle + (delta.X * rotationSpeed);
				xRotation.Angle = initialXAngle + (delta.Y * rotationSpeed);
			}

			if (isRightButtonDown)
			{
				double moveSpeed = 0.05;
				translateTransform.OffsetX += (currentPos.X - lastMousePosition.X) * moveSpeed;
				translateTransform.OffsetY -= (currentPos.Y - lastMousePosition.Y) * moveSpeed;
			}
		}

		private void Viewport_MouseWheel( object sender, MouseWheelEventArgs e )
		{
			// 줌 인/아웃
			double zoomSpeed = 1;
			cameraDistance *= (1 - e.Delta * zoomSpeed / 1000.0);
			camera.Position = new Point3D( camera.Position.X, camera.Position.Y, -cameraDistance );
		}
	}
}
