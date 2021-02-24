using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CropImageSample
{
  public class BorderAdorner : Adorner
  {
    //use thumb for resizing elements
    Thumb topLeft, topRight, bottomLeft, bottomRight;
    //visual child collection for adorner
    VisualCollection visualChilderns;

    readonly UIElement _adornedElement;

    public BorderAdorner(UIElement element) : base(element)
    {
      _adornedElement = element;

      visualChilderns = new VisualCollection(this);

      //adding thumbs for drawing adorner rectangle and setting cursor
      BuildAdornerCorners(ref topLeft, Cursors.SizeNWSE);
      BuildAdornerCorners(ref topRight, Cursors.SizeNESW);
      BuildAdornerCorners(ref bottomLeft, Cursors.SizeNESW);
      BuildAdornerCorners(ref bottomRight, Cursors.SizeNWSE);

      //registering drag delta events for thumb drag movement
      topLeft.DragDelta += TopLeft_DragDelta;
      topRight.DragDelta += TopRight_DragDelta;
      bottomLeft.DragDelta += BottomLeft_DragDelta;
      bottomRight.DragDelta += BottomRight_DragDelta;
    }

    private void BottomRight_DragDelta(object sender, DragDeltaEventArgs e)
    {
      FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
      Thumb bottomRightCorner = sender as Thumb;
      //setting new height and width after drag
      if (adornedElement != null && bottomRightCorner != null)
      {
        EnforceSize(adornedElement);

        double oldWidth = adornedElement.Width;
        double oldHeight = adornedElement.Height;

        double newWidth = Math.Max(adornedElement.Width + e.HorizontalChange, bottomRightCorner.DesiredSize.Width);
        double newHeight = Math.Max(e.VerticalChange + adornedElement.Height, bottomRightCorner.DesiredSize.Height);

        adornedElement.Width = newWidth;
        adornedElement.Height = newHeight;
      }
    }

    private void TopRight_DragDelta(object sender, DragDeltaEventArgs e)
    {
      FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
      Thumb topRightCorner = sender as Thumb;
      //setting new height, width and canvas top after drag
      if (adornedElement != null && topRightCorner != null)
      {
        EnforceSize(adornedElement);

        double oldWidth = adornedElement.Width;
        double oldHeight = adornedElement.Height;

        double newWidth = Math.Max(adornedElement.Width + e.HorizontalChange, topRightCorner.DesiredSize.Width);
        double newHeight = Math.Max(adornedElement.Height - e.VerticalChange, topRightCorner.DesiredSize.Height);
        adornedElement.Width = newWidth;

        double oldTop = Canvas.GetTop(adornedElement);
        double newTop = oldTop - (newHeight - oldHeight);
        if (newTop < 0)
          return;
        adornedElement.Height = newHeight;
        Canvas.SetTop(adornedElement, newTop);
      }
    }

    private void TopLeft_DragDelta(object sender, DragDeltaEventArgs e)
    {
      FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
      Thumb topLeftCorner = sender as Thumb;
      //setting new height, width and canvas top, left after drag
      if (adornedElement != null && topLeftCorner != null)
      {
        EnforceSize(adornedElement);

        double oldWidth = adornedElement.Width;
        double oldHeight = adornedElement.Height;

        double newWidth = Math.Max(adornedElement.Width - e.HorizontalChange, topLeftCorner.DesiredSize.Width);
        double newHeight = Math.Max(adornedElement.Height - e.VerticalChange, topLeftCorner.DesiredSize.Height);

        double oldLeft = Canvas.GetLeft(adornedElement);
        double newLeft = oldLeft - (newWidth - oldWidth);
        double oldTop = Canvas.GetTop(adornedElement);
        double newTop = oldTop - (newHeight - oldHeight);
        if (newLeft < 0)
          return;
        if (newTop < 0)
          return;
        adornedElement.Width = newWidth;
        Canvas.SetLeft(adornedElement, newLeft);

        adornedElement.Height = newHeight;
        Canvas.SetTop(adornedElement, newTop);
      }
    }

    private void BottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
    {
      FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
      Thumb bottomLeftCorner = sender as Thumb;
      //setting new height, width and canvas left after drag
      if (adornedElement != null && bottomLeftCorner != null)
      {
        EnforceSize(adornedElement);

        double oldWidth = adornedElement.Width;

        double newWidth = Math.Max(adornedElement.Width - e.HorizontalChange, bottomLeftCorner.DesiredSize.Width);
        double newHeight = Math.Max(adornedElement.Height + e.VerticalChange, bottomLeftCorner.DesiredSize.Height);

        double oldLeft = Canvas.GetLeft(adornedElement);
        double newLeft = oldLeft - (newWidth - oldWidth);
        if (newLeft < 0)
          return;
        adornedElement.Width = newWidth;
        Canvas.SetLeft(adornedElement, newLeft);

        adornedElement.Height = newHeight;
      }
    }

    public void BuildAdornerCorners(ref Thumb cornerThumb, Cursor customizedCursors)
    {
      //adding new thumbs for adorner to visual childern collection
      if (cornerThumb != null) return;
      cornerThumb = new Thumb() { Cursor = customizedCursors, Height = 20, Width = 20, Opacity = 0.5, Background = new SolidColorBrush(Colors.Red) };
      visualChilderns.Add(cornerThumb);
    }

    public void EnforceSize(FrameworkElement element)
    {
      if (element.Width.Equals(Double.NaN))
        element.Width = element.DesiredSize.Width;
      if (element.Height.Equals(Double.NaN))
        element.Height = element.DesiredSize.Height;

      //enforce size of element not exceeding to it's parent element size
      FrameworkElement parent = element.Parent as FrameworkElement;

      if (parent != null)
      {
        element.MaxHeight = parent.ActualHeight;
        element.MaxWidth = parent.ActualWidth;
      }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      base.ArrangeOverride(finalSize);

      double desireWidth = AdornedElement.DesiredSize.Width;
      double desireHeight = AdornedElement.DesiredSize.Height;

      double adornerWidth = this.DesiredSize.Width;
      double adornerHeight = this.DesiredSize.Height;

      //arranging thumbs
      topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
      topRight.Arrange(new Rect(desireWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
      bottomLeft.Arrange(new Rect(-adornerWidth / 2, desireHeight - adornerHeight / 2, adornerWidth, adornerHeight));
      bottomRight.Arrange(new Rect(desireWidth - adornerWidth / 2, desireHeight - adornerHeight / 2, adornerWidth, adornerHeight));

      return finalSize;
    }
    protected override int VisualChildrenCount { get { return visualChilderns.Count; } }
    protected override Visual GetVisualChild(int index) { return visualChilderns[index]; }
    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);
    }
  }

}
