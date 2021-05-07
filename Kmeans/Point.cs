using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Kmeans
{
    [DataContract]
    class Point: INotifyPropertyChanged
    {
        public enum Type: byte{KMEANS, KNN }

        public Type Typology
        {
            get;
            set;
        } = Type.KMEANS;


        private int _range;
        public int Range
        {
            get
            {
                return _range;
            }
            set
            {
                if (Typology == Type.KNN)
                    _range = value;
            }
        }


        [DataMember(Name ="X")]
        private double _x;

        [DataMember(Name ="Y")]
        private double _y;

        /// <summary>
        /// Coordinates X for this point
        /// </summary>
        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("X"));
            }
        }

        /// <summary>
        /// Coordinates Y for this point
        /// </summary>
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Y"));
            }
        }


        private Cluster _member;

        public Cluster Member
        {
            get
            {
                return _member;
            }
            set
            {
                _member = value;
            }
        }

        [DataMember]
        public UIPoint Element { get; set; }

        public Point(double x, double y, Type t)
        {
            X = x;
            Y = y;
            Element = new UIPoint(this);
            Typology = t;
        }
        public Point()
        {
            Element = new UIPoint(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Init(Canvas cnv, Cluster member)
        {
            Element.Init(cnv,this);
            _member = member;
        }

        //KNN
        #region
        public void SearchType(UIPoint[] points, out Cluster finded)
        {
            List<UIPoint> p = new List<UIPoint>(points);

            p.Sort((a, b) =>
            {
                return Compare(a, b);
            });

            List<UIPoint> near;
            if (Range >= p.Count)
            {
                near = p;
            }
            else
                near = p.GetRange(0, Range);
            List<KNNSerachTypes> result = new List<KNNSerachTypes>();
            for (int index1 = 1; index1 < near.Count; index1++) // parte da 1 perchè il punto più vicino è se stesso
            {

                bool finded1 = false;
                for (int index = 0; index < result.Count; index++)
                {
                    if (near[index1].Element.Member != null)
                    {
                        if (result[index].Cluster == near[index1].Element.Member)
                        {
                            result[index].Number++;
                            finded1 = true;
                        }
                    }
                }
                if (!finded1)
                {
                    result.Add(new KNNSerachTypes() { Number = 1, Cluster = near[index1].Element.Member });
                }
            }

            int max = int.MinValue;
            int pos = 0;
            for (int index = 0; index < result.Count; index++)
            {
                if (result[index].Number > max)
                {
                    pos = index;
                    max = result[index].Number;
                }
            }
            if (result[pos].Cluster == null)
                throw new Exception();
            finded = result[pos].Cluster;
        }

        public int Compare(UIPoint x, UIPoint y)
        {
            double distance1 = Math.Sqrt(Math.Pow(x.Element.X - X, 2) + Math.Pow(x.Element.Y - Y, 2));
            double distance2 = Math.Sqrt(Math.Pow(y.Element.X - X, 2) + Math.Pow(y.Element.Y - Y, 2));

            if (x.Element.Member == null && y.Element.Member == null)
                return 0;
            if (x.Element.Member == null)
                return 1;
            else if (y.Element.Member == null)
                return -1;

            return distance1.CompareTo(distance2);
        }
        #endregion


        //KMeans
        #region
        /// <summary>
        /// Search the nearest cluster
        /// </summary>
        /// <param name="cluster">List of cluster considered</param>
        public void SearchCluster(SyncronisedList<UICluster> cluster, out Cluster finded)
        {
            // If parameters is equal to null
            if (cluster == null || cluster.Count == 0)
                throw new InvalidOperationException("There's no cluster");

            double distance = double.MaxValue;
            Cluster min = cluster[0].Element;

            // Find the real nearest cluster
            for (int index = 0; index < cluster.Count; index++)
            {
                double tmp = Math.Sqrt(Math.Pow(cluster[index].Element.X - X, 2) + Math.Pow(cluster[index].Element.Y - Y, 2));
                if (tmp < distance)
                {
                    min = cluster[index].Element;
                    distance = tmp;
                }
            }

            //Saves the cluster nearest 
            if (Member != min)
            {
                finded = min;
            }
            else finded = null;
        }
        #endregion
    }
}
