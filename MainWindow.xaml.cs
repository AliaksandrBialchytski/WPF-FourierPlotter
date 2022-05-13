using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
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
using System.Windows.Threading;
using System.Xml.Serialization;

namespace WpfFourierPlotter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        CircleOwn circle;
        ObservableCollection<CircleOwn> custdata;
        Stopwatch stopWatch = new Stopwatch();
        bool drawCircles = true;
        bool drawLines = true;
        bool started;
        public MainWindow()
        {
            InitializeComponent();
            started = true;
            custdata = new ObservableCollection<CircleOwn>();
            circle = new CircleOwn(100, 100);
            circle.tab = new int[32, 44];
            circle.tab[0, 1] = 4;
            custdata.Add(circle);
            custdata.Add(new CircleOwn(100, 500));
            //custdata.Add(new CircleOwn(50, 100));
            circleGrid.DataContext = custdata;
            circleGrid.CellEditEnding += circleGrid_CellEditEnding;
            timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 25) };
            timer.Tick += Timer_Tick;

        }
        private void Timer_Tick(object sender, object e)
        {
            TimeSpan ts = stopWatch.Elapsed;
            progressBar.Value = ts.TotalMilliseconds / 100;
            if (progressBar.Value == 100)
            {
                stopWatch.Stop();
                timer.Stop();
                if (drawCircles)
                    Draw_Canvas();
            }
        }
        private void New_Click(object sender, RoutedEventArgs e)
        {
            Clean_Canvas();
            circleGrid.DataContext = new ObservableCollection<CircleOwn>();
            stopWatch.Reset();
            this.timer.Stop();
            this.progressBar.Value = 0;
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
           MessageBoxResult result = MessageBox.Show("Do you want to leave?", 
                                                    "Exit", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    this.Close();
                    break;
                default:
                    break;
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Start();
            this.timer.Start();
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Stop();
            this.timer.Stop();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Reset();
            this.timer.Stop();
            this.progressBar.Value = 0;
            Clean_Canvas();
        }
        private void Clean_Canvas()
        {
            using (var bmp = new Bitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight))
            using (var gfx = Graphics.FromImage(bmp))
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black))
            {
                gfx.Clear(System.Drawing.Color.White);
                image.Source = BmpImageFromBmp(bmp);
            }
        }
        private void Draw_Canvas()
        {
            // https://swharden.com/CsharpDataVis/drawing/3-drawing-in-wpf.md.html
            using (var bmp = new Bitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight))
            using (var gfx = Graphics.FromImage(bmp))
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var radius = custdata[0].Radius;
                var rect = new System.Drawing.Rectangle((int)canvas.ActualWidth / 2 - radius / 2, (int)canvas.ActualHeight / 2 - radius / 2, radius, radius);
                gfx.DrawEllipse(pen, rect);
                image.Source = BmpImageFromBmp(bmp);
            }
        }
        private BitmapImage BmpImageFromBmp(Bitmap bmp)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        void circleGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;
                    if (bindingPath == "Radius")
                    {
                        var el = e.EditingElement as TextBox;
                        int radius = Int32.Parse(el.Text);
                        //int prop = ((CircleOwn)e.Row.Item).Radius;
                        using (var bmp = new Bitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight))
                        using (var gfx = Graphics.FromImage(bmp))
                        using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black))
                        {
                            gfx.Clear(System.Drawing.Color.White);
                            image.Source = BmpImageFromBmp(bmp);
                            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            var rect = new System.Drawing.Rectangle((int)canvas.ActualWidth / 2 - radius / 2, (int)canvas.ActualHeight / 2 - radius / 2, radius, radius);
                            gfx.DrawEllipse(pen, rect);
                            image.Source = BmpImageFromBmp(bmp);
                        }
                    }
                }
            }
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(CircleOwnList));
                        var circles = (CircleOwnList)serializer.Deserialize(fs);
                        //circleGrid.DataContext = circles.Circles;
                        custdata = new ObservableCollection<CircleOwn>();
                        foreach (var item in circles.Circles)
                            custdata.Add(item);
                        circleGrid.DataContext = custdata;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Select correct .xml file!", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml";
            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CircleOwnList));
                    var list = new CircleOwnList();
                    list.Circles =(ObservableCollection<CircleOwn>)circleGrid.DataContext;
                    serializer.Serialize(fs, list);
                }
            }
        }
        private void Circles_Checked(object sender, RoutedEventArgs e)
        {
            drawCircles = true;
            if (started)
                Draw_Canvas();
        }

        private void Circles_Unchecked(object sender, RoutedEventArgs e)
        {
            drawCircles = false;
            if (started)
                Clean_Canvas();
        }
        private void Lines_Checked(object sender, RoutedEventArgs e)
        {
            drawLines = true;
        }

        private void Lines_Unchecked(object sender, RoutedEventArgs e)
        {
            drawLines = false;
        }

    }
    
}
