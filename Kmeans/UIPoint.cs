using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace Kmeans
{
    [DataContract]
    class UIPoint
    {
        public Point Element { get; set; }

        [DataMember(Name = "Width", IsRequired = false)]
        private int _width = 10;

        [DataMember(Name = "Height", IsRequired = false)]
        private int _height = 10;

        public Ellipse UIElement { get; set; }

        private Canvas _father { get; set; }


        private bool _visiblePoint = false;
        public bool VisiblePoint
        {
            get
            {
                return _visiblePoint;
            }
            private set
            {
                _visiblePoint = value;
            }

        }

        private bool _visibleLine=false;
        public bool VisibleLine
        {
            get
            {
                return _visibleLine;
            }
            private set
            {
                _visibleLine = value;
            }
        }

        public Line Line { get; set; }

        public UIPoint(Point p, int height=10, int width=10)
        {
            Element = p;
            p.PropertyChanged += P_PropertyChanged;

            UIElement = new Ellipse();
            
            UIElement.Width = width;
            UIElement.Height = height;

            _width = width;
            _height = height;

            UIElement.Fill = Brushes.Black;

            Line = new Line() { X1 = Element.X, Y1 = Element.Y, StrokeThickness = 1 };
            Canvas.SetZIndex(Line, 1);
        }

        private void P_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Canvas.SetTop(UIElement, Element.Y - UIElement.Height / 2);
            Canvas.SetLeft(UIElement, Element.X - UIElement.Width / 2);
            Line.X1 = Element.X;
            Line.Y1 = Element.Y;
        }

        public void AddFatherPoint(Canvas father)
        {
            if (_father != null && VisiblePoint)
                throw new Exception("Canvas already set");
            father.Children.Add(UIElement);

            Canvas.SetTop(UIElement, Element.Y-UIElement.Height/2);
            Canvas.SetLeft(UIElement, Element.X - UIElement.Width / 2);

            VisiblePoint = true;
            _father = father;
        }

        public void RemoveFatherPoint()
        {
            if (_father != null && !VisiblePoint)
                throw new Exception("Canvas already remover or not inserted");
            _father.Children.Remove(UIElement);
            VisiblePoint = false;
            _father = null;
        }

        public void AddFatherLine(Canvas father)
        {
            if (_father != null && VisibleLine)
                throw new Exception("Canvas already set");
            father.Children.Add(Line);

            VisibleLine = true;
            _father = father;
            if(Element.Member!=null)
            {
                Line.X1 = Element.Member.X;
                Line.Y1 = Element.Member.Y;
            }
        }

        public void RemoveFatherLine()
        {
            if (_father != null && !VisibleLine)
                throw new Exception("Canvas already remover or not inserted");
            _father.Children.Remove(Line);

            VisibleLine = false;
            _father = null;
        }

        public void SearchCluster(SyncronisedList<UICluster> cluster, SyncronisedList<UIPoint> points)
        {
            Cluster finded=null;

            if (Element.Typology== Point.Type.KMEANS)
            {
                Element.SearchCluster(cluster, out finded);
            }
            else if(Element.Typology == Point.Type.KNN)
            {
                Element.SearchType(points.ToArray(), out finded);
            }

            if (finded != null)
            {
                if (Element.Member != null)
                    Element.Member.Points.Remove(Element);
                Element.Member = finded;
                Element.Member.Points.Add(Element);
            }
        }

        public void Init(Canvas cnv, Point p)
        {
            Element = p;

            AddFatherLine(cnv);
            AddFatherPoint(cnv);

            Element.PropertyChanged += P_PropertyChanged;

            UIElement = new Ellipse();

            UIElement.Width = _width;
            UIElement.Height = _height;
            UIElement.Fill = Brushes.Black;

            Line = new Line() { X1 = Element.X, Y1 = Element.Y, StrokeThickness = 1 };
            if(Element.Member!=null)
            {
                Line.Fill = Element.Member.Element.Colour;
                Line.Stroke= Element.Member.Element.Colour;
            }

            Canvas.SetZIndex(Line, 1);
        }
    }
}
