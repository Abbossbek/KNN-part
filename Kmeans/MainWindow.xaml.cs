using System;
using System.Collections.Generic;
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
using System.Threading;
using Microsoft.Win32;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;

namespace Kmeans
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SyncronisedList<UIPoint> _pointsUI = new SyncronisedList<UIPoint>();

        private SyncronisedList<UICluster> _clusterUI = new SyncronisedList<UICluster>();
        const int MAXPOINT = int.MaxValue;

        private DispatcherTimer _dsp = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(40) };
        private bool _firstPlay = true;
        public MainWindow()
        {
            InitializeComponent();
            btnClusters.IsEnabled = false;
            btnPoints.IsEnabled = false;
            _dsp.Tick += Dsp_Tick;

        }

        private void btnPoints_Click(object sender, RoutedEventArgs e)
        {

            Random rnd = new Random();
            for (int index = 0; index < int.Parse(txtNumberPoints.Text) && !(_pointsUI.Count > MAXPOINT); index++)
            {
                System.Windows.Point tmp;

                do
                {
                    tmp = GenerateRandomPoint(rnd, cnvPoints.ActualHeight * 1);
                    tmp.X = (int)(tmp.X + cnvPoints.ActualWidth / 2);
                    tmp.Y = (int)(tmp.Y + cnvPoints.ActualHeight / 2);

                } while (tmp.X > cnvPoints.ActualWidth || tmp.Y > cnvPoints.ActualHeight || tmp.X < 0 || tmp.Y < 0); // If the point exit to the margin of the canvas recalculate it

                if (!((tmp.X > cnvPoints.ActualWidth) || (tmp.Y > cnvPoints.ActualHeight)))
                {
                    UIPoint _new = new UIPoint(new Point(tmp.X, tmp.Y, Point.Type.KMEANS));
                    _new.AddFatherPoint(cnvPoints);
                    _pointsUI.Add(_new);

                }
                if (!_firstPlay)
                {
                    _dsp.Start();
                }
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _dsp.Start();
            _firstPlay = false;
        }

        private void Dsp_Tick(object sender, EventArgs e)
        { 
                int numberThread = (int)(_pointsUI.Count / 500);

                if (numberThread > 1000)
                    numberThread = 1000;
                if (numberThread <= 0)
                    numberThread = 1;

                int rangeForThread = 1 + _pointsUI.Count / numberThread;

                ManualResetEvent[] doneEvent = new ManualResetEvent[numberThread];
                Thread[] ts = new Thread[numberThread];
                for (int index = 0, startRange = 0, endRange = rangeForThread - 1; index < numberThread; index++)
                {
                    doneEvent[index] = new ManualResetEvent(false);
                    ts[index] = new Thread(new ParameterizedThreadStart((object o) =>
                     {
                         SearchClusterRange((PointsDataCalc)o);
                     }));
                    ts[index].Start(new PointsDataCalc(doneEvent[index], startRange, endRange));
                    startRange += rangeForThread;
                    endRange += rangeForThread;
                }

                //Aspetta che i thread finiscano
                for (int index = 0; index < ts.Length; index++)
                    ts[index].Join();

            bool changed = false;
            for (int index = 0; index < _clusterUI.Count; index++)
            {
                _clusterUI[index].UpdatePosition();
                //if(_clusterUI[index].) Aggiunge il father se non ce l'ha DA FARE
                if (_clusterUI[index].Element.Changing)
                {
                    changed = true;
                    _firstPlay = false;
                }
            }
            if (!changed)
                _dsp.Stop();

        }

        void SearchClusterRange(PointsDataCalc calc)
        {
            for (int index = calc.StartIndex; index < _pointsUI.Count && index <= calc.EndIndex; index++)
            {
                _pointsUI[index].SearchCluster(_clusterUI, _pointsUI);

            }
            calc.DoneEvent.Set();
        }

        private void cnvPoints_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_pointsUI.Count < MAXPOINT)
            {
                System.Windows.Point tmp = Mouse.GetPosition(cnvPoints);
                UIPoint _new;

                if ((bool)rbPoint.IsChecked)
                {
                    _new = new UIPoint(new Point(tmp.X, tmp.Y, Point.Type.KMEANS));
                    _new.AddFatherPoint(cnvPoints);
                    _pointsUI.Add(_new);
                }
                else if ((bool)rbCluster.IsChecked)
                {
                    UICluster uic = new UICluster(tmp.X, tmp.Y, new Random());
                    uic.AddFather(cnvPoints);
                    _clusterUI.Add(uic);
                }
                else
                {
                    MessageBox.Show("Scegliere se usare il Kmeans o il KNN", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!_firstPlay)
                {
                    _dsp.Start();
                }
            }
        }

        private void txtNumberPoints_TextChanged(object sender, TextChangedEventArgs e)
        {
            int a = 0;
            if (!int.TryParse(txtNumberPoints.Text, out a) || a > MAXPOINT - _pointsUI.Count || a < 1)
            {
                txtNumberPointsLbl.Foreground = Brushes.Red;
                btnPoints.IsEnabled = false;
            }
            else
            {
                txtNumberPointsLbl.Foreground = Brushes.Black;
                btnPoints.IsEnabled = true;
            }
        }

        private void txtNumberCluster_TextChanged(object sender, TextChangedEventArgs e)
        {
            int a = 0;
            if (!int.TryParse(txtNumberCluster.Text, out a) || a > 1000 || a < 1)
            {
                txtNumberClusterLbl.Foreground = Brushes.Red;
                btnClusters.IsEnabled = false;
            }
            else
            {
                txtNumberClusterLbl.Foreground = Brushes.Black;
                btnClusters.IsEnabled = true;
            }
        }

        private void btnClusters_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            for (int index = 0; index < int.Parse(txtNumberCluster.Text); index++)
            {
                UICluster tmp = new UICluster(rnd.Next((int)cnvPoints.ActualWidth), rnd.Next((int)cnvPoints.ActualHeight), rnd);
                tmp.AddFather(cnvPoints);
                _clusterUI.Add(tmp);
            }
            if (!_firstPlay)
                _dsp.Start();
        }

        private System.Windows.Point GenerateRandomPoint(Random rnd, double radius)
        {
            double u = rnd.NextDouble();
            double v = rnd.NextDouble();

            int x = (int)(radius * Math.Sqrt(-2 * Math.Log(u)) * Math.Sin(2 * Math.PI * v));
            int y = (int)(radius * Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v));

            return new System.Windows.Point(x, y);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _dsp.Stop();
            _firstPlay = true;
        }
        private void mnuSalvaCome_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
