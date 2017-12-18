using ModelObjective;
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

namespace WPFRender
{
    public class WorldElement : FrameworkElement
    {
        public delegate void UpdateFun();

        UpdateFun updateFun;
        Matrix matrix;
        World world;
        WorldController controller;
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
        
        public void setController(WorldController controller)
        {            
            this.controller = controller;
            updateTimer.Interval = 60;

            controller.OnNeedInvalidate += controllerUpdateTick;
        }
        
        public void setWorld(World world)
        {
            this.world = world;
            updateFun = selfUpdate;
            container = (IInputElement)Parent;
            matrix = new Matrix();            

            updateTimer = new Timer(updateTimer.Interval);
            updateTimer.Interval = 16;            
            updateTimer.Elapsed += (a, b) => updateFun();
            updateTimer.Start();            
        }

        private void selfUpdate()
        {
            redraw();
        }        

        private void controllerUpdateTick()
        {
            redraw();
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

            //draw life zone
            SolidColorBrush zoneBrush = new SolidColorBrush(Colors.DarkGreen)
            {
                Opacity = 0.01
            };
            Point leftTop = matrix.Transform(new Point(world.leftLifeBorder, world.topLifeBorder));
            Point rightBottom = matrix.Transform(new Point(world.rightLifeBorder, world.bottomLifeBorder));
            Rect rect = new Rect(leftTop, rightBottom);
            drawingContext.DrawRectangle(zoneBrush, new Pen(new SolidColorBrush(Colors.Black) { Opacity = 0.3 }, 10), rect);

            //draw food
            List<Food> food;
            lock (world.food)
                food = world.food.ToList();
            foreach (Food f in food)
            {
                Point center = new Point(f.point.x, f.point.y);
                center = matrix.Transform(center);

                double zeroX = matrix.Transform(new Point(0, 0)).X;
                double width = Math.Abs(zeroX - matrix.Transform(new Point(foodWidth, 0)).X);

                Pen pen = new Pen(Brushes.Black, 1);
                SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)(f.meat * 255), (byte)(f.herb * 255), (byte)(f.water * 255)));
                drawingContext.DrawRectangle(brush, null, new Rect(center, new Size(width, width)));
            }

            //draw zoas
            lock (world.Protozoas)
                foreach (Protozoa zoa in world.Protozoas)
                {
                    Point center = new Point(zoa.x, zoa.y);
                    center = matrix.Transform(center);

                    double zeroX = matrix.Transform(new Point(0, 0)).X;
                    double radius = Math.Abs(zeroX - matrix.Transform(new Point(zoa.radius, 0)).X);

                    Pen pen = new Pen(new SolidColorBrush(Colors.Black)
                    {
                        Opacity = 0.1
                    }, 1);                    
                    pen.Freeze();

                    StreamGeometry viewZoneGeometry = new StreamGeometry();
                    using (StreamGeometryContext ctx = viewZoneGeometry.Open())
                    {
                        ctx.BeginFigure(matrix.Transform(zoa.centerP.toPoint()), true, true);
                        ctx.LineTo(matrix.Transform(zoa.leftP.toPoint()), true, false);
                        ctx.LineTo(matrix.Transform(zoa.rightP.toPoint()), true, false);
                    }
                    viewZoneGeometry.Freeze();

                    Brush viewBrush = new SolidColorBrush(Colors.Azure)
                    {
                        Opacity = 0
                    };
                    viewBrush.Freeze();
                    Brush circleBrush = new SolidColorBrush(new ZoaHSL(zoa.color).toColor())
                    {
                        Opacity = zoa.radius / viewedSize
                    };
                    if (zoa.cooldown > 0)
                        circleBrush.Opacity = 0.5;
                    circleBrush.Freeze();

                    drawingContext.DrawGeometry(viewBrush, pen, viewZoneGeometry);
                    drawingContext.DrawEllipse(circleBrush, pen, center, radius, radius);                
                }            
        }

        public void redraw()
        {
            try
            {
                Dispatcher.Invoke(() => InvalidateVisual());
            }
            catch(Exception e)
            {

            }
        }
    }
}
