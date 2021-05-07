using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.ComponentModel;

namespace Kmeans
{
    [DataContract]
    class Cluster:INotifyPropertyChanged
    {
        [DataMember(Name ="X")]
        private double _x;

        [DataMember(Name ="Y")]
        private double _y;

        public double X
        {
            get
            { return _x; }
            set
            {
                _x = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("X"));
            }
        }

        public double Y
        {
            get
            { return _y; }
            set
            {
                _y = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Y"));
            }
        }

        [DataMember]
        public SyncronisedList<Point> Points { get; set; }

        public UICluster Element { get; set; }
        public bool Changing { get; set; }
        public Cluster(double x, double y, UICluster element)
        {
            X = x;
            Y = y;
            Points = new SyncronisedList<Point>();
            Element = element;
        }

        public Cluster()
        {
            Points = new SyncronisedList<Point>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdatePosition()
        {
            if (Points.Count > 0)
            {
                double x = 0, y = 0, number=0;
                foreach (Point p in Points)
                {
                    if (p.Typology== Point.Type.KMEANS)
                    {
                        x += p.X;
                        y += p.Y;
                        number++;
                    }
                }
                x = x / number;
                y = y / number;
                if (x == X && y == Y)
                    Changing = false;
                else
                {
                    X = x;
                    Y = y;
                    Changing = true;
                }
            }
        }

        public void Init(Canvas cnv)
        {
            foreach(Point p in Points)
            {
                p.Init(cnv, this);
            }
        }
    }
}
