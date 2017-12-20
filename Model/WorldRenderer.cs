using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Model
{
    public class WorldRenderer
    {
        public World world;

        public double foodScale = 3;
        public double zoaScale = 1;
        public double surfaceOpacity = 0.5;

        public Dictionary<SourceType, double> sourcesCoeffs = new Dictionary<SourceType, double>()
        {
            {SourceType.Toxicity, 50},
            {SourceType.Fertility, 1550 },
            {SourceType.Viscosity, 50 },
            {SourceType.Fire, 0},
            {SourceType.Grass, 0 },
            {SourceType.Ocean, 0 }
        };

        //my brushes, pens
        Dictionary<SourceType, RadialGradientBrush> sourceBrushes = new Dictionary<SourceType, RadialGradientBrush>()
        {
            {SourceType.Toxicity, new RadialGradientBrush(Colors.LightPink, Colors.Transparent) },
            {SourceType.Fertility, new RadialGradientBrush(Colors.DarkGreen, Colors.Transparent) },
            {SourceType.Viscosity, new RadialGradientBrush(Colors.SandyBrown, Colors.Transparent) },
            {SourceType.Fire, new RadialGradientBrush(Colors.Firebrick, Colors.Transparent) },
            {SourceType.Grass, new RadialGradientBrush(Colors.DarkSeaGreen, Colors.Transparent) },
            {SourceType.Ocean, new RadialGradientBrush(Colors.DeepSkyBlue, Colors.Transparent) }
        };        

        public WorldRenderer(World world)
        {
            this.world = world;            
        }
        public void Render(DrawingContext drawingContext, Matrix matrix)
        {
            Point zeroPoint = matrix.Transform(new Point(0, 0));

            //firstly, let's draw the surface
            foreach (SourcePoint spoint in world.surface.sourcePoints)
            {
                RadialGradientBrush brush = sourceBrushes[spoint.sourceType];
                double coeff = sourcesCoeffs[spoint.sourceType];
                brush.Opacity = surfaceOpacity;// * spoint.strength;                
                double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(spoint.strength * coeff, 0)).X);
                drawingContext.DrawEllipse(brush, null, matrix.Transform(spoint.location.toPoint()), radius, radius);
            }

            //now food
            lock(world.food)
                for (int i = 0; i < world.food.Count; i++)
                {
                    Food f = world.food[i];
                    Point center = new Point(f.point.x, f.point.y);
                    center = matrix.Transform(center);

                    double zeroX = matrix.Transform(new Point(0, 0)).X;
                    double width = Math.Abs(zeroX - matrix.Transform(new Point(foodScale, 0)).X);

                    Pen pen = new Pen(Brushes.Black, 1);
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)(f.fireRate * 255), (byte)(f.grassRate * 255), (byte)(f.oceanRate * 255)));
                    drawingContext.DrawRectangle(brush, null, new Rect(center, new Size(width, width)));
                }

            //now zoas' view fields
            foreach (Protozoa zoa in world.protozoas)
            {
                StreamGeometry viewZoneGeometry = new StreamGeometry();
                using (StreamGeometryContext ctx = viewZoneGeometry.Open())
                {
                    ctx.BeginFigure(matrix.Transform(zoa.centerP.toPoint()), true, true);
                    ctx.LineTo(matrix.Transform(zoa.leftViewP.toPoint()), true, false);
                    ctx.LineTo(matrix.Transform(zoa.rightViewP.toPoint()), true, false);
                }
                viewZoneGeometry.Freeze();
                Brush viewBrush = new SolidColorBrush(Colors.Azure)
                {
                    Opacity = 0
                };
                viewBrush.Freeze();
                Pen pen = new Pen(new SolidColorBrush(Colors.Black)
                {
                    Opacity = 0.1
                }, 1);
                pen.Freeze();
                drawingContext.DrawGeometry(viewBrush, pen, viewZoneGeometry);
            }

            //now zoas
            foreach (Protozoa zoa in world.protozoas)
            {
                Color color = zoa.zoaColor.toColor();
                SolidColorBrush brush = new SolidColorBrush(color);
                Pen pen = new Pen(brush, 1);
                brush.Opacity = zoa.energy / zoa.energyCapacity;
                
                double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(zoa.radius, 0)).X);
                Point translatedCenter = matrix.Transform(zoa.centerP.toPoint());
                drawingContext.DrawEllipse(brush, pen, translatedCenter, radius, radius);
            }
        }
    }
}
