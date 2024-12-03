using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DamoOneVision
{
	internal class SelectionAdorner : Adorner
	{
		private RectangleGeometry _rectangleGeometry;
		private Brush _brush;

		public Rect SelectionRectangle { get; set; }

		public SelectionAdorner( UIElement adornedElement ) : base( adornedElement )
		{
			_rectangleGeometry = new RectangleGeometry();
			_brush = new SolidColorBrush( Color.FromArgb( 40, 255, 0, 0 ) );
		}

		protected override void OnRender( DrawingContext drawingContext )
		{
			base.OnRender( drawingContext );

			_rectangleGeometry.Rect = SelectionRectangle;
			drawingContext.DrawGeometry( null, new Pen( Brushes.Red, 2 ), _rectangleGeometry );
			drawingContext.DrawGeometry( _brush, null, _rectangleGeometry );
		}
	}
}
