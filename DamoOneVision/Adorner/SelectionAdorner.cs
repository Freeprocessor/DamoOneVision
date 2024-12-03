using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DamoOneVision
{
	internal class SelectionAdorner : Adorner
	{
		private VisualCollection _visuals;
		private RectangleGeometry _rectangleGeometry;
		private Brush _brush;
		private Thumb topLeft, topRight, bottomLeft, bottomRight;
		private Thumb left, top, right, bottom;

		private Thumb moveThumb;


		public Rect SelectionRectangle { get; set; }

		public SelectionAdorner( UIElement adornedElement ) : base( adornedElement )
		{
			_visuals = new VisualCollection( this );
			_rectangleGeometry = new RectangleGeometry();
			_brush = new SolidColorBrush( Color.FromArgb( 40, 0, 255, 0 ) );

			// moveThumb 초기화
			moveThumb = new Thumb
			{
				Cursor = Cursors.SizeAll,
				Opacity = 0,
				Background = Brushes.Transparent
			};
			_visuals.Add( moveThumb );

			// 코너와 변을 위한 Thumb 생성
			BuildAdornerCorner( ref topLeft, Cursors.SizeNWSE );
			BuildAdornerCorner( ref topRight, Cursors.SizeNESW );
			BuildAdornerCorner( ref bottomLeft, Cursors.SizeNESW );
			BuildAdornerCorner( ref bottomRight, Cursors.SizeNWSE );

			BuildAdornerCorner( ref left, Cursors.SizeWE );
			BuildAdornerCorner( ref top, Cursors.SizeNS );
			BuildAdornerCorner( ref right, Cursors.SizeWE );
			BuildAdornerCorner( ref bottom, Cursors.SizeNS );

			// 이벤트 핸들러 등록
			topLeft.DragDelta += HandleTopLeft;
			topRight.DragDelta += HandleTopRight;
			bottomLeft.DragDelta += HandleBottomLeft;
			bottomRight.DragDelta += HandleBottomRight;

			left.DragDelta += HandleLeft;
			top.DragDelta += HandleTop;
			right.DragDelta += HandleRight;
			bottom.DragDelta += HandleBottom;

			// moveThumb의 이벤트 핸들러 등록
			moveThumb.DragDelta += MoveThumb_DragDelta;
		}

		private void MoveThumb_DragDelta( object sender, DragDeltaEventArgs e )
		{
			double deltaX = e.HorizontalChange;
			double deltaY = e.VerticalChange;

			double newX = SelectionRectangle.X + deltaX;
			double newY = SelectionRectangle.Y + deltaY;

			// 선택 영역이 이미지 경계를 벗어나지 않도록 제한
			newX = Math.Max( 0, Math.Min( newX, AdornedElement.RenderSize.Width - SelectionRectangle.Width ) );
			newY = Math.Max( 0, Math.Min( newY, AdornedElement.RenderSize.Height - SelectionRectangle.Height ) );

			SelectionRectangle = new Rect( newX, newY, SelectionRectangle.Width, SelectionRectangle.Height );

			InvalidateArrange();
			InvalidateVisual();
		}


		private void BuildAdornerCorner( ref Thumb cornerThumb, Cursor customizedCursor )
		{
			if (cornerThumb != null)
				return;

			cornerThumb = new Thumb
			{
				Cursor = customizedCursor,
				Width = 10,
				Height = 10,
				Opacity = 0.5,
				Background = Brushes.White,
				BorderBrush = Brushes.Black,
				BorderThickness = new Thickness( 1 )
			};
			_visuals.Add( cornerThumb );
		}

		protected override Size ArrangeOverride( Size finalSize )
		{
			base.ArrangeOverride( finalSize );

			double leftPos = SelectionRectangle.Left;
			double topPos = SelectionRectangle.Top;
			double rightPos = SelectionRectangle.Right;
			double bottomPos = SelectionRectangle.Bottom;
			double width = SelectionRectangle.Width;
			double height = SelectionRectangle.Height;

			// moveThumb를 선택 영역 전체에 배치
			moveThumb.Arrange( new Rect( leftPos, topPos, width, height ) );

			// 코너 Thumb 배치
			topLeft.Arrange( new Rect( leftPos - 5, topPos - 5, 10, 10 ) );
			topRight.Arrange( new Rect( rightPos - 5, topPos - 5, 10, 10 ) );
			bottomLeft.Arrange( new Rect( leftPos - 5, bottomPos - 5, 10, 10 ) );
			bottomRight.Arrange( new Rect( rightPos - 5, bottomPos - 5, 10, 10 ) );

			// 변 Thumb 배치
			left.Arrange( new Rect( leftPos - 5, topPos + height / 2 - 5, 10, 10 ) );
			top.Arrange( new Rect( leftPos + width / 2 - 5, topPos - 5, 10, 10 ) );
			right.Arrange( new Rect( rightPos - 5, topPos + height / 2 - 5, 10, 10 ) );
			bottom.Arrange( new Rect( leftPos + width / 2 - 5, bottomPos - 5, 10, 10 ) );

			return finalSize;
		}

		protected override int VisualChildrenCount => _visuals.Count;

		protected override Visual GetVisualChild( int index )
		{
			return _visuals[ index ];
		}

		protected override void OnRender( DrawingContext drawingContext )
		{
			base.OnRender( drawingContext );

			_rectangleGeometry.Rect = SelectionRectangle;
			drawingContext.DrawGeometry( null, new Pen( Brushes.Green, 1 ), _rectangleGeometry );
			drawingContext.DrawGeometry( _brush, null, _rectangleGeometry );
		}

		// 선택 영역 이동 메서드
		public void Move( double deltaX, double deltaY )
		{
			SelectionRectangle = new Rect(
				SelectionRectangle.X + deltaX,
				SelectionRectangle.Y + deltaY,
				SelectionRectangle.Width,
				SelectionRectangle.Height );

			InvalidateArrange();
			InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
		}

		public void Resize( double deltaWidth, double deltaHeight )
		{
			double minWidth = 10;
			double minHeight = 10;

			double newWidth = SelectionRectangle.Width + deltaWidth;
			double newHeight = SelectionRectangle.Height + deltaHeight;

			// 너비 조정
			if (newWidth >= minWidth)
			{
				// 너비가 최소 크기 이상인 경우 업데이트
				SelectionRectangle = new Rect(
					SelectionRectangle.X,
					SelectionRectangle.Y,
					newWidth,
					SelectionRectangle.Height );
			}
			else
			{
				// 최소 크기 이하로 줄어들지 않도록 제한
				newWidth = minWidth;
				SelectionRectangle = new Rect(
					SelectionRectangle.X,
					SelectionRectangle.Y,
					newWidth,
					SelectionRectangle.Height );
			}

			// 높이 조정
			if (newHeight >= minHeight)
			{
				// 높이가 최소 크기 이상인 경우 업데이트
				SelectionRectangle = new Rect(
					SelectionRectangle.X,
					SelectionRectangle.Y,
					SelectionRectangle.Width,
					newHeight );
			}
			else
			{
				// 최소 크기 이하로 줄어들지 않도록 제한
				newHeight = minHeight;
				SelectionRectangle = new Rect(
					SelectionRectangle.X,
					SelectionRectangle.Y,
					SelectionRectangle.Width,
					newHeight );
			}

			InvalidateArrange();
			InvalidateVisual();
		}


		// 핸들러 메서드들
		private void HandleTopLeft( object sender, DragDeltaEventArgs e )
		{
			double newLeft = SelectionRectangle.Left + e.HorizontalChange;
			double newTop = SelectionRectangle.Top + e.VerticalChange;
			double newWidth = SelectionRectangle.Width - e.HorizontalChange;
			double newHeight = SelectionRectangle.Height - e.VerticalChange;

			if (newWidth > 10 && newHeight > 10)
			{
				SelectionRectangle = new Rect( newLeft, newTop, newWidth, newHeight );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}

		private void HandleTopRight( object sender, DragDeltaEventArgs e )
		{
			double newTop = SelectionRectangle.Top + e.VerticalChange;
			double newWidth = SelectionRectangle.Width + e.HorizontalChange;
			double newHeight = SelectionRectangle.Height - e.VerticalChange;

			if (newWidth > 10 && newHeight > 10)
			{
				SelectionRectangle = new Rect( SelectionRectangle.Left, newTop, newWidth, newHeight );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}

		private void HandleBottomLeft( object sender, DragDeltaEventArgs e )
		{
			double newLeft = SelectionRectangle.Left + e.HorizontalChange;
			double newWidth = SelectionRectangle.Width - e.HorizontalChange;
			double newHeight = SelectionRectangle.Height + e.VerticalChange;

			if (newWidth > 10 && newHeight > 10)
			{
				SelectionRectangle = new Rect( newLeft, SelectionRectangle.Top, newWidth, newHeight );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}

		private void HandleBottomRight( object sender, DragDeltaEventArgs e )
		{
			double newWidth = SelectionRectangle.Width + e.HorizontalChange;
			double newHeight = SelectionRectangle.Height + e.VerticalChange;

			if (newWidth > 10 && newHeight > 10)
			{
				SelectionRectangle = new Rect( SelectionRectangle.Left, SelectionRectangle.Top, newWidth, newHeight );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}

		private void HandleLeft( object sender, DragDeltaEventArgs e )
		{
			double newLeft = SelectionRectangle.Left + e.HorizontalChange;
			double newWidth = SelectionRectangle.Width - e.HorizontalChange;

			if (newWidth > 10)
			{
				SelectionRectangle = new Rect( newLeft, SelectionRectangle.Top, newWidth, SelectionRectangle.Height );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}

		private void HandleTop( object sender, DragDeltaEventArgs e )
		{
			double newTop = SelectionRectangle.Top + e.VerticalChange;
			double newHeight = SelectionRectangle.Height - e.VerticalChange;

			if (newHeight > 10)
			{
				SelectionRectangle = new Rect( SelectionRectangle.Left, newTop, SelectionRectangle.Width, newHeight );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}

		private void HandleRight( object sender, DragDeltaEventArgs e )
		{
			double newWidth = SelectionRectangle.Width + e.HorizontalChange;

			if (newWidth > 10)
			{
				SelectionRectangle = new Rect( SelectionRectangle.Left, SelectionRectangle.Top, newWidth, SelectionRectangle.Height );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}

		private void HandleBottom( object sender, DragDeltaEventArgs e )
		{
			double newHeight = SelectionRectangle.Height + e.VerticalChange;

			if (newHeight > 10)
			{
				SelectionRectangle = new Rect( SelectionRectangle.Left, SelectionRectangle.Top, SelectionRectangle.Width, newHeight );
				InvalidateArrange();
				InvalidateVisual(); // 추가: 시각적 업데이트를 위해 호출
			}
		}
	}
}
