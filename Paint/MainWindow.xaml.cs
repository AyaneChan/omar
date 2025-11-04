using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        SolidColorBrush GetBrush()
        {
            if (cmbFigura.Text == "Borrador") return new SolidColorBrush(Colors.White);
            byte r = (byte)Red.Value;
            byte g = (byte)Green.Value;
            byte b = (byte)Blue.Value;
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Lienzo.Children.Clear();
        }
        bool bandera = false;
        Point puntoInicial;
        Shape? figura;
        private void Lienzo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bandera = false;
            figura = null;
        }

        private void Lienzo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bandera = true;
            var seleccion = cmbFigura.Text;
            puntoInicial = e.GetPosition(Lienzo);

            if (seleccion == "Linea")
            {
                figura = new Line
                {
                    X1 = puntoInicial.X,
                    Y1 = puntoInicial.Y,
                    X2 = puntoInicial.X,
                    Y2 = puntoInicial.Y,
                };
            }
            else if (seleccion == "Borrador")
            {
                figura = new Line
                {
                    X1 = puntoInicial.X,
                    Y1 = puntoInicial.Y,
                    X2 = puntoInicial.X,
                    Y2 = puntoInicial.Y,
                };
                rectanguloColor.Fill = GetBrush();
            }
            else if (seleccion == "Ellipse")
            {
                figura = new Ellipse
                {
                    Width = puntoInicial.X,
                    Height = puntoInicial.Y
                };
                Canvas.SetLeft(figura, puntoInicial.X);
                Canvas.SetTop(figura, puntoInicial.Y);
            }
            else if (seleccion == "Rectangulo")
            {
                figura = new Rectangle
                {
                    Width = puntoInicial.X,
                    Height = puntoInicial.Y
                };

                Canvas.SetLeft(figura, puntoInicial.X);
                Canvas.SetTop(figura, puntoInicial.Y);
            }
            else if (seleccion == "Curva")
            {
                puntosBezier.Add(puntoInicial);
                if (puntosBezier.Count == 4)
                {
                    DibujarCurvaBezier();
                    puntosBezier.Clear();
                }
                return;
            }
            figura.Stroke = GetBrush();
            figura.StrokeThickness = sldGrosor.Value;
            Lienzo.Children.Add(figura);

        }

        private void Lienzo_MouseMove(object sender, MouseEventArgs e)
        {
            if (!bandera || figura == null) return;
            Point newPoint = e.GetPosition(Lienzo);
            if (cmbFigura.Text == "Linea")
            {
                Line l = (Line)figura;
                l.X2 = newPoint.X;
                l.Y2 = newPoint.Y;
            }
            else if (cmbFigura.Text == "Borrador")
            {
                figura = new Line
                {
                    X1 = puntoInicial.X,
                    Y1 = puntoInicial.Y,
                    X2 = newPoint.X,
                    Y2 = newPoint.Y,
                    StrokeThickness = sldGrosor.Value,
                    Stroke = new SolidColorBrush(Colors.White)
                };

                Lienzo.Children.Add(figura);
                puntoInicial = newPoint;
            }
            else if (cmbFigura.Text == "Rectangulo")
            {
                Rectangle r = (Rectangle)figura;
                r.Width = Math.Abs(puntoInicial.X - newPoint.X);
                r.Height = Math.Abs(puntoInicial.Y - newPoint.Y);
                r.Stroke = GetBrush();
                r.StrokeThickness = sldGrosor.Value;
                Canvas.SetLeft(r, Math.Min(puntoInicial.X, newPoint.X));
                Canvas.SetTop(r, Math.Min(puntoInicial.Y, newPoint.Y));
            }
            else if (cmbFigura.Text == "Ellipse")
            {
                Ellipse el = (Ellipse)figura;
                el.Width = Math.Abs(puntoInicial.X - newPoint.X);
                el.Height = Math.Abs(puntoInicial.Y - newPoint.Y);
                el.Stroke = GetBrush();
                el.StrokeThickness = sldGrosor.Value;
                Canvas.SetLeft(el, Math.Min(puntoInicial.X, newPoint.X));
                Canvas.SetTop(el, Math.Min(puntoInicial.Y, newPoint.Y));
            }
        }

        private void sldGrosor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (figura != null)
            {
                figura.StrokeThickness = sldGrosor.Value;
            }

        }

        private void Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rectanguloColor.Fill = GetBrush();
        }

        private void Lienzo_MouseLeave(object sender, MouseEventArgs e)
        {
            bandera = false;
            figura = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Lienzo.Clip = new RectangleGeometry(new Rect(0, 0, Lienzo.ActualWidth, Lienzo.ActualHeight));
            rectanguloColor.Fill = GetBrush();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)//el guardar
        {
            if (Lienzo != null)
            {
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)Lienzo.ActualWidth + 230,
                    (int)Lienzo.ActualHeight + 230,
                    96d, 96d, PixelFormats.Pbgra32);
                renderBitmap.Render(Lienzo);

                SaveFileDialog dlg = new() { Filter = "PNG|*.png|JPEG|*.jpg" };
                if (dlg.ShowDialog() == true)
                {
                    string path = dlg.FileName;
                    BitmapEncoder encoder;
                    if (System.IO.Path.GetExtension(path).ToLower() == "jpg")
                    {
                        encoder = new JpegBitmapEncoder();
                    }
                    else
                    {
                        encoder = new PngBitmapEncoder();
                    }
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                    using (FileStream fileStream = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
        }
        private List<Point> puntosBezier = new();

        private void DibujarCurvaBezier()
        {
            PathFigure figura = new PathFigure { StartPoint = puntosBezier[0] };
            BezierSegment segmento = new BezierSegment(puntosBezier[1], puntosBezier[2], puntosBezier[3], true);
            PathSegmentCollection segmentos = new() { segmento };
            figura.Segments = segmentos;

            PathGeometry geometria = new() { Figures = new PathFigureCollection { figura } };

            System.Windows.Shapes.Path curva = new()
            {
                Stroke = GetBrush(),
                StrokeThickness = sldGrosor.Value,
                Data = geometria
            };

            Lienzo.Children.Add(curva);
        }

        private void cmbFigura_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFigura.Text == "Borrador")
            {
                rectanguloColor.Fill = GetBrush();
            }
        }
    }
}