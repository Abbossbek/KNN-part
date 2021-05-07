using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Kmeans
{
    [DataContract]
    class UICluster
    {
        [DataMember]
        public Cluster Element { get; set; }

        [DataMember(Name ="Width", IsRequired =false)]
        private int _width = 10;

        [DataMember(Name ="Height", IsRequired =false)]
        private int _height = 10;

        public Ellipse UIElement { get; set; }

        private Canvas _father;

        public bool IsVisible { get; set; }

        Brush _colour;
        public Brush Colour { get { return _colour; } }

        public UICluster()
        {
            Element = new Cluster();
            Element.Element= this;
            Element.X = 0;
            Element.Y = 0;
            Element.PropertyChanged += Element_PropertyChanged;

            Random rnd = new Random();
            Thread.Sleep(10);
            _colour = PickBrush(rnd);

            UIElement = new Ellipse();

            UIElement.Fill = _colour;
            UIElement.Width = _width;
            UIElement.Height = _height;
        }

        

        public UICluster(double x, double y, Random rnd, int height = 10, int width = 10)
        {
            Element = new Cluster(x, y, this);
            Element.PropertyChanged += Element_PropertyChanged;

            _colour = PickBrush(rnd);
            UIElement = new Ellipse();
            UIElement.Fill = _colour;
            UIElement.Width = width;
            UIElement.Height = height;

            _height = height;
            _width = width;
        }

        private void Element_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Canvas.SetTop(UIElement, Element.Y - UIElement.Height / 2);
                Canvas.SetLeft(UIElement, Element.X - UIElement.Width / 2);
            }
        }

        private Brush PickBrush(Random rnd)
        {
            Brush result = Brushes.Transparent;

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(128,properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }

       

        public void AddFather(Canvas father)
        {
            if (_father != null)
                throw new Exception("Canvas already set");
            father.Children.Add(UIElement);

            Canvas.SetTop(UIElement, Element.Y - UIElement.Height / 2);
            Canvas.SetLeft(UIElement, Element.X - UIElement.Width / 2);

            _father = father;
            IsVisible = true;
        }

        public void RemoveFather()
        {
            if (_father == null)
                throw new Exception("Canvas already remover or not inserted");
            _father.Children.Remove(UIElement);
            _father = null;
            IsVisible = false;
        }

        public void UpdatePosition()
        {
            Element.UpdatePosition();
            Canvas.SetTop(UIElement, Element.Y - UIElement.Height / 2);
            Canvas.SetLeft(UIElement, Element.X - UIElement.Width / 2);

            if (_father != null)
            {
                for (int index = 0; index < Element.Points.Count; index++)
                {

                    if (!Element.Points[index].Element.VisibleLine)
                        Element.Points[index].Element.AddFatherLine(_father);
                    Element.Points[index].Element.Line.X2 = Element.X;
                    Element.Points[index].Element.Line.Y2 = Element.Y;
                    Element.Points[index].Element.Line.X1 = Element.Points[index].Element.Element.X;
                    Element.Points[index].Element.Line.Y1 = Element.Points[index].Element.Element.Y;
                    Element.Points[index].Element.Line.Fill = _colour;
                    Element.Points[index].Element.Line.Stroke = _colour;
                }
            }
        }

        public void Init(Canvas cnv, Random rnd)
        {
            Element.Element = this;
            Element.Init(cnv);

            AddFather(cnv);
            _colour = PickBrush(rnd);

            UIElement = new Ellipse();
            UIElement.Fill = _colour;
            UIElement.Width = _width;
            UIElement.Height = _height;
        }
    }
}
