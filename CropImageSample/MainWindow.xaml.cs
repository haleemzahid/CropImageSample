using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CropImageSample
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    AdornerLayer adornerLayer;
    Matrix saveMatrix = new Matrix();
    Rect savedCropRect;
    public MainWindow()
    {
      InitializeComponent();
      this.Loaded += MainWindow_Loaded;
      savedCropRect = new Rect();
    }
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {

      var selectedElement = cropRect as UIElement;

      adornerLayer = AdornerLayer.GetAdornerLayer(selectedElement);
      adornerLayer.Add(new BorderAdorner(selectedElement));

      contentViewer.Reset(true);
      savedCropRect = new Rect(Canvas.GetLeft(cropRect), Canvas.GetTop(cropRect), cropRect.Width, cropRect.Height);
    }

    private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
      contentViewer.OnMouseDownMessage(sender, e);
      showSTATs();
    }

    private void mainGrid_MouseMove(object sender, MouseEventArgs e)
    {
      contentViewer.OnMouseMoveMessage(sender, e);
      showSTATs();
    }

    private void mainGrid_MouseUp(object sender, MouseButtonEventArgs e)
    {
      contentViewer.OnMouseUpMessage(sender, e);
      showSTATs();
    }

    private void mainGrid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      contentViewer.OnMouseWheelMessage(sender, e);
      showSTATs();
    }
    private void mainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (mainGrid.ActualHeight < cropRect.Height + Canvas.GetTop(cropRect))
      {
        if (mainGrid.ActualHeight < Canvas.GetTop(cropRect))
        {
          Canvas.SetTop(cropRect, 0);
        }
        else
        {
          cropRect.Height = mainGrid.ActualHeight - Canvas.GetTop(cropRect);
        }
      }
      if (mainGrid.ActualWidth < cropRect.Width + Canvas.GetLeft(cropRect))
      {
        if (mainGrid.ActualWidth < Canvas.GetLeft(cropRect))
        {
          Canvas.SetLeft(cropRect, 0);
        }
        else
        {
          cropRect.Width = mainGrid.ActualWidth - Canvas.GetLeft(cropRect);
        }
      }
      showSTATs();
    }

    private void DoneButton_Click(object sender, RoutedEventArgs e)
    {
      saveMatrix = contentViewer.ContentTransform.Matrix;
      savedCropRect = new Rect(Canvas.GetLeft(cropRect), Canvas.GetTop(cropRect), cropRect.Width, cropRect.Height);
      SaveDoneImage();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
      contentViewer.Reset(true);
      Canvas.SetLeft(cropRect, 0);
      Canvas.SetTop(cropRect, 0);
      cropRect.Width = mainGrid.ActualWidth;
      cropRect.Height = mainGrid.ActualHeight;
      showSTATs();
    }
    private void showSTATs()
    {
      cropRectLabel.Content = $"{Math.Round(Canvas.GetLeft(cropRect), 2)},{Math.Round(Canvas.GetTop(cropRect), 2)} - {Math.Round(cropRect.Width, 2)},{Math.Round(cropRect.Height, 2)}";
      zoomLabel.Content = $"{contentViewer.Zoom}";
    }

    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
      contentViewer.Restore(saveMatrix);
      Canvas.SetLeft(cropRect, savedCropRect.Left);
      Canvas.SetTop(cropRect, savedCropRect.Top);
      cropRect.Width = savedCropRect.Width;
      cropRect.Height = savedCropRect.Height;
    }
        public void WriteToPng(UIElement element, string filename,Rect data)
        {
            var rect = data;
            var visual = new DrawingVisual();

            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(element), null, rect);
            }

            var bitmap = new RenderTargetBitmap(
                (int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
           
            using (var file = File.OpenWrite(filename))
            {
                encoder.Save(file);
               
                imgViewer.Source = null;
                file.Close();


            }
            var d = Environment.CurrentDirectory;
            var dataa = new BitmapImage();


            dataa.BeginInit();
            dataa.CacheOption = BitmapCacheOption.OnLoad;
            dataa.UriSource = new Uri(d + "\\" + filename);
            dataa.EndInit();
            imgViewer.Source = dataa;
            
            savedCropRect = new Rect();
            RefreshButton_Click(null,null);
            MainWindow_Loaded(null,null);
            
           




          
           

        }


        private void SaveDoneImage()
    {
            /*
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                               (int)savedCropRect.Width, (int)savedCropRect.Height,
                                96d, 96d, PixelFormats.Pbgra32);
            // needed otherwise the image output is black
            contentViewer.Measure(new Size((int)contentViewer.ActualWidth, (int)contentViewer.ActualHeight));
            contentViewer.Arrange(new Rect(new Size((int)contentViewer.ActualWidth, (int)contentViewer.ActualHeight)));
            contentViewer.UpdateLayout();

            renderTarget.Render(contentViewer);
            */
            
       BitmapSource image = imgViewer.Source as BitmapSource;
      var scale = saveMatrix.M11;
      var (width, height) = (
        (int)Math.Round(image.PixelWidth * scale),
        (int)Math.Round(image.PixelHeight * scale));
      var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

      var left = savedCropRect.Left + saveMatrix.OffsetX;
      var top = savedCropRect.Top + saveMatrix.OffsetY;

      var size = new Rect(left, top, savedCropRect.Width - saveMatrix.OffsetX, savedCropRect.Height - saveMatrix.OffsetY);
            
      var pageView = contentViewer;
      WriteToPng(contentViewer, "testing.jpg", size);
      //      pageView.Measure(size.Size);
      //pageView.Arrange(size);
      //pageView.UpdateLayout();

      //renderTarget.Render(pageView);
      //renderTarget.Freeze();
           
      //      var imageEncoder = new JpegBitmapEncoder();

      //imageEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

      //using (FileStream file = File.Create(@"test.jpg"))
      //{
      //  imageEncoder.Save(file);

      //}
    }
  }
}
