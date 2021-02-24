using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Math;

namespace CropImageSample
{
  [TemplatePart(Name = ContentPresenter, Type = typeof(ContentViewer))]
  public class ContentViewer : ContentControl
  {
    const string ContentPresenter = "PART_ContentPresenter";
    ContentPresenter _presenter;
    Matrix _oldFitInverse = Matrix.Identity;
    Point _offset;
    bool _isFit;
    bool _isMouseDragging;
    bool _internalOffsetUpdate;

    static ContentViewer()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentViewer), new FrameworkPropertyMetadata(typeof(ContentViewer)));
    }

    public bool IsSettingMode
    {
      get => (bool)GetValue(IsSettingModeProperty);
      set => SetValue(IsSettingModeProperty, value);
    }

    public static readonly DependencyProperty IsSettingModeProperty =
        DependencyProperty.Register(nameof(IsSettingMode), typeof(bool), typeof(ContentViewer), new PropertyMetadata(false));

    public double ZoomSensivity
    {
      get => (double)GetValue(ZoomSensivityProperty);
      set => SetValue(ZoomSensivityProperty, value);
    }

    public static readonly DependencyProperty ZoomSensivityProperty =
        DependencyProperty.Register(nameof(ZoomSensivity), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0009));

    public double ViewportWidth
    {
      get => (double)GetValue(ViewportWidthProperty);
      set => SetValue(ViewportWidthProperty, value);
    }

    public static readonly DependencyProperty ViewportWidthProperty =
        DependencyProperty.Register(nameof(ViewportWidth), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0));

    public double ViewportHeight
    {
      get => (double)GetValue(ViewportHeightProperty);
      set => SetValue(ViewportHeightProperty, value);
    }

    public static readonly DependencyProperty ViewportHeightProperty =
        DependencyProperty.Register(nameof(ViewportHeight), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0));

    public double HorizontalOffset
    {
      get => (double)GetValue(HorizontalOffsetProperty);
      set => SetValue(HorizontalOffsetProperty, value);
    }

    public static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.Register(nameof(HorizontalOffset), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

    private static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) =>
      ((ContentViewer)o).Offset((double)e.OldValue - (double)e.NewValue, 0);

    public double VerticalOffset
    {
      get => (double)GetValue(VerticalOffsetProperty);
      set => SetValue(VerticalOffsetProperty, value);
    }

    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.Register(nameof(VerticalOffset), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

    private static void OnVerticalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) =>
      ((ContentViewer)o).Offset(0, (double)e.OldValue - (double)e.NewValue);

    public Visibility ComputedHorizontalScrollBarVisibility
    {
      get => (Visibility)GetValue(ComputedHorizontalScrollBarVisibilityProperty);
      set => SetValue(ComputedHorizontalScrollBarVisibilityProperty, value);
    }

    public static readonly DependencyProperty ComputedHorizontalScrollBarVisibilityProperty =
        DependencyProperty.Register(nameof(ComputedHorizontalScrollBarVisibility), typeof(Visibility), typeof(ContentViewer), new PropertyMetadata(Visibility.Visible));

    public Visibility ComputedVerticalScrollBarVisibility
    {
      get => (Visibility)GetValue(ComputedVerticalScrollBarVisibilityProperty);
      set => SetValue(ComputedVerticalScrollBarVisibilityProperty, value);
    }

    public static readonly DependencyProperty ComputedVerticalScrollBarVisibilityProperty =
        DependencyProperty.Register(nameof(ComputedVerticalScrollBarVisibility), typeof(Visibility), typeof(ContentViewer), new PropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Horizontal <seealso cref="ScrollBar"/> Minimum.
    /// </summary>
    public double HorizontalMinimum
    {
      get => (double)GetValue(HorizontalMinimumProperty);
      set => SetValue(HorizontalMinimumProperty, value);
    }

    public static readonly DependencyProperty HorizontalMinimumProperty =
        DependencyProperty.Register(nameof(HorizontalMinimum), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0));

    /// <summary>
    /// Vertical <seealso cref="ScrollBar"/> Minimum.
    /// </summary>
    public double VerticalMinimum
    {
      get => (double)GetValue(VerticalMinimumProperty);
      set => SetValue(VerticalMinimumProperty, value);
    }

    public static readonly DependencyProperty VerticalMinimumProperty =
        DependencyProperty.Register(nameof(VerticalMinimum), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0));

    /// <summary>
    /// Horizontal <seealso cref="ScrollBar"/> Maximum.
    /// </summary>
    public double HorizontalMaximum
    {
      get => (double)GetValue(HorizontalMaximumProperty);
      set => SetValue(HorizontalMaximumProperty, value);
    }

    public static readonly DependencyProperty HorizontalMaximumProperty =
        DependencyProperty.Register(nameof(HorizontalMaximum), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0));

    /// <summary>
    /// Vertical <seealso cref="ScrollBar"/> Maximum.
    /// </summary>
    public double VerticalMaximum
    {
      get => (double)GetValue(VerticalMaximumProperty);
      set => SetValue(VerticalMaximumProperty, value);
    }

    public static readonly DependencyProperty VerticalMaximumProperty =
        DependencyProperty.Register(nameof(VerticalMaximum), typeof(double), typeof(ContentViewer), new PropertyMetadata(0.0));

    public static Point GetOrigin(DependencyObject obj) =>
      (Point)obj.GetValue(OriginProperty);

    public static void SetOrigin(DependencyObject obj, Point value) =>
      obj.SetValue(OriginProperty, value);

    public static readonly DependencyProperty OriginProperty =
        DependencyProperty.RegisterAttached("Origin", typeof(Point), typeof(ContentViewer), new PropertyMetadata(new Point()));

    public double Zoom
    {
      get => (double)GetValue(ZoomProperty);
      private set => SetValue(ZoomProperty, value);
    }

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(ContentViewer), new PropertyMetadata(1.0));

    public MatrixTransform ContentTransform
    {
      get => (MatrixTransform)GetValue(ContentTransformProperty);
      set => SetValue(ContentTransformProperty, value);
    }

    public static readonly DependencyProperty ContentTransformProperty =
        DependencyProperty.Register(nameof(ContentTransform), typeof(MatrixTransform), typeof(ContentViewer), new PropertyMetadata(MatrixTransform.Identity));

    public int MinTouchPointsForRotation
    {
      get => (int)GetValue(MinTouchPointsForRotationProperty);
      set => SetValue(MinTouchPointsForRotationProperty, value);
    }

    public static readonly DependencyProperty MinTouchPointsForRotationProperty =
        DependencyProperty.Register(nameof(MinTouchPointsForRotation), typeof(int), typeof(ContentViewer), new PropertyMetadata(3));

    // handlers

    protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
    {
      if (e.Source != this)
        return;
      e.ManipulationContainer = this;
      e.Handled = true;
    }

    protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
    {
      if (e.Source != this)
        return;
      double rotation =
        (e.Manipulators.Count() < MinTouchPointsForRotation) ? 0.0 : e.DeltaManipulation.Rotation;
      Transform(
        e.DeltaManipulation.Translation,
        e.DeltaManipulation.Scale.X,
        rotation,
        e.ManipulationOrigin);
      e.Handled = true;
    }
    /*
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      base.OnMouseDown(e);
      _offset = e.GetPosition(this);
      if (CaptureMouse())
        _isMouseDragging = true;
      // e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      if (_isMouseDragging)
      {
        Point position = e.GetPosition(this);
        Translate(position - _offset);
        _offset = position;
        e.Handled = true;
      }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
      base.OnMouseUp(e);
      if (_isMouseDragging)
      {
        _isMouseDragging = false;
        ReleaseMouseCapture();
      }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      base.OnMouseWheel(e);
      double scale = 1 + e.Delta * ZoomSensivity;
      Point position = e.GetPosition(this);
      Scale(scale, position.X, position.Y);
    }
    */
    public void OnMouseDownMessage(object sender, MouseButtonEventArgs e)
    {
      base.OnMouseDown(e);
      _offset = e.GetPosition(this);
      if (CaptureMouse())
        _isMouseDragging = true;
      // e.Handled = true;
    }

    public void OnMouseMoveMessage(object sender, MouseEventArgs e)
    {
      base.OnMouseMove(e);
      if (_isMouseDragging)
      {
        Point position = e.GetPosition(this);
        Translate(position - _offset);
        _offset = position;
        e.Handled = true;
      }
    }

    public void OnMouseUpMessage(object sender, MouseButtonEventArgs e)
    {
      base.OnMouseUp(e);
      if (_isMouseDragging)
      {
        _isMouseDragging = false;
        ReleaseMouseCapture();
      }
    }

    public void OnMouseWheelMessage(object sender, MouseWheelEventArgs e)
    {
      base.OnMouseWheel(e);
      double scale = 1 + e.Delta * ZoomSensivity;
      Point position = e.GetPosition(this);
      Scale(scale, position.X, position.Y);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      if (_presenter != null)
      {
        _presenter.RenderTransform = System.Windows.Media.Transform.Identity;
        _presenter.SizeChanged -= OnContentSizeChanged;
      }
      _presenter = GetTemplateChild(ContentPresenter) as ContentPresenter;
      if (_presenter != null)
      {
        var transform = new MatrixTransform();
        ContentTransform = transform;
        _presenter.RenderTransform = transform;
        _presenter.SizeChanged += OnContentSizeChanged;
      }
    }

    private void OnContentSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (_isFit)
      {
        UpdateViewport();
        return;
      }

      var size = new Size(ActualWidth, ActualHeight);
      // add size condition
      if (ActualWidth == 0 || ActualHeight == 0)
        return;
      var contentSize = e.NewSize;
      var (zoom, fit) = FitToContainer(contentSize, size);
      var matrix = ContentTransform.Matrix;
      matrix.Prepend(_oldFitInverse);
      matrix.Prepend(fit);
      ContentTransform.Matrix = matrix;
      Zoom = zoom;
      fit.Invert();
      _oldFitInverse = fit;
      UpdateViewport();
      if (contentSize != new Size(0, 0))
        _isFit = true;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      //UpdateViewport();
      // reset when screen size changed
      var contentSize = new Size(_presenter.ActualWidth, _presenter.ActualHeight);

      // return if contentSize is (0, 0)
      //if (contentSize == new Size(0, 0))
      //    return ;

      Console.WriteLine(string.Format("RenderSizeChanged: Width {0}, Height {1}\n", _presenter.ActualWidth, _presenter.ActualHeight));
      Reset();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
      //base.OnContentChanged(oldContent, newContent);
      //Reset();
    }

    private void Offset(double x, double y)
    {
      if (_internalOffsetUpdate)
        return;
      Matrix matrix = ContentTransform.Matrix;
      matrix.Translate(x, y);
      ContentTransform.Matrix = matrix;
    }

    private void Translate(Vector offset) => Translate(offset.X, offset.Y);

    private void Translate(double x, double y)
    {
      Matrix matrix = ContentTransform.Matrix;
      matrix.Translate(x, y);

      // check if transform is available or not
      if (!Is_Transform(matrix, 1, x, y))
        return;
      ContentTransform.Matrix = matrix;
      UpdateViewport(matrix);
    }

    private void Scale(double scale, double centerX, double centerY)
    {
      Matrix matrix = ContentTransform.Matrix;
      matrix.ScaleAt(scale, scale, centerX, centerY);

      // check if scale is available or not
      if (!Is_Transform(matrix, scale, 0, 0))
        return;

      Zoom *= scale;
      ContentTransform.Matrix = matrix;
      UpdateViewport(matrix);

      // fix gap when zoom out
      if (scale < 1)
      {
        Fix_Gap();
      }
    }

    private void Transform(Vector translate, double scale, double rotation, Point center)
    {
      Zoom *= scale;
      Matrix matrix = ContentTransform.Matrix;
      // matrix.RotateAt(rotation, center.X, center.Y);
      matrix.ScaleAt(scale, scale, center.X, center.Y);
      matrix.Translate(translate.X, translate.Y);
      ContentTransform.Matrix = matrix;
      UpdateViewport(matrix);
    }

    private void UpdateViewport() => UpdateViewport(ContentTransform.Matrix);

    private void UpdateViewport(Matrix matrix)
    {
      if (_presenter == null)
        return;

      var size = new Size(ActualWidth, ActualHeight);
      var contentSize = new Size(_presenter.ActualWidth, _presenter.ActualHeight);
      var extent = GetExtent(GetContentOrigin(_presenter), contentSize, size, matrix);
      ((HorizontalMinimum, HorizontalMaximum),
       (VerticalMinimum, VerticalMaximum)) = GetMinMax(size, extent);

      ViewportWidth = ActualWidth * ActualWidth / extent.Width;
      ViewportHeight = ActualHeight * ActualHeight / extent.Height;

      UpdateScollBarsOffsets();
      UpdateScrollBarsVisibilities();

      // Get gap

      Console.WriteLine($"UpdateViewport : ");
      Console.WriteLine($"               : Zoom {Zoom}");
      Console.WriteLine($"               : viewport  {ViewportWidth},{ViewportHeight}");
      Console.WriteLine($"               : offset    {ContentTransform.Value.OffsetX},{ContentTransform.Value.OffsetY}");
      Console.WriteLine($"               : extent    {extent.Width},{extent.Height}");
      Console.WriteLine($"               : actual    {ActualWidth},{ActualHeight}");
      Console.WriteLine($"               : content   {contentSize.Width},{contentSize.Height}");
    }

    private static Point GetContentOrigin(ContentPresenter presenter)
    {
      if (VisualTreeHelper.GetChildrenCount(presenter) == 0)
        return new Point();
      return GetOrigin(VisualTreeHelper.GetChild(presenter, 0));
    }

    private void UpdateScollBarsOffsets()
    {
      _internalOffsetUpdate = true;
      HorizontalOffset = 0;
      VerticalOffset = 0;
      _internalOffsetUpdate = false;
    }

    private void UpdateScrollBarsVisibilities()
    {
      ComputedHorizontalScrollBarVisibility = HorizontalMaximum - HorizontalMinimum == 0.0 ?
        Visibility.Hidden : Visibility.Visible;
      ComputedVerticalScrollBarVisibility = VerticalMaximum - VerticalMinimum == 0.0 ?
        Visibility.Hidden : Visibility.Visible;
    }

    /// <summary>
    /// Computes minimum and maximum values for scroll bars.
    /// </summary>
    public static ((double, double) horizontal, (double, double) vertical)
      GetMinMax(Size size, Rect extent) => (
        (Min(extent.Left, 0), Max(extent.Right - size.Width, 0)),
        (Min(extent.Top, 0), Max(extent.Bottom - size.Height, 0))
      );

    /// <summary>
    /// Gets full extent (content + white space) with <paramref name="matrix"/> transform applied.
    /// </summary>
    public static Rect GetExtent(Point origin, Size contentSize, Size size, Matrix matrix)
    {
      var rect = new Rect(origin, contentSize);
      rect.Transform(matrix);
      return new Rect(
        new Point(Min(rect.Left, 0), Min(rect.Top, 0)),
        new Point(Max(rect.Right, size.Width), Max(rect.Bottom, size.Height)));
    }

    public void Reset(bool byForce = false)
    {
      if (IsSettingMode && !byForce)
        return;
      if (_presenter == null)
        return;
      Console.WriteLine(string.Format("Reset:screen width: {2}, screen height: {3}, Width {0}, Height {1}\n", _presenter.ActualWidth, _presenter.ActualHeight, ActualWidth, ActualHeight));
      if (_presenter.ActualWidth == 0 || _presenter.ActualHeight == 0)
        return;
      var size = new Size(ActualWidth, ActualHeight);
      var contentSize = new Size(_presenter.ActualWidth, _presenter.ActualHeight);
      var (zoom, fit) = FitToContainer(contentSize, size);
      ContentTransform.Matrix = fit;
      Zoom = zoom;
      UpdateViewport(fit);
    }
    public void Restore(Matrix matrix)
    {
      ContentTransform.Matrix = matrix;
      Zoom = matrix.M11;
      UpdateViewport(matrix);
    }

    /// <summary>
    /// Provides transform which fits content to its container.
    /// </summary>
    /// <param name="contentSize">Content size.</param>
    /// <param name="size">Container size.</param>
    public static (double, Matrix) FitToContainer(Size contentSize, Size size)
    {
      if (contentSize.Width == 0.0 || contentSize.Height == 0)
        return (1.0, Matrix.Identity);
      double scale = Min(size.Width / contentSize.Width, size.Height / contentSize.Height);
      var (scaledWidth, scaledHeight) = (scale * contentSize.Width, scale * contentSize.Height);
      double x = (size.Width - scaledWidth) / 2;
      double y = (size.Height - scaledHeight) / 2;
      return (scale, new Matrix(scale, 0, 0, scale, x, y));
    }

    /// <summary>
    /// Check if transform has gaps or not.
    /// </summary>
    /// <param name="matrix">Transform Matrix.</param>
    /// <param name="scale">Transform scale.</param>
    /// <param name="x">Transform x.</param>
    /// <param name="y">Transform x.</param>
    public bool Is_Transform(Matrix matrix, double scale = 1, double x = 0, double y = 0)
    {
      if (_presenter == null)
        return true;
      // Get pre result matrix
      var size = new Size(ActualWidth, ActualHeight);
      var contentSize = new Size(_presenter.ActualWidth, _presenter.ActualHeight);
      var rect = new Rect(GetContentOrigin(_presenter), contentSize);
      rect.Transform(matrix);

      // return if contentSize is (0, 0)
      if (contentSize == new Size(0, 0))
        return false;
      // scale logic
      if (scale > 1)
        return true;

      if (scale < 1)
      {
        if ((rect.Width < ActualWidth) && (rect.Height < ActualHeight))
        {
          Reset();
          return false;
        }
        else return true;
      }
      // end scale logic

      // transform logic
      if ((rect.Left > 0 || rect.Right < size.Width) || (rect.Top > 0 || rect.Bottom < size.Height))
      {
        // If x axis transform is available or not
        if (x > 0 && rect.Left > 0 || x < 0 && rect.Right < size.Width)
          x = 0;

        // If y axis transform is available or not
        if (y > 0 && rect.Top > 0 || y < 0 && rect.Bottom < size.Height)
          y = 0;
        Matrix org_matrix = ContentTransform.Matrix;
        org_matrix.Translate(x, y);
        ContentTransform.Matrix = org_matrix;
        UpdateViewport(org_matrix);
        return false;
      }
      return true;
    }

    public void Fix_Gap()
    {
      var size = new Size(ActualWidth, ActualHeight);
      var contentSize = new Size(_presenter.ActualWidth, _presenter.ActualHeight);
      Matrix matrix = ContentTransform.Matrix;

      // get content visible Rect
      var rect = new Rect(GetContentOrigin(_presenter), contentSize);
      rect.Transform(matrix);
      double x = 0;
      double y = 0;
      // calculate x, y as center
      // calculate x axis, y axis gap
      if (rect.Width > ActualWidth)
      {
        if (rect.Left > 0)
          x = -rect.Left;
        if (rect.Right < ActualWidth)
          x = ActualWidth - rect.Right;
      }
      else
      {
        x = ActualWidth / 2 - (rect.Left + rect.Width) / 2;
      }
      if (rect.Height > ActualHeight)
      {
        if (rect.Top > 0)
          y = -rect.Top;
        if (rect.Bottom < ActualHeight)
          y = ActualHeight - rect.Bottom;
      }
      else
      {
        y = ActualHeight / 2 - (rect.Top + rect.Height) / 2;
      }
      matrix.Translate(x, y);

      ContentTransform.Matrix = matrix;
      UpdateViewport(matrix);
    }

  }
}
