using System;
using System.Collections.Generic;
using System.Globalization;
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

        public int maxFoodRendered = 1000;
        public int maxZoasRendered = 500;
        public int maxSourcesRendered = 100;

        public Dictionary<SourceType, double> sourcesCoeffs = new Dictionary<SourceType, double>()
        {
            {SourceType.Toxicity, 1},
            {SourceType.Fertility, 0.3 },
            {SourceType.Viscosity, 1 },
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

        public void Render(DrawingContext drawingContext, Matrix matrix, Point leftTopView, Point rightBottomView)
        {
            Point zeroPoint = matrix.Transform(new Point(0, 0));                                    
            Action drawAll = new Action(() =>
            {
                DinamicPoint[] elementsToDraw = world.pointsManager.getPoints(leftTopView.X, rightBottomView.X, leftTopView.Y, rightBottomView.Y);
                DinamicPointsSet[] setsToDraw = world.pointsManager.getPointsSets(leftTopView.X, rightBottomView.X, leftTopView.Y, rightBottomView.Y, maxFoodRendered + maxZoasRendered);
                //DinamicPoint[] elementsToDraw = world.pointsManager.getPointsByIdBorders(0, 50, 0, 5);
                int clustersDrawed = 0;
                foreach (Cluster c in world.pointsManager.clusters)
                {
                    Point[] edges = new Point[]
                    {
                        new Point(c.x, c.y),
                        new Point(c.x + c.size, c.y + c.size)
                    };
                    matrix.Transform(edges);
                    SolidColorBrush brush = new SolidColorBrush(Colors.Black) { Opacity = 0.4 }; ;
                    if (c.idX >= world.pointsManager.li && c.idX <= world.pointsManager.ri && c.idY >= world.pointsManager.ti && c.idY <= world.pointsManager.bi)
                    {
                        brush = Brushes.Red;
                        clustersDrawed++;
                        drawingContext.DrawRectangle(null, new Pen(brush, 1), new Rect(edges[0], edges[1]));
                    }
                }        

                List<Food> allFoodToDraw = new List<Food>();
                List<Protozoa> allZoasToDraw = new List<Protozoa>();
                List<SourcePoint> allSourcePointsToDraw = new List<SourcePoint>(world.surface.sourcePoints.Values);
                /*foreach (DinamicPoint p in elementsToDraw)
                    switch (p.type)
                    {
                        case World.ZoaType:
                            allZoasToDraw.Add(world.protozoas[p.id]);
                            break;
                        case World.FoodType:
                            allFoodToDraw.Add(world.food[p.id]);
                            break;
                    }*/
                foreach (DinamicPointsSet set in setsToDraw)
                    switch (set.type)
                    {
                        case World.ZoaType:                            
                            //allZoasToDraw.Add(world.protozoas[p.id]);
                            break;
                        case World.FoodType:
                            int size = set.points.Count;
                            double fire = 0;
                            double grass = 0;
                            double ocean = 0;
                            double toxicity = 0;
                            foreach (DinamicPoint p in set.points)
                            {
                                Food f = world.food[p.id];
                                fire += f.fire;
                                grass += f.grass;
                                ocean += f.ocean;
                                toxicity += f.toxicity;
                            }
                            toxicity /= set.points.Count;
                            Food joinedFood = new Food(new Pnt(set.x, set.y), fire, grass, ocean, toxicity, 0, size);
                            allFoodToDraw.Add(joinedFood);
                            break;
                    }
                List<Food> foodToDraw = new List<Food>(allFoodToDraw);
                List<Protozoa> zoasToDraw = new List<Protozoa>();
                List<SourcePoint> sourcePointsToDraw = new List<SourcePoint>();
                /*double foodStep = (double)maxFoodRendered / allFoodToDraw.Count;
                double zoasStep = (double)maxZoasRendered / allZoasToDraw.Count;
                double sourcesStep = (double)maxSourcesRendered / allSourcePointsToDraw.Count;                
                double buff = 0;
                for (int i = 0; i < allFoodToDraw.Count; i++)
                {
                    buff += foodStep;
                    if (buff < 1)
                        continue;
                    buff -= 1;
                    foodToDraw.Add(allFoodToDraw[i]);
                }
                buff = 0;
                for (int i = 0; i < allZoasToDraw.Count; i++)
                {
                    buff += zoasStep;
                    if (buff < 1)
                        continue;
                    buff -= 1;
                    zoasToDraw.Add(allZoasToDraw[i]);
                }
                buff = 0;
                for (int i = 0; i < allSourcePointsToDraw.Count; i++)
                {
                    buff += sourcesStep;
                    if (buff < 1)
                        continue;
                    buff -= 1;
                    sourcePointsToDraw.Add(allSourcePointsToDraw[i]);
                }*/


                //firstly, let's draw the surface
                foreach (SourcePoint spoint in sourcePointsToDraw)
                {
                    RadialGradientBrush brush = sourceBrushes[spoint.sourceType];
                    double coeff = sourcesCoeffs[spoint.sourceType];
                    brush.Opacity = surfaceOpacity * coeff;
                    double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(spoint.distance, 0)).X);
                    drawingContext.DrawEllipse(brush, null, matrix.Transform(spoint.location.toPoint()), radius * 2, radius * 2);
                    //drawingContext.DrawEllipse(null, new Pen(new SolidColorBrush(Colors.Black) { Opacity = 0.3 }, 1), matrix.Transform(spoint.location.toPoint()), radius, radius);
                }

                //now food                                
                foreach (Food f in foodToDraw)
                {

                    Point center = new Point(f.point.x - f.size * foodScale / 2, f.point.y - f.size * foodScale / 2);
                    center = matrix.Transform(center);

                    double zeroX = matrix.Transform(new Point(0, 0)).X;
                    double width = Math.Abs(zeroX - matrix.Transform(new Point(f.size * foodScale, 0)).X);

                    Pen pen = new Pen(Brushes.Black, 1);
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)(f.fireRate * 255), (byte)(f.grassRate * 255), (byte)(f.oceanRate * 255)));
                    drawingContext.DrawRectangle(brush, null, new Rect(center, new Size(width, width)));
                }

                Window mainWindow = Application.Current.Windows[0];
                mainWindow.Title = String.Format(
                    "ClustersDrawed: {0}/{1}, ZoaDrawed: {2}/{3}, FoodDrawed: {4}/{5}, SourcePointsDrawed: {6}/{7}", 
                    clustersDrawed, world.pointsManager.clusters.Length,
                    zoasToDraw.Count, world.protozoas.Count,
                    foodToDraw.Count, world.food.Count,
                    sourcePointsToDraw.Count, world.surface.sourcePoints.Count);
            });

            lock (world.surface)
                lock (world.food)
                    lock (world.protozoas)
                        lock (world.pointsManager)
                            drawAll();            
        }

        //without-clustering render
        /*public void Render(DrawingContext drawingContext, Matrix matrix)
        {
            Point zeroPoint = matrix.Transform(new Point(0, 0));

            //firstly, let's draw the surface
            foreach (SourcePoint spoint in world.surface.sourcePoints)
            {
                RadialGradientBrush brush = sourceBrushes[spoint.sourceType];
                double coeff = sourcesCoeffs[spoint.sourceType];
                brush.Opacity = surfaceOpacity * coeff;                
                double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(spoint.distance, 0)).X);
                drawingContext.DrawEllipse(brush, null, matrix.Transform(spoint.location.toPoint()), radius * 2, radius * 2);                
                drawingContext.DrawEllipse(null, new Pen(new SolidColorBrush(Colors.Black) { Opacity = 0.3 }, 1), matrix.Transform(spoint.location.toPoint()), radius, radius);

                double penWidth = 30;
                double penRadius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(penWidth, 0)).X);
                //drawingContext.DrawEllipse(null, new Pen(brush, penWidth), matrix.Transform(spoint.location.toPoint()), radius, radius);
            }

            //now food
            lock(world.food)
                foreach (Food f in world.food.Values)
                {                    
                    Point center = new Point(f.point.x, f.point.y);
                    center = matrix.Transform(center);

                    double zeroX = matrix.Transform(new Point(0, 0)).X;
                    double width = Math.Abs(zeroX - matrix.Transform(new Point(foodScale, 0)).X);

                    Pen pen = new Pen(Brushes.Black, 1);
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)(f.fireRate * 255), (byte)(f.grassRate * 255), (byte)(f.oceanRate * 255)));
                    drawingContext.DrawRectangle(brush, null, new Rect(center, new Size(width, width)));
                }

            //now zoas' view fields
            foreach (Protozoa zoa in world.protozoas.Values)
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
            foreach (Protozoa zoa in world.protozoas.Values)
            {
                Color color = zoa.zoaColor.toColor();
                SolidColorBrush brush = new SolidColorBrush(color);
                Pen pen = new Pen(brush, 1);
                brush.Opacity = zoa.energy / zoa.energyCapacity;
                
                double radius = Math.Abs(zeroPoint.X - matrix.Transform(new Point(zoa.radius, 0)).X);
                Point translatedCenter = matrix.Transform(zoa.centerP.toPoint());
                drawingContext.DrawEllipse(brush, pen, translatedCenter, radius, radius);
            }
        }*/
    }
}
