using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFRendererModel2
{
    public class WorldElement : FrameworkElement
    {
        public delegate void UpdateFun();

        WorldRenderer renderer;
        Matrix matrix;          
        IInputElement container;
        double foodWidth = 5;
        double viewedSize = 6;

        Timer updateTimer = new Timer();

        public double FoodSize
        {
            get { return foodWidth; }
            set { foodWidth = value; }
        }

        public double ViewRadius
        {
            get { return viewedSize; }
            set { viewedSize = value; }
        }

        public double UpdateInterval
        {
            get { return updateTimer.Interval; }
            set { updateTimer.Interval = value; }
        }

        public WorldElement(IInputElement parent)
        {
            container = parent;
            container.MouseWheel += On_MouseWheel;
            container.MouseMove += On_MouseMove;
            container.MouseLeftButtonDown += On_MouseDown;            
        }

        public void setWorldRenderer(WorldRenderer renderer)
        {
            this.renderer = renderer;            
            container = (IInputElement)Parent;
            matrix = new Matrix();
            matrix.Translate(100, 100);
            matrix.Scale(3, 3);

            updateTimer = new Timer(updateTimer.Interval);
            updateTimer.Interval = 16;
            updateTimer.Elapsed += (a, b) =>
            {
                try
                {
                    Parent.Dispatcher.Invoke(InvalidateVisual);
                }
                catch (Exception e)
                {

                }
            };
            updateTimer.Start();
        }

        private void On_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(container);

            if (e.Delta > 0)
                matrix.ScaleAt(1.1, 1.1, p.X, p.Y);
            else
                matrix.ScaleAt(1 / 1.1, 1 / 1.1, p.X, p.Y);

            InvalidateVisual();
        }

        Point pressedMouse;
        Point coordinates;
        private void On_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pressedMouse = e.GetPosition(container);
        }

        private void On_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = e.GetPosition(container);
            coordinates = mouse;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Vector delta = Point.Subtract(mouse, pressedMouse); // delta from old mouse to current mouse 
                pressedMouse = mouse;
                matrix.Translate(delta.X, delta.Y);
                e.Handled = true;

                InvalidateVisual();
            }
            pressedMouse = mouse;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            renderer.Render(drawingContext, matrix);
        }
    }
}
